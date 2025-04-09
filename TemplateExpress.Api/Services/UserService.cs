using System.Security;
using FluentValidation;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Entities;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;
using TemplateExpress.Api.Utils;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace TemplateExpress.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IServiceProvider _serviceProvider;
    public UserService(IServiceProvider serviceProvider, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _serviceProvider = serviceProvider;
    }   

    public async Task<Result<UserEmailDto>> CreateUserAsync(CreateUserDto createUserDto)
    {

        IValidator<CreateUserDto> validator = _serviceProvider.GetRequiredService<IValidator<CreateUserDto>>();
            ValidationResult validationResult = await validator.ValidateAsync(createUserDto);
            
        if (!validationResult.IsValid)
        {
            // → Abstrair ↓
            var errors = validationResult.Errors
                .Select(failure => new ErrorMessage(failure.ErrorMessage, "Fix the " + failure.PropertyName.ToLower() + " field."))
                .ToList<IErrorMessage>();
            
            return Result<UserEmailDto>.Failure(
                new Error(
                    (byte)ErrorCodes.InvalidInput,
                    (byte)ErrorTypes.InputValidationError,
                    errors));
        }

        var thereIsEmail = await _userRepository.FindAnEmailAsync(createUserDto.Email);
        if (thereIsEmail)
        {
            List<IErrorMessage> errorMessages = [new ErrorMessage("This email is already in use.", "Try another email.")];
            Error error = new((byte)ErrorCodes.EmailAlreadyExists, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages);
            return Result<UserEmailDto>.Failure(error);
        }
        
        var createTime = DateTime.Now;
        
        await using var transaction = await _userRepository.BeginTransactionAsync();
        try
        {
            var userEntity = new UserEntity
            {
                Email = createUserDto.Email,
                Username = createUserDto.Username,
                Password = BCryptUtil.HashPassword(createUserDto.Password, 12),
                CreatedAt = createTime,
                UpdatedAt = createTime,
            };

            var createdUser = _userRepository.InsertUser(userEntity);
            await _userRepository.SaveChangesAsync();

            ITokenManager tokenManager = _serviceProvider.GetRequiredService<ITokenManager>();
            var token = tokenManager.GenerateEmailConfirmationToken(
                new UserIdAndEmailDto(createdUser.Id, createdUser.Email)
            );

            var tokenEntity = new EmailConfirmationTokenEntity
            {
                Token = token,
                UserId = createdUser.Id,
                CreatedAt = createTime,
                UpdatedAt = createTime
            };

            _userRepository.InsertEmailConfirmationToken(tokenEntity);
            await _userRepository.SaveChangesAsync();

            await transaction.CommitAsync();

            return Result<UserEmailDto>.Success(new UserEmailDto(createdUser.Email));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new SecurityException("An error occured while trying to create the user.");
        }

    }
}