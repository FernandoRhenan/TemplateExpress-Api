using System.Transactions;
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

    public async Task<Result<JwtConfirmationAccountTokenDto>> CreateUserAndTokenAsync(CreateUserDto createUserDto, IValidator<CreateUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(createUserDto);
            
        if (!validationResult.IsValid)
        {
            // → Abstrair ↓
            var errors = validationResult.Errors
                .Select(failure => new ErrorMessage(failure.ErrorMessage, "Fix the " + failure.PropertyName.ToLower() + " field."))
                .ToList<IErrorMessage>();
            
            return Result<JwtConfirmationAccountTokenDto>.Failure(
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
            return Result<JwtConfirmationAccountTokenDto>.Failure(error);
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

            await transaction.CommitAsync();

            return Result<JwtConfirmationAccountTokenDto>.Success(new JwtConfirmationAccountTokenDto(token));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new TransactionException("An error occured while trying to create the user.", ex);
        }

    }

    // TODO: It wasn't unit tested.
    public async Task<Result<string>> ConfirmAccountAsync(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto)
    {
        var tokenValidation = await _tokenManager.TokenValidation(jwtConfirmationAccountTokenDto);

        if (!tokenValidation.IsSuccess)
        {
            return Result<string>.Failure(tokenValidation.Error!);
        }

        var userIdAndEmail = _tokenManager.GetJwtConfirmationAccountTokenClaims(tokenValidation.Value!);

        var isNowConfirmedUser = await _userRepository.ChangeConfirmedAccountColumnToTrue(userIdAndEmail.Id);
        
        if (isNowConfirmedUser) return Result<string>.Success(String.Empty);
        
        throw new Exception("An error occured while trying to confirm the user.");
        
    }

    // TODO: It wasn't unit tested.
    public async Task<Result<JwtConfirmationAccountTokenDto>> GenerateConfirmationAccountTokenAsync(EmailAndPasswordDto emailAndPasswordDto, IValidator<EmailAndPasswordDto> validator)
    {
        var validationResult = await validator.ValidateAsync(emailAndPasswordDto);
        
        if (!validationResult.IsValid)
        {
            // → Abstrair ↓
            var errors = validationResult.Errors
                .Select(failure => new ErrorMessage(failure.ErrorMessage, "Fix the " + failure.PropertyName.ToLower() + " field."))
                .ToList<IErrorMessage>();
            
            return Result<JwtConfirmationAccountTokenDto>.Failure(
                new Error(
                    (byte)ErrorCodes.InvalidInput,
                    (byte)ErrorTypes.InputValidationError,
                    errors));
        }
        
        var email = new UserEmailDto(emailAndPasswordDto.Email);
        var user = await _userRepository.FindEmailAsync(email);

        List<IErrorMessage> errorMessages = [new ErrorMessage("Invalid Email or Password.", "Post valid credentials.")];
        Error error = new((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages);
        
        if (user == null) return Result<JwtConfirmationAccountTokenDto>.Failure(error);

        var comparedPassword = _bCryptUtil.ComparePassword(emailAndPasswordDto.Password, user.Password);

        if (comparedPassword == false) return Result<JwtConfirmationAccountTokenDto>.Failure(error);
        
        var userIdAndEmailDto = new UserIdAndEmailDto(user.Id, user.Email);
        var token = _tokenManager.GenerateEmailConfirmationToken(userIdAndEmailDto);
        var jwtConfirmationAccountTokenDto = new JwtConfirmationAccountTokenDto(token);
        return Result<JwtConfirmationAccountTokenDto>.Success(jwtConfirmationAccountTokenDto);

    }
    
}