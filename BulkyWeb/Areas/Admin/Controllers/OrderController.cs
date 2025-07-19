using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
         _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<OrderHeader> ObjorderHeaders = _unitOfWork.OrderHeaderRepository
                .GetAll(includeProperties: "applicationUser").ToList();
            return Json(new { data = ObjorderHeaders });
        }

        #endregion
    }
}
