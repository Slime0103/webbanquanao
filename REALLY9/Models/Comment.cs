using System;
using System.Collections.Generic;

namespace REALLY9.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int? CustomerId { get; set; }

    public int? ProductId { get; set; }

    public string? CommentText { get; set; }

    public int? Active { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? Rating { get; set; }

    public string? Email { get; set; }

    public string? CustomerName { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Product? Product { get; set; }
}
