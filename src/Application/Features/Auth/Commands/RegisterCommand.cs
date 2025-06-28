using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Application.Common.Exceptions;

namespace Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequest Request) : IRequest<BaseResponse<AuthResponse>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, BaseResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<BaseResponse<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Request.Email);
        if (existingUser != null)
        {
            throw new ValidationException(new[] { "Email already exists" });
        }

        var user = new User
        {
            Email = request.Request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Request.Password),
            FirstName = request.Request.FirstName,
            LastName = request.Request.LastName,
            PhoneNumber = request.Request.PhoneNumber
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

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

        return BaseResponse<AuthResponse>.SuccessResult(response, "Registration successful");
    }
}
