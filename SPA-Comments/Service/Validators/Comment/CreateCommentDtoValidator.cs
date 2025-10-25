using Common.Helper;
using Dto.Comment;
using FluentValidation;
using Ganss.Xss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Validators.Comment;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("UserName must contain only Latin letters and digits.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.HomePage)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Invalid URL format.");

        RuleFor(x => x.CAPTCHA)
            .NotEmpty().WithMessage("CAPTCHA is required.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("CAPTCHA must contain only Latin letters and digits.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .Must(SanitizeCommentHtml).WithMessage("HTML tags are not allowed in Text.");

        RuleForEach(x => x.Files)
                .Custom((file, context) =>
                {
                    if (file != null)
                    {
                        if (!FileHelper.IsValidFile(file, out string error))
                        {
                            context.AddFailure($"File '{file.FileName}' is invalid: {error}");
                        }
                    }
                });
    }

    public bool SanitizeCommentHtml(string inputHtml)
    {
        var sanitizer = new HtmlSanitizer();

        // Очищаем whitelist и добавляем разрешённые теги
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.Add("a");
        sanitizer.AllowedTags.Add("code");
        sanitizer.AllowedTags.Add("i");
        sanitizer.AllowedTags.Add("strong");

        // Разрешаем только атрибуты href и title для тега <a>
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedAttributes.Add("title");

        // Очищаем все остальные атрибуты для других тегов
        sanitizer.AllowedCssProperties.Clear();

        // Можно дополнительно настроить разрешённые схемы для ссылок (http, https, mailto)
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");

        try
        {
            var cleanHtml = sanitizer.Sanitize(inputHtml);

            return !string.IsNullOrWhiteSpace(cleanHtml);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
