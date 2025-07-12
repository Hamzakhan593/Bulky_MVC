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
    public interface IShopingCartRepository : IRepository<ShoppingCart> 
    {
        void Update(ShoppingCart shoppingCart);
        //void Save();
    }
}
