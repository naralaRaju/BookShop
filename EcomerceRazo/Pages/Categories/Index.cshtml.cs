using EcomerceRazo.Models;
using ECommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcomerceRazo.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext db;
        public IndexModel(ApplicationDbContext applicationDbContext)
        {
            db=applicationDbContext;
        }
        public List<Category> categories { get; set; }
        public void OnGet()
        {
            categories=db.Categories.ToList();
        }
    }
}
