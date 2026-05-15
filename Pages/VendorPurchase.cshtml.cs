using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SRRAMOils.Models;
using SRRAMOils.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRRAMOils.Pages
{
    public class VendorPurchaseModel : PageModel
    {
        public List<SelectListItem> VendorOptions { get; set; } = new List<SelectListItem>();


        public List<DropDownModel> VendorNames { get; set; } = new();

        #region BindProperty
        [BindProperty]
        public string VendorName { get; set; } = string.Empty;

        [BindProperty]
        public int vendorId { get; set; }


        [BindProperty]
        public string InvoiceNumber { get; set; } = string.Empty;

        [BindProperty]
        public decimal PurchaseAmount { get; set; }

        [BindProperty]
        public DateTime OrderDate { get; set; }

        [BindProperty]
        public bool IsGst { get; set; }
        #endregion

        public async Task OnGetAsync()
        {
            VendorService vs = new VendorService();

            // Await the async call and assign the result directly to VendorNames
            VendorNames = await vs.GetVendorNames();

            // Start with a placeholder "select" option
            var options = new List<SelectListItem>
            {
                new SelectListItem { Value = string.Empty, Text = "-- Select Vendor --", Selected = true }
            };

            // Map VendorNames to SelectListItems using reflection to support multiple DropDownModel shapes
            var mapped = VendorNames
                .Select(v =>
                {
                    var type = v?.GetType();
                    string value = type?.GetProperty("Value")?.GetValue(v)?.ToString()
                                   ?? type?.GetProperty("Id")?.GetValue(v)?.ToString()
                                   ?? string.Empty;
                    string text = type?.GetProperty("Text")?.GetValue(v)?.ToString()
                                  ?? type?.GetProperty("Name")?.GetValue(v)?.ToString()
                                  ?? value;
                    return new SelectListItem { Value = value, Text = text };
                })
                .ToList();

            options.AddRange(mapped);

            VendorOptions = options;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            
            var invoiceNumber = InvoiceNumber;
            var purchaseAmount = PurchaseAmount;
            var orderDate = OrderDate;
            var isGst = IsGst;

            VendorService vs = new VendorService();
           await vs.AddVendorPurchase(vendorId, InvoiceNumber, PurchaseAmount, OrderDate, 0, IsGst, false);

            return new JsonResult(new { success = true, name = "VendorName" });
        }
    }
}
