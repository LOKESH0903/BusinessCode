using SRRAMOils.Models;
using System.Data;
using System.Data.SqlClient;

namespace SRRAMOils.Service
{
    public class VendorService
    {
        public async Task<bool> CheckVendor(string vendorName)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();

                var connectionString = configuration.GetConnectionString("DevConnection")
                                       ?? configuration["ConnectionStrings:DefaultConnection"]
                                       ?? configuration["ConnectionString"]
                                       ?? configuration["ConnectionStrings:Connection"];

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Database connection string not found in configuration.");

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(1) FROM Vendor WHERE VendorName = @VendorName";
                var param = new SqlParameter("@VendorName", SqlDbType.NVarChar, 256)
                {
                    Value = vendorName ?? (object)DBNull.Value
                };
                command.Parameters.Add(param);

                var result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value) return false;
                if (result is int i) return i > 0;
                if (result is long l) return l > 0;
                if (int.TryParse(result.ToString(), out var parsed)) return parsed > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking vendor: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> AddVendor(
            string vendorName,
            string vendorGST,
            string email,
            string phone,
            string bankName,
            string bankAccountNumber,
            string bankIFSC,
            string bankBranch,
            int cityId,
            string website,
            string address,
            bool isActive)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();

                var connectionString = configuration.GetConnectionString("DevConnection")
                                       ?? configuration["ConnectionStrings:DefaultConnection"]
                                       ?? configuration["ConnectionString"]
                                       ?? configuration["ConnectionStrings:Connection"];

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Database connection string not found in configuration.");

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Vendor
                    (VendorName, VendorGST, VendorPhoneNumber, VendorBankName, VendorBankAccountNumber, VendorBankIFSCCode, VendorBankBranch, CityId,  VendorAddress, IsActive)
                    VALUES
                    (@VendorName, @VendorGST, @VendorPhoneNumber, @VendorBankName, @VendorBankAccountNumber, @VendorBankIFSCCode, @VendorBankBranch, @CityId, @VendorAddress, @IsActive)";

                command.Parameters.Add(new SqlParameter("@VendorName", SqlDbType.NVarChar, 256) { Value = vendorName ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@VendorGST", SqlDbType.NVarChar, 150) { Value = vendorGST ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@VendorPhoneNumber", SqlDbType.NVarChar, 25) { Value = phone ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@VendorBankName", SqlDbType.NVarChar, 100) { Value = bankName ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@VendorBankAccountNumber", SqlDbType.NVarChar, 100) { Value = bankAccountNumber ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@VendorBankIFSCCode", SqlDbType.NVarChar, 20) { Value = bankIFSC ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@VendorBankBranch", SqlDbType.NVarChar, 100) { Value = bankBranch ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@CityId", SqlDbType.NVarChar, 50) { Value = cityId });
                command.Parameters.Add(new SqlParameter("@VendorAddress", SqlDbType.NVarChar, 2000) { Value = address ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = isActive });

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding vendor: {ex.Message}");
                return false;
            }
        }


        public async Task<List<DropDownModel>> GetVendorNames()
        {
            var vendorNames = new List<DropDownModel>();
            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();
                var connectionString = configuration.GetConnectionString("DevConnection")
                                       ?? configuration["ConnectionStrings:DefaultConnection"]
                                       ?? configuration["ConnectionString"]
                                       ?? configuration["ConnectionStrings:Connection"];
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Database connection string not found in configuration.");
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, VendorName FROM Vendor WHERE IsActive = 1";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                    {
                        vendorNames.Add(new DropDownModel
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving vendor names: {ex.Message}");
            }
            return vendorNames;
        }
    }
}
