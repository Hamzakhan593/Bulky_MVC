using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var ObjectCategoryList = _db.categories.ToList();
            return View(ObjectCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if(obj.CategoryName == obj.CategoryDisplayOrder.ToString())
            {
                ModelState.AddModelError("CategoryName", "The display order cannot exactly match the Category Name");
            }
            if (obj.CategoryName.ToLower() == "text")
            {
                ModelState.AddModelError("", "text is invalid");
            }
            if (ModelState.IsValid)
            {
                _db.categories.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("index");
            }
            return View();
        }
    }
}
