namespace NovaSaaS.WebApi.Constants
{
    /// <summary>
    /// Enterprise Error Codes - Mã lỗi chuẩn cho toàn hệ thống.
    /// Format: ERR_{MODULE}_{NUMBER}
    /// </summary>
    public static class ErrorCodes
    {
        // ========================================
        // SYSTEM ERRORS (000-099)
        // ========================================
        public const string SYSTEM_UNKNOWN = "ERR_SYSTEM_000";
        public const string SYSTEM_DATABASE_ERROR = "ERR_SYSTEM_001";
        public const string SYSTEM_CONFIGURATION_ERROR = "ERR_SYSTEM_002";
        public const string SYSTEM_TIMEOUT = "ERR_SYSTEM_003";
        public const string SYSTEM_SERVICE_UNAVAILABLE = "ERR_SYSTEM_004";
        public const string SYSTEM_RATE_LIMITED = "ERR_SYSTEM_005";

        // ========================================
        // AUTHENTICATION ERRORS (100-199)
        // ========================================
        public const string AUTH_UNAUTHORIZED = "ERR_AUTH_100";
        public const string AUTH_TOKEN_EXPIRED = "ERR_AUTH_101";
        public const string AUTH_TOKEN_INVALID = "ERR_AUTH_102";
        public const string AUTH_FORBIDDEN = "ERR_AUTH_103";
        public const string AUTH_INVALID_CREDENTIALS = "ERR_AUTH_104";
        public const string AUTH_ACCOUNT_LOCKED = "ERR_AUTH_105";
        public const string AUTH_ACCOUNT_DISABLED = "ERR_AUTH_106";

        // ========================================
        // TENANT ERRORS (200-299)
        // ========================================
        public const string TENANT_NOT_FOUND = "ERR_TENANT_200";
        public const string TENANT_SUSPENDED = "ERR_TENANT_201";
        public const string TENANT_SUBSCRIPTION_EXPIRED = "ERR_TENANT_202";
        public const string TENANT_LIMIT_EXCEEDED = "ERR_TENANT_203";
        public const string TENANT_FEATURE_DISABLED = "ERR_TENANT_204";

        // ========================================
        // VALIDATION ERRORS (300-399)
        // ========================================
        public const string VALIDATION_FAILED = "ERR_VALID_300";
        public const string VALIDATION_REQUIRED_FIELD = "ERR_VALID_301";
        public const string VALIDATION_INVALID_FORMAT = "ERR_VALID_302";
        public const string VALIDATION_OUT_OF_RANGE = "ERR_VALID_303";
        public const string VALIDATION_DUPLICATE = "ERR_VALID_304";

        // ========================================
        // RESOURCE ERRORS (400-499)
        // ========================================
        public const string RESOURCE_NOT_FOUND = "ERR_RES_400";
        public const string RESOURCE_ALREADY_EXISTS = "ERR_RES_401";
        public const string RESOURCE_CONFLICT = "ERR_RES_402";
        public const string RESOURCE_LOCKED = "ERR_RES_403";
        public const string RESOURCE_DELETED = "ERR_RES_404";

        // ========================================
        // BUSINESS LOGIC ERRORS (500-599)
        // ========================================
        public const string BUSINESS_INSUFFICIENT_STOCK = "ERR_BIZ_500";
        public const string BUSINESS_ORDER_INVALID_STATUS = "ERR_BIZ_501";
        public const string BUSINESS_PAYMENT_FAILED = "ERR_BIZ_502";
        public const string BUSINESS_INVOICE_ALREADY_PAID = "ERR_BIZ_503";
        public const string BUSINESS_COUPON_EXPIRED = "ERR_BIZ_504";
        public const string BUSINESS_COUPON_LIMIT_REACHED = "ERR_BIZ_505";

        // ========================================
        // AI ERRORS (600-699)
        // ========================================
        public const string AI_SERVICE_UNAVAILABLE = "ERR_AI_600";
        public const string AI_RATE_LIMITED = "ERR_AI_601";
        public const string AI_INVALID_RESPONSE = "ERR_AI_602";
        public const string AI_DOCUMENT_TOO_LARGE = "ERR_AI_603";
        public const string AI_UNSUPPORTED_FORMAT = "ERR_AI_604";

        // ========================================
        // INTEGRATION ERRORS (700-799)
        // ========================================
        public const string INTEGRATION_STRIPE_ERROR = "ERR_INT_700";
        public const string INTEGRATION_EMAIL_FAILED = "ERR_INT_701";
        public const string INTEGRATION_REDIS_ERROR = "ERR_INT_702";
        public const string INTEGRATION_EXTERNAL_API_ERROR = "ERR_INT_703";
    }
}
