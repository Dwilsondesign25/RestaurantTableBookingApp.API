using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LSC.RestaurantTableBookingApp.Function
{
    public class TimeslotGenerationFunction
    {
        [FunctionName("TimeslotGenerationFunction")]
        public static async Task Run([TimerTrigger("*/12 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                // Get the connection string from the configuration
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                string connectionString = config.GetConnectionString("DbContext");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Query to get the RestaurantBranchId and its last ReservationDate
                    string getLastReservationDateQuery = @"
                    SELECT DiningTable.RestaurantBranchId, MAX(TimeSlots.ReservationDay) AS LastReservationDate
                    FROM DiningTable
                    LEFT OUTER JOIN TimeSlots ON DiningTable.Id = TimeSlots.DiningTableId
                    GROUP BY DiningTable.RestaurantBranchId";

                    SqlCommand getLastReservationDateCommand = new SqlCommand(getLastReservationDateQuery, connection);

                    List<(int BranchId, DateTime? LastReservationDate)> branchData = new List<(int, DateTime?)>();

                    using (SqlDataReader reader = await getLastReservationDateCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int branchId = reader.GetInt32(reader.GetOrdinal("RestaurantBranchId"));
                            DateTime? lastReservationDate = reader.IsDBNull(reader.GetOrdinal("LastReservationDate"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("LastReservationDate"));
                            branchData.Add((branchId, lastReservationDate));
                        }
                    }

                    // Process each branch
                    foreach (var data in branchData)
                    {
                        int branchId = data.BranchId;
                        DateTime? lastReservationDate = data.LastReservationDate;

                        // Calculate the reservation end date (current date + 1 or 2 days)
                        DateTime currentDate = DateTime.Now.Date;
                        DateTime reservationEndDate = lastReservationDate.HasValue && lastReservationDate.Value.Date < currentDate
                            ? currentDate.AddDays(2)
                            : (lastReservationDate.GetValueOrDefault(currentDate.AddDays(-1))).AddDays(2);

                        if (lastReservationDate.GetValueOrDefault(currentDate.AddDays(-1)) <= currentDate.AddDays(2))
                        {
                            // Query to get the DiningTableIds for the branch
                            string getDiningTableIdsQuery = @"
                            SELECT Id AS DiningTableId
                            FROM DiningTable
                            WHERE RestaurantBranchId = @BranchId";

                            SqlCommand getDiningTableIdsCommand = new SqlCommand(getDiningTableIdsQuery, connection);
                            getDiningTableIdsCommand.Parameters.AddWithValue("@BranchId", branchId);

                            List<int> diningTableIds = new List<int>();

                            using (SqlDataReader diningTableIdReader = await getDiningTableIdsCommand.ExecuteReaderAsync())
                            {
                                while (await diningTableIdReader.ReadAsync())
                                {
                                    int diningTableId = diningTableIdReader.GetInt32(diningTableIdReader.GetOrdinal("DiningTableId"));
                                    diningTableIds.Add(diningTableId);
                                }
                            }

                            // Generate and insert new timeslots for the next 1 or 2 days for each dining table
                            foreach (int diningTableId in diningTableIds)
                            {
                                for (DateTime reservationDate = (lastReservationDate.GetValueOrDefault(currentDate.AddDays(-1)).Date).AddDays(1);
                                    reservationDate <= reservationEndDate;
                                    reservationDate = reservationDate.AddDays(1))
                                {
                                    // Insert available slots into the TimeSlots table for each meal type
                                    foreach (string mealType in new[] { "Breakfast", "Lunch", "Dinner" })
                                    {
                                        string insertTimeslotQuery = @"
                                        INSERT INTO TimeSlots (DiningTableId, ReservationDay, MealType, TableStatus)
                                        VALUES (@DiningTableId, @ReservationDay, @MealType, @TableStatus)";

                                        SqlCommand insertTimeslotCommand = new SqlCommand(insertTimeslotQuery, connection);
                                        insertTimeslotCommand.Parameters.AddWithValue("@DiningTableId", diningTableId);
                                        insertTimeslotCommand.Parameters.AddWithValue("@ReservationDay", reservationDate);
                                        insertTimeslotCommand.Parameters.AddWithValue("@MealType", mealType);
                                        insertTimeslotCommand.Parameters.AddWithValue("@TableStatus", "Available");

                                        await insertTimeslotCommand.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                        }
                    }
                }

                log.LogInformation($"Timeslot generation completed at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while processing the request.");
            }
        }
    }
}
