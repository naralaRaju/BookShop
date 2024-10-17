using ECommerce.DataAccess.Repository;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using ECommerce.Models.ViewModel;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;
using Session = Stripe.Checkout.Session;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using SessionService = Stripe.Checkout.SessionService;

namespace ECommerce.Areas.Admin.Controllers
{
	[Area("admin")]
    [Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public OrderVM orderVM {  get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
		{
			return View();
		}
        public IActionResult Details(int OrderId)
        {
            orderVM = new()
            {
                OrderHeader = unitOfWork.OrderHeader.Get(u => u.Id == OrderId, includeProperties: "ApplicationUser"),
                orderDetail = unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == OrderId, includeProperties: "Product")
            };

            return View(orderVM);
        }
        [HttpPost]  
        [Authorize(Roles = SD.Role_Admin+","+ SD.Role_Employee)]
        public IActionResult UpdateOrderDetails()
        {
            var orderdetailsfromdb = unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderdetailsfromdb.Name=orderVM.OrderHeader.Name;
            orderdetailsfromdb.PhoneNumber=orderVM.OrderHeader.PhoneNumber;
            orderdetailsfromdb.StreetAddress=orderVM.OrderHeader.StreetAddress;
            orderdetailsfromdb.state=orderVM.OrderHeader.state;
            orderdetailsfromdb.City=orderVM.OrderHeader.City;
            orderdetailsfromdb.PostalCode=orderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderdetailsfromdb.Carrier=orderVM.OrderHeader.Carrier;
            }
            if(!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderdetailsfromdb.TrackingNumber=orderVM.OrderHeader.TrackingNumber;
            }
            unitOfWork.OrderHeader.Update(orderdetailsfromdb);
            unitOfWork.Save();
            TempData["success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { OrderId=orderdetailsfromdb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id,SD.StatusInProgress);
            unitOfWork.Save();
            TempData["success"] = "Order Details Updated successfully";
            return RedirectToAction(nameof(Details), new { OrderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderheader = unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderheader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderheader.Carrier = orderVM.OrderHeader.Carrier;
            orderheader.OrderStatus = SD.StatusShipped;
            orderheader.ShippingDate = DateTime.Now;
            if (orderheader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderheader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            unitOfWork.OrderHeader.Update(orderheader);
            unitOfWork.Save();
            TempData["success"] = "Order shipped successfully";
            return RedirectToAction(nameof(Details), new { OrderId = orderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus==SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefund);

            }
            else
            {
                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            unitOfWork.Save();
            TempData["success"] = "Order cancelled successfully";
            return RedirectToAction(nameof(Details), new { OrderId = orderVM.OrderHeader.Id });
        }
        [ActionName("Details")]
        [HttpPost]
        public IActionResult Delails_Pay_Now()
        {
            orderVM.OrderHeader = unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderVM.orderDetail = unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id, includeProperties: "Product");
            //regular customer account 
            //strip logic
            var domine = "https://localhost:7020/";

            var options = new SessionCreateOptions()
            {
                SuccessUrl = domine + $"admin/order/PaymentConfirmation?OrderHeaderId={orderVM.OrderHeader.Id}",
                CancelUrl = domine + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in orderVM.orderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 1000),//$20.50==>2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();

            Session session = service.Create(options);

            unitOfWork.OrderHeader.UpdatePaymentStripId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation(int OrderHeaderId)
        {
            OrderHeader orderHeader = unitOfWork.OrderHeader.Get(u => u.Id == OrderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdatePaymentStripId(OrderHeaderId, session.Id, session.PaymentIntentId);
                    unitOfWork.OrderHeader.UpdateStatus(OrderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    unitOfWork.Save();

                }
                List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart
                    .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
                unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                unitOfWork.Save();
            }

            return View(OrderHeaderId);
        }

        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status)
		{
            IEnumerable<OrderHeader> objListOrderHeader;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objListOrderHeader= unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsidentity = (ClaimsIdentity)User.Identity;
                var userid = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objListOrderHeader = unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == userid,includeProperties:"ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objListOrderHeader = objListOrderHeader.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objListOrderHeader = objListOrderHeader.Where(u => u.OrderStatus == SD.StatusInProgress);
                    break;
                case "approved":
                    objListOrderHeader = objListOrderHeader.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                case "completed":
                    objListOrderHeader = objListOrderHeader.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                default:
                   
                    break;
            }
            return Json(new { data = objListOrderHeader });
		}
		

		#endregion
	}
}
