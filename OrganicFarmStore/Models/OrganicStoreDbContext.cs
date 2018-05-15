using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicFarmStore.Models
{
    public class OrganicStoreDbContext : IdentityDbContext<OrganicStoreUser>
    {
        public OrganicStoreDbContext() : base()
        {

        }

        public OrganicStoreDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Product> Products { get; set; }
        public DbSet<TestEntity> TestEntities { get; set; }
    }
        public class OrganicStoreUser : IdentityUser
        {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        }

    public class TestEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
    

}
