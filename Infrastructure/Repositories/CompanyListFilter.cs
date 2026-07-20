namespace Infrastructure.Repositories;

/// <summary>
/// Liste sorguları için şirket filtresi.
/// GA: her iki alan null → tüm kayıtlar.
/// Tek üyelik: CompanyId dolu.
/// Çoklu üyelik: AccessibleCompanyIdsCsv dolu (virgülle ayrılmış ID listesi).
/// </summary>
public sealed record CompanyListFilter(int? CompanyId, string? AccessibleCompanyIdsCsv);
