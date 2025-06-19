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
            if (obj.CategoryName == obj.CategoryDisplayOrder.ToString())
            {
                ModelState.AddModelError("CategoryName", "The display order cannot exactly match the Category Name");
            }

            //if (obj.CategoryName.ToLower() == "text")
            //{
            //    ModelState.AddModelError("", "text is invalid");
            //}


            if (ModelState.IsValid)
            {
                _db.categories.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("index");
            }
            return View();
        }




        //Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category categoryFromDb = _db.categories.FirstOrDefault(u => u.CategoryId == id);
            //Category categoryFromDb = _db.categories.Find(id);
            //Category categoryFromDb = _db.categories.where(u=>u.categoryId == id).firstordefault;

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            //if (obj.CategoryName == obj.CategoryDisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("CategoryName", "The display order cannot exactly match the Category Name");
            //}

            //if (obj.CategoryName.ToLower() == "text")
            //{
            //    ModelState.AddModelError("", "text is invalid");
            //}


            if (ModelState.IsValid)
            {
                _db.categories.Update(obj);
                _db.SaveChanges();
                return RedirectToAction("index");
            }
            return View();
        }



        //Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category categoryFromDb = _db.categories.FirstOrDefault(u => u.CategoryId == id);
            //Category categoryFromDb = _db.categories.Find(id);
            //Category categoryFromDb = _db.categories.where(u=>u.categoryId == id).firstordefault;

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            //if (obj.CategoryName == obj.CategoryDisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("CategoryName", "The display order cannot exactly match the Category Name");
            //}

            //if (obj.CategoryName.ToLower() == "text")
            //{
            //    ModelState.AddModelError("", "text is invalid");
            //}

            Category? obj =  _db.categories.Find(id);
            if(obj == null)
            {
                return NotFound();
            }
             _db.categories.Remove(obj);
             _db.SaveChanges();
             return RedirectToAction("index");

        }

    }
}
