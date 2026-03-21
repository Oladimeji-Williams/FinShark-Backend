using FluentAssertions;
using FinShark.Application.Comments.Queries.GetAllComments;
using FinShark.Application.Comments.Queries.GetCommentsByStockId;
using FinShark.Application.Comments.Validators;
using FinShark.Domain.Queries;
using Xunit;

namespace FinShark.Tests.Unit.Validators;

public class GetAllCommentsQueryValidatorTests
{
    private readonly GetAllCommentsQueryValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_WithValidParameters_ShouldPass()
    {
        var query = new GetAllCommentsQuery(PageNumber: 1, PageSize: 20);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidPageSize_ShouldFail()
    {
        var query = new GetAllCommentsQuery(PageNumber: 1, PageSize: 0);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidSortDirection_ShouldFail()
    {
        var query = new GetAllCommentsQuery(PageNumber: 1, PageSize: 20, SortBy: CommentSortBy.Created, SortDirection: (SortDirection)99);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
    }
}

public class GetCommentsByStockIdQueryValidatorTests
{
    private readonly GetCommentsByStockIdQueryValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_WithValidParameters_ShouldPass()
    {
        var query = new GetCommentsByStockIdQuery(StockId: 1, PageNumber: 1, PageSize: 20);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidStockId_ShouldFail()
    {
        var query = new GetCommentsByStockIdQuery(StockId: 0, PageNumber: 1, PageSize: 20);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidSortBy_ShouldFail()
    {
        var query = new GetCommentsByStockIdQuery(StockId: 1, PageNumber: 1, PageSize: 20, SortBy: (CommentSortBy)99, SortDirection: SortDirection.Desc);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidRatings_ShouldFail()
    {
        var query = new GetCommentsByStockIdQuery(StockId: 1, MinRating: 5, MaxRating: 2);
        var result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
    }
}
