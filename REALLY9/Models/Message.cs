using System;
using System.Collections.Generic;

namespace REALLY9.Models;

public partial class Message
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int? AccountId { get; set; }

    public string? Text { get; set; }

    public DateTime? TimeStamp { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Customer? Customer { get; set; }
}
