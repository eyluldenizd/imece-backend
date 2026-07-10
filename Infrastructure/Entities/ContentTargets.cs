using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class ContentTargets
{
    public long TargetRowId { get; set; }

    public string ContentType { get; set; } = null!;

    public long ContentId { get; set; }

    public string TargetType { get; set; } = null!;

    public int TargetId { get; set; }
}
