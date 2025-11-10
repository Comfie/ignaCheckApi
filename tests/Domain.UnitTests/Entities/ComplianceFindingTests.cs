using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Domain.UnitTests.Entities;

[TestFixture]
public class ComplianceFindingTests
{
    [Test]
    public void ComplianceFinding_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var finding = new ComplianceFinding();

        // Assert
        finding.FindingCode.ShouldBe(string.Empty);
        finding.Title.ShouldBe(string.Empty);
        finding.Description.ShouldBe(string.Empty);
        finding.Status.ShouldBe(ComplianceStatus.NotAssessed);
        finding.WorkflowStatus.ShouldBe(FindingWorkflowStatus.Open);
        finding.RiskLevel.ShouldBe(RiskLevel.Medium);
        finding.AnalysisVersion.ShouldBe(1);
        finding.IsReviewed.ShouldBe(false);
        finding.Evidence.ShouldNotBeNull();
        finding.Evidence.ShouldBeEmpty();
        finding.Comments.ShouldNotBeNull();
        finding.Comments.ShouldBeEmpty();
    }

    [Test]
    public void ComplianceFinding_Should_Set_Properties_Correctly()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var controlId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(30);

        // Act
        var finding = new ComplianceFinding
        {
            OrganizationId = organizationId,
            ProjectId = projectId,
            ControlId = controlId,
            FindingCode = "FND-001",
            Title = "Missing Data Encryption Policy",
            Description = "No documented encryption policy found for data at rest",
            Status = ComplianceStatus.NonCompliant,
            WorkflowStatus = FindingWorkflowStatus.InProgress,
            RiskLevel = RiskLevel.High,
            RemediationGuidance = "Create and implement a comprehensive encryption policy",
            EstimatedEffort = 40.0m,
            ConfidenceScore = 0.95m,
            DueDate = dueDate,
            AnalysisModel = "claude-3-sonnet",
            AnalysisVersion = 2
        };

        // Assert
        finding.OrganizationId.ShouldBe(organizationId);
        finding.ProjectId.ShouldBe(projectId);
        finding.ControlId.ShouldBe(controlId);
        finding.FindingCode.ShouldBe("FND-001");
        finding.Title.ShouldBe("Missing Data Encryption Policy");
        finding.Description.ShouldBe("No documented encryption policy found for data at rest");
        finding.Status.ShouldBe(ComplianceStatus.NonCompliant);
        finding.WorkflowStatus.ShouldBe(FindingWorkflowStatus.InProgress);
        finding.RiskLevel.ShouldBe(RiskLevel.High);
        finding.RemediationGuidance.ShouldBe("Create and implement a comprehensive encryption policy");
        finding.EstimatedEffort.ShouldBe(40.0m);
        finding.ConfidenceScore.ShouldBe(0.95m);
        finding.DueDate.ShouldBe(dueDate);
        finding.AnalysisModel.ShouldBe("claude-3-sonnet");
        finding.AnalysisVersion.ShouldBe(2);
    }

    [Test]
    public void ComplianceFinding_Should_Track_Review_Information()
    {
        // Arrange
        var finding = new ComplianceFinding();
        var reviewerId = "user-123";
        var reviewDate = DateTime.UtcNow;
        var reviewNotes = "Verified with security team";

        // Act
        finding.IsReviewed = true;
        finding.ReviewedBy = reviewerId;
        finding.ReviewedDate = reviewDate;
        finding.ReviewNotes = reviewNotes;

        // Assert
        finding.IsReviewed.ShouldBe(true);
        finding.ReviewedBy.ShouldBe(reviewerId);
        finding.ReviewedDate.ShouldBe(reviewDate);
        finding.ReviewNotes.ShouldBe(reviewNotes);
    }

    [Test]
    public void ComplianceFinding_Should_Track_Assignment()
    {
        // Arrange
        var finding = new ComplianceFinding();
        var assignedUserId = "user-456";

        // Act
        finding.AssignedTo = assignedUserId;
        finding.WorkflowStatus = FindingWorkflowStatus.InProgress;

        // Assert
        finding.AssignedTo.ShouldBe(assignedUserId);
        finding.WorkflowStatus.ShouldBe(FindingWorkflowStatus.InProgress);
    }

    [Test]
    public void ComplianceFinding_Should_Track_Resolution()
    {
        // Arrange
        var finding = new ComplianceFinding
        {
            WorkflowStatus = FindingWorkflowStatus.InProgress
        };
        var resolverId = "user-789";
        var resolvedDate = DateTime.UtcNow;
        var resolutionNotes = "Implemented encryption policy and validated with IT team";

        // Act
        finding.WorkflowStatus = FindingWorkflowStatus.Resolved;
        finding.ResolvedBy = resolverId;
        finding.ResolvedDate = resolvedDate;
        finding.ResolutionNotes = resolutionNotes;

        // Assert
        finding.WorkflowStatus.ShouldBe(FindingWorkflowStatus.Resolved);
        finding.ResolvedBy.ShouldBe(resolverId);
        finding.ResolvedDate.ShouldBe(resolvedDate);
        finding.ResolutionNotes.ShouldBe(resolutionNotes);
    }

    [Test]
    public void ComplianceFinding_Should_Allow_Adding_Evidence()
    {
        // Arrange
        var finding = new ComplianceFinding();
        var evidence = new FindingEvidence
        {
            Id = Guid.NewGuid(),
            FindingId = finding.Id,
            Notes = "Screenshot of encryption settings"
        };

        // Act
        finding.Evidence.Add(evidence);

        // Assert
        finding.Evidence.Count.ShouldBe(1);
        finding.Evidence.First().Notes.ShouldBe("Screenshot of encryption settings");
    }

    [Test]
    public void ComplianceFinding_Should_Allow_Adding_Comments()
    {
        // Arrange
        var finding = new ComplianceFinding();
        var comment = new FindingComment
        {
            Id = Guid.NewGuid(),
            FindingId = finding.Id,
            Content = "Need to follow up with compliance team"
        };

        // Act
        finding.Comments.Add(comment);

        // Assert
        finding.Comments.Count.ShouldBe(1);
        finding.Comments.First().Content.ShouldBe("Need to follow up with compliance team");
    }

    [Test]
    [TestCase(RiskLevel.Critical)]
    [TestCase(RiskLevel.High)]
    [TestCase(RiskLevel.Medium)]
    [TestCase(RiskLevel.Low)]
    public void ComplianceFinding_Should_Support_All_RiskLevel_Values(RiskLevel riskLevel)
    {
        // Arrange & Act
        var finding = new ComplianceFinding { RiskLevel = riskLevel };

        // Assert
        finding.RiskLevel.ShouldBe(riskLevel);
    }

    [Test]
    [TestCase(ComplianceStatus.Compliant)]
    [TestCase(ComplianceStatus.NonCompliant)]
    [TestCase(ComplianceStatus.PartiallyCompliant)]
    [TestCase(ComplianceStatus.NotAssessed)]
    public void ComplianceFinding_Should_Support_All_ComplianceStatus_Values(ComplianceStatus status)
    {
        // Arrange & Act
        var finding = new ComplianceFinding { Status = status };

        // Assert
        finding.Status.ShouldBe(status);
    }

    [Test]
    [TestCase(FindingWorkflowStatus.Open)]
    [TestCase(FindingWorkflowStatus.InProgress)]
    [TestCase(FindingWorkflowStatus.Resolved)]
    [TestCase(FindingWorkflowStatus.Accepted)]
    [TestCase(FindingWorkflowStatus.FalsePositive)]
    public void ComplianceFinding_Should_Support_All_WorkflowStatus_Values(FindingWorkflowStatus status)
    {
        // Arrange & Act
        var finding = new ComplianceFinding { WorkflowStatus = status };

        // Assert
        finding.WorkflowStatus.ShouldBe(status);
    }

    [Test]
    public void ComplianceFinding_Should_Store_AI_Analysis_Metadata()
    {
        // Arrange
        var finding = new ComplianceFinding();
        var lastAnalysisDate = DateTime.UtcNow;

        // Act
        finding.AnalysisModel = "claude-3-opus";
        finding.AnalysisVersion = 3;
        finding.LastAnalysisDate = lastAnalysisDate;
        finding.ConfidenceScore = 0.92m;
        finding.RawAnalysisData = "{\"model\":\"claude-3-opus\",\"tokens\":1234}";

        // Assert
        finding.AnalysisModel.ShouldBe("claude-3-opus");
        finding.AnalysisVersion.ShouldBe(3);
        finding.LastAnalysisDate.ShouldBe(lastAnalysisDate);
        finding.ConfidenceScore.ShouldBe(0.92m);
        finding.RawAnalysisData.ShouldContain("claude-3-opus");
    }
}
