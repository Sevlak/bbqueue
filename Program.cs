using BBQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace BBQueue;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<VotesContext>(opt =>
        {
            opt.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
        });

        builder.Services.AddSingleton<Queue>();



        var app = builder.Build();

        using (var serviceScope = app.Services.CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<VotesContext>();

            if(context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }

        app.MapPost("/api/votes", async (Vote vote, VotesContext context, Queue q) =>
        {
            var cont = await context.Contestants.FindAsync(vote.ContestantId);

            if (cont is null)
            {
                return Results.NotFound();
            }

            q.Publish(vote.ContestantId.ToString());

            return Results.Accepted();
        });

        app.Run();
    }
}

