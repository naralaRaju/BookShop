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
     public class OrderHeaderRepository :Repository<OrderHeader> ,IOrderHeaderRepository
    {
        private readonly ApplicationDbContext db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db) 
        {
            this.db = db;
        }
       

        public void Update(OrderHeader obj)
        {
            db.Update(obj);
        }

		void IOrderHeaderRepository.UpdatePaymentStripId(int id, string sessionid, string paymentintentid)
		{
			var Orderfromdb=db.orderHeaders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(sessionid))
            {
                Orderfromdb.SessionId= sessionid;
            }
            if (!string.IsNullOrEmpty(paymentintentid))
            {
                Orderfromdb.PaymentIntentId= paymentintentid;
                Orderfromdb.PaymentDate= DateTime.Now;
            }
		}

		void IOrderHeaderRepository.UpdateStatus(int id, string orderstatus, string? paymentStatus)
		{
			var Orderfromdb = db.orderHeaders.FirstOrDefault(x => x.Id == id);
			if (Orderfromdb != null)
			{
				Orderfromdb.OrderStatus = orderstatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    Orderfromdb.PaymentStatus=paymentStatus;
                }
			}
		}
	}
}
