using ETicaretApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ETicaretApi.Data
{
    public class ETicaretContext : DbContext
    {
        public ETicaretContext(DbContextOptions<ETicaretContext> options) : base(options) 
        {
            
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
