using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Domain.Common;
using Domain.Interfaces;
using MediatR;
using Application.Common.Exceptions;

namespace Application.Features.Auth.Commands;

public record LoginCommand(LoginRequest Request) : IRequest<BaseResponse<AuthResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, BaseResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<BaseResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Request.Email);
        
        if (user == null || !_passwordHasher.VerifyPassword(request.Request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        var response = new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString()
            }
        };

        return BaseResponse<AuthResponse>.SuccessResult(response, "Login successful");
    }
}
