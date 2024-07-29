using RestaurantTableBookingApp.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantTableBookingApp.Data
{
    public class RestaurantRepository : IRestaurantRepository
    {
        public RestaurantRepository() { 
        
        }
        public Task<IEnumerable<RestaurantModel>> GetAllRestaurantsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
