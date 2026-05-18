using FluentValidation;
using Users.Application.Models;

namespace Users.Application.Validations
{
    public class UpdateUserRequestValidator : BaseRequestValidator<UpdateUserRequestModel>
    {
        public UpdateUserRequestValidator()
        {
            AddFullNameRules(x => x.FullName);
            
            When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
            {
                AddEmailRules(x => x.Email!);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Password), () =>
            {
                AddPasswordRules(x => x.Password!);
            });
        }
    }
}
