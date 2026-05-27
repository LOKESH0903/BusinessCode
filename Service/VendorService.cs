using SRRAMOils.Models;
using System.Data;
using System.Data.SqlClient;
using System.Net;

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
                command.CommandText = "SELECT Id, VendorName FROM Vendor WHERE IsActive = 1 ORDER BY VendorName";
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


        public List<DropDownModel> GetInvoiceNumbersByVendor(int vendorId)
        {
            var invoiceNumbers = new List<DropDownModel>();
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
                using var connection = new SqlConnection(connectionString);
                connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, InvoiceNumber FROM VendorPurchase WHERE VendorId = @VendorId AND ISNULL(ISPaymentDone, 0) = 0";
                command.Parameters.Add(new SqlParameter("@VendorId", SqlDbType.Int) { Value = vendorId });
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                    {
                        invoiceNumbers.Add(new DropDownModel
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving invoice numbers: {ex.Message}");
            }
            return invoiceNumbers;
        }


        public List<DropDownModel> GetClosedInvoiceNumbersByVendor(int vendorId)
        {
            var invoiceNumbers = new List<DropDownModel>();
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
                using var connection = new SqlConnection(connectionString);
                connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, InvoiceNumber FROM VendorPurchase WHERE VendorId = @VendorId AND ISNULL(ISPaymentDone, 0) = 1";
                command.Parameters.Add(new SqlParameter("@VendorId", SqlDbType.Int) { Value = vendorId });
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                    {
                        invoiceNumbers.Add(new DropDownModel
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving invoice numbers: {ex.Message}");
            }
            return invoiceNumbers;
        }




        public async Task<bool> AddVendorPurchase(int VendorId, string InvoiceNumber, decimal Amount, DateTime OrderDate, decimal TravelCharge, bool IsGSTBill, bool ISPaymentDone, bool IsCreditPayment)
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
                    INSERT INTO VendorPurchase
                    (VendorId,InvoiceNumber,Amount,OrderDate,TravelCharge,IsGSTBill,IsCreditPayment)
                    VALUES
                    (@VendorId,@InvoiceNumber,@Amount,@OrderDate,@TravelCharge,@IsGSTBill,@IsCreditPayment)";

                command.Parameters.Add(new SqlParameter("@VendorId", SqlDbType.Int, 256) { Value = VendorId });
                command.Parameters.Add(new SqlParameter("@InvoiceNumber", SqlDbType.NVarChar, 150) { Value = InvoiceNumber ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal, 25) { Value = Amount });
                command.Parameters.Add(new SqlParameter("@OrderDate", SqlDbType.Date, 100) { Value = OrderDate });
                command.Parameters.Add(new SqlParameter("@TravelCharge", SqlDbType.Decimal, 100) { Value = TravelCharge });
                command.Parameters.Add(new SqlParameter("@IsGSTBill", SqlDbType.Bit, 20) { Value = IsGSTBill });
                command.Parameters.Add(new SqlParameter("@IsCreditPayment", SqlDbType.Bit, 20) { Value = IsCreditPayment });

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding vendor: {ex.Message}");
                return false;
            }
        }

        public async Task<List<VendorPayment>> GetVendorPaymentsByInvoiceId(int vendorPurchaseId)
        {
            var payments = new List<VendorPayment>();
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
                    SELECT PaymentDate, PT.PaymentModeName,Amount, PaymentReferenceNumber
                    FROM VendorPayment VP INNER JOIN PaymentType PT ON VP.PaymentTypeId = PT.Id WHERE VP.VendorPurchaseId = @VendorPurchaseId";
                command.Parameters.Add(new SqlParameter("@VendorPurchaseId", SqlDbType.Int) { Value = vendorPurchaseId });
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    payments.Add(new VendorPayment
                    {
                        PaymentDate = reader.IsDBNull(0) ? string.Empty : reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                        PaymentModeName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Amount = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                        PaymentReferenceNumber = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving vendor payments: {ex.Message}");
            }

            return payments;
        }


        public VendorPaymentHistory GetInvoiceDetailsByInvoiceId(int vendorPurchaseId)
        {
            VendorPaymentHistory paymentHistory = new VendorPaymentHistory();
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
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();


                //command.CommandText = @"
                //    SELECT VP.Amount,  VP.OrderDate, VP.InvoiceNumber FROM VendorPurchase VP WHERE VP.Id = @VendorPurchaseId";


                command.CommandText = @"SELECT VP.Amount,VP.OrderDate, VP.InvoiceNumber, V.VendorName, C.CityName, VP.OrderDate
                        FROM VendorPurchase VP INNER JOIN Vendor V ON VP.VendorId = V.Id
 					    INNER JOIN City C ON V.CityId = C.Id WHERE VP.Id = @VendorPurchaseId";


                command.Parameters.Add(new SqlParameter("@VendorPurchaseId", SqlDbType.Int) { Value = vendorPurchaseId });
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    paymentHistory.PurchaseAmount = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    paymentHistory.PurchaseDate = reader.IsDBNull(1) ? string.Empty : reader.GetDateTime(1).ToString("yyyy-MM-dd");
                    paymentHistory.InvoiceId = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    paymentHistory.VendorName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                    paymentHistory.CityName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                    paymentHistory.OrderDate = reader.IsDBNull(5) ? string.Empty : reader.GetDateTime(5).ToString("yyyy-MM-dd");
                }

                paymentHistory.Payments = GetVendorPayments(vendorPurchaseId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving vendor payments: {ex.Message}");
            }

            return paymentHistory;
        }

        private List<VendorPayment> GetVendorPayments(int vendorPurchaseId)
        {
            List<VendorPayment> paymentHistory = new List<VendorPayment>();
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
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT VP.PaymentDate, VP.Amount, PT.PaymentModeName, VP.PaymentReferenceNumber FROM VendorPayment VP INNER JOIN PaymentType PT ON VP.PaymentTypeId = PT.Id WHERE VendorPurchaseId = @VendorPurchaseId";
                command.Parameters.Add(new SqlParameter("@VendorPurchaseId", SqlDbType.Int) { Value = vendorPurchaseId });
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    paymentHistory.Add(new VendorPayment
                    {
                        PaymentDate = reader.IsDBNull(0) ? string.Empty : reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                        Amount = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                        PaymentModeName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        PaymentReferenceNumber = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving vendor payments: {ex.Message}");
            }

            return paymentHistory;
        }

        public bool VendorPayment(int VendorPurchaseId, decimal Amount, int PayType, string PaymentReference, DateTime PaymentDate, bool ISPaymentDone)
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
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO VendorPayment
                    (VendorPurchaseId, Amount, PaymentTypeId, PaymentReferenceNumber, PaymentDate)
                    VALUES
                    (@VendorPurchaseId, @Amount, @PaymentTypeId, @PaymentReferenceNumber, @PaymentDate)";

                command.Parameters.Add(new SqlParameter("@VendorPurchaseId", SqlDbType.Int) { Value = VendorPurchaseId });
                command.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal) { Value = Amount });
                command.Parameters.Add(new SqlParameter("@PaymentTypeId", SqlDbType.Int) { Value = PayType });
                command.Parameters.Add(new SqlParameter("@PaymentReferenceNumber", SqlDbType.NVarChar, 200) { Value = PaymentReference ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@PaymentDate", SqlDbType.DateTime) { Value = PaymentDate });
                command.Parameters.Add(new SqlParameter("@ISPaymentDone", SqlDbType.Bit) { Value = ISPaymentDone });
                var rows = command.ExecuteNonQuery();

                if (ISPaymentDone)
                {
                    using var updateCommand = connection.CreateCommand();
                    updateCommand.CommandText = "UPDATE VendorPurchase SET ISPaymentDone = 1 WHERE Id = @VendorPurchaseId";
                    updateCommand.Parameters.Add(new SqlParameter("@VendorPurchaseId", SqlDbType.Int) { Value = VendorPurchaseId });
                    updateCommand.ExecuteNonQuery();
                }
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing vendor payment: {ex.Message}");
                return false;
            }
        }

        public List<VendorPaymentReport> GetVendorPaymentReportByVendorId(int vendorId)
        {
            var paymentReports = new List<VendorPaymentReport>();
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
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                
                command.CommandText = @"SELECT 
	                                    V.VendorName, VP.InvoiceNumber, VP.OrderDate, VP.Amount, ISNULL(VP.ISPaymentDone, 0) AS  ISPaymentDone,
	                                    VPP.Amount AS PaidAmount,VPP.PaymentDate, VPP.PaymentReferenceNumber,
                                        CASE WHEN ISNULL(VP.IsGSTBill, 0) = 1 THEN 'Bank Payment' ELSE 'UPI Payment' END AS BillPaymentMode
                                FROM VENDOR V INNER JOIN VendorPurchase VP ON V.Id = VP.VendorId 
			                                  LEFT JOIN VendorPayment VPP ON VP.Id = VPP.VendorPurchaseId
                                WHERE VP.VendorId = @VendorId ORDER BY VP.OrderDate ";

                command.Parameters.Add(new SqlParameter("@VendorId", SqlDbType.Int) { Value = vendorId });
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    paymentReports.Add(new VendorPaymentReport
                    {
                        VendorName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        InvoiceNumber = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        OrderDate = reader.IsDBNull(2) ? string.Empty : reader.GetDateTime(2).ToString("dd MMMM yyyy"),
                        PurchaseAmount = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                        ISPaymentDone = reader.IsDBNull(4) ? false : reader.GetBoolean(4),
                        PaidAmount = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                        PaymentDate = reader.IsDBNull(6) ? string.Empty : reader.GetDateTime(6).ToString("dd MMMM yyyy"),
                        PaymentReferenceNumber = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        BillPaymentMode = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                        //InvoiceNumber = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        //OrderDate = reader.IsDBNull(1) ? string.Empty : reader.GetDateTime(1).ToString("yyyy-MM-dd"),
                        //PurchaseAmount = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                        //ISPaymentDone = reader.IsDBNull(3) ? false : reader.GetBoolean(3)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving vendor payment report: {ex.Message}");
            }
            return paymentReports;
        }
    }
}
