using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Telegram_Remote_Control.Model
{
    internal static class DataBaseLogic
    {

        public static DbContextOptions<ApplicationContext> options;

        public static void StartSettings()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            //setting options for ApplicationContext
            options = optionsBuilder.UseSqlite("Data Source=mainDB.db").Options;
        }
        internal static void AddNewFileItem(FileCommand item)
        {
            using(ApplicationContext db = new ApplicationContext(options))
            {
                db.FileCommands.Add(item);
                db.SaveChanges();
            }
        }

        internal static void AddWhiteListItem(WhiteListItem item)
        {
            using (ApplicationContext db = new ApplicationContext(options))
            {
                db.WhiteListItems.Add(item);
                db.SaveChanges();
            }
        }

        internal static void RemoveFileItem(FileCommand item)
        {
            using (ApplicationContext db = new ApplicationContext(options))
            {
                if(item != null)
                {
                    db.FileCommands.Remove(item);
                    db.SaveChanges();
                }
            }
        }

        internal static void RemoveWhiteListItem(WhiteListItem item)
        {
            using (ApplicationContext db = new ApplicationContext(options))
            {
                if (item != null)
                {
                    db.WhiteListItems.Remove(item);
                    db.SaveChanges();
                }
            }
        }

        internal static void UpdateFileItem(FileCommand item)
        {
            using(ApplicationContext db = new ApplicationContext(options))
            {
                if(item != null)
                {
                    db.FileCommands.Update(item);
                    db.SaveChanges();
                }
            }
        }

        internal static void UpdateWhiteListItem(WhiteListItem item)
        {
            using (ApplicationContext db = new ApplicationContext(options))
            {
                if (item != null)
                {
                    db.WhiteListItems.Update(item);
                    db.SaveChanges();
                }
            }
        }

        internal static FileCommand[] GetFileItems()
        {
            using (ApplicationContext db = new ApplicationContext(options))
            {
                var items = db.FileCommands.ToList();
                return items.ToArray();
            }
        }

        internal static WhiteListItem[] GetWhiteListItems()
        {
            using (ApplicationContext db = new ApplicationContext(options))
            {
                var items = db.WhiteListItems.ToList();
                return items.ToArray();
            }
        }

        //internal static InfoCommand[] GetInfoItems()
        //{
        //    using(ApplicationContext db = new ApplicationContext(options))
        //    {
        //        var items = db.InfoCommands.ToList();
        //        return items.ToArray();
        //    }
        //}

        //internal static DefaultCommand[] GetDefaultCommandItems()
        //{
        //    using (ApplicationContext db = new ApplicationContext(options))
        //    {
        //        var items = db.DefaultCommands.ToList();
        //        return items.ToArray();
        //    }
        //}
    }
}
