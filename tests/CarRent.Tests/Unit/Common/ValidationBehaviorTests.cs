using CarRent.Application.Common.Behaviors;
using CarRent.Application.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Common;

public class ValidationBehaviorTests
{
    public class SamplePayload
    {
        public string Name { get; set; } = string.Empty;
    }

    public class SampleCommand : IRequest<string>
    {
        public SamplePayload Request { get; set; } = new();
    }

    [Fact]
    public async Task Handle_WhenNoValidationFailures_CallsNextDelegate()
    {
        // Arrange
        SampleCommand request = new();
        IServiceProvider serviceProvider = new ServiceCollection().BuildServiceProvider();
        ValidationBehavior<SampleCommand, string> behavior = new(
            Enumerable.Empty<IValidator<SampleCommand>>(),
            serviceProvider);

        RequestHandlerDelegate<string> next = _ => Task.FromResult("success");

        // Act
        string result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WhenCommandValidationFails_ThrowsValidationException()
    {
        // Arrange
        SampleCommand request = new();
        Mock<IValidator<SampleCommand>> commandValidatorMock = new();
        ValidationResult failureResult = new(new[] { new ValidationFailure("Request", "Invalid request") });

        commandValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        IServiceProvider serviceProvider = new ServiceCollection().BuildServiceProvider();
        ValidationBehavior<SampleCommand, string> behavior = new(
            new[] { commandValidatorMock.Object },
            serviceProvider);

        RequestHandlerDelegate<string> next = _ => Task.FromResult("success");

        // Act
        Func<Task> act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Invalid request*");
    }

    [Fact]
    public async Task Handle_WhenPayloadValidationFails_ThrowsValidationException()
    {
        // Arrange
        SampleCommand request = new();
        Mock<IValidator<SamplePayload>> payloadValidatorMock = new();
        ValidationResult failureResult = new(new[] { new ValidationFailure("Name", "Name is required") });

        payloadValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        ServiceCollection services = new();
        services.AddSingleton(payloadValidatorMock.Object);
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        ValidationBehavior<SampleCommand, string> behavior = new(
            Enumerable.Empty<IValidator<SampleCommand>>(),
            serviceProvider);

        RequestHandlerDelegate<string> next = _ => Task.FromResult("success");

        // Act
        Func<Task> act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Name is required*");
    }
}
