using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using Stripe;
using Stripe.Checkout;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.Services.Payment
{
    /// <summary>
    /// StripePaymentService - Tích hợp Stripe Payment Gateway.
    /// Sử dụng Stripe Checkout Sessions cho luồng thanh toán.
    /// </summary>
    public class StripePaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly string _webhookSecret;

        public StripePaymentService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<StripePaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;

            // Configure Stripe API key
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            _webhookSecret = _configuration["Stripe:WebhookSecret"] ?? "";
        }

        /// <inheritdoc />
        public async Task<CheckoutResult> CreateCheckoutSessionAsync(
            Guid invoiceId, 
            string successUrl, 
            string cancelUrl)
        {
            try
            {
                // Lấy Invoice
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
                if (invoice == null)
                {
                    return new CheckoutResult
                    {
                        Success = false,
                        ErrorMessage = "Invoice not found."
                    };
                }

                // Kiểm tra trạng thái
                if (invoice.Status == InvoiceStatus.Paid)
                {
                    return new CheckoutResult
                    {
                        Success = false,
                        ErrorMessage = "Invoice is already paid."
                    };
                }

                // Lấy thông tin Customer từ Order
                var order = await _unitOfWork.Orders.GetByIdAsync(invoice.OrderId);
                var customer = order != null ? await _unitOfWork.Customers.GetByIdAsync(order.CustomerId) : null;

                // Tạo Stripe Checkout Session
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    Mode = "payment",
                    SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = cancelUrl,
                    ClientReferenceId = invoiceId.ToString(),
                    CustomerEmail = customer?.Email,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "vnd", // hoặc "usd"
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Invoice #{invoice.InvoiceNumber}",
                                    Description = $"Payment for invoice {invoice.InvoiceNumber}"
                                },
                                UnitAmount = (long)(invoice.TotalAmount) // Stripe uses smallest currency unit
                            },
                            Quantity = 1
                        }
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "invoice_id", invoiceId.ToString() },
                        { "invoice_number", invoice.InvoiceNumber ?? "" }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                // Tạo PaymentTransaction record
                var transaction = new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoiceId,
                    GatewayTransactionId = session.PaymentIntentId ?? session.Id,
                    Gateway = PaymentGateway.Stripe,
                    Amount = invoice.TotalAmount,
                    Currency = "VND",
                    Status = PaymentTransactionStatus.Pending,
                    GatewayMetadata = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        SessionId = session.Id,
                        PaymentIntentId = session.PaymentIntentId
                    }),
                    CreateAt = DateTime.UtcNow
                };

                _unitOfWork.PaymentTransactions.Add(transaction);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Stripe checkout session created: {SessionId} for Invoice: {InvoiceId}", 
                    session.Id, invoiceId);

                return new CheckoutResult
                {
                    Success = true,
                    CheckoutUrl = session.Url,
                    SessionId = session.Id
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating checkout session for Invoice: {InvoiceId}", invoiceId);
                return new CheckoutResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout session for Invoice: {InvoiceId}", invoiceId);
                return new CheckoutResult
                {
                    Success = false,
                    ErrorMessage = "Failed to create checkout session."
                };
            }
        }

        /// <inheritdoc />
        public async Task<bool> HandleWebhookAsync(string payload, string signature)
        {
            try
            {
                // Verify webhook signature
                var stripeEvent = EventUtility.ConstructEvent(
                    payload, 
                    signature, 
                    _webhookSecret);

                _logger.LogInformation("Received Stripe webhook: {EventType}", stripeEvent.Type);

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        await HandleCheckoutSessionCompleted(stripeEvent);
                        break;

                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;

                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;

                    default:
                        _logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook verification failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Stripe webhook");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<PaymentWebhookEvent?> GetPaymentStatusAsync(string sessionId)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                return new PaymentWebhookEvent
                {
                    EventType = session.PaymentStatus,
                    TransactionId = session.PaymentIntentId ?? "",
                    SessionId = session.Id,
                    Amount = session.AmountTotal.HasValue ? session.AmountTotal.Value / 100m : 0,
                    Currency = session.Currency,
                    Metadata = session.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for session: {SessionId}", sessionId);
                return null;
            }
        }

        #region Private Helpers

        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return;

            var invoiceIdStr = session.ClientReferenceId ?? session.Metadata?.GetValueOrDefault("invoice_id");
            if (string.IsNullOrEmpty(invoiceIdStr) || !Guid.TryParse(invoiceIdStr, out var invoiceId))
            {
                _logger.LogWarning("No invoice_id found in checkout session: {SessionId}", session.Id);
                return;
            }

            await UpdateInvoicePaymentStatus(invoiceId, session.Id, PaymentTransactionStatus.Completed);
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogInformation("PaymentIntent succeeded: {PaymentIntentId}", paymentIntent.Id);
            // Payment already handled by checkout.session.completed in most cases
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogWarning("PaymentIntent failed: {PaymentIntentId} - {Error}", 
                paymentIntent.Id, 
                paymentIntent.LastPaymentError?.Message);

            // Update transaction status
            var transactions = await _unitOfWork.PaymentTransactions.GetAllAsync(
                t => t.GatewayTransactionId == paymentIntent.Id);

            foreach (var transaction in transactions)
            {
                transaction.Status = PaymentTransactionStatus.Failed;
                transaction.ErrorMessage = paymentIntent.LastPaymentError?.Message;
                transaction.UpdateAt = DateTime.UtcNow;
                _unitOfWork.PaymentTransactions.Update(transaction);
            }

            await _unitOfWork.CompleteAsync();
        }

        private async Task UpdateInvoicePaymentStatus(
            Guid invoiceId, 
            string sessionId, 
            PaymentTransactionStatus status)
        {
            // Update Invoice
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
            if (invoice != null)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
                invoice.UpdateAt = DateTime.UtcNow;
                _unitOfWork.Invoices.Update(invoice);

                _logger.LogInformation("Invoice marked as paid: {InvoiceId}", invoiceId);
            }

            // Update Transaction
            var transactions = await _unitOfWork.PaymentTransactions.GetAllAsync(
                t => t.InvoiceId == invoiceId && t.Status == PaymentTransactionStatus.Pending);

            foreach (var transaction in transactions)
            {
                transaction.Status = status;
                transaction.CompletedAt = DateTime.UtcNow;
                transaction.UpdateAt = DateTime.UtcNow;
                _unitOfWork.PaymentTransactions.Update(transaction);
            }

            await _unitOfWork.CompleteAsync();
        }

        #endregion
    }
}
