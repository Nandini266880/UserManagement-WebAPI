using FluentValidation;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Users.Application.Validations
{
    public class BaseRequestValidator<T> : AbstractValidator<T> where T: class
    {
        protected internal int data = 23;
        /// <summary>
        /// Full name rules
        /// </summary>
        /// <param name="expression"></param>
        protected void AddFullNameRules(Expression<Func<T, string>> expression)
        {
            RuleFor(expression)
                .Cascade(CascadeMode.Stop)
                .Matches(RegexPatterns.FullName).WithMessage("Special Characters and digits are not allowed.")
                .Length(2, 100).WithMessage("Name must be 2-100 characters.");
        }

        /// <summary>
        /// Email rules with NotEmpty — when email is required.
        /// </summary>
        /// <param name="expression"></param>
        protected void AddEmailRules(Expression<Func<T, string>> expression)
        {
            RuleFor(expression)
                .Cascade(CascadeMode.Stop)
                .Matches(RegexPatterns.Email).WithMessage("Invalid email format.")
                .MaximumLength(200).WithMessage("Email must be at most 200 characters.");
        }

        /// <summary>
        /// Password rules
        /// </summary>
        /// <param name="expression"></param>
        protected void AddPasswordRules(Expression<Func<T, string>> expression)
        {
            RuleFor(expression)
                .Cascade(CascadeMode.Stop)
                .Matches(RegexPatterns.Password).WithMessage("Password must not contain whitespaces.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }

    public static class RegexPatterns
    {
        // Email — standard format check
        public static readonly Regex Email = new(
            @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Full name — letters (any language), spaces, apostrophe, hyphen, dot
        public static readonly Regex FullName = new(
             @"^[\p{L}\s'\.]+$",   
                 RegexOptions.Compiled);

        // Password - no whitespaces 
        public static readonly Regex Password = new(@"^\S+$", RegexOptions.Compiled);
    }
}
