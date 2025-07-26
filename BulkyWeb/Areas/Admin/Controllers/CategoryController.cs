using Bulky.DataAccess.Resository;
using Bulky.DataAccess.Resository.IRepository;
using Bulky.Utility;
using BulkyWeb.DataAccess;
using BulkyWeb.DataAccess.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Static_Details.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _UnitofWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _UnitofWork = unitOfWork;
        } 

        public IActionResult Index()
        {
            var ObjectCategoryList = _UnitofWork.CategoryRepository.GetAll().ToList();
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
                _UnitofWork.CategoryRepository.Add(obj);
                _UnitofWork.IUWSave();
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

            Category categoryFromDb = _UnitofWork.CategoryRepository.Get(u => u.CategoryId == id);
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
            if (ModelState.IsValid)
            {
                _UnitofWork.CategoryRepository.Update(obj);
                _UnitofWork.IUWSave();
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

            Category categoryFromDb = _UnitofWork.CategoryRepository.Get(u => u.CategoryId == id);
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
            Category? obj = _UnitofWork.CategoryRepository.Get(u => u.CategoryId == id);
            if (obj == null)
            {
                return NotFound();
            }
             _UnitofWork.CategoryRepository.Remove(obj);
             _UnitofWork.IUWSave();
            TempData["success"] = "Category Removed Successfully";
            return RedirectToAction("index");

        }

    }
}
