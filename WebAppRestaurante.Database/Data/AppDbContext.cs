using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppRestaurante.Models.Entities.Products;

namespace WebAppRestaurante.Database.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {       
            Database.EnsureCreated();
        }

        public DbSet<ProductModel> Products { get; set; }

    }
}
