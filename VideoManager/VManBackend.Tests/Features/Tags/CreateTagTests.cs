using FluentAssertions;
using VManBackend.Features.Tags;
using VManBackend.Tests.TestHelpers;
using Xunit;

namespace VManBackend.Tests.Features.Tags;

public class CreateTagTests
{
    [Fact]
    public async Task Handle_CreatesTag_WhenNameIsUnique()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var handler = new CreateTag.Handler(db);

        var response = await handler.Handle(new CreateTag.Request("Vacation"), CancellationToken.None);

        response.Name.Should().Be("Vacation");
        db.Tags.Should().ContainSingle(t => t.Name == "Vacation");
    }

    [Fact]
    public async Task Handle_TrimsWhitespace_FromName()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var handler = new CreateTag.Handler(db);

        var response = await handler.Handle(new CreateTag.Request("  Vacation  "), CancellationToken.None);

        response.Name.Should().Be("Vacation");
    }

    [Fact]
    public async Task Handle_Throws_WhenTagNameAlreadyExists_CaseInsensitive()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var handler = new CreateTag.Handler(db);
        await handler.Handle(new CreateTag.Request("Vacation"), CancellationToken.None);

        var act = () => handler.Handle(new CreateTag.Request("vacation"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_Rejects_EmptyOrWhitespaceName(string name)
    {
        var isValid = CreateTag.Validator.Validate(new CreateTag.Request(name), out var error);

        isValid.Should().BeFalse();
        error.Should().Be("Tag name is required");
    }

    [Fact]
    public void Validator_Rejects_NameLongerThan100Characters()
    {
        var name = new string('a', 101);

        var isValid = CreateTag.Validator.Validate(new CreateTag.Request(name), out var error);

        isValid.Should().BeFalse();
        error.Should().Be("Tag name must be 100 characters or less");
    }

    [Fact]
    public void Validator_Accepts_ValidName()
    {
        var isValid = CreateTag.Validator.Validate(new CreateTag.Request("Vacation"), out var error);

        isValid.Should().BeTrue();
        error.Should().BeNull();
    }
}
