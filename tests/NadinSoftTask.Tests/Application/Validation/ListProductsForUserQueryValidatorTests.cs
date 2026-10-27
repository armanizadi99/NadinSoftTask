using FluentValidation.TestHelper;
using NadinSoftTask.Application.Queries.ListProductsForUser;
using NadinSoftTask.Tests.Application.TestData;
using Xunit;

namespace NadinSoftTask.Tests.Application.Validation;

public class ListProductsForUserQueryValidatorTests
{
    private readonly ListProductsForUserQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidUserId_HasNoErrors()
    {
        TestValidationResult<ListProductsForUserQuery> result =
            _validator.TestValidate(new ListProductsForUserQuery(ProductTestData.OwnerId));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingUserId_HasUserIdError(string? userId)
    {
        TestValidationResult<ListProductsForUserQuery> result =
            _validator.TestValidate(new ListProductsForUserQuery(userId!));

        result.ShouldHaveValidationErrorFor(query => query.UserId)
            .WithErrorMessage("User id cannot be empty or whitespace.");
    }
}
