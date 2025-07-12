using Bulky.DataAccess.IRepository;
using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using BulkyWeb.DataAccess.Data;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Resository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public ICategoryRepository CategoryRepository { get; set; }

        public IProductRepository ProductRepository {  get; set; }
        public ICompanyRepository CompanyRepository { get; set; }
        public IShopingCartRepository ShoppingCartRepository { get; set; }
        public IApplicationUserRepository ApplicationUserRepository { get; set; }   

        //public IRepository<Product> ProductRespository {  get; set; }   

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            CategoryRepository = new CategoryRepository(_db);
            //ProductRespository = new Repository<Product>(_db);
            ProductRepository = new ProductRepository(_db);
            CompanyRepository = new companyRepository(_db);
            ShoppingCartRepository = new ShopingCartRepository(_db);
            ApplicationUserRepository = new ApplicationUserRepository(_db);
        }


        public void IUWSave()
        {
            _db.SaveChanges();
        }
    }
}
