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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
       
        public CompanyController(IUnitOfWork applicationDbContext)
        {
            unitOfWork = applicationDbContext;
           
        }
        public async Task<IActionResult> Index()
        {
            List<Company> objListCategory = unitOfWork.Company.GetAll().ToList();

            return View(objListCategory);
        }

        public async Task<IActionResult> UpSert(int? id)
        {


            //ViewBag.CategoryList=categoryList;

           
            if (id == null || id == 0)
            {
                //create
                return View(new Company());
            }
            else
            {
                //update
                Company company = unitOfWork.Company.Get(u => u.Id == id);
                return View(company);
            }

        }
        [HttpPost]
        public async Task<IActionResult> UpSert(Company company)
        {
            //if (obj.Name == obj.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("name", "Display order and name shold not be same");
            //}


            if (ModelState.IsValid)
            {
               

                if (company.Id == 0)
                {

                    unitOfWork.Company.Add(company);
                    unitOfWork.Save();
                    TempData["success"] = "Company added Successfully";
                }
                else
                {

                    unitOfWork.Company.Update(company);
                    unitOfWork.Save();
                    TempData["success"] = "Company updated Successfully";
                }


                return RedirectToAction("Index");
            }
            else
            {
               

                return View(company);
            }
            // return View();


        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objListCategory = unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objListCategory });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = unitOfWork.Company.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
           
            unitOfWork.Company.Remove(companyToBeDeleted);
            unitOfWork.Save();

            return Json(new { success = true, message = "Deleted successfully" });
        }

        #endregion
    }
}
