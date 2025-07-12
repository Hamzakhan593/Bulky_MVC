using Bulky.DataAccess.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Resository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void update(Company company);
    }
}
