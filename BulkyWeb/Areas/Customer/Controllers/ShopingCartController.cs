using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class ShopingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public ShopingCartController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product")
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += cart.Price * cart.Count;
            }


            return View(ShoppingCartVM);
        }



        public IActionResult Summary()
        {
            return View();
        }


        public IActionResult Plus(int cartId)  
        {
            var cartItem = _unitOfWork.ShoppingCartRepository.Get(u => u.ShopingCartId == cartId);
            cartItem.Count += 1;
            _unitOfWork.ShoppingCartRepository.Update(cartItem);
            _unitOfWork.IUWSave();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartid)  
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.Get(u => u.ShopingCartId == cartid); 

            if (cartFromDb == null)
            {
                return NotFound(); 
            }

            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCartRepository.Update(cartFromDb);
            }

            _unitOfWork.IUWSave();  
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartid)  
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.Get(u => u.ShopingCartId == cartid); 

            if (cartFromDb == null)
            {
                return NotFound();  
            }

            _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.IUWSave();  
            return RedirectToAction(nameof(Index));
        }


        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }

    }
}
