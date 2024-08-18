using BBQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace BBQueue;

public class VotosContext: DbContext
{
    public VotosContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Participante> Participantes { get; set; }
}