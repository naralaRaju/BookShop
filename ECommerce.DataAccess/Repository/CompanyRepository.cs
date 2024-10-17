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
     public class CompanyRepository :Repository<Company> ,ICompanyRepository
    {
        private readonly ApplicationDbContext db;
        public CompanyRepository(ApplicationDbContext db):base(db) 
        {
            this.db = db;
        }
       

        public void Update(Company obj)
        {
            db.Update(obj);
        }
    }
}
