using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;
using HomeInventoryManager.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.InMemory;

namespace HomeInventoryManager.Test.AppFactories
{
    //So I dont write to the database for now.
    public class MemoryWebApplicationFactory<Program>: WebApplicationFactory<Program> where Program: class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            builder.ConfigureServices(services =>
            {
                // Remove the existing AppDbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add AppDbContext using in-memory database
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Build the service provider and ensure the database is created
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();

                    //USERSET
                    db.USERSET.Add(new User
                    {
                        user_id = 1,
                        user_name = "egds34",
                        email = "testuser@example.com",
                        first_name = "Christiana",
                        last_name = "Ramey",
                        role = "Basic",
                        password_hash = Encoding.UTF8.GetBytes("b4c3f613325fb91cf6e62eaaa06219ec2c80c1b7826c16854a7a09b5f2f974b20937f6e8392da4abb070aba482f306299616a390599d424a5030e65d053d7288"),
                        password_salt = Encoding.UTF8.GetBytes("e7fd9b3fb106a3e2afad24dcbd3d0b84")
                    });

                    //CATEGORIES
                    db.CATEGORIES.Add(new Category
                    {
                        id = 1,
                        name = "Electronics",
                        user_id = "1"
                    });
                    db.CATEGORIES.Add(new Category
                    {
                        id = 2,
                        name = "Furniture",
                        user_id = "1"
                    });

                    //LOCATIONS
                    db.LOCATIONS.Add(new Location
                    {
                        id = 1,
                        name = "Living Room",
                        user_id = "1"
                    });
                    db.LOCATIONS.Add(new Location
                    {
                        id = 2,
                        name = "Kitchen",
                        user_id = "1"
                    });

                    //PHOTOS - Non Implemented
                    db.PHOTOS.Add(new Photo
                    {
                        id = 1,
                        url = "http://example.com/photo.jpg",
                        user_id = "1"
                    });

                    //RECEIPTS
                    db.RECEIPTS.Add(new Receipt
                    {
                        id = "R1",
                        Receipt_id = 1,
                        location_purchased = "Online",
                        was_delivered = true
                    });

                    //ITEMS
                    db.ITEMS.Add(new Item
                    {
                        id = 1,
                        name = "Laptop",
                        description = "A test laptop",
                        user_id = 1,
                        barcode = "123456789",
                        category_id = 1,
                        location_id = 1,
                        photo_id = 1,
                        receipt_id = 1
                    });
                    db.ITEMS.Add(new Item
                    {
                        id = 2,
                        name = "Phone",
                        description = "A test phone",
                        user_id = 1,
                        barcode = "987654321",
                        category_id = 1,
                        location_id = 1,
                        photo_id = 1,
                        receipt_id = 1
                    });
                    db.ITEMS.Add(new Item
                    {
                        id = 3,
                        name = "Tablet",
                        description = "A test tablet",
                        user_id = 1,
                        barcode = "456789123",
                        category_id = 1,
                        location_id = 1,
                        photo_id = 1,
                        receipt_id = 1
                    });

                    //TAGS
                    db.TAGS.Add(new Tag
                    {
                        id = 1,
                        name = "Electronics",
                        user_id = "1"
                    });
                    db.TAGS.Add(new Tag
                    {
                        id = 2,
                        name = "Test",
                        user_id = "1"
                    });
                    db.TAGS.Add(new Tag
                    {
                        id = 3,
                        name = "Laptop",
                        user_id = "1"
                    });

                    //ITEM_TAGS
                    db.ITEM_TAGS.Add(new ItemTag
                    {
                        item_id = 1,
                        tag_id = 1
                    });

                    db.SaveChanges();
                }
            });
        }
    }
}
