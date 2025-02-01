using CorePlanetMusicPlayer.Models.Statistics;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Jpeg;
using Windows.Storage;
using Windows.UI.Xaml.Controls.Primitives;

namespace CorePlanetMusicPlayer.Models
{
    public class SQLiteManager
    {
        public static StorageFolder DataBaseFolder { get; set; }
        //public class MusicListDataBasesHelper
        //{
        //    public static async Task CreateTableAsync(StorageFolder folder, String dataBaseName, String tableName)
        //    {
        //        await InitializeDatabase(folder, dataBaseName);
        //        String tableCommand = "CREATE TABLE IF NOT " +
        //                "EXISTS " + tableName + " (" +
        //                "TYPE           INT     NOT NULL," +
        //                "DATACODE       TEXT    PRIMARY KEY    NOT NULL," +
        //                ");";
        //        RunSQLCommand(folder.Path + "\\" + dataBaseName + ".db", tableCommand);
        //    }

        //    public static List<String> GetAllTableNames(String dataBasePath)
        //    {
        //        List<String> names = new List<String>();
        //        //"SELECT name FROM sqlite_master WHERE type = 'table';"
        //        using (SqliteConnection db =
        //        new SqliteConnection($"Filename={dataBasePath}"))
        //        {
        //            db.Open();
        //            SqliteCommand selectCommand = new SqliteCommand
        //                ("SELECT name FROM sqlite_master WHERE type = 'table';", db);
        //            SqliteDataReader query = selectCommand.ExecuteReader();
        //            while (query.Read())
        //            {
        //                names.Add(query.GetString(0));
        //            }
        //            db.Close();
        //        }
        //        return names;
        //    }

        //    public static void SetTableData(String dataBasePath, String tableName, List<Music> musicList)
        //    {
        //        for (int i = 0; i < musicList.Count; i++)
        //        {
        //            string command = "INSERT OR REPLACE INTO " + tableName + " (TYPE, DATACODE) VALUES (" +
        //                    (int)musicList[i].MusicType + ",'" +
        //                    musicList[i].DataCode.Replace("'", "''") + "');";
        //            RunSQLCommand(dataBasePath, command);
        //        }
        //    }

        //    public static void SetTableData(String dataBasePath, String tableName, Music music)
        //    {
        //        string command = "INSERT OR REPLACE INTO " + tableName + " (TYPE, DATACODE) VALUES (" +
        //                    (int)music.MusicType + ",'" +
        //                    music.DataCode.Replace("'", "''") + "');";
        //        RunSQLCommand(dataBasePath, command);
        //    }

        //    public static List<Music> GetTableData(String dataBasePath, String tableName)
        //    {
        //        List<Music> musicList = new List<Music>();

        //        using (SqliteConnection db =
        //        new SqliteConnection($"Filename={dataBasePath}"))
        //        {
        //            db.Open();
        //            SqliteCommand selectCommand = new SqliteCommand
        //                ("SELECT * FROM " + tableName, db);
        //            SqliteDataReader query = selectCommand.ExecuteReader();

        //            while (query.Read())
        //            {
        //                //Music music = new Music();
        //                //music.MusicType = query.GetInt32(0)==0?MusicType.Local:MusicType.ExternalLocal;
        //                //music.DataCode = query.GetString(1);
        //                //music.Title = query.GetString(2);
        //                //music.Album = query.GetString(3);
        //                //music.Artist = query.GetString(4);
        //                //music.Bitrate = (uint)query.GetInt32(5);
        //                //music.Year = (uint)query.GetInt32(6);
        //                //music.TrackNumber = (uint)query.GetInt32(7);
        //                //music.DiscNumber = (uint)query.GetInt32(8);
        //                //music.Duration = query.GetString(9);
        //                //musicList.Add(music);
        //            }
        //            db.Close();
        //        }
        //        return musicList;
        //    }
        //}
        public class MusicSimpleDataBasesHelper//播放历史
        {
            //public static string MainCacheDataBaseName = "MusicCache";
            public static async Task CreateTableAsync(StorageFolder folder, String dataBaseName, String tableName)
            {
                await InitializeDatabase(folder, dataBaseName);
                String tableCommand = "CREATE TABLE IF NOT " +
                        "EXISTS " + tableName + " (" +
                        "TYPE       TEXT    NOT NULL," +
                        "DATACODE   TEXT    NOT NULL" +
                        ");";
                RunSQLCommand(folder.Path + "\\" + dataBaseName + ".db", tableCommand);
            }

