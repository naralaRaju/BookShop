using ECommerce.DataAccess;
using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
     [Authorize (Roles=SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        public CategoryController(IUnitOfWork applicationDbContext)
        {
             unitOfWork= applicationDbContext;
        }
        public async Task<IActionResult> Index()
        {
            List<Category> objListCategory =  unitOfWork.Category.GetAll().ToList();
            return View(objListCategory);
        }
        
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Category obj)
        {
            //if (obj.Name == obj.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("name", "Display order and name shold not be same");
            //}
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Add (obj);
                unitOfWork.Save();
                TempData["success"] = "Category added Successfully";
                return RedirectToAction("Index");
            }
            return View();
            
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null && id == 0)
            {
                return NotFound();
            }
            Category? categoryfromdb = unitOfWork.Category.Get(u=>u.id==id);
            if (categoryfromdb == null)
            {
                return NotFound();
            }
            return View(categoryfromdb);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Category obj)
        {
             
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Update(obj);
                unitOfWork.Save();
                TempData["success"] = "Category updated Successfully";
                return RedirectToAction("Index");
            }
            return View();

        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null && id == 0)
            {
                return NotFound();
            }
            Category? categoryfromdb = unitOfWork.Category.Get (u => u.id == id);
            if (categoryfromdb == null)
            {
                return NotFound();
            }
            return View(categoryfromdb);
        }
        [HttpPost,ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            Category? obj = unitOfWork.Category.Get(u => u.id == id);
            if (obj == null)
            {
                return NotFound();
            }
            unitOfWork.Category.Remove(obj);
            unitOfWork.Save();
            TempData["success"] = "Category Deleted Successfully";
            return RedirectToAction("Index");
            

        }
    }
}
