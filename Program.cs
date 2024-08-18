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

        app.MapGet("/", () => "Hello World!");
        app.MapPost("/api/votos", async (Voto voto, VotosContext context, Queue q) =>
        {
            var part = await context.Participantes.FindAsync(voto.ParticipanteId);

            if (part is null)
            {
                return Results.NotFound();
            }

            return Results.Accepted("Solicitação aceita.");
        });

        app.Run();
    }
}

