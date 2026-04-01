using Microsoft.AspNetCore.Identity;

namespace AutoSchoolProject.Services
{
    public class BulgarianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
            => new() { Code = nameof(DefaultError), Description = "Възникна неизвестна грешка." };

        public override IdentityError ConcurrencyFailure()
            => new() { Code = nameof(ConcurrencyFailure), Description = "Данните бяха променени от друг процес. Опитай отново." };

        public override IdentityError PasswordMismatch()
            => new() { Code = nameof(PasswordMismatch), Description = "Невалидна парола." };

        public override IdentityError InvalidToken()
            => new() { Code = nameof(InvalidToken), Description = "Невалиден токен." };

        public override IdentityError LoginAlreadyAssociated()
            => new() { Code = nameof(LoginAlreadyAssociated), Description = "Този вход вече е свързан с друг профил." };

        public override IdentityError InvalidUserName(string userName)
            => new() { Code = nameof(InvalidUserName), Description = $"Потребителското име '{userName}' е невалидно." };

        public override IdentityError InvalidEmail(string email)
            => new() { Code = nameof(InvalidEmail), Description = $"Имейлът '{email}' е невалиден." };

        public override IdentityError DuplicateUserName(string userName)
            => new() { Code = nameof(DuplicateUserName), Description = $"Потребителското име '{userName}' вече съществува." };

        public override IdentityError DuplicateEmail(string email)
            => new() { Code = nameof(DuplicateEmail), Description = $"Имейлът '{email}' вече се използва." };

        public override IdentityError InvalidRoleName(string role)
            => new() { Code = nameof(InvalidRoleName), Description = $"Ролята '{role}' е невалидна." };

        public override IdentityError DuplicateRoleName(string role)
            => new() { Code = nameof(DuplicateRoleName), Description = $"Ролята '{role}' вече съществува." };

        public override IdentityError UserAlreadyHasPassword()
            => new() { Code = nameof(UserAlreadyHasPassword), Description = "Потребителят вече има зададена парола." };

        public override IdentityError UserLockoutNotEnabled()
            => new() { Code = nameof(UserLockoutNotEnabled), Description = "Заключването на този потребител не е включено." };

        public override IdentityError UserAlreadyInRole(string role)
            => new() { Code = nameof(UserAlreadyInRole), Description = $"Потребителят вече има ролята '{role}'." };

        public override IdentityError UserNotInRole(string role)
            => new() { Code = nameof(UserNotInRole), Description = $"Потребителят няма ролята '{role}'." };

        public override IdentityError PasswordTooShort(int length)
            => new() { Code = nameof(PasswordTooShort), Description = $"Паролата трябва да е поне {length} символа." };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Паролата трябва да съдържа поне един специален символ." };

        public override IdentityError PasswordRequiresDigit()
            => new() { Code = nameof(PasswordRequiresDigit), Description = "Паролата трябва да съдържа поне една цифра." };

        public override IdentityError PasswordRequiresLower()
            => new() { Code = nameof(PasswordRequiresLower), Description = "Паролата трябва да съдържа поне една малка буква." };

        public override IdentityError PasswordRequiresUpper()
            => new() { Code = nameof(PasswordRequiresUpper), Description = "Паролата трябва да съдържа поне една главна буква." };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Паролата трябва да съдържа поне {uniqueChars} различни символа." };

        public override IdentityError RecoveryCodeRedemptionFailed()
            => new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = "Кодът за възстановяване е невалиден." };
    }
}
