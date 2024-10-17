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
     public class OrderDetailRepository :Repository<OrderDetail> ,IOrderDetailRepository
    {
        private readonly ApplicationDbContext db;
        public OrderDetailRepository(ApplicationDbContext db):base(db) 
        {
            this.db = db;
        }
       

        public void Update(OrderDetail obj)
        {
            db.Update(obj);
        }
    }
}
