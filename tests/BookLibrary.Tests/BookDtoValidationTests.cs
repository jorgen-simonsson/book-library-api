using BookLibrary.Application.Validation;
using FluentAssertions;
using Xunit;

namespace BookLibrary.Tests;

public class BookDtoValidationTests
{
    private readonly ValidPublishedYearAttribute _attr = new();

    // --- Valid values ---

    [Fact]
    public void Null_IsValid()
    {
        _attr.IsValid(null).Should().BeTrue();
    }

    [Theory]
    [InlineData("2024")]
    [InlineData("1900")]
    [InlineData("0000")]
    [InlineData("9999")]
    public void FourDigitString_IsValid(string year)
    {
        _attr.IsValid(year).Should().BeTrue();
    }

    // --- Invalid values ---

    [Theory]
    [InlineData("")]          // empty string
    [InlineData("abc")]       // non-digits
    [InlineData("123")]       // too short
    [InlineData("12345")]     // too long
    [InlineData("20 4")]      // contains space
    [InlineData("20a4")]      // mixed digits and letters
    [InlineData("-024")]      // negative sign
    public void InvalidFormat_IsNotValid(string year)
    {
        _attr.IsValid(year).Should().BeFalse();
    }

    [Fact]
    public void ErrorMessage_MentionsExpectedFormat()
    {
        var result = _attr.GetValidationResult("bad", new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Contain("4-digit");
    }
}
