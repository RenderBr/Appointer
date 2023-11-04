using Appointer.Models;
using PetaPoco;

namespace Appointer.Database.Tables
{
    public class PlaytimeTable
    {
        public IDatabase database;
        public DbType type;

        public string MySQLStatement = @"CREATE TABLE IF NOT EXISTS UserPlaytime (
                                AccountName VARCHAR(255) NOT NULL,
                                Playtime INT NOT NULL,
                                PRIMARY KEY (AccountName)
                            );";

        public string SQLiteStatement = @"CREATE TABLE IF NOT EXISTS UserPlaytime (
                                AccountName TEXT NOT NULL,
                                Playtime INTEGER NOT NULL,
                                PRIMARY KEY (AccountName)
                            );";

        public PlaytimeTable(IDatabase database, DbType type)
        {
            this.type = type;
            this.database = database;

            if (this.type == DbType.Sqlite)
            {
                database.Execute(SQLiteStatement);
            }
            else
            {
                database.Execute(MySQLStatement);
            }
        }


    }
}
