using Auth.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Auth.Repositories.Context
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
