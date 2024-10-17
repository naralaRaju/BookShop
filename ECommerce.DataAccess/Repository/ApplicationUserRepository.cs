using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository
{
     public class ApplicationUserRepository :Repository<ApplicationUser> ,IApplicationUserRepository
    {
        private readonly ApplicationDbContext db;
        public ApplicationUserRepository(ApplicationDbContext db):base(db) 
        {
            this.db = db;
        }
       

     
    }
}
