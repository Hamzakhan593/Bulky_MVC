using Bulky.DataAccess.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Resository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; }
        //IRepository<Product> ProductRespository {  get; }
        IProductRepository ProductRepository { get; }
        
        ICompanyRepository CompanyRepository { get; }

        IShopingCartRepository ShoppingCartRepository { get; }
        IApplicationUserRepository ApplicationUserRepository { get; }
        void IUWSave();
    }
}
