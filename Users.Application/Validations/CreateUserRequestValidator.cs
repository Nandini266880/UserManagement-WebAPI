using FluentValidation;
using Users.Application.Models;

namespace Users.Application.Validations
{
    public class CreateUserRequestValidator : BaseRequestValidator<CreateUserRequestModel>
    {
        public CreateUserRequestValidator()
        {
            AddFullNameRules(x => x.FullName);
            AddEmailRules(x => x.Email);
            AddPasswordRules(x => x.Password);
        }
    }
}
