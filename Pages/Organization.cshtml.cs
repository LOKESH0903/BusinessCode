using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRRAMOils.Models;
using SRRAMOils.Service;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SRRAMOils.Pages
{
    public class OrganizationModel : PageModel
    {
      

        [BindProperty]
        public string OrganizationName { get; set; } = string.Empty;

        // Expose data to the Razor view: a list of rows, each row is a dictionary
        public List<Dictionary<string, object>> Organizations { get; set; } = new();

        public async Task OnGetAsync()
        {
            DataTable result = new DataTable();
            OrganizationService Org = new OrganizationService();
            result = await Org.GetOrgianizations();

            Organizations.Clear();

            if (result != null && result.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    var dict = new Dictionary<string, object>();

                    foreach (DataColumn col in result.Columns)
                    {
                        object cell = row[col];
                        // Convert DBNull to empty string to avoid issues in the view
                        dict[col.ColumnName] = cell is DBNull ? string.Empty : cell!;
                    }

                    Organizations.Add(dict);
                }
            }

            Results.Ok(Organizations); // Return an empty list or modify as needed
        }

        public void OnPost()
        {
            var name = OrganizationName;
            // Existing behavior preserved; saving logic can be added here if needed.
        }
    }
}
