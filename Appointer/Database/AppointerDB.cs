using Appointer.Models;
using PetaPoco;
using PetaPoco.Providers;
using System;
using TShockAPI;

namespace Appointer.Database
{
    public class AppointerDB
    {
        public IDatabase DB;
        public void InitializeDB(bool useMysql)
        {
            if (useMysql)
            {
                var tsconf = TShock.Config.Settings;
                var connString = $"Server={tsconf.MySqlHost};Database={tsconf.MySqlDbName};Username={tsconf.MySqlUsername};Password={tsconf.MySqlPassword};";

                DB = DatabaseConfiguration.Build()
                    .UsingConnectionString(connString)
                    .UsingProvider<MySqlDatabaseProvider>()
                    .Create();

                try
                {
                    EnsureTableStructure(DbType.Mysql);
                    TShock.Log.Info($"Appointer database connected! (MySQL)");
                }
                catch (Exception ex)
                {
                    TShock.Log.Info($"Appointer experienced a database connection error! (MySQL)");
                    TShock.Log.Info(ex.Message);
                }
            }
            else // using sqlite
            {
                var connString = $"Data Source=tshock/appointer.sqlite";
                DB = DatabaseConfiguration.Build()
                    .UsingConnectionString(connString)
                    .UsingProvider<SQLiteDatabaseProvider>()
                    .Create();

                try
                {
                    EnsureTableStructure(DbType.Sqlite);
                    TShock.Log.Info($"Appointer database connected! (SQLite)");
                }
                catch (Exception ex)
                {
                    TShock.Log.Info($"Appointer experienced a database connection error! (SQLite)");
                    TShock.Log.Info(ex.Message);
                }

            }
        }

        public void EnsureTableStructure(DbType type)
        {
            new Tables.PlaytimeTable(DB, type);
        }
    }
}
