using Core.Auditing;

namespace Imece.Authorization.Tests;

public sealed class AuditMutationClassifierTests
{
    [Theory]
    [InlineData("CreateCampaignDto", null, true, "Create", "Campaign")]
    [InlineData("UpdateUserDto", null, true, "Update", "User")]
    [InlineData("DeleteRoleRequest", null, true, "Delete", "Role")]
    [InlineData("AssignPermissionsDto", null, true, "Assign", "Permissions")]
    [InlineData("ContentListQueryDto", null, false, "", "")]
    [InlineData("IdRequest", "DELETE", true, "Delete", "Entity")]
    [InlineData("IdRequest", "GET", false, "", "")]
    public void Classifies_Mutation_Request_Types(
        string typeName,
        string? httpMethod,
        bool expected,
        string expectedVerb,
        string expectedEntity)
    {
        var ok = AuditMutationClassifier.TryClassify(
            typeName,
            httpMethod,
            out var verb,
            out var entity);

        Assert.Equal(expected, ok);
        if (expected)
        {
            Assert.Equal(expectedVerb, verb);
            Assert.Equal(expectedEntity, entity);
        }
    }

    [Fact]
    public void Audit_Categories_Are_Stable()
    {
        Assert.Equal("Request", AuditCategories.Request);
        Assert.Equal("Mutation", AuditCategories.Mutation);
        Assert.Equal("Error", AuditCategories.Error);
        Assert.Equal("Security", AuditCategories.Security);
        Assert.Equal("Sql", AuditCategories.Sql);
    }
}
