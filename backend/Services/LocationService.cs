using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tour_Project.Data;
using backend.Application.Location;
using Tour_Project.Models;

namespace backend.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _db;

        public LocationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<LocationDto>> GetAllAsync()
        {
            return await _db.Locations
                .AsNoTracking()
                .Select(l => new LocationDto {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Address = l.Address,
                    Prio = l.Prio
                })
                .ToListAsync();
        }

        public async Task<LocationDto?> GetByIdAsync(int id)
        {
            var l = await _db.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (l == null) return null;
            return new LocationDto {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Address = l.Address,
                Prio = l.Prio
            };
        }

        public async Task<LocationDto> CreateAsync(CreateLocationRequest req)
        {
            var l = new Location {
                Name = req.Name,
                Description = req.Description,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Address = req.Address,
                Prio = req.Prio ?? 0
            };
            _db.Locations.Add(l);
            await _db.SaveChangesAsync();
            return new LocationDto {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Address = l.Address,
                Prio = l.Prio
            };
        }

        public async Task<LocationDto?> UpdateAsync(int id, UpdateLocationRequest req)
        {
            var l = await _db.Locations.FindAsync(id);
            if (l == null) return null;
            l.Name = req.Name ?? l.Name;
            l.Description = req.Description ?? l.Description;
            if (req.Latitude.HasValue) l.Latitude = req.Latitude.Value;
            if (req.Longitude.HasValue) l.Longitude = req.Longitude.Value;
            l.Address = req.Address ?? l.Address;
            if (req.Prio.HasValue) l.Prio = req.Prio.Value;
            await _db.SaveChangesAsync();
            return new LocationDto {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Address = l.Address,
                Prio = l.Prio
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var l = await _db.Locations.FindAsync(id);
            if (l == null) return false;
            _db.Locations.Remove(l);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
namespace Tour_Project.Services
{
    public class LocationService
    {
        public double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) *
                       Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) *
                       Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c * 1000;
        }
    }
}
