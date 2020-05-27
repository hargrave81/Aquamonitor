using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaMonitor.Data.Context
{
    public class AquaDbContext : DbContext
    {
        private static bool created = false;

        public AquaDbContext()
        {
            if (!created)
            {
                created = true;
                Database.EnsureCreated();
                Database.GetPendingMigrations();
                Database.Migrate();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite("Data Source=" + Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) , "aquadata.db"));
        }


        public void Migrate()
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<AppSetting>().Property(p => p.AdminPassword).HasDefaultValue("fishy");
            base.OnModelCreating(modelBuilder);
        }


        protected DbSet<PowerRelay> Relays { get; set; }
        protected DbSet<WaterLevel> WaterLevels { get; set; }
        protected DbSet<AppSetting> Settings { get; set; }
        protected DbSet<HistoryRecord> Records { get; set; }

        #region History

        public HistoryRecord SaveHistory(HistoryRecord record)
        {
            var result = Records.Add(record);
            this.SaveChanges();
            return result.Entity;
        }
        public async Task<HistoryRecord> SaveHistoryAsync(HistoryRecord record)
        {
            var result = await Records.AddAsync(record);
            await this.SaveChangesAsync();
            return result.Entity;
        }

        public IEnumerable<HistoryRecord> GetHistory(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var result = Records.Include(t => t.PowerReadings).Include(t => t.WaterReadings).Where(t => t.Created >= startDate.UtcDateTime && t.Created <= endDate.UtcDateTime).OrderBy(t => t.Created).ToList();
            foreach (var r in result)
                r.UpdateCreatedToLocal(startDate);
            return result;
        }

        public async Task<IEnumerable<HistoryRecord>> GetHistoryAsync(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var result = await Records.Include(t => t.PowerReadings).Include(t => t.WaterReadings).Where(t => t.Created >= startDate.UtcDateTime && t.Created <= endDate.UtcDateTime).OrderBy(t => t.Created).ToListAsync();
            foreach (var r in result)
                r.UpdateCreatedToLocal(startDate);
            return result;
        }

        #endregion

        #region Settings

        public AppSetting GetSetting()
        {
            return Settings.FirstOrDefault();
        }

        public AppSetting SaveSettings(AppSetting appSetting)
        {
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AppSetting> result;
            if (!this.Settings.Any())
                result = this.Settings.Add(appSetting);
            else
                result = this.Settings.Update(appSetting);
            this.SaveChanges();
            return result.Entity;
        }
        public async Task<AppSetting> SaveSettingsAsync(AppSetting appSetting)
        {
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AppSetting> result;
            if (!this.Settings.Any())
                result = await this.Settings.AddAsync(appSetting);
            else
                result = this.Settings.Update(appSetting);
            await this.SaveChangesAsync();
            return result.Entity;
        }

        #endregion

        #region Relays

        public IEnumerable<PowerRelay> GetRelays()
        {
            return Relays.ToList();
        }

        public async Task<IEnumerable<PowerRelay>> GetRelaysAsync()
        {
            return await Relays.ToListAsync();
        }

        public void AddRelays(IEnumerable<PowerRelay> gdRelays)
        {
            this.Relays.AddRange(gdRelays);
            this.SaveChanges();
        }

        public async Task AddRelaysAsync(IEnumerable<PowerRelay> gdRelays)
        {
            await this.Relays.AddRangeAsync(gdRelays);
            await this.SaveChangesAsync();
        }

        public PowerRelay AddRelay(PowerRelay gdRelay)
        {
            var result = this.Relays.Add(gdRelay);
            this.SaveChanges();
            return result.Entity;
        }
        public async Task<PowerRelay> AddRelayAsync(PowerRelay gdRelay)
        {
            var result = await this.Relays.AddAsync(gdRelay);
            await this.SaveChangesAsync();
            return result.Entity;
        }


        public PowerRelay UpdateRelay(PowerRelay gdPowerRelay)
        {
            var result = this.Relays.Update(gdPowerRelay);
            this.SaveChanges();
            return result.Entity;
        }

        public async Task<PowerRelay> UpdateRelayAsync(PowerRelay gdPowerRelay)
        {
            var result = this.Relays.Update(gdPowerRelay);
            await this.SaveChangesAsync();
            return result.Entity;
        }



        public void UpdateRelays(IEnumerable<PowerRelay> gdPowerRelays)
        {
            this.Relays.UpdateRange(gdPowerRelays);
            this.SaveChanges();            
        }

        public async Task UpdateRelaysAsync(IEnumerable<PowerRelay> gdPowerRelays)
        {
            this.Relays.UpdateRange(gdPowerRelays);
            await this.SaveChangesAsync();            
        }


        public bool DeleteRelay(int Id)
        {
            var entity = this.Relays.First(t => t.Id == Id);
            var result = this.Relays.Remove(entity);
            this.SaveChanges();
            if (result.State == EntityState.Deleted)
                return true;
            return false;
        }

        public async Task<bool> DeleteRelayAsync(int Id)
        {
            var entity = await this.Relays.FirstAsync(t => t.Id == Id);
            var result = this.Relays.Remove(entity);
            await this.SaveChangesAsync();
            if (result.State == EntityState.Deleted)
                return true;
            return false;

        }

        public bool DeleteRelays(IEnumerable<PowerRelay> relays)
        {
            var Ids = relays.Select(t => t.Id);
            var entities = this.Relays.Where(t => Ids.Contains(t.Id)).ToList();
            this.Relays.RemoveRange(entities);
            this.SaveChanges();
            return true;
        }

        public async Task<bool> DeleteRelaysAsync(IEnumerable<PowerRelay> relays)
        {
            var Ids = relays.Select(t => t.Id);
            var entities = await this.Relays.Where(t => Ids.Contains(t.Id)).ToListAsync();
            this.Relays.RemoveRange(entities);
            await this.SaveChangesAsync();
            return true;            
        }
        #endregion

        #region WaterLevels

        public IEnumerable<WaterLevel> GetWaterLevels()
        {
            return this.WaterLevels.ToList();
        }

        public async Task<IEnumerable<WaterLevel>> GetWaterLevelsAsync()
        {
            return await this.WaterLevels.ToListAsync();

        }


        public void AddWaterLevels(IEnumerable<WaterLevel> gdWaterLevels)
        {
            this.WaterLevels.AddRange(gdWaterLevels);
            this.SaveChanges();
        }

        public async Task AddWaterLevelsAsync(IEnumerable<WaterLevel> gdWaterLevels)
        {
            await this.WaterLevels.AddRangeAsync(gdWaterLevels);
            await this.SaveChangesAsync();
        }

        public WaterLevel AddWaterLevel(WaterLevel gdWaterLevel)
        {
            var result = this.WaterLevels.Add(gdWaterLevel);
            this.SaveChanges();
            return result.Entity;
        }

        public async Task<WaterLevel> AddWaterLevelAsync(WaterLevel gdWaterLevel)
        {
            var result = await this.WaterLevels.AddAsync(gdWaterLevel);
            await this.SaveChangesAsync();
            return result.Entity;
        }

        public WaterLevel UpdateWaterLevel(WaterLevel gdWaterLevel)
        {
            var result = this.WaterLevels.Update(gdWaterLevel);
            this.SaveChanges();
            return result.Entity;
        }
        public async Task<WaterLevel> UpdateWaterLevelAsync(WaterLevel gdWaterLevel)
        {
            var result = this.WaterLevels.Update(gdWaterLevel);
            await this.SaveChangesAsync();
            return result.Entity;
        }


        public void UpdateWaterLevels(IEnumerable<WaterLevel> gdWaterLevels)
        {
            this.WaterLevels.UpdateRange(gdWaterLevels);
            this.SaveChanges();            
        }
        public async Task UpdateWaterLevelsAsync(IEnumerable<WaterLevel> gdWaterLevels)
        {
            this.WaterLevels.UpdateRange(gdWaterLevels);
            await this.SaveChangesAsync();            
        }


        public bool DeleteWaterLevel(int Id)
        {
            var entity = this.WaterLevels.First(t => t.Id == Id);
            var result = this.WaterLevels.Remove(entity);
            this.SaveChanges();
            if (result.State == EntityState.Deleted)
                return true;
            return false;
        }

        public async Task<bool> DeleteWaterLevelAsync(int Id)
        {
            var entity = await this.WaterLevels.FirstAsync(t => t.Id == Id);
            var result = this.WaterLevels.Remove(entity);
            await this.SaveChangesAsync();
            if (result.State == EntityState.Deleted)
                return true;
            return false;
        }


        public bool DeleteWaterLevels(IEnumerable<WaterLevel> gdWaterLevels)
        {
            var Ids = gdWaterLevels.Select(t => t.Id);
            var entities = this.WaterLevels.Where(t => Ids.Contains(t.Id)).ToList();
            this.WaterLevels.RemoveRange(entities);
            this.SaveChanges();
            
            return true;
            
        }

        public async Task<bool> DeleteWaterLevelsAsync(IEnumerable<WaterLevel> gdWaterLevels)
        {
            var Ids = gdWaterLevels.Select(t => t.Id);
            var entities = await this.WaterLevels.Where(t => Ids.Contains(t.Id)).ToListAsync();
            this.WaterLevels.RemoveRange(entities);
            await this.SaveChangesAsync();            
            return true;
            
        }
        #endregion
    }
}
