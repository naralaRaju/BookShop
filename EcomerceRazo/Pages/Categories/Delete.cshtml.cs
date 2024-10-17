using EcomerceRazo.Models;
using ECommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EcomerceRazo.Pages.Categories
{
      [BindProperties]
        public class DeleteModel : PageModel
        {
            private readonly ApplicationDbContext db;

            public Category Category { get; set; }
            public DeleteModel(ApplicationDbContext applicationDb)
            {
                db = applicationDb;
            }
            public void OnGet(int? id)
            {
                if (id != null && id != 0)
                {
                    Category = db.Categories.Find(id);
                }
            }
            public IActionResult OnPost()
            {
                Category? obj = db.Categories.Find(Category.id);
                if (obj == null)
                {
                    return NotFound();
                }
                db.Categories.Remove(obj);
                db.SaveChangesAsync();
                TempData["success"] = "Category Deleted Successfully";
                return RedirectToPage("Index");

            }
        }
    }
           
