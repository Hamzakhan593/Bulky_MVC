using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Static_Details.Role_Admin + "," +
                    Static_Details.Role_Employee + "," +
                    Static_Details.Role_Customer + "," +
                    Static_Details.Role_Company)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
         _unitOfWork = unitOfWork;
         _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }



        public IActionResult Details(int id)  
        {
             OrderVM = new OrderVM
            {
                orderHeader = _unitOfWork.OrderHeaderRepository.Get(o => o.OrderHeaderId == id),
                orderDetails = _unitOfWork.OrderDetailRepository.GetAll(od => od.OrderHeaderId == id, includeProperties: "product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = Static_Details.Role_Admin + "," +  Static_Details.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.Get(u => u. OrderHeaderId== OrderVM.orderHeader.OrderHeaderId);

            orderHeaderFromDb.Name = OrderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.orderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.orderHeader.City;
            orderHeaderFromDb.State = OrderVM.orderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.orderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderVM.orderHeader.carrier))
            {
                orderHeaderFromDb.carrier = OrderVM.orderHeader.carrier;
            }

            if (!string.IsNullOrEmpty(OrderVM.orderHeader.TrackingNumber))
            {
                orderHeaderFromDb.carrier = OrderVM.orderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.IUWSave();
            TempData["sucesss"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new {id = orderHeaderFromDb.OrderHeaderId});
        }




        [HttpPost]
        [Authorize(Roles = Static_Details.Role_Admin + "," + Static_Details.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.orderHeader.OrderHeaderId, Static_Details.StatusInProcess);
            _unitOfWork.IUWSave();
            TempData["success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new {id = OrderVM.orderHeader.OrderHeaderId});
        }



        [HttpPost]
        [Authorize(Roles = Static_Details.Role_Admin + "," + Static_Details.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var ordeHeader = _unitOfWork.OrderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.orderHeader.OrderHeaderId);
            ordeHeader.TrackingNumber = OrderVM.orderHeader.TrackingNumber;
            ordeHeader.carrier = OrderVM.orderHeader.carrier;
            ordeHeader.OrderStatus = Static_Details.StatusShipped;
            ordeHeader.ShippingDate = DateTime.Now;

            if(ordeHeader.PaymentStatus == Static_Details.PaymentStatusDelayedPyement)
            {
                ordeHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeaderRepository.Update(ordeHeader);

            _unitOfWork.IUWSave();
            TempData["success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { id = OrderVM.orderHeader.OrderHeaderId });
        }




        [HttpPost]
        [Authorize(Roles = Static_Details.Role_Admin + "," + Static_Details.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var ordeHeader = _unitOfWork.OrderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.orderHeader.OrderHeaderId);

            if(ordeHeader.PaymentStatus == Static_Details.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = ordeHeader.PaymentIntendId,
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStatus(ordeHeader.OrderHeaderId, Static_Details.StatusCancelled, Static_Details.StatusRefund);

            }
            else
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(ordeHeader.OrderHeaderId, Static_Details.StatusCancelled, Static_Details.StatusCancelled);

            }


            _unitOfWork.IUWSave();
            TempData["success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { id = OrderVM.orderHeader.OrderHeaderId });
        

        }





        [ActionName(nameof(Details))]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVM.orderHeader = _unitOfWork.OrderHeaderRepository
                .Get(u => u.OrderHeaderId == OrderVM.orderHeader.OrderHeaderId, includeProperties: "applicationUser");

            OrderVM.orderDetails = _unitOfWork.OrderDetailRepository
                .GetAll(u => u.OrderHeaderId == OrderVM.orderHeader.OrderHeaderId, includeProperties: "Product");

            // stripe logic
            StripeConfiguration.ApiKey = "sk_test_51RmH2AIAel1J6g0wfBRX2jWqbIqoruunjcZAFUNLYWivAYrAm4wK70cGz5qFGH2ypKyMXUSJpGXAprAePD6xOsrg00BCtJ7l3b";

            var domain = "https://localhost:7267";  // No trailing slash
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"/admin/order/PaymentConfirmation?orderHeaderid={OrderVM.orderHeader.OrderHeaderId}",
                CancelUrl = domain + $"/admin/order/details?orderId = {OrderVM.orderHeader.OrderHeaderId}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };


            foreach (var item in OrderVM.orderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(OrderVM.orderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
            _unitOfWork.IUWSave();
            Response.Headers.Add("Location", session.Url);


            return new StatusCodeResult(303);
        }







        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.Get(
                u => u.OrderHeaderId == orderHeaderid );

            if (orderHeader == null)
            {
                return NotFound();
            }

            if (orderHeader.PaymentStatus == Static_Details.PaymentStatusDelayedPyement)
            {
                // order by companny
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(
                        orderHeaderid,
                        session.Id,
                        session.PaymentIntentId
                    );
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(
                        orderHeaderid,
                        orderHeader.OrderStatus,
                        Static_Details.PaymentStatusApproved
                    );
                    _unitOfWork.IUWSave();
                }
            }

            return View(orderHeaderid);
        }





        #region API CALLS

        [HttpGet]
        public IActionResult GetAllOrders(string status)
        {
            IEnumerable<OrderHeader> ObjOrderHeaders;

            if(User.IsInRole(Static_Details.Role_Admin) || User.IsInRole(Static_Details.Role_Employee))
            {
                ObjOrderHeaders = _unitOfWork.OrderHeaderRepository
               .GetAll(includeProperties: "applicationUser")
               .ToList();
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                ObjOrderHeaders = _unitOfWork.OrderHeaderRepository.GetAll(u=>u.ApplicationUserId == userId, includeProperties: "applicationUser");
            }


                switch (status?.ToLower())
                {
                    case "pending":
                    ObjOrderHeaders = ObjOrderHeaders.Where(u => u.PaymentStatus == Static_Details.PaymentStatusDelayedPyement).ToList();
                        break;
                    case "inprocess":
                    ObjOrderHeaders = ObjOrderHeaders.Where(u => u.OrderStatus == Static_Details.StatusInProcess).ToList();
                        break;
                    case "completed":
                    ObjOrderHeaders = ObjOrderHeaders.Where(u => u.OrderStatus == Static_Details.StatusShipped).ToList();
                        break;
                    case "approved":
                    ObjOrderHeaders = ObjOrderHeaders.Where(u => u.OrderStatus == Static_Details.StatusApproved).ToList();
                        break;
                }

            var result = ObjOrderHeaders.Select(o => new
            {
                id = o.OrderHeaderId,
                name = o.Name,
                phoneNumber = o.PhoneNumber,
                email = o.applicationUser != null ? o.applicationUser.Email : "",
                orderStatus = o.OrderStatus,
                orderTotal = o.OrderTotal
            });

            return Json(new { data = result });
        }


        #endregion
    }
}
