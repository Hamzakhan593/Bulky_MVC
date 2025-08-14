using Azure.Identity;
using Bulky.Models;
using Bulky.Utility;
using BulkyWeb.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            //migratins if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
            }

            //create roles if they are not created
            if (!_roleManager.RoleExistsAsync(Static_Details.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Static_Details.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Static_Details.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Static_Details.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Static_Details.Role_Company)).GetAwaiter().GetResult();


                // if roles are not created, then we ll create admin user as well
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "adminhamza@gmail.com", // ← Fixed: Changed from "adminHamza@.com"
                    Email = "adminhamza@gmail.com",
                    Name = "Hamza Khan",
                    PhoneNumber = "1234567890",
                    StreetAddress = "test 123 ave",
                    State = "Sindh",
                    PostalCode = "12345",
                    City = "Sanghar",
                }, "Admin@1").GetAwaiter().GetResult(); 

                ApplicationUser user = _db.applicationUsers.FirstOrDefault(u => u.Email == "adminhamza@gmail.com");
                _userManager.AddToRoleAsync(user, Static_Details.Role_Admin).GetAwaiter().GetResult();


            }
            return;
        }
    }
}
