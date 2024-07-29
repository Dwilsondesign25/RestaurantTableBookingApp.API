using RestaurantTableBookingApp.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantTableBookingApp.Data
{
    public interface IRestaurantRepository
    {
        Task<List<RestaurantModel>> GetAllRestaurantsAsync();
        Task<IEnumerable<RestaurantBranchModel>> GetRestaurantBranchsByRestaurantIdAsync(int restaurantId);

        /// <summary>
        /// LINQ query retrieves dining tables and their associated time slots for a specific branchId and date. The result is sorted by Id and then MealType. The data is then projected into a list of DiningTableWithTimeSlotsModel view models and returned.
        /// </summary>
        /// <param name="branchId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<IEnumerable<DiningTableWithTimeSlotsModel>> GetDiningTablesByBranchAsync(int branchId, DateTime date);

        /// <summary>
        /// LINQ query retrieves dining tables and their associated time slots for a specific branchId and date which are current and future. The result is sorted by Id and then MealType. The data is then projected into a list of DiningTableWithTimeSlotsModel view models and returned.
        /// </summary>
        /// <param name="branchId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<IEnumerable<DiningTableWithTimeSlotsModel>> GetDiningTablesByBranchAndDateAsync(int branchId, DateTime date);

        Task<IEnumerable<DiningTableWithTimeSlotsModel>> GetDiningTablesByBranchAsync(int branchId);
    }
}
