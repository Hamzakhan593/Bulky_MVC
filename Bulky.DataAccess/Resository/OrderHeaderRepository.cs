using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using BulkyWeb.DataAccess.Data;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Resository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        //public void Save()
        //{
        //    //_db.SaveChanges();
        //}

        public void Update(OrderHeader orderHeader)
        {
            _db.orderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.orderHeaders.FirstOrDefault(u=>u.OrderHeaderId == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string? paymentIntendId)
        {
            var orderFromDb = _db.orderHeaders.FirstOrDefault(u => u.OrderHeaderId == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntendId))
            {
                orderFromDb.PaymentIntendId = paymentIntendId;
                orderFromDb.PaymentDate = DateTime.Now; 
            }
        }
    }
}
