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
        /*
         PSEUDOCODE / PLAN (detailed):
         1. Add a public property `Organizations` of type `List<Dictionary<string, object>>`
            - This will hold each row as a dictionary (column name -> value) and will be
              accessible from the Razor view.
         2. In `OnGet`:
            - Instantiate `OrganizationService` and call the existing `GetOrgianizations` method.
            - If the returned `DataTable` is not null and has rows:
                - For each `DataRow` in `result.Rows`:
                    - Create a `Dictionary<string, object>` for the row.
                    - For each `DataColumn` in `result.Columns`:
                        - Read the cell value, convert `DBNull` to `string.Empty`.
                        - Assign to the dictionary with the column name as key.
                    - Add the dictionary to `Organizations`.
         3. Keep `OnPost` behavior unchanged (it binds `OrganizationName`), but ensure `OnGet`
            fills `Organizations` so the Razor page can render the data after a GET.
         4. Use `Organizations` in the Razor view to iterate and render rows/columns.
        */

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
