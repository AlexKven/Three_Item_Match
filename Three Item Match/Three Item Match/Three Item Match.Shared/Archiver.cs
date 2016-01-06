using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Storage;

namespace Three_Item_Match
{
    public static class Archiver
    {
        public static readonly string DBPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Database.db");

        public static SQLiteConnection GetConnection()
        {
            SQLiteConnection result = new SQLiteConnection(DBPath);
            EnsureTablesAndTriggers(result);
            return result;
        }

        public static void EnsureTablesAndTriggers(SQLiteConnection connection)
        {
            ExecuteSQL(connection, @"create table if not exists GameArchive(TimePlayed char(19) primary key, Game varchar(16) not null, IncorrectBehavior varchar(7) not null, AutoDeal char not null, EnsureSets char not null, PenaltyOnDealWithSets char not null, TrainingMode char not null, DrawThree char not null, InstantDeal char not null, DurationSeconds integer not null, CollectedSets integer not null, MissedSets integer not null, Hints integer not null, DealsWithSets integer not null);");
        }

        public static string[,] ExecuteSQL(SQLiteConnection conn, string line, out string[] columns, out int numRows)
        {
            List<string[]> rows = new List<string[]>();
            using (var statement = conn.Prepare(line + ";"))
            {
                if (statement.ColumnCount == 0)
                {
                    statement.Step();
                    columns = new string[0];
                    numRows = 0;
                    return null;
                }
                else
                {
                    columns = new string[statement.ColumnCount];
                    for (int i = 0; i < statement.ColumnCount; i++)
                        columns[i] = statement.ColumnName(i);

                    while (statement.Step() != SQLiteResult.DONE)
                    {
                        string[] row = new string[statement.ColumnCount];
                        for (int i = 0; i < statement.ColumnCount; i++)
                        {
                            row[i] = statement[i]?.ToString();
                        }
                        rows.Add(row);
                    }
                }
            }
            numRows = rows.Count;
            string[,] result = new string[columns.Length, numRows];
            for (int y = 0; y < numRows; y++)
            {
                for (int x = 0; x < columns.Length; x++)
                {
                    result[x, y] = rows[y][x];
                }
            }
            return result;
        }

        public static string[,] ExecuteSQL(SQLiteConnection conn, string line, out string[] columns)
        {
            int dummy;
            return ExecuteSQL(conn, line, out columns, out dummy);
        }

        public static string[,] ExecuteSQL(SQLiteConnection conn, string line)
        {
            string[] dummy;
            return ExecuteSQL(conn, line, out dummy);
        }

        public static int ExtractInt(this object[,] sqlResult, int def)
        {
            if (sqlResult.GetLength(0) != 1 || sqlResult.GetLength(1) != 1)
                return def;
            int res;
            if (int.TryParse(sqlResult[0, 0].ToString(), out res))
                return res;
            else
                return def;
        }
    }
}
