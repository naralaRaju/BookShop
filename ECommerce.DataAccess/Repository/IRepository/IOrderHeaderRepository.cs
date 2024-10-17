using ECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository:IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus (int id,string orderstatus,string? paymentStatus=null);
        void UpdatePaymentStripId(int id, string sessionid, string paymentintentid);
       
    }
}
