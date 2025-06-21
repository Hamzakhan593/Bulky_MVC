using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.categories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public Category  item { get; set; }
        public EditModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            item = _db.categories.FirstOrDefault(u => u.CategoryId == id);
            if (item == null)
            {
                return NotFound();
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            _db.categories.Update(item);
            _db.SaveChanges();
            TempData["success"] = "Category Updated Successfully";
            return RedirectToPage("index");
        }
    }
}
