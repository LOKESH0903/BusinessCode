using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SRRAMOils.DBRepository;

namespace SRRAMOils.Service
{
    public class OrganizationService
    {
        public OrganizationService()
        {
        }

        public async Task<DataTable> GetOrgianizations()
        {
            DataTable dataTable = new DataTable();
            DataBase db = new DataBase(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
            await db.ExecuteQueryAsync("SELECT * FROM Organization").ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    dataTable = task.Result;
                    // Process the dataTable as needed
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var dict = new System.Collections.Generic.Dictionary<string, object>();
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            dict[col.ColumnName] = row[col] ?? string.Empty;
                        }
                    }
                }
                else
                {
                    // Handle exceptions
                    Console.WriteLine($"Error fetching organizations: {task.Exception?.Message}");
                }
            });

            return dataTable;
        }

        public async Task<bool> CheckOrgianization(string OrgName)
        {
            try
            {
                // Load configuration and obtain connection string.
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();

                // Try common keys for connection string
                var connectionString = configuration.GetConnectionString("DevConnection")
                                       ?? configuration["ConnectionStrings:DefaultConnection"]
                                       ?? configuration["ConnectionString"]
                                       ?? configuration["ConnectionStrings:Connection"];

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Database connection string not found in configuration.");
                }

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(1) FROM Organization WHERE OrganizationName = @OrgName";
                var param = new SqlParameter("@OrgName", SqlDbType.NVarChar, 256)
                {
                    Value = OrgName ?? (object)DBNull.Value
                };
                command.Parameters.Add(param);

                var result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    return false;
                }

                // Handle common numeric types returned by ExecuteScalarAsync
                if (result is int i) return i > 0;
                if (result is long l) return l > 0;
                if (result is decimal d) return d > 0;
                if (int.TryParse(result.ToString(), out var parsed)) return parsed > 0;
            }
            catch (Exception ex)
            {
                // Log the exception (for demonstration, we're writing to console)
                Console.WriteLine($"Error checking organization: {ex.Message}");
            }

            return false;
        }


        public async Task<bool> AddOrganization(string OrgName)
        {
            try
            {
                // Load configuration and obtain connection string.
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();
                // Try common keys for connection string
                var connectionString = configuration.GetConnectionString("DevConnection")
                                       ?? configuration["ConnectionStrings:DefaultConnection"]
                                       ?? configuration["ConnectionString"]
                                       ?? configuration["ConnectionStrings:Connection"];
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Database connection string not found in configuration.");
                }
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Organization (OrganizationName,IsActive) VALUES (@OrgName,@IsActive)";
                var param = new SqlParameter("@OrgName", SqlDbType.NVarChar, 256)
                {
                    Value = OrgName ?? (object)DBNull.Value
                };

                var isActiveParam = new SqlParameter("@IsActive", SqlDbType.Bit)
                {
                    Value = true
                };

                command.Parameters.Add(param);
                command.Parameters.Add(isActiveParam);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log the exception (for demonstration, we're writing to console)
                Console.WriteLine($"Error adding organization: {ex.Message}");
            }
            return false;
        }
    }
}
