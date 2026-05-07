namespace SRRAMOils.Models
{
    /// <summary>
    /// Represents a vendor, including identification, name, GST number, bank details, and active status.
    /// </summary>
    public class Vendor
    {
        public int Id { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string VendorGST { get; set; } = string.Empty;
        public string VendorBankDetails { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
