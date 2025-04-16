using System.Security;
using FluentValidation;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Entities;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Interfaces.Utils;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace TemplateExpress.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateUserDto> _validator;
    private readonly IBCryptUtil _bCryptUtil;
    private readonly ITokenManager _tokenManager;
    public UserService(
        IUserRepository userRepository,
        IValidator<CreateUserDto> validator,
        IBCryptUtil bCryptUtil,
        ITokenManager tokenManager
        )
    {
        _userRepository = userRepository;
        _validator = validator;
        _bCryptUtil = bCryptUtil;
        _tokenManager = tokenManager;
    }   

    public async Task<Result<UserEmailDto>> CreateUserAsync(CreateUserDto createUserDto)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(createUserDto);
            
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
        
        var userEntity = new UserEntity
        {
            Email = createUserDto.Email,
            Username = createUserDto.Username,
            Password = _bCryptUtil.HashPassword(createUserDto.Password),
            CreatedAt = createTime,
            UpdatedAt = createTime,
        };
            
        await using var transaction = await _userRepository.BeginTransactionAsync();
        try
        {

            var createdUser = _userRepository.InsertUser(userEntity);
            await _userRepository.SaveChangesAsync();


            var userIdAndEmailDto = new UserIdAndEmailDto(createdUser.Id, createdUser.Email); 
            var token = _tokenManager.GenerateEmailConfirmationToken(userIdAndEmailDto);

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
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new SecurityException("An error occured while trying to create the user.", ex);
        }

    }
}