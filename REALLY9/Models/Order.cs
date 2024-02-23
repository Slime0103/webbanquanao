using System;
using System.Collections.Generic;

namespace REALLY9.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? Orderdate { get; set; }

    public string? Address { get; set; }

    public DateTime? ShipDate { get; set; }

    public int? TransactStatusId { get; set; }

    public int? TotalMoney { get; set; }

    public int? TotalMoneyAfterusedis { get; set; }

    public bool? Deleted { get; set; }

    public bool? Paid { get; set; }

    public DateTime? PaymentDay { get; set; }

    public int? PaymentId { get; set; }

    public string? Note { get; set; }

    public int? DiscountId { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Discount? Discount { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Shipper> Shippers { get; set; } = new List<Shipper>();

    public virtual TranscatStatus? TransactStatus { get; set; }
}
