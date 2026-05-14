using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRRAMOils.Service;
using System.Threading.Tasks;

namespace SRRAMOils.Pages
{
    public class VendorModel : PageModel
    {
        [BindProperty]
        public string VendorName { get; set; } = string.Empty;

        [BindProperty]
        public string VendorGST { get; set; } = string.Empty;

        [BindProperty]
        public string VendorPhoneNumber { get; set; } = string.Empty;

        [BindProperty]
        public string VendorBankAccountNumber { get; set; } = string.Empty;

        [BindProperty]
        public string VendorBankName { get; set; } = string.Empty;

        [BindProperty]
        public string VendorBankIFSCCode { get; set; } = string.Empty;

        [BindProperty]
        public string VendorBankBranch { get; set; } = string.Empty;

        [BindProperty]
        public string VendorBankDetails { get; set; } = string.Empty;

        [BindProperty]
        public string VendorAddress { get; set; } = string.Empty;

        [BindProperty]
        public int CityId { get; set; } = 0;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        { 
            var name = VendorName;
            var gst = VendorGST;
            var phoneNumber = VendorPhoneNumber;
            var bankAccountNumber = VendorBankAccountNumber;
            var bankName = VendorBankName;
            var bankIFSCCode = VendorBankIFSCCode;
            var bankBranch = VendorBankBranch;
            var bankDetails = VendorBankDetails;
            var address = VendorAddress;
            var cityId = CityId;

            VendorService VS = new VendorService();

            if (!await VS.CheckVendor(name))
            {
                // Vendor already exists, handle accordingly (e.g., show an error message)
            }
            else
            {
                return BadRequest(new { success = false, error = "Organization Name already available." });
            }

            return new JsonResult(new { success = true, name = VendorName });
        }
    }
}
