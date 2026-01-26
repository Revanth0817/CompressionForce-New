using CompressionForce.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class AutoTareBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AutoTareBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rand = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var row = db.AutoTareStatuses.First();

            row.MotorStatus = true;
            row.MotorTrip = false;

            row.S1Main = rand.Next(0, 50);
            row.S2Main = rand.Next(0, 50);
            row.S1Pre = rand.Next(0, 30);
            row.S2Pre = rand.Next(0, 30);
            row.S1Eject = rand.Next(0, 20);
            row.S2Eject = rand.Next(0, 20);

            row.last_updated = DateTime.UtcNow;

            await db.SaveChangesAsync();
            await Task.Delay(2000, stoppingToken);
        }
    }
}
