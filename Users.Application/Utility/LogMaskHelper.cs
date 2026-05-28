using Users.Application.Services;

namespace Users.Application.Utility
{
    public static class LogMaskHelper
    {

        public static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "***";

            var emailSplit = email.Split('@');
            if (emailSplit.Length < 2)
                return "***";

            var localPart = emailSplit[0];
            var domain = emailSplit[1];

            var maskedEmail = $"{localPart[0]}***@{domain}";

            return maskedEmail;
        }
    }
}
