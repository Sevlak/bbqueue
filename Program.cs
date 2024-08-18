using BBQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace BBQueue;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<VotosContext>(opt =>
        {
            opt.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
        });

        builder.Services.AddSingleton<Queue>();



        var app = builder.Build();

        using (var serviceScope = app.Services.CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<VotosContext>();

            if(context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }

        app.MapPost("/api/votos", async (Voto voto, VotosContext context, Queue q) =>
        {
            var part = await context.Participantes.FindAsync(voto.ParticipanteId);

            if (part is null)
            {
                return Results.NotFound();
            }

            q.Publish(voto.ParticipanteId.ToString());

            return Results.Accepted();
        });

        app.Run();
    }
}

