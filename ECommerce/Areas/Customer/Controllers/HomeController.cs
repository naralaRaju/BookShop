using ECommerce.DataAccess.Repository;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ECommerce.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var claims = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value).Count());
            }
            IEnumerable<Product> productList = unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }
        public IActionResult Details(int productid)
        {
            ShoppingCart shoppingcart = new()
            {
               Product= unitOfWork.Product.Get(u => u.Id == productid, includeProperties: "Category"),
               Count=1,
               ProductId=productid
            };
            
            return View(shoppingcart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsidentity=(ClaimsIdentity)User.Identity;
            var userid = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId= userid;
            ShoppingCart shoppingCartfromdb=unitOfWork.ShoppingCart.Get(u=>u.ApplicationUserId==userid && u.ProductId==shoppingCart.ProductId);
            if(shoppingCartfromdb!=null)
            {
                //update
                shoppingCartfromdb.Count += shoppingCart.Count;
                unitOfWork.ShoppingCart.Update(shoppingCartfromdb);
                unitOfWork.Save();
                TempData["success"] = "Cart updated successfully";
            }
            else
            {
                //addd

                unitOfWork.ShoppingCart.Add(shoppingCart);
                unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userid).Count());
                TempData["success"] = "Cart created successfully";
            }
           
           
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
