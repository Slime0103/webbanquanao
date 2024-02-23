using System;
using System.Collections.Generic;

namespace REALLY9.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public string? CustomerName { get; set; }

    public string? ProductName { get; set; }

    public int? OrderNumber { get; set; }

    public int? Quantity { get; set; }

    public int? Discount { get; set; }

    public int? Total { get; set; }

    public DateTime? ShipDate { get; set; }

    public int? Price { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
