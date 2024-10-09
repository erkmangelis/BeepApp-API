using BeepApp_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using Serilog;
using BeepApp_API;


namespace BeepApp_API.Data
{
    public class BeepAppDbContext : IdentityDbContext<User>
    {
        public BeepAppDbContext(DbContextOptions<BeepAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<DOTest> DOTests { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerTeam> PlayerTeams { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Test> Tests { get; set; }

        public DbSet<UserLogs> Logs { get; set; }

        public async Task AddLogAsync(Guid dataId, LogAction action, string userId)
        {
            try
            {
                var log = new UserLogs
                {
                    Id = Guid.NewGuid(),
                    DataId = dataId,
                    Action = action,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                Logs.Add(log);
                await SaveChangesAsync();

                Log.Debug("Log created successfully for {Model} with DataId: {DataId} by UserId: {UserId}", dataId, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while creating log for {Model} with DataId: {DataId} by UserId: {UserId}", dataId, userId);
                throw; // Hatanın üst katmana iletilmesi
            }

        }
    }
}
