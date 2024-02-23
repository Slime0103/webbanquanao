using System;
using System.Collections.Generic;

namespace REALLY9.Models;

public partial class Discount
{
    public int DiscountId { get; set; }

    public string Discountcode { get; set; } = null!;

    public DateTime? CreateDate { get; set; }

    public bool? Active { get; set; }

    public int? Levels { get; set; }

    public bool? Activeuse { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
