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
     public class ProductRepository :Repository<Product> ,IProductRepository
    {
        private readonly ApplicationDbContext db;
        public ProductRepository(ApplicationDbContext db):base(db) 
        {
            this.db = db;
        }
       

        public void Update(Product obj)
        {
            var objfromdb=db.Products.FirstOrDefault(x => x.Id == obj.Id);
            if (objfromdb != null)
            {
                objfromdb.Title = obj.Title;
                objfromdb.ISBN = obj.ISBN;
                objfromdb.ListPrice = obj.ListPrice;
                objfromdb.Price = obj.Price;
                objfromdb.Description = obj.Description;
                objfromdb.Price50 = obj.Price50;
                objfromdb.Price100 = obj.Price100;
                objfromdb.Author = obj.Author;
                objfromdb.CategoryId = obj.CategoryId;
               if(obj.ImageUrl != null)
                {
                    objfromdb.ImageUrl= obj.ImageUrl;
                }

            }
            
        }
    }
}
