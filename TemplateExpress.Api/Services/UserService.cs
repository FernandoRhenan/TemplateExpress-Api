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

    public async Task<Result<UserEmailDto>> CreateUserAsync(IValidator<CreateUserDto> userValidator, CreateUserDto createUserDto)
    {

        ValidationResult validationResult = await userValidator.ValidateAsync(createUserDto);
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
            return Result<UserEmailDto>.Success(new UserEmailDto(createUserDto.Email));
        }
        
        var createTime = DateTime.Now;
        
        var userEntity = new UserEntity
        {
            Email = createUserDto.Email,
            Username = createUserDto.Username,
            Password = BCryptUtil.HashPassword(createUserDto.Password, 12),
            CreatedAt = createTime,
            UpdatedAt = createTime,
        };
        
        var createdUser = await _userRepository.InsertUserAsync(userEntity);

        ITokenManager tokenManager = _serviceProvider.GetRequiredService<ITokenManager>();
        var token = tokenManager.GenerateEmailConfirmationTokenAsync(new UserIdAndEmailDto(createdUser.Id, userEntity.Email));

        var tokenEntity = new EmailConfirmationTokenEntity
        {
            Token = token,
            UserId = createdUser.Id,
            CreatedAt = createTime,
            UpdatedAt = createTime
        };
        
        await _userRepository.InsertEmailConfirmationTokenAsync(tokenEntity);
        
        return Result<UserEmailDto>.Success(new UserEmailDto(createdUser.Email));

    }
}