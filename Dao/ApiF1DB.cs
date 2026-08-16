using Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Dao
{
    public class ApiF1DB : DbContext
    {
        
        
        public ApiF1DB(DbContextOptions<ApiF1DB> ctx) : base(ctx)
        {
            
        }

        public ApiF1DB()
        {
            
        }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<Meeting> Meetings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
