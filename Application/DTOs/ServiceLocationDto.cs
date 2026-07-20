using Core.Common.Validation;

namespace Application.DTOs;

public sealed class ServiceLocationDto
{
    public long ServiceLocationId { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ServiceLocationTypeId { get; set; }
    public string? TypeName { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateServiceLocationDto
{
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Konum adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Konum adı en fazla 256 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    public int? ServiceLocationTypeId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Konum türü en fazla 32 karakter olabilir.")]
    public string? LocationType { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Adres en fazla 512 karakter olabilir.")]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public sealed class UpdateServiceLocationDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir konum ID değeri gönderilmelidir.")]
    public long ServiceLocationId { get; set; }

    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Konum adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Konum adı en fazla 256 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    public int? ServiceLocationTypeId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Konum türü en fazla 32 karakter olabilir.")]
    public string? LocationType { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Adres en fazla 512 karakter olabilir.")]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ServiceRouteStopDto
{
    public long ServiceRouteStopId { get; set; }
    public long ServiceRouteId { get; set; }
    public long ServiceLocationId { get; set; }
    public int StopOrder { get; set; }
    public string? ArrivalTime { get; set; }
    public string? DepartureTime { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ServiceRouteStopInputDto
{
    public long? ServiceRouteStopId { get; set; }
    public long ServiceLocationId { get; set; }
    public int StopOrder { get; set; }
    public string? ArrivalTime { get; set; }
    public string? DepartureTime { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
