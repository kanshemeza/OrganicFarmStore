using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OrganicFarmStore.Models;

namespace OrganicFarmStore
{
    internal static class DbInitializer
    {
        internal static void Initialize( this OrganicStoreDbContext db)
        {
            db.Database.Migrate();

            if(db.Products.Count() == 0)
            {
                db.Database.Migrate();

                db.Products.Add(new Product {
                    Description = "Lorem ipsum ipsum lorem",
                    Name = "Ginger",
                    Image = "/images/ginger-root.png",
                    Price = 7.99m
                });

                db.Products.Add(new Product
                {
                    Description = "Lorem ipsum ipsum lorem",
                    Name = "Hass Avocado",
                    Image = "/images/HassAvocado.jpg",
                    Price = 5.49m
                });

                db.Products.Add(new Product
                {
                    Description = "Lorem ipsum ipsum lorem",
                    Name = "Raspberries",
                    Image = "/images/raspberries.jpg",
                    Price = 2.99m
                });
                db.SaveChanges();
            }
        }
    }
}