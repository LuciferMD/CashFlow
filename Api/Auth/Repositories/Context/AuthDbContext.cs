using Auth.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Auth.Repositories.Context
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
