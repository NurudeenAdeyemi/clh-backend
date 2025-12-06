using MediatR;
using System.ComponentModel.DataAnnotations;

namespace CLHCRM.Application.Features.Auth.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<AuthResult>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
