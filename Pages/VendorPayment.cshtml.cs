using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SRRAMOils.Models;
using SRRAMOils.Service;

namespace SRRAMOils.Pages
{
    public class VendorPaymentModel : PageModel
    {
        public List<SelectListItem> VendorOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> InvoiceNumbers { get; set; } = new List<SelectListItem>();


        public List<DropDownModel> VendorNames { get; set; } = new();

        public List<DropDownModel> InvoiceNumberList { get; set; } = new();

        [BindProperty]
        public string VendorName { get; set; } = string.Empty;

        [BindProperty]
        public int VendorId { get; set; }

        [BindProperty]
        public string InvoiceNumber { get; set; } = string.Empty;


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

        public JsonResult OnGetInvoiceDetailsByInvoiceId(int vendorPurchaseId)
        {
            VendorService vs = new VendorService();
            var vendorPaymentHistory = vs.GetInvoiceDetailsByInvoiceId(vendorPurchaseId);
            string json = JsonConvert.SerializeObject(vendorPaymentHistory);
            var data = new { data = json };
            return new JsonResult(data);
        }
    }
}
