using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class ContentTargets
{
    [DbManager.DbColumn("target_row_id")]
    public long TargetRowId { get; set; }

    [DbManager.DbColumn("content_type")]
    public string ContentType { get; set; } = null!;

    [DbManager.DbColumn("content_id")]
    public long ContentId { get; set; }

    [DbManager.DbColumn("target_type")]
    public string TargetType { get; set; } = null!;

    [DbManager.DbColumn("target_id")]
    public int TargetId { get; set; }
}
