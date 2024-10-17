using EcomerceRazo.Models;
using ECommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcomerceRazo.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext db;
        
        public Category Category { get; set; }
        public EditModel(ApplicationDbContext applicationDb)
        {
            db = applicationDb;
        }
        public void OnGet(int? id)
        {
            if(id!=null && id != 0)
            {
                Category = db.Categories.Find(id);
            }
        }
        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                db.Categories.Update(Category);
                db.SaveChanges();
                TempData["success"] = "Updated Successfully.";
                return RedirectToPage("Index");
            }
            return Page();
            
        }
    }
}
