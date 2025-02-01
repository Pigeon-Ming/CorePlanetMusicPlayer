using CorePlanetMusicPlayer.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models.Statistics
{
    public class PlayHistoryItem
    {
        public int Type { get; set; }
        public string DataCode { get; set; }
    }

    public class PlayHistoryManager
    {
        public static string GetTodayTableName()
        {
            return "T"+DateTime.Now.ToString("yyyyMMdd");
        }

        public static async Task AddDataAsync(Music music)
        {
            await SQLiteManager.MusicSimpleDataBasesHelper.CreateTableAsync(SQLiteManager.DataBaseFolder, "PlayHistory", GetTodayTableName());
            SQLiteManager.MusicSimpleDataBasesHelper.AddTableData(SQLiteManager.DataBaseFolder.Path+"\\PlayHistory.db", GetTodayTableName(), music);
        }

        public static List<PlayHistoryItem> GetData()
        {
            return SQLiteManager.MusicSimpleDataBasesHelper.GetTableData(SQLiteManager.DataBaseFolder.Path + "\\PlayHistory.db", GetTodayTableName());
        }
    }
    //public class PlayHistoryItemManager
    //{
    //    public static async Task InitTableAsync()
    //    {
    //        await DataBaseHelper.InitializeDatabase("Statistics");
    //        String tableCommand = "CREATE TABLE IF NOT " +
    //                "EXISTS PLAYHISTORY (" +
    //                "TYPE        INT     NOT NULL," +
    //                "DATACODE    TEXT    NOT NULL," +
    //                "TIME        TEXT    NOT NULL" +
    //                ");";
    //        await DataBaseHelper.RunSQLCommandAsync("Statistics",tableCommand);
    //    }

    //    public static async Task AddData(PlayHistoryItem playHistoryItem)
    //    {
    //        string dataCode = playHistoryItem.DataCode.Replace("'","''");
    //        string insertCommand = "INSERT INTO PLAYHISTORY VALUES (" + playHistoryItem.Type + ",'" + dataCode + "','" + DateTime.Now + "');";
    //        await DataBaseHelper.RunSQLCommandAsync("Statistics",insertCommand);
    //    }

    //    public static List<PlayHistoryItem> GetData()
    //    {
    //        List<PlayHistoryItem> playHistoryItems = new List<PlayHistoryItem>();
    //        string dbpath = ApplicationData.Current.LocalFolder.Path + "\\DataBases\\Statistics.db";
    //        using (SqliteConnection db =
    //           new SqliteConnection($"Filename={dbpath}"))
    //        {
    //            db.Open();

    //            SqliteCommand selectCommand = new SqliteCommand
    //                ("SELECT * FROM PLAYHISTORY", db);

    //            SqliteDataReader query = selectCommand.ExecuteReader();

    //            while (query.Read())
    //            {
    //                //Debug.WriteLine(query.ToString());
    //                PlayHistoryItem playHistoryItem = new PlayHistoryItem();
    //                playHistoryItem.Type = query.GetInt32(0);
    //                playHistoryItem.DataCode = query.GetString(1);
    //                playHistoryItem.Time = DateTime.Parse(query.GetString(2));
    //                playHistoryItems.Add(playHistoryItem);
    //            }
    //        }

    //        return playHistoryItems;
    //    }
    //}
}
