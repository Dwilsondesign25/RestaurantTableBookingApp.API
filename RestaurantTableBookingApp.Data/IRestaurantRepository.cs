using RestaurantTableBookingApp.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantTableBookingApp.Data
{
    public interface IRestaurantRepository
    {

        Task<IEnumerable<RestaurantModel>> GetAllRestaurantsAsync();
    }
}
