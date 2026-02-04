using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using System.Text.RegularExpressions;

namespace NovaSaaS.Infrastructure.Logging
{
    /// <summary>
    /// Data Masking Enricher - Tự động che giấu dữ liệu nhạy cảm trong logs.
    /// Bảo vệ PII (Personally Identifiable Information) và thông tin thanh toán.
    /// </summary>
    public class DataMaskingEnricher : ILogEventEnricher
    {
        private static readonly string[] SensitivePropertyNames = new[]
        {
            "password", "pwd", "secret", "secretkey", "apikey", "api_key",
            "creditcard", "credit_card", "cardnumber", "card_number",
            "cvv", "cvc", "expiry", "expirydate",
            "ssn", "socialsecurity",
            "token", "accesstoken", "access_token", "refreshtoken", "refresh_token",
            "authorization", "bearer",
            "connectionstring", "connection_string"
        };

        private static readonly Regex CreditCardPattern = new(
            @"\b(?:\d{4}[-\s]?){3}\d{4}\b",
            RegexOptions.Compiled);

        private static readonly Regex EmailPattern = new(
            @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
            RegexOptions.Compiled);

        private static readonly Regex PhonePattern = new(
            @"\b(?:\+84|0)(?:\d{9,10})\b",
            RegexOptions.Compiled);

        private const string MaskedValue = "***MASKED***";
        private const string MaskedEmail = "***@***.***";
        private const string MaskedPhone = "***-***-****";

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var maskedProperties = new List<LogEventProperty>();

            foreach (var prop in logEvent.Properties)
            {
                var maskedValue = MaskPropertyValue(prop.Key, prop.Value);
                if (maskedValue != null)
                {
                    maskedProperties.Add(new LogEventProperty(prop.Key, maskedValue));
                }
            }

            foreach (var maskedProp in maskedProperties)
            {
                logEvent.AddOrUpdateProperty(maskedProp);
            }
        }

        private LogEventPropertyValue? MaskPropertyValue(string propertyName, LogEventPropertyValue value)
        {
            // Check if property name is sensitive
            if (IsSensitivePropertyName(propertyName))
            {
                return new ScalarValue(MaskedValue);
            }

            // Handle different value types
            return value switch
            {
                ScalarValue scalar => MaskScalarValue(scalar),
                StructureValue structure => MaskStructureValue(structure),
                SequenceValue sequence => MaskSequenceValue(sequence),
                DictionaryValue dict => MaskDictionaryValue(dict),
                _ => null
            };
        }

        private bool IsSensitivePropertyName(string propertyName)
        {
            var lowerName = propertyName.ToLowerInvariant();
            return SensitivePropertyNames.Any(s => lowerName.Contains(s));
        }

        private LogEventPropertyValue? MaskScalarValue(ScalarValue scalar)
        {
            if (scalar.Value is string stringValue)
            {
                var masked = MaskSensitivePatterns(stringValue);
                if (masked != stringValue)
                {
                    return new ScalarValue(masked);
                }
            }
            return null;
        }

        private LogEventPropertyValue? MaskStructureValue(StructureValue structure)
        {
            var maskedProps = new List<LogEventProperty>();
            var hasChanges = false;

            foreach (var prop in structure.Properties)
            {
                if (IsSensitivePropertyName(prop.Name))
                {
                    maskedProps.Add(new LogEventProperty(prop.Name, new ScalarValue(MaskedValue)));
                    hasChanges = true;
                }
                else
                {
                    var maskedValue = MaskPropertyValue(prop.Name, prop.Value);
                    if (maskedValue != null)
                    {
                        maskedProps.Add(new LogEventProperty(prop.Name, maskedValue));
                        hasChanges = true;
                    }
                    else
                    {
                        maskedProps.Add(prop);
                    }
                }
            }

            return hasChanges ? new StructureValue(maskedProps, structure.TypeTag) : null;
        }

        private LogEventPropertyValue? MaskSequenceValue(SequenceValue sequence)
        {
            var maskedElements = new List<LogEventPropertyValue>();
            var hasChanges = false;

            foreach (var element in sequence.Elements)
            {
                if (element is ScalarValue scalar)
                {
                    var maskedValue = MaskScalarValue(scalar);
                    if (maskedValue != null)
                    {
                        maskedElements.Add(maskedValue);
                        hasChanges = true;
                    }
                    else
                    {
                        maskedElements.Add(element);
                    }
                }
                else
                {
                    maskedElements.Add(element);
                }
            }

            return hasChanges ? new SequenceValue(maskedElements) : null;
        }

        private LogEventPropertyValue? MaskDictionaryValue(DictionaryValue dict)
        {
            var maskedElements = new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>();
            var hasChanges = false;

            foreach (var element in dict.Elements)
            {
                var keyName = element.Key.Value?.ToString() ?? "";
                if (IsSensitivePropertyName(keyName))
                {
                    maskedElements.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                        element.Key, new ScalarValue(MaskedValue)));
                    hasChanges = true;
                }
                else
                {
                    maskedElements.Add(element);
                }
            }

            return hasChanges ? new DictionaryValue(maskedElements) : null;
        }

        private string MaskSensitivePatterns(string input)
        {
            var result = input;

            // Mask credit card numbers
            result = CreditCardPattern.Replace(result, "****-****-****-****");

            // Mask email addresses (partial)
            result = EmailPattern.Replace(result, match =>
            {
                var email = match.Value;
                var atIndex = email.IndexOf('@');
                if (atIndex > 2)
                {
                    return email[0] + "***" + email[(atIndex - 1)..];
                }
                return MaskedEmail;
            });

            // Mask phone numbers
            result = PhonePattern.Replace(result, MaskedPhone);

            return result;
        }
    }

    /// <summary>
    /// Stripe-specific log filter - Chỉ cho phép log GatewayTransactionId.
    /// </summary>
    public static class StripeLogFilter
    {
        private static readonly HashSet<string> AllowedStripeFields = new()
        {
            "GatewayTransactionId",
            "PaymentIntentId",
            "CustomerId",
            "InvoiceId",
            "Status",
            "Amount",
            "Currency"
        };

        public static object SanitizeStripePayload(object payload)
        {
            if (payload is IDictionary<string, object> dict)
            {
                return dict
                    .Where(kvp => AllowedStripeFields.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            return new { Note = "Stripe payload sanitized for security" };
        }
    }
}
