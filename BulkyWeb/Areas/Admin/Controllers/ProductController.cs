using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var productList = _unitOfWork.ProductRepository.GetAll();
            return View(productList); 
        }


        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid) 
            { 
                _unitOfWork.ProductRepository.Add(product);
                _unitOfWork.IUWSave();
                return RedirectToAction("Index");
            }

            return View();
        }


        public IActionResult Edit(int? id)
        {
            Product ProductObj = _unitOfWork.ProductRepository.Get(u=>u.ProductId == id);
            return View(ProductObj);
        }
        [HttpPost]
        public IActionResult Edit(Product obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductRepository.Update(obj);
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


        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Product obj = _unitOfWork.ProductRepository.Get(u=>u.ProductId==id);    

                _unitOfWork.ProductRepository.Remove(obj);
                _unitOfWork.IUWSave();
                return RedirectToAction("Index");
        }
    }

}