            public static void SetTableData(String dataBasePath, String tableName, List<Music> musicList)
            {
                string command = "";
                for (int i = 0; i < musicList.Count; i++)
                {
                    command += "INSERT INTO " + tableName + " (TYPE, DATACODE) VALUES (" +
                            (int)musicList[i].MusicType + ",'" +
                            musicList[i].DataCode.Replace("'", "''") + "');\n";
                    
                }
                RunSQLCommand(dataBasePath, command);
            }

            public static void AddTableData(String dataBasePath, String tableName, Music music)
            {
                string command = "INSERT INTO " + tableName + " (TYPE, DATACODE) VALUES (" +
                            (int)music.MusicType + ",'" +
                            music.DataCode.Replace("'", "''") + "');";
                RunSQLCommand(dataBasePath, command);
            }

            public static void SetTableData(String dataBasePath, String tableName, Music music)
            {
                string command = "INSERT OR REPLACE INTO " + tableName + " (TYPE, DATACODE) VALUES (" +
                            (int)music.MusicType + ",'" +
                            music.DataCode.Replace("'", "''") + "');";
                RunSQLCommand(dataBasePath, command);
            }

            public static void DeleteTableData(String dataBasePath, String tableName, String DeleteItemDataCode)
            {
                string command = "DELETE FROM " + tableName + " WHERE DATACODE = '" + DeleteItemDataCode.Replace("'", "''") + "';";
                RunSQLCommand(dataBasePath, command);
            }

            public static void ClearTableData(String dataBasePath, String tableName)
            {
                string command = "DROP TABLE IF EXISTS " + tableName + ";\n" +
                    "CREATE TABLE " + tableName + " (" +
                    "TYPE       TEXT    NOT NULL," +
                    "DATACODE   TEXT    NOT NULL" +
                    ");";
                //Debug.WriteLine("执行SQL命令：[" + dataBasePath + "]" + command);
                RunSQLCommand(dataBasePath, command);

            }

            public static List<PlayHistoryItem> GetTableData(String dataBasePath, String tableName)
            {
                List<PlayHistoryItem> playHistoryItems = new List<PlayHistoryItem>();
                using (SqliteConnection db =
                new SqliteConnection($"Filename={dataBasePath}"))
                {
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand
                        ("SELECT * FROM " + tableName, db);
                    SqliteDataReader query = selectCommand.ExecuteReader();

                    while (query.Read())
                    {
                        PlayHistoryItem playHistoryItem = new PlayHistoryItem();
                        playHistoryItem.Type = query.GetInt32(0);
                        playHistoryItem.DataCode = query.GetString(1);
                        playHistoryItems.Add(playHistoryItem);
                    }
                    db.Close();
                }
                return playHistoryItems;
            }
        }
        public class MusicDataBasesHelper//用于音乐信息缓存的存储
        {
            //public static string MainCacheDataBaseName = "MusicCache";
            public static async Task CreateTableAsync(StorageFolder folder, String dataBaseName, String tableName)
            {
                await InitializeDatabase(folder, dataBaseName);
                String tableCommand = "CREATE TABLE IF NOT " +
                        "EXISTS " + tableName + " (" +
                        //"TYPE           INT     NOT NULL," +
                        "DATACODE       TEXT    PRIMARY KEY    NOT NULL," +
                        "TITLE          TEXT    NOT NULL," +
                        "ALBUM          TEXT," +
                        "ARTIST         TEXT," +
                        "BITRATE        INT," +
                        "YEAR           INT," +
                        "TRACKNUMBER    INT," +
                        "DISCNUMBER     INT," +
                        "DURATION       TEXT," +
                        "KEY            TEXT" +
                        ");";
                RunSQLCommand(folder.Path + "\\" + dataBaseName + ".db", tableCommand);
            }

