namespace Core.Auditing;

/// <summary>
/// Request DTO adından mutation semantiği çıkarır.
/// Executor dekoratörü ve testler aynı kuralı paylaşır.
/// </summary>
public static class AuditMutationClassifier
{
    private static readonly string[] MutationPrefixes =
    [
        "Create", "Update", "Delete", "SoftDelete", "Remove",
        "Assign", "Replace", "Publish", "Unpublish", "Cancel",
        "Approve", "Reject", "Activate", "Deactivate"
    ];

    public static bool TryClassify(
        string requestTypeName,
        string? httpMethod,
        out string actionVerb,
        out string entityType)
    {
        actionVerb = string.Empty;
        entityType = requestTypeName;

        foreach (var prefix in MutationPrefixes)
        {
            if (!requestTypeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            actionVerb = prefix;
            entityType = requestTypeName[prefix.Length..];
            entityType = TrimSuffix(entityType, "Dto");
            entityType = TrimSuffix(entityType, "Request");

            if (string.IsNullOrWhiteSpace(entityType))
            {
                entityType = requestTypeName;
            }

            return true;
        }

        if (requestTypeName.Equals("IdRequest", StringComparison.OrdinalIgnoreCase)
            && httpMethod is not null
            && httpMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
        {
            actionVerb = "Delete";
            entityType = "Entity";
            return true;
        }

        return false;
    }

    private static string TrimSuffix(string value, string suffix)
    {
        if (value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            && value.Length > suffix.Length)
        {
            return value[..^suffix.Length];
        }

        return value;
    }
}
