using EnhanzerApi.Data;
using EnhanzerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EnhanzerApi.Services;

public interface ILocationService
{
    Task SaveAsync(IEnumerable<LocationDto> locations, CancellationToken ct = default);
    Task<List<LocationDto>> GetAllAsync(CancellationToken ct = default);
}

public class LocationService(AppDbContext db) : ILocationService
{
    /// <summary>Upsert by Location_Code so repeated logins never create duplicates.</summary>
    public async Task SaveAsync(IEnumerable<LocationDto> locations, CancellationToken ct = default)
    {
        var incoming = locations
            .GroupBy(l => l.LocationCode)
            .Select(g => g.First())
            .ToList();
        if (incoming.Count == 0) return;

        var codes = incoming.Select(l => l.LocationCode).ToList();
        var existing = await db.LocationDetails
            .Where(l => codes.Contains(l.LocationCode))
            .ToDictionaryAsync(l => l.LocationCode, ct);

        foreach (var loc in incoming)
        {
            if (existing.TryGetValue(loc.LocationCode, out var row))
            {
                row.LocationName = loc.LocationName;
                row.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                db.LocationDetails.Add(new LocationDetail
                {
                    LocationCode = loc.LocationCode,
                    LocationName = loc.LocationName
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    public Task<List<LocationDto>> GetAllAsync(CancellationToken ct = default) =>
        db.LocationDetails
            .AsNoTracking()
            .OrderBy(l => l.LocationName)
            .Select(l => new LocationDto(l.LocationCode, l.LocationName))
            .ToListAsync(ct);
}