            public static void SetTableData(String dataBasePath, String tableName, List<Music> musicList)
            {
                for (int i = 0; i < musicList.Count; i++)
                {
                    string command = "INSERT OR REPLACE INTO " + tableName + " (DATACODE, TITLE, ALBUM, ARTIST, BITRATE, YEAR, TRACKNUMBER, DISCNUMBER, DURATION,KEY) VALUES ('" +
                            musicList[i].DataCode.Replace("'", "''") + "','" +
                            musicList[i].Title.Replace("'", "''") + "','" +
                            musicList[i].Album.Replace("'", "''") + "','" +
                            musicList[i].Artist.Replace("'", "''") + "'," +
                            musicList[i].Bitrate + "," +
                            musicList[i].Year + "," +
                            musicList[i].TrackNumber + "," +
                            musicList[i].DiscNumber + ",'" +
                            musicList[i].Duration.Replace("'", "''") + "','"+
                            musicList[i].Key.Replace("'", "''") + "');";
                    /*string insertCommand = "INSERT INTO " + tableName + " (DATACODE, TITLE, ALBUM, ARTIST, BITRATE, YEAR, TRACKNUMBER, DISCNUMBER, DURATION) VALUES (" + 
                    //    (int)musicList[i].MusicType + 
                    //    ",'" + dataCode + "','" + 
                    //    musicList[i].Title + "','" + 
                    //    musicList[i].Album + "','" + 
                    //    musicList[i].Artist + "'," + 
                    //    musicList[i].Bitrate + "," + 
                    //    musicList[i].Year + "," + 
                    //    musicList[i].TrackNumber + "," + 
                    //    musicList[i].DiscNumber + ",'" + 
                    //    musicList[i].Duration + "')" +
                    //    " ON DUPLICATE KEY UPDATE TITLE = VALUES(TITLE), ALBUM = VALUES(ALBUM), ARTIST = VALUES(ARTIST), BITRATE = VALUES(BITRATE), YEAR = VALUES(YEAR), TRACKNUMBER = VALUE(TRACKNUMBER), DISCNUMBER = VALUE(DISCNUMBER), DURATION = VALUE(DURATION);";
                    //    //"INSERT INTO " + tableName + " VALUES (" + (int)musicList[i].MusicType + ",'" + dataCode + "','" + musicList[i].Title + "','" + musicList[i].Album +"','"+musicList[i].Artist + "'," + musicList[i].Bitrate + "," + musicList[i].Year+","+ musicList[i].TrackNumber + "," + musicList[i].DiscNumber + ",'" + musicList[i].Duration + "');";*/
                    RunSQLCommand(dataBasePath, command);
                }
            }

            public static void SetTableData(String dataBasePath, String tableName, Music music)
            {
                string command = "INSERT OR REPLACE INTO " + tableName + " (DATACODE, TITLE, ALBUM, ARTIST, BITRATE, YEAR, TRACKNUMBER, DISCNUMBER, DURATION, KEY) VALUES ('" +
                            music.DataCode.Replace("'", "''") + "','" +
                            music.Title.Replace("'", "''") + "','" +
                            music.Album.Replace("'", "''") + "','" +
                            music.Artist.Replace("'", "''") + "'," +
                            music.Bitrate + "," +
                            music.Year + "," +
                            music.TrackNumber + "," +
                            music.DiscNumber + ",'" +
                            music.Duration.Replace("'", "''") + "','" +
                            music.Key.Replace("'", "''") + "');";
                RunSQLCommand(dataBasePath, command);
            }

