namespace Infrastructure.Database.Seeding;

public static class DevelopmentSeedPolicy
{
    /// <summary>
    /// Development/Test ortamında ve SeedDevelopmentData=true iken seed çalışır.
    /// Production'da asla çalışmaz.
    /// </summary>
    public static bool ShouldSeed(bool isDevelopmentOrTest, bool seedDevelopmentData) =>
        isDevelopmentOrTest && seedDevelopmentData;
}
