using BBQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace BBQueue;

public class VotesContext: DbContext
{
    public VotesContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Contestant> Contestants { get; set; }
}