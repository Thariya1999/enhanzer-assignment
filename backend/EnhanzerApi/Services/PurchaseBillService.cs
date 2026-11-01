using EnhanzerApi.Data;
using EnhanzerApi.Models;

namespace EnhanzerApi.Services;

public interface IPurchaseBillService
{
    Task<SavePurchaseBillResponse> SaveAsync(SavePurchaseBillRequest request, string user, CancellationToken ct = default);
}

public class PurchaseBillService(AppDbContext db) : IPurchaseBillService
{
    public async Task<SavePurchaseBillResponse> SaveAsync(SavePurchaseBillRequest request, string user, CancellationToken ct = default)
    {
        var bill = new PurchaseBill { CreatedBy = user };

        foreach (var dto in request.Items)
        {
            // Totals are recalculated on the server - never trust values computed by the client.
            var totalCost = Math.Round(dto.StandardCost * dto.Qty * (1 - dto.DiscountPercent / 100m), 2);
            var totalSelling = Math.Round(dto.StandardPrice * dto.Qty, 2);

            bill.Items.Add(new PurchaseBillItem
            {
                Item = dto.Item,
                Batch = dto.Batch,
                StandardCost = dto.StandardCost,
                StandardPrice = dto.StandardPrice,
                Margin = dto.StandardPrice - dto.StandardCost,
                Qty = dto.Qty,
                FreeQty = dto.FreeQty,
                DiscountPercent = dto.DiscountPercent,
                TotalCost = totalCost,
                TotalSelling = totalSelling
            });
        }

        bill.TotalItems = bill.Items.Count;
        bill.TotalQty = bill.Items.Sum(i => i.Qty);
        bill.TotalCost = bill.Items.Sum(i => i.TotalCost);
        bill.TotalSelling = bill.Items.Sum(i => i.TotalSelling);

        db.PurchaseBills.Add(bill);
        await db.SaveChangesAsync(ct);

        return new SavePurchaseBillResponse(bill.Id, bill.TotalItems, bill.TotalQty, bill.TotalCost, bill.TotalSelling);
    }
}
