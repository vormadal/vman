using FluentAssertions;
using VManBackend.Common.Models;
using VManBackend.Features.Tags;
using VManBackend.Tests.TestHelpers;
using Xunit;

namespace VManBackend.Tests.Features.Tags;

public class GetTagsTests
{
    private static Tag NewTag(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_ReturnsTags_OrderedByName()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Tags.AddRange(NewTag("Zebra"), NewTag("Apple"), NewTag("Mango"));
        await db.SaveChangesAsync();
        var handler = new GetTags.Handler(db);

        var response = await handler.Handle(new GetTags.Request(), CancellationToken.None);

        response.Tags.Select(t => t.Name).Should().ContainInOrder("Apple", "Mango", "Zebra");
        response.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_FiltersBySearch_CaseInsensitive()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Tags.AddRange(NewTag("Vacation"), NewTag("Work"));
        await db.SaveChangesAsync();
        var handler = new GetTags.Handler(db);

        var response = await handler.Handle(new GetTags.Request(Search: "vac"), CancellationToken.None);

        response.Tags.Should().ContainSingle(t => t.Name == "Vacation");
        response.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AppliesPagination()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Tags.AddRange(NewTag("A"), NewTag("B"), NewTag("C"));
        await db.SaveChangesAsync();
        var handler = new GetTags.Handler(db);

        var response = await handler.Handle(new GetTags.Request(Page: 2, PageSize: 2), CancellationToken.None);

        response.Tags.Should().ContainSingle(t => t.Name == "C");
        response.TotalCount.Should().Be(3);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public void Validator_Rejects_InvalidPage(int page, int pageSize)
    {
        var isValid = GetTags.Validator.Validate(new GetTags.Request(Page: page, PageSize: pageSize), out var error);

        isValid.Should().BeFalse();
        error.Should().Be("Page must be greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validator_Rejects_InvalidPageSize(int pageSize)
    {
        var isValid = GetTags.Validator.Validate(new GetTags.Request(PageSize: pageSize), out var error);

        isValid.Should().BeFalse();
        error.Should().Be("PageSize must be between 1 and 100");
    }
}
