using System.Transactions;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Entities;
using TemplateExpress.Api.EnumResponseTypes;
using TemplateExpress.Api.EnumTypes;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Interfaces.Utils;
using TemplateExpress.Api.Results;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace TemplateExpress.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBCryptUtil _bCryptUtil;
    private readonly ITokenManager _tokenManager;
    public UserService(
        IUserRepository userRepository,
        IBCryptUtil bCryptUtil,
        ITokenManager tokenManager
        )
    {
        _userRepository = userRepository;
        _bCryptUtil = bCryptUtil;
        _tokenManager = tokenManager;
    }   

    public async Task<Result<JwtTokenDto>> CreateUserAndTokenAsync(CreateUserDto createUserDto, IValidator<CreateUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(createUserDto);
            
        if (!validationResult.IsValid)
        {
            // → Abstrair ↓
            var errors = validationResult.Errors
                .Select(failure => new ErrorMessage(failure.ErrorMessage, "Fix the " + failure.PropertyName.ToLower() + " field."))
                .ToList<IErrorMessage>();
            
            return Result<JwtTokenDto>.Failure(
                new Error(
                    (byte)ErrorCodes.InvalidInput,
                    (byte)ErrorTypes.InputValidationError,
                    errors));
        }

        var email = new UserEmailDto(createUserDto.Email);
        
        var thereIsEmail = await _userRepository.FindAnEmailAsync(email);
        if (thereIsEmail)
        {
            List<IErrorMessage> errorMessages = [new ErrorMessage("This email is already in use.", "Try another email.")];
            Error error = new((byte)ErrorCodes.EmailAlreadyExists, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages);
            return Result<JwtTokenDto>.Failure(error);
        }
        
        var createTime = DateTime.Now;
        
        var userEntity = new UserEntity
        {
            Email = createUserDto.Email,
            Username = createUserDto.Username,
            Password = _bCryptUtil.HashPassword(createUserDto.Password),
            Role = UserRoles.User,
            CreatedAt = createTime,
            UpdatedAt = createTime,
        };
            
        await using var transaction = await _userRepository.BeginTransactionAsync();
        try
        {

            var createdUser = _userRepository.InsertUser(userEntity);
            await _userRepository.SaveChangesAsync();

            var userIdAndEmailDto = new UserIdAndEmailDto(createdUser.Id, createdUser.Email); 
            var token = _tokenManager.GenerateAccountConfirmationToken(userIdAndEmailDto);

            await transaction.CommitAsync();

            return Result<JwtTokenDto>.Success(new JwtTokenDto(token));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new TransactionException("An error occured while trying to create the user.", ex);
        }

    }

    public async Task<Result<JwtTokenDto>> ConfirmAccountAsync(JwtTokenDto jwtTokenDto)
    {
        var tokenValidation = await _tokenManager.ValidateAccountConfirmationTokenAsync(jwtTokenDto);
        if (!tokenValidation.IsSuccess)
        {
            return Result<JwtTokenDto>.Failure(tokenValidation.Error!);
        }
        var userIdAndEmail = _tokenManager.GetJwtConfirmationAccountTokenClaims(tokenValidation.Value!);

        var user = await _userRepository.ChangeConfirmedAccountColumnToTrue(userIdAndEmail.Id);
        
        if (user == null) throw new Exception("An error occured while trying to confirm the user.");

        var userIdAndRoleDto = new UserIdAndRoleDto(user.Id, user.Role);
        var authToken = _tokenManager.GenerateAuthenticationToken(userIdAndRoleDto);
        var authTokenDto = new JwtTokenDto(authToken);
        return Result<JwtTokenDto>.Success(authTokenDto);
        
    }

    public async Task<Result<JwtTokenDto>> GenerateConfirmationAccountTokenAsync(EmailAndPasswordDto emailAndPasswordDto, IValidator<EmailAndPasswordDto> validator)
    {
        var validationResult = await validator.ValidateAsync(emailAndPasswordDto);
        
        if (!validationResult.IsValid)
        {
            // → Abstrair ↓
            var errors = validationResult.Errors
                .Select(failure => new ErrorMessage(failure.ErrorMessage, "Fix the " + failure.PropertyName.ToLower() + " field."))
                .ToList<IErrorMessage>();
            
            return Result<JwtTokenDto>.Failure(
                new Error(
                    (byte)ErrorCodes.InvalidInput,
                    (byte)ErrorTypes.InputValidationError,
                    errors));
        }
        
        var email = new UserEmailDto(emailAndPasswordDto.Email);
        var user = await _userRepository.FindEmailAsync(email);

        List<IErrorMessage> errorMessages = [new ErrorMessage("Invalid Email or Password.", "Post valid credentials.")];
        Error error = new((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages);
        
        if (user == null) return Result<JwtTokenDto>.Failure(error);

        var comparedPassword = _bCryptUtil.ComparePassword(emailAndPasswordDto.Password, user.Password);

        if (comparedPassword == false) return Result<JwtTokenDto>.Failure(error);
        
        var userIdAndEmailDto = new UserIdAndEmailDto(user.Id, user.Email);
        var token = _tokenManager.GenerateAccountConfirmationToken(userIdAndEmailDto);
        var jwtConfirmationAccountTokenDto = new JwtTokenDto(token);
        return Result<JwtTokenDto>.Success(jwtConfirmationAccountTokenDto);

    }

    public async Task<Result<JwtTokenDto>> LoginAsync(EmailAndPasswordDto emailAndPasswordDto, IValidator<EmailAndPasswordDto> validator)
    {

        var validationResult = await validator.ValidateAsync(emailAndPasswordDto);
        
        if (!validationResult.IsValid)
        {
            // → Abstrair ↓
            var errors = validationResult.Errors
                .Select(failure => new ErrorMessage(failure.ErrorMessage, "Fix the " + failure.PropertyName.ToLower() + " field."))
                .ToList<IErrorMessage>();
            
            return Result<JwtTokenDto>.Failure(
                new Error(
                    (byte)ErrorCodes.InvalidInput,
                    (byte)ErrorTypes.InputValidationError,
                    errors));
        }
        
        var user = await _userRepository.FindEmailAsync(new UserEmailDto(emailAndPasswordDto.Email));

        List<IErrorMessage> errorMessages = [new ErrorMessage("Invalid Email or Password.", "Post valid credentials.")];
        Error error = new((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages);
            
        if (user == null)
        {
            return Result<JwtTokenDto>.Failure(error);
        }
        
        var comparedPassword = _bCryptUtil.ComparePassword(emailAndPasswordDto.Password, user.Password);
        
        if (comparedPassword == false)
        {
            return Result<JwtTokenDto>.Failure(error);
        }

        var userIdAndRoleDto = new UserIdAndRoleDto(user.Id, user.Role);

        var token = _tokenManager.GenerateAuthenticationToken(userIdAndRoleDto);
        
        return Result<JwtTokenDto>.Success(new JwtTokenDto(token));

    }
}