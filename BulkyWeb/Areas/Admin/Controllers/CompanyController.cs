using Bulky.DataAccess.Resository;
using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using BulkyWeb.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")] 
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var companyList = _unitOfWork.CompanyRepository.GetAll();
            return View(companyList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Company companydb)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CompanyRepository.Add(companydb);
                _unitOfWork.IUWSave();
                TempData["success"] = "Company Created Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int id)
        {
            if (id == 0 || id == null)
            { 
                return NotFound();
            }

            var company = _unitOfWork.CompanyRepository.Get(u => u.companyId == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpPost]
        public IActionResult Edit(Company company)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CompanyRepository.update(company);
                _unitOfWork.IUWSave();
                TempData["success"] = "Company Update Successflly";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var deleteCompany = _unitOfWork.CompanyRepository.Get(x => x.companyId == id);
            if (deleteCompany == null)
            {
                return NotFound();
            }

            return View(deleteCompany);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {

            var deletecompany = _unitOfWork.CompanyRepository.Get(x => x.companyId == id);

            if (deletecompany == null)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                _unitOfWork.CompanyRepository.Remove(deletecompany);
                _unitOfWork.IUWSave();
                TempData["success"] = "Company Deleted Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

    }
}
