using ECommerce.DataAccess;
using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using ECommerce.Models.ViewModel;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Collections.Generic;
namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;
        public ProductController(IUnitOfWork applicationDbContext, IWebHostEnvironment webHostEnvironment)
        {
            unitOfWork = applicationDbContext;
            this.webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> objListCategory = unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return View(objListCategory);
        }

        public async Task<IActionResult> UpSert(int? id)
        {


            //ViewBag.CategoryList=categoryList;

            ProductVM productVM = new()
            {
                CategoryList = unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.id.ToString()
                }),
                Product = new Product()
            };
            if(id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product=unitOfWork.Product.Get(u=>u.Id==id);
                return View(productVM);
            }
            
        }
        [HttpPost]
        public async Task<IActionResult> UpSert(ProductVM productVM,IFormFile? file)
        {
            //if (obj.Name == obj.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("name", "Display order and name shold not be same");
            //}
            

            if (ModelState.IsValid)
            {
                string rootpath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productpath = Path.Combine(rootpath, @"Images\Product");
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //delete old one
                        var oldimgpath = Path.Combine(rootpath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldimgpath))
                        {
                            System.IO.File.Delete(oldimgpath);
                        }
                    }
                    using (var filestream = new FileStream(Path.Combine(productpath, filename), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    productVM.Product.ImageUrl = @"Images\Product\" + filename;
                }

                    if (productVM.Product.Id == 0)
                    {
                        
                        unitOfWork.Product.Add(productVM.Product);
                        unitOfWork.Save();
                        TempData["success"] = "Product added Successfully";
                }
                    else
                    {
                        
                        unitOfWork.Product.Update(productVM.Product);
                        unitOfWork.Save();
                        TempData["success"] = "Product updated Successfully";
                    }
                
               
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.id.ToString()
                });


                return View(productVM);
            }
           // return View();
            

        }
       

       
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objListCategory = unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objListCategory });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = unitOfWork.Product.Get(u => u.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new { success = false ,message="Error while deleting"});
            }
            var oldimgpath = Path.Combine(webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldimgpath))
            {
                System.IO.File.Delete(oldimgpath);
            }
            unitOfWork.Product.Remove(productToBeDeleted);
            unitOfWork.Save();
           
            return Json(new { success=true,message="Deleted successfully" });
        }

        #endregion
    }
}
