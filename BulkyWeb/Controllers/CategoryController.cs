using Bulky.DataAccess.Resository.IRepository;
using BulkyWeb.DataAccess;
using BulkyWeb.DataAccess.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;
        public CategoryController(ICategoryRepository db)
        {
            _categoryRepo = db;
        }

        public IActionResult Index()
        {
            var ObjectCategoryList = _categoryRepo.GetAll().ToList();
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
                _categoryRepo.Add(obj);
                _categoryRepo.Save();
                TempData["success"] = "Category Created Successfully";
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

            Category categoryFromDb = _categoryRepo.Get(u => u.CategoryId == id);
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
                _categoryRepo.Update(obj);
                _categoryRepo.Save();
                TempData["success"] = "Category Updated Successfully";
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

            Category categoryFromDb = _categoryRepo.Get(u => u.CategoryId == id);
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

            Category? obj = _categoryRepo.Get(u => u.CategoryId == id);
            if (obj == null)
            {
                return NotFound();
            }
             _categoryRepo.Remove(obj);
             _categoryRepo.Save();
            TempData["success"] = "Category Removed Successfully";
            return RedirectToAction("index");

        }

    }
}
