using Bulky.DataAccess.IRepository;
using Bulky.Models;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Resository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader> 
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentID(int id, string sessionId, string? paymentIntendId);
    }
}
