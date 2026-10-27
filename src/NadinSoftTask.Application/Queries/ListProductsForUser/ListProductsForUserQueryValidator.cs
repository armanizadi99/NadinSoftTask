using FluentValidation;

namespace NadinSoftTask.Application.Queries.ListProductsForUser;

public sealed class ListProductsForUserQueryValidator : AbstractValidator<ListProductsForUserQuery>
{
    public ListProductsForUserQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("User id cannot be empty or whitespace.");
    }
}
