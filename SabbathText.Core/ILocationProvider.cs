using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public interface ILocationProvider
    {
        Task<Entities.Location> GetLocationByZipCode(string zipCode);
        Task<Entities.LocationTimeInfo> GetTimeInfoByZipCode(string zipCode, DateTime date);
    }
}
