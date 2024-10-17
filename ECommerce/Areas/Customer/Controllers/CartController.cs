using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using ECommerce.Models.ViewModel;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using System.Security.Claims;
using Session = Stripe.Checkout.Session;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using SessionService = Stripe.Checkout.SessionService;

namespace ECommerce.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        public readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var userid = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userid,
                includeProperties: "Product"),
                OrderHeader=new()
            };
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
               
            }
            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var userid = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userid,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser= unitOfWork.ApplicationUser.Get(u=>u.Id == userid);
            if (ShoppingCartVM.OrderHeader.ApplicationUser != null)
            {
                ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
                ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                ShoppingCartVM.OrderHeader.state = ShoppingCartVM.OrderHeader.ApplicationUser.State;
                ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            }

           

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsidentity = (ClaimsIdentity)User.Identity;
			var userid = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userid,
                includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId= userid;
			ApplicationUser applicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userid);
			
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

			}

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //regular customer account 
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                ShoppingCartVM.OrderHeader.PaymentStatus=SD.PaymentStatusPending;
            }
            else
            {
				//company user
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
			}
            unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            unitOfWork.Save();
            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Count = cart.Count,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id
                };
                unitOfWork.OrderDetail.Add(orderDetail);
                unitOfWork.Save();
            }
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                //regular customer account 
                //strip logic
                var domine = "https://localhost:7020/";

                var options = new SessionCreateOptions()
                {
                    SuccessUrl = domine + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domine +"customer/cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach(var item in ShoppingCartVM.ShoppingCartList)
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
                        Quantity=item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();

				Session session = service.Create(options);

                unitOfWork.OrderHeader.UpdatePaymentStripId(ShoppingCartVM.OrderHeader.Id,session.Id,session.PaymentIntentId);
                unitOfWork.Save();

                Response.Headers.Add("Location",session.Url);
                return new StatusCodeResult(303);
			}
			return RedirectToAction(nameof(OrderConfirmation), new {id=ShoppingCartVM.OrderHeader.Id});
		}
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader= unitOfWork.OrderHeader.Get(u=>u.Id==id,includeProperties:"ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service=new SessionService();
                Session session=service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdatePaymentStripId(id,session.Id,session.PaymentIntentId);
                    unitOfWork.OrderHeader.UpdateStatus(id,SD.StatusApproved,SD.PaymentStatusApproved);
                    unitOfWork.Save();

                }
                List<ShoppingCart> shoppingCarts=unitOfWork.ShoppingCart
                    .GetAll(u=>u.ApplicationUserId==orderHeader.ApplicationUserId).ToList();
                unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                unitOfWork.Save();
            }
            HttpContext.Session.Clear();
            return View(id);
        }
		public IActionResult plus(int id)
        {
            var cartfromDb = unitOfWork.ShoppingCart.Get(u => u.Id == id);
            cartfromDb.Count += 1;
            unitOfWork.ShoppingCart.Update(cartfromDb);
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int id)
        {
            var cartfromDb = unitOfWork.ShoppingCart.Get(u => u.Id == id,tracked:true);
            if (cartfromDb.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, unitOfWork.ShoppingCart.
               GetAll(u => u.ApplicationUserId == cartfromDb.ApplicationUserId).Count()-1);
                unitOfWork.ShoppingCart.Remove(cartfromDb);

            }
            else
            {
                cartfromDb.Count -= 1;
                
                unitOfWork.ShoppingCart.Update(cartfromDb);
            }

            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult remove(int id)
        {
            var cartfromDb = unitOfWork.ShoppingCart.Get(u => u.Id == id,tracked:true);
            HttpContext.Session.SetInt32(SD.SessionCart,unitOfWork.ShoppingCart.
                GetAll(u=>u.ApplicationUserId==cartfromDb.ApplicationUserId).Count()-1);
            unitOfWork.ShoppingCart.Remove(cartfromDb);
            unitOfWork.Save();
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
