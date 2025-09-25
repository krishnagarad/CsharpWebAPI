using EventCalendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EventCalendar.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
    }
}
