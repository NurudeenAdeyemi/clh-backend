using MediatR;
using Microsoft.AspNetCore.Identity;
using CLHCRM.Domain.Entities;
using CLHCRM.Application.Interfaces;

namespace CLHCRM.Application.Features.Auth.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "User with this email already exists." }
                };
            }

            var newUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email // Using email as username for simplicity
            };

            var isCreated = await _userManager.CreateAsync(newUser, request.Password);

            if (!isCreated.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = isCreated.Errors.Select(x => x.Description).ToList()
                };
            }

            // Generate JWT token
            var roles = await _userManager.GetRolesAsync(newUser);
            var token = _tokenService.GenerateAccessToken(newUser, roles);

            return new AuthResult
            {
                Success = true,
                Token = token
            };
        }
    }
}
