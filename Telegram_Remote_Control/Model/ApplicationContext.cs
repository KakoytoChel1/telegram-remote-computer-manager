using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram_Remote_Control.Model
{
    internal class ApplicationContext : DbContext
    {

        public DbSet<FileCommand> FileCommands => Set<FileCommand>();
        public DbSet<WhiteListItem> WhiteListItems => Set<WhiteListItem>();
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) => Database.EnsureCreated();

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlite("Data Source=mainDB.db");
        //}
    }
}
