using Bulky.DataAccess.Resository.IRepository;
using Bulky.Models;
using BulkyWeb.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Resository
{
    public class companyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationDbContext _context;
        public companyRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public void update(Company company)
        {
            _context.Companys.Update(company);
        }
    }
}
