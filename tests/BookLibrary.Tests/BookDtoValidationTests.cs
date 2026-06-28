using System.ComponentModel.DataAnnotations;
using BookLibrary.Application.DTOs;
using BookLibrary.Application.Validation;
using FluentAssertions;
using Xunit;

namespace BookLibrary.Tests;

public class BookDtoValidationTests
{
    private static IList<ValidationResult> Validate(object dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
        return results;
    }

    private static CreateBookDto MakeCreate(string? publishedYear) =>
        new("978-1", "Title", "Author", "Publisher", publishedYear, 100, null, null);

    private static UpdateBookDto MakeUpdate(string? publishedYear) =>
        new("978-1", "Title", "Author", "Publisher", publishedYear, 100, null, null);

    // --- Valid values ---

    [Fact]
    public void CreateBookDto_PublishedYear_Null_IsValid()
    {
        Validate(MakeCreate(null)).Should().BeEmpty();
    }

    [Theory]
    [InlineData("2024")]
    [InlineData("1900")]
    [InlineData("0000")]
    [InlineData("9999")]
    public void CreateBookDto_PublishedYear_FourDigits_IsValid(string year)
    {
        Validate(MakeCreate(year)).Should().BeEmpty();
    }

    [Fact]
    public void UpdateBookDto_PublishedYear_Null_IsValid()
    {
        Validate(MakeUpdate(null)).Should().BeEmpty();
    }

    [Theory]
    [InlineData("2024")]
    [InlineData("1900")]
    [InlineData("0000")]
    [InlineData("9999")]
    public void UpdateBookDto_PublishedYear_FourDigits_IsValid(string year)
    {
        Validate(MakeUpdate(year)).Should().BeEmpty();
    }

    // --- Invalid values ---

    [Theory]
    [InlineData("")]          // empty string
    [InlineData("abc")]       // non-digits
    [InlineData("123")]       // too short
    [InlineData("12345")]     // too long
    [InlineData("20 4")]      // contains space
    [InlineData("20a4")]      // mixed
    [InlineData("-024")]      // negative sign
    public void CreateBookDto_PublishedYear_InvalidFormat_FailsValidation(string year)
    {
        var errors = Validate(MakeCreate(year));
        errors.Should().ContainSingle()
            .Which.MemberNames.Should().Contain("PublishedYear");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("20 4")]
    [InlineData("20a4")]
    [InlineData("-024")]
    public void UpdateBookDto_PublishedYear_InvalidFormat_FailsValidation(string year)
    {
        var errors = Validate(MakeUpdate(year));
        errors.Should().ContainSingle()
            .Which.MemberNames.Should().Contain("PublishedYear");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12345")]
    public void CreateBookDto_PublishedYear_InvalidFormat_ErrorMessageMentionsExpectedFormat(string year)
    {
        var errors = Validate(MakeCreate(year));
        errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("4-digit");
    }
}
