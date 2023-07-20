using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraMours.Chat.Ava.Models {
    public class ChatDbcontext :DbContext{
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatList> ChatLists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={AppSettings.Instance.DbPath}"); // 这里是您的 SQLite 连接字符串
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // 添加实体配置
             modelBuilder.Entity<ChatMessage>().HasKey(e => e.ChatRecordId);
            modelBuilder.Entity<ChatList>().HasKey(e => e.Id);

            base.OnModelCreating(modelBuilder);
        }

        //切换数据库连接
        public void ChangeConnection(string connectionString) {
            // 修改数据库连接字符串，并重新配置 DbContext
            Database.GetDbConnection().ConnectionString = connectionString;
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = true;
        }

        public bool CheckIfTableExists<T>() where T : class {
            var tableExists = this.Model.FindEntityType(typeof(T)) != null;

            return tableExists;
        }

    }
}
