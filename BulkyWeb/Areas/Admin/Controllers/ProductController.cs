using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Bulky.Utility;
using BulkyWeb.DataAccess.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Static_Details.Role_Admin)]
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
            var productList = _unitOfWork.ProductRepository.GetAll(
                includeProperties: "Category");
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
                    .Get(u => u.ProductId == id);
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


                    // updating existing image
                    if (!string.IsNullOrEmpty(obj.Product.ImageURL))
                    {
                        //delete old image
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageURL.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Step 6: Save the uploaded file to disk using a FileStream
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    // Step 7: Set the relative path (used in HTML img tag or saving to DB)
                    obj.Product.ImageURL = @"\images\product\" + fileName;
                }

                if (obj.Product.ProductId == 0)
                {
                    _unitOfWork.ProductRepository.Add(obj.Product);
                    TempData["success"] = "Category Created Successfully";
                }
                else
                {
                    _unitOfWork.ProductRepository.Update(obj.Product);
                    TempData["success"] = "Category updated Successfully";

                }

                _unitOfWork.IUWSave();
                return RedirectToAction("Index");
            }
            return View();
            }
        

        public IActionResult Delete(int? id)
        {
            Product ProductObj = _unitOfWork.ProductRepository.Get(u => u.ProductId == id);
            return View(ProductObj);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var product = _unitOfWork.ProductRepository.Get(u => u.ProductId == id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                // Delete image if exists
                if (!string.IsNullOrEmpty(product.ImageURL))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageURL.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductRepository.Remove(product);
                _unitOfWork.IUWSave();

                return Json(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.ProductRepository.GetAll(
              includeProperties: "Category").ToList();
            return Json(new {data = objProductList});
        }
        #endregion
    }

}