            public static void DeleteTableData(String dataBasePath, String tableName,String DeleteItemDataCode)
            {
                string command = "DELETE FROM "+tableName+" WHERE DATACODE = '"+DeleteItemDataCode.Replace("'","''")+"';";
                RunSQLCommand(dataBasePath, command);
            }

            public static void ClearTableData(String dataBasePath, String tableName)
            {
                string command = "DROP TABLE IF EXISTS " + tableName + ";\n" +
                    "CREATE TABLE " + tableName + " (" +
                    "DATACODE       TEXT    PRIMARY KEY    NOT NULL," +
                        "TITLE          TEXT    NOT NULL," +
                        "ALBUM          TEXT," +
                        "ARTIST         TEXT," +
                        "BITRATE        INT," +
                        "YEAR           INT," +
                        "TRACKNUMBER    INT," +
                        "DISCNUMBER     INT," +
                        "DURATION       TEXT," +
                        "KEY            TEXT" +
                        ");";
                //Debug.WriteLine("执行SQL命令：[" + dataBasePath + "]" + command);
                RunSQLCommand(dataBasePath, command);

            }



            public static List<Music> GetTableData(String dataBasePath, String tableName)
            {
                List<Music> musicList = new List<Music>();
                
                MusicType musicType = MusicType.Local;
                if (tableName == "ExternalLocalMusic")
                    musicType = MusicType.ExternalLocal;
                else if (tableName == "OnlineMusic")
                    musicType = MusicType.Online;
                else
                    musicType = MusicType.Local;
                using (SqliteConnection db =
                new SqliteConnection($"Filename={dataBasePath}"))
                {
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand
                        ("SELECT * FROM " + tableName, db);
                    SqliteDataReader query = selectCommand.ExecuteReader();

                    while (query.Read())
                    {
                        Music music = new Music();
                        music.MusicType = musicType;
                        music.DataCode = query.GetString(0);
                        music.Title = query.GetString(1);
                        music.Album = query.GetString(2);
                        music.Artist = query.GetString(3);
                        music.Bitrate = (uint)query.GetInt32(4);
                        music.Year = (uint)query.GetInt32(5);
                        music.TrackNumber = (uint)query.GetInt32(6);
                        music.DiscNumber = (uint)query.GetInt32(7);
                        music.Duration = query.GetString(8);
                        music.Key = query.GetString(9);
                        musicList.Add(music);
                    }
                    db.Close();
                }
                return musicList;
            }
        }

        public static async Task InitDataBases()
        {
            DataBaseFolder = await StorageManager.GetApplicationDataFolder("DataBases");
            await MusicDataBasesHelper.CreateTableAsync(DataBaseFolder, "MusicLibrary", "OnlineMusic");
            //await MusicListDataBasesHelper.CreateTableAsync(await StorageManager.GetApplicationDataFolder("DataBases"), "MusicLibrary", "ExternalMusic");
            //await InitializeDatabase(await StorageManager.GetApplicationDataFolder("DataBases"),"PlayHistory");
        }
        public static async Task InitializeDatabase(StorageFolder folder,string dataBaseName)
        {
            //StorageFolder storageFolder = await StorageManager.GetFolder(ApplicationData.Current.LocalFolder, "DataBases");
            await folder.CreateFileAsync(dataBaseName + ".db", CreationCollisionOption.OpenIfExists);
            //string dbpath = ApplicationData.Current.LocalFolder.Path + "\\"+ dataBaseName+".db";
        }

        public static void RunSQLCommand(string dataBasePath, string Command)
        {
            //Debug.WriteLine("执行SQL命令：[" + dataBasePath + "]" + Command);
            //StorageFolder storageFolder = await StorageManager.GetFolder(ApplicationData.Current.LocalFolder, "DataBases");
            using (SqliteConnection db =
                   new SqliteConnection($"Filename={dataBasePath}"))
            {
                db.Open();
                SqliteCommand sqliteCommand = new SqliteCommand(Command, db);
                sqliteCommand.ExecuteReader();
                db.Close();
            }
        }
    }
}
