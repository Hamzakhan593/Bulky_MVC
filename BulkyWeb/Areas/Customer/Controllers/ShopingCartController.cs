using Bulky.DataAccess.Resository;
using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class ShopingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        [BindProperty]
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
                includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }


            return View(ShoppingCartVM);
        }



        public IActionResult Summary()
        {
            var userId = _userManager.GetUserId(User);

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.applicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.applicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.applicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.applicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.applicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.applicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.applicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }


        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var userId = _userManager.GetUserId(User);

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                ShoppingCartVM.OrderHeader.PaymentStatus = Static_Details.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = Static_Details.StatusPending;
            }
            else
            {
                // it is a comppany user
                ShoppingCartVM.OrderHeader.PaymentStatus = Static_Details.PaymentStatusDelayedPyement;
                ShoppingCartVM.OrderHeader.OrderStatus = Static_Details.StatusApproved;
            }
            _unitOfWork.OrderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.IUWSave();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.OrderHeaderId,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _unitOfWork.OrderDetailRepository.Add(orderDetail);
                _unitOfWork.IUWSave();
            }


            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                // stripe logic
                StripeConfiguration.ApiKey = "sk_test_51RmH2AIAel1J6g0wfBRX2jWqbIqoruunjcZAFUNLYWivAYrAm4wK70cGz5qFGH2ypKyMXUSJpGXAprAePD6xOsrg00BCtJ7l3b";

                var domain = "https://localhost:7267";  // No trailing slash
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"/customer/shopingcart/orderconfirmation?id={ShoppingCartVM.OrderHeader.OrderHeaderId}",
                    CancelUrl = domain + "/customer/shopingcart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };


                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
                _unitOfWork.IUWSave();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.OrderHeaderId });
        }


        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.Get(
                u => u.OrderHeaderId == id,
                includeProperties: "applicationUser"
            );

            if (orderHeader == null)
            {
                return NotFound();
            }

            if (orderHeader.PaymentStatus != Static_Details.PaymentStatusDelayedPyement)
            {
                // This is a customer order (not a company order)
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(
                        id, 
                        session.Id,
                        session.PaymentIntentId
                    );
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(
                        id,
                        Static_Details.StatusApproved,
                        Static_Details.PaymentStatusApproved
                    );
                    _unitOfWork.IUWSave();
                }
            }

            // Clear the shopping cart
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCartRepository
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId)
                .ToList();

            _unitOfWork.ShoppingCartRepository.DeleteRange(shoppingCarts);
            _unitOfWork.IUWSave();

            return View(id);
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
