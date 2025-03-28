using FluentValidation;
using TemplateExpress.Api.Dto.UserDtos;
using TemplateExpress.Api.Entities;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Utils;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace TemplateExpress.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserEmailDto>> CreateUserAsync(IValidator<CreateUserDto> userValidator, CreateUserDto createUserDto)
    {

        ValidationResult validationResult = await userValidator.ValidateAsync(createUserDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(failure => new ErrorMessage(failure.ErrorMessage, "Fix the " + failure.PropertyName.ToLower() + " field."))
                .ToList<IErrorMessage>();
            return Result<UserEmailDto>.Failure(new Error("InvalidInput","InputValidationError", errors));
        }

        var thereIsAnEmail = await _userRepository.FindAnEmailAsync(createUserDto.Email);
        if (thereIsAnEmail)
        {
            return Result<UserEmailDto>.Success(new UserEmailDto(createUserDto.Email));
        }
        
        var thereIsAnUsername = await _userRepository.FindAnUsernameAsync(createUserDto.Username);
        if (thereIsAnUsername)
        {
            List<IErrorMessage> errors = [new ErrorMessage("This username already exists.", "Try another username.")];
            return Result<UserEmailDto>.Failure(new Error("UsernameAlreadyExists","BusinessLogicValidationError", errors));
        }
        
        var userEntity = new UserEntity
        {
            Email = createUserDto.Email,
            Username = createUserDto.Username,
            Password = BCryptUtil.HashPassword(createUserDto.Password, 12),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        
        var createdUser = await _userRepository.InsertUserAsync(userEntity);
        
        return Result<UserEmailDto>.Success(new UserEmailDto(createdUser.Email));

    }
}