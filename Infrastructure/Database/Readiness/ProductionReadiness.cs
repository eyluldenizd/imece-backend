using Infrastructure.Database.Options;

namespace Infrastructure.Database.Readiness;

public sealed class ProductionReadinessSnapshot
{
    public bool IsProduction { get; init; }

    public bool ContentIsolationEnforced { get; init; }

    public SchemaSyncMode SchemaMode { get; init; }

    public bool AuditEnabled { get; init; }

    public bool SqlBackedDirectoryProvider { get; init; }

    public bool DevelopmentSeedEnabled { get; init; }

    public bool ResetDatabaseEnabled { get; init; }
}

public sealed class ProductionReadinessCheckResult
{
    public required string Name { get; init; }

    public bool Passed { get; init; }

    public bool Required { get; init; }

    public string? Detail { get; init; }
}

public sealed class ProductionReadinessResult
{
    public bool IsSafe { get; init; }

    public IReadOnlyList<ProductionReadinessCheckResult> Checks { get; init; } = [];
}

public static class ProductionReadinessEvaluator
{
    public static ProductionReadinessResult Evaluate(
        ProductionSafetyOptions options,
        ProductionReadinessSnapshot snapshot)
    {
        // options reserved for future thresholds; currently snapshot-driven.
        _ = options;

        var checks = new List<ProductionReadinessCheckResult>();

        if (!snapshot.IsProduction)
        {
            checks.Add(new ProductionReadinessCheckResult
            {
                Name = "Environment",
                Passed = true,
                Required = false,
                Detail = "Non-production ortamı; production kuralları uygulanmaz."
            });

            return new ProductionReadinessResult
            {
                IsSafe = true,
                Checks = checks
            };
        }

        checks.Add(Check(
            "ContentIsolationEnforced",
            snapshot.ContentIsolationEnforced,
            required: true,
            passDetail: "ContentIsolation:Enforce=true",
            failDetail: "Production'da ContentIsolation:Enforce=true olmalıdır."));

        checks.Add(Check(
            "SchemaModeValidateOnly",
            snapshot.SchemaMode == SchemaSyncMode.ValidateOnly,
            required: true,
            passDetail: "Schema mode ValidateOnly",
            failDetail: "Production'da DatabaseSchema:Mode ValidateOnly olmalıdır."));

        checks.Add(Check(
            "AuditEnabled",
            snapshot.AuditEnabled,
            required: true,
            passDetail: "Audit etkin",
            failDetail: "Production'da Audit:Enabled=true olmalıdır."));

        checks.Add(Check(
            "SqlBackedDirectoryProvider",
            snapshot.SqlBackedDirectoryProvider,
            required: false,
            passDetail: "SQL-backed directory provider",
            failDetail: "Production'da DevelopmentDirectoryUserService kullanılmamalı."));

        checks.Add(Check(
            "DevelopmentSeedDisabled",
            !snapshot.DevelopmentSeedEnabled,
            required: true,
            passDetail: "Development seed kapalı",
            failDetail: "Production'da SeedDevelopmentData=false olmalıdır."));

        checks.Add(Check(
            "ResetDatabaseDisabled",
            !snapshot.ResetDatabaseEnabled,
            required: true,
            passDetail: "ResetDatabase kapalı",
            failDetail: "Production'da ResetDatabase=false olmalıdır."));

        var requiredFailed = checks.Any(c => c.Required && !c.Passed);

        return new ProductionReadinessResult
        {
            IsSafe = !requiredFailed,
            Checks = checks
        };
    }

    private static ProductionReadinessCheckResult Check(
        string name,
        bool passed,
        bool required,
        string passDetail,
        string failDetail) =>
        new()
        {
            Name = name,
            Passed = passed,
            Required = required,
            Detail = passed ? passDetail : failDetail
        };
}
