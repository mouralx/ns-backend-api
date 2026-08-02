using FluentAssertions;
using MassageBooking.Application.DTOs;
using MassageBooking.Application.Services;
using MassageBooking.Domain.Common;
using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Interfaces;
using Moq;
using Xunit;

namespace MassageBooking.Tests.Services;

public class AvailabilityServiceTests
{
    private readonly Mock<IAvailabilityRepository> _availabilityRepoMock;
    private readonly AvailabilityService _sut;

    public AvailabilityServiceTests()
    {
        _availabilityRepoMock = new Mock<IAvailabilityRepository>();
        _sut = new AvailabilityService(_availabilityRepoMock.Object);
    }

    [Fact]
    public async Task GetRulesAsync_ReturnsFilteredRules()
    {
        // Arrange
        var therapistId = Guid.NewGuid();
        var rules = new List<AvailabilityRule>
        {
            new() { Id = Guid.NewGuid(), TherapistId = therapistId, DayOfWeek = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), TherapistId = Guid.NewGuid(), DayOfWeek = 1, IsActive = true }
        };
        _availabilityRepoMock.Setup(x => x.GetAllRulesAsync()).ReturnsAsync(rules);

        // Act
        var result = await _sut.GetRulesAsync(therapistId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateRuleAsync_ValidRequest_CreatesRule()
    {
        // Arrange
        var therapistId = Guid.NewGuid();
        var request = new CreateRuleRequest(1, new TimeOnly(9, 0), new TimeOnly(17, 0));
        var createdRule = new AvailabilityRule
        {
            Id = Guid.NewGuid(),
            TherapistId = therapistId,
            DayOfWeek = 1,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = true
        };
        _availabilityRepoMock.Setup(x => x.AddRuleAsync(It.IsAny<AvailabilityRule>()))
            .ReturnsAsync(createdRule);

        // Act
        var result = await _sut.CreateRuleAsync(therapistId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.DayOfWeek.Should().Be(1);
    }

    [Fact]
    public async Task UpdateRuleAsync_ExistingRule_UpdatesSuccessfully()
    {
        // Arrange
        var therapistId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var existingRule = new AvailabilityRule
        {
            Id = ruleId,
            TherapistId = therapistId,
            DayOfWeek = 1,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };
        var request = new UpdateRuleRequest(new TimeOnly(10, 0), new TimeOnly(18, 0), false);

        _availabilityRepoMock.Setup(x => x.GetRuleByIdAsync(ruleId)).ReturnsAsync(existingRule);
        _availabilityRepoMock.Setup(x => x.UpdateRuleAsync(It.IsAny<AvailabilityRule>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateRuleAsync(therapistId, ruleId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.StartTime.Should().Be(new TimeOnly(10, 0));
    }

    [Fact]
    public async Task DeleteRuleAsync_ExistingRule_DeletesSuccessfully()
    {
        // Arrange
        var ruleId = Guid.NewGuid();
        _availabilityRepoMock.Setup(x => x.RemoveRuleAsync(ruleId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteRuleAsync(ruleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
