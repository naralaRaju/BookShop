using ECommerce.DataAccess.Data;
using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public DbInitializer(ApplicationDbContext db,UserManager<IdentityUser> userManager,RoleManager<IdentityRole> roleManager)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public void Initialize()
        {
            //migrations if they are not applied

            try
            {
                if (db.Database.GetPendingMigrations().Count() > 0)
                {
                    db.Database.Migrate();
                }

            }
            catch (Exception ex)
            {

            }

            //create roles if theyb are not created
            if (!roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();


                //if roles are not created ,then we will create admin user as well
                userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "raju@gmail.com",
                    Email = "raju@gmail.com",
                    Name = "Raju",
                    PhoneNumber = "7601067244",
                    StreetAddress = "Main road",
                    City = "prakasam",
                    State = "Andhrapradesh",
                    PostalCode = "523320"
                }, "Admin@123").GetAwaiter().GetResult();
                ApplicationUser user = db.applicationUsers.FirstOrDefault(u => u.Email == "raju@gmail.com");
                userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
