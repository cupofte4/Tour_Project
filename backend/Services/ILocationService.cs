using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Application.Location;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationDto>> GetAllAsync();
        Task<LocationDto?> GetByIdAsync(int id);
        Task<LocationDto> CreateAsync(CreateLocationRequest req);
        Task<LocationDto?> UpdateAsync(int id, UpdateLocationRequest req);
        Task<bool> DeleteAsync(int id);
    }
}
