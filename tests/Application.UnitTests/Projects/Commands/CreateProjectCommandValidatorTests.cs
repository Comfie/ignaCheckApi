using FluentValidation.TestHelper;
using IgnaCheck.Application.Projects.Commands.CreateProject;

namespace IgnaCheck.Application.UnitTests.Projects.Commands;

[TestFixture]
public class CreateProjectCommandValidatorTests
{
    private CreateProjectCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateProjectCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = new CreateProjectCommand { Name = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        // Arrange
        var command = new CreateProjectCommand { Name = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Too_Short()
    {
        // Arrange
        var command = new CreateProjectCommand { Name = "AB" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        // Arrange
        var command = new CreateProjectCommand { Name = new string('A', 101) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Test]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        // Arrange
        var command = new CreateProjectCommand { Name = "Valid Project Name" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }

    [Test]
    public void Should_Have_Error_When_Description_Is_Too_Long()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Valid Name",
            Description = new string('A', 501)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Test]
    public void Should_Not_Have_Error_When_Description_Is_Null()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Valid Name",
            Description = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Description);
    }

    [Test]
    public void Should_Not_Have_Error_When_Description_Is_Valid()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Valid Name",
            Description = "This is a valid description"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Description);
    }

    [Test]
    public void Should_Have_Error_When_TargetDate_Is_In_Past()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Valid Name",
            TargetDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TargetDate);
    }

    [Test]
    public void Should_Not_Have_Error_When_TargetDate_Is_In_Future()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Valid Name",
            TargetDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.TargetDate);
    }

    [Test]
    public void Should_Not_Have_Error_When_TargetDate_Is_Null()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Valid Name",
            TargetDate = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.TargetDate);
    }

    [Test]
    [TestCase("A")]
    [TestCase("AB")]
    [TestCase("ProjectNameThatIsWayTooLongAndExceedsTheMaximumAllowedLengthOf100CharactersWhichShouldCauseValidationToFail")]
    public void Should_Have_Error_When_Name_Length_Invalid(string name)
    {
        // Arrange
        var command = new CreateProjectCommand { Name = name };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Test]
    [TestCase("ABC")]
    [TestCase("Valid Project")]
    [TestCase("SOC 2 Type II Compliance Audit 2024")]
    [TestCase("A valid project name with exactly 100 characters including spaces and numbers like 123456789012")]
    public void Should_Not_Have_Error_When_Name_Length_Valid(string name)
    {
        // Arrange
        var command = new CreateProjectCommand { Name = name };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }
}
