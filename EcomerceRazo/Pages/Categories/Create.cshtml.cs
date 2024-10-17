using EcomerceRazo.Models;
using ECommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcomerceRazo.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext db;
        [BindProperty]
        public Category Category { get; set; }
        public CreateModel(ApplicationDbContext applicationDb)
        {
            db = applicationDb;   
        }
       
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            db.Categories.Add(Category);
            db.SaveChanges();
            TempData["success"] = "Category Created Successfully";
            return RedirectToPage("Index");
        }
    }
}
