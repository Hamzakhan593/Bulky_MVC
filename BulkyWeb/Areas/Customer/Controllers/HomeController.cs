using Bulky.DataAccess.Resository;
using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // showed element in the cart
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var clam = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (clam != null)
            {
                HttpContext.Session.SetInt32(Static_Details.SessionCart,
                _unitOfWork.ShoppingCartRepository.GetAll(x => x.ApplicationUserId == clam.Value).Count());
            }
            IEnumerable<Product> productList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.ProductRepository.Get(u => u.ProductId == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };

            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userId = _userManager.GetUserId(User);
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.Get(x => x.ApplicationUserId == userId
            && x.ProductId == shoppingCart.ProductId);

            if (cartFromDb == null)
            {
                // cart not exist
                _unitOfWork.ShoppingCartRepository.Add(shoppingCart);
                _unitOfWork.IUWSave();
                HttpContext.Session.SetInt32(Static_Details.SessionCart,
                    _unitOfWork.ShoppingCartRepository.GetAll(x => x.ApplicationUserId == userId).Count());
            }
            else
            {
                //shopping cart exist
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCartRepository.Update(cartFromDb);
                _unitOfWork.IUWSave();
                
            }
            TempData["success"] = "Cart updated successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
