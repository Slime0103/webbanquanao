using System;
using System.Collections.Generic;

namespace REALLY9.Models;

public partial class TranscatStatus
{
    public int TranscatStatusId { get; set; }

    public string? Status { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
