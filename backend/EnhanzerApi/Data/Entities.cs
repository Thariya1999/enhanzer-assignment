using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnhanzerApi.Data;

[Table("Location_Details")]
public class LocationDetail
{
    public int Id { get; set; }

    [Column("Location_Code"), MaxLength(100), Required]
    public string LocationCode { get; set; } = string.Empty;

    [Column("Location_Name"), MaxLength(250), Required]
    public string LocationName { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Purchase_Bills")]
public class PurchaseBill
{
    public int Id { get; set; }

    [MaxLength(256)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TotalItems { get; set; }
    public int TotalQty { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSelling { get; set; }

    public List<PurchaseBillItem> Items { get; set; } = new();
}

[Table("Purchase_Bill_Items")]
public class PurchaseBillItem
{
    public int Id { get; set; }
    public int PurchaseBillId { get; set; }
    public PurchaseBill? PurchaseBill { get; set; }

    [MaxLength(100)] public string Item { get; set; } = string.Empty;
    [MaxLength(250)] public string Batch { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")] public decimal StandardCost { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal StandardPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Margin { get; set; }

    public int Qty { get; set; }
    public int FreeQty { get; set; }

    [Column(TypeName = "decimal(5,2)")] public decimal DiscountPercent { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalCost { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalSelling { get; set; }
}
