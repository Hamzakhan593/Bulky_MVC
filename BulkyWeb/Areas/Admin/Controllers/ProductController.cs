using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using BulkyWeb.DataAccess.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _db;

        public ProductController(IUnitOfWork unitOfWork, ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var productList = _unitOfWork.ProductRepository.GetAll();
            return View(productList);
        }


        public IActionResult Upsert(int? id)
        {
            //IEnumerable<SelectListItem> CList = _db.categories
            //    .Select(c => new SelectListItem
            //    {
            //        Text = c.CategoryName,
            //        Value = c.CategoryId.ToString()
            //    });
            //ViewBag.CategoryList = CategoryList;



            ProductViewModel productVM = new()
            {
                CategoryList = _unitOfWork.CategoryRepository
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.CategoryName,
                    Value = u.CategoryId.ToString()
                }),
                Product = new Product()
            };


            if (id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.ProductRepository
                    .Get(u => u.CategoryId == id);
                return View(productVM);

            }
        }

            [HttpPost]
            public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
            {
                if (ModelState.IsValid)
                {

                if (file != null && file.Length > 0)
                {
                    // Step 1: Get the physical path to the wwwroot folder
                    string wwwRootPath = _webHostEnvironment.WebRootPath;

                    // Step 2: Define the folder where images will be saved (e.g., wwwroot/images/product)
                    string uploadFolder = Path.Combine(wwwRootPath, "images", "product");

                    // Step 3: If the folder doesn't exist, create it
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // Step 4: Generate a unique file name using GUID to prevent name clashes
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    // Step 5: Combine folder path + file name to get the full path
                    string fullPath = Path.Combine(uploadFolder, fileName);

                    // Step 6: Save the uploaded file to disk using a FileStream
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    // Step 7: Set the relative path (used in HTML img tag or saving to DB)
                    obj.Product.ImageURL = @"\images\product\" + fileName;
                }


                    _unitOfWork.ProductRepository.Add(obj.Product);
                    _unitOfWork.IUWSave();
                    TempData["success"] = "Category Created Successfully";
                    return RedirectToAction("Index");
                }

                //    else {
                //         obj.CategoryList = _unitOfWork.CategoryRepository
                //            .GetAll().Select( u=> new SelectListItem
                //            {
                //                Text = u.CategoryName,
                //                Value = u.CategoryId.ToString()
                //            });
                //              return View(obj);
                //            }

                return View();
            }
        

        

        //public IActionResult Edit(int? id)
        //{
        //    Product ProductObj = _unitOfWork.ProductRepository.Get(u=>u.ProductId == id);
        //    return View(ProductObj);
        //}
        //[HttpPost]
        //public IActionResult Edit(Product obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.ProductRepository.Update(obj);
        //        _unitOfWork.IUWSave();
        //        TempData["success"] = "Category Updated Successfully";
        //        return RedirectToAction("Index");
        //    }

        //    return View();
        //}



        public IActionResult Delete(int? id)
        {
            Product ProductObj = _unitOfWork.ProductRepository.Get(u => u.ProductId == id);
            return View(ProductObj);
        }


        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Product obj = _unitOfWork.ProductRepository.Get(u=>u.ProductId==id);    

                _unitOfWork.ProductRepository.Remove(obj);
                _unitOfWork.IUWSave();
            TempData["success"] = "Category Deleted Successfully";
            return RedirectToAction("Index");
        }
    }

}
