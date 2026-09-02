using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using FluentValidation.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtService jwtService, 
            IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            // Check wheathere the user already exist
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if(existingUser != null)
            {
                throw new InvalidOperationException(
                    "A user with this email already exist.");
            }

            // Hash the password
            var passwordHash = _passwordHasher.Hash(request.Password);

            // Create the User
            var user = new User(
                request.FirstName.Trim(),
                request.LastName.Trim(),
                email,
                passwordHash);

            // Save user to database
            await _userRepository.AddAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshTokenValue = _jwtService.GenerateRefreshToken();
            var refreshTokenDays = double.Parse(
                _configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

            var refreshToken = new RefreshToken(
                user.Id,
                refreshTokenValue,
                DateTime.UtcNow.AddDays(refreshTokenDays));
            await _refreshTokenRepository.AddAsync(refreshToken);

            // Return Response
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(email);

            if(user is null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            if(!user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "User account is inactive.");
            }

            var passwordValid = _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

            if (!passwordValid)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            var accessToken = _jwtService.GenerateAccessToken(user);

            var refreshTokenValue = _jwtService.GenerateRefreshToken();

            var refreshTokenDays = double.Parse(
                _configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

            var refreshToken = new RefreshToken(
                user.Id,
                refreshTokenValue,
                DateTime.UtcNow.AddDays(refreshTokenDays));

            await _refreshTokenRepository.AddAsync(refreshToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                UserId = user.Id,
                Email = user.Email,
                Role= user.Role
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(
            RefreshTokenRequest request)
        {
            var refreshToken = await _refreshTokenRepository
                .GetByTokenAsync(request.RefreshToken);

            if (refreshToken is null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token.");
            }

            if (!refreshToken.IsActive())
            {
                throw new UnauthorizedAccessException(
                    "Refresh token is expired or revoked.");
            }

            var user = refreshToken.User;

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "User account is inactive.");
            }

            // Revoke old refresh token
            refreshToken.Revoke();

            await _refreshTokenRepository.UpdateAsync(refreshToken);

            // Generate new tokens
            var accessToken = _jwtService.GenerateAccessToken(user);

            var newRefreshTokenValue =
                _jwtService.GenerateRefreshToken();

            var refreshTokenDays = double.Parse(
                _configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

            var newRefreshToken = new RefreshToken(
                user.Id,
                newRefreshTokenValue,
                DateTime.UtcNow.AddDays(refreshTokenDays));

            await _refreshTokenRepository.AddAsync(newRefreshToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshTokenValue,
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task LogoutAsync(LogoutRequest request)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if(refreshToken == null)
            {
                return;
            }

            if(!refreshToken.IsRevoked())
            {
                refreshToken.Revoke();

                await _refreshTokenRepository.UpdateAsync(refreshToken);
            }
        }
    }
}
