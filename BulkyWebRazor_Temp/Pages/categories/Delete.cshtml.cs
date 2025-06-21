using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public Category deleteCategory { get; set; }

        public DeleteModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult OnGet(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            deleteCategory = _db.categories.FirstOrDefault(u=>u.CategoryId==id);

            if (deleteCategory == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost(int? id)
        {
            deleteCategory = _db.categories.Find(id);

            _db.categories.Remove(deleteCategory);
            _db.SaveChanges();
            TempData["success"] = "Category Removed Successfully";
            return RedirectToPage("index");
        }
    }
}
