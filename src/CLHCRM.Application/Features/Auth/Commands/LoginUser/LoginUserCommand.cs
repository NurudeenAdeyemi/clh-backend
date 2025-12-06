using MediatR;
using System.ComponentModel.DataAnnotations;

namespace CLHCRM.Application.Features.Auth.Commands.LoginUser
{
    public class LoginUserCommand : IRequest<AuthResult>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
