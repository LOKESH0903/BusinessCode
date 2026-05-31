using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRRAMOils.Models;
using SRRAMOils.Service;

namespace SRRAMOils.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public List<VendorPaymentReport> VendorPurchagePendingDetails { get; set; } = new List<VendorPaymentReport>();
        public IndexModel()
        {
        }

        public void OnGet()
        {
            DashboardService _dashboardService = new DashboardService();
            VendorPurchagePendingDetails = _dashboardService.GetVendorPurchasePaymentPendingDetails();
        }
    }
}
