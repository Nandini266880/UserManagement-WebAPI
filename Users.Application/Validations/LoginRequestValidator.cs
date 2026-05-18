using FluentValidation;
using Users.Application.Models;

namespace Users.Application.Validations
{
    public class LoginRequestValidator : BaseRequestValidator<LoginRequestModel>
    {
        public LoginRequestValidator()
        {
            AddEmailRules(x => x.Email);
            AddPasswordRules(x => x.Password);
        }
    }
}
