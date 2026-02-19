using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using UWPTools.Models;
using Windows.Storage;
using Windows.UI.Xaml;

namespace CorePlanetMusicPlayer.Models
{
    /// <summary>
    /// 用于统计用户播放记录，只记录歌曲基本信息与记录创建时间
    /// </summary>
    public class PlayRecord
    {
        public PlayRecord(string title, string album, string artist, int year, int genre, TimeSpan duration)
        {
            Title = title;
            Album = album;
            Artist = artist;
            Year = year;
            Genre = genre;
            Duration = duration;
            DateTime = DateTime.Now;
        }

        public PlayRecord(IMusic music)
        {
            Title = music.Title;
            Album = music.Album;
            Artist = music.Artist;
            Year = (int)music.Year;
            Genre = (int)music.Genre;
            Duration = ParseDurationString(music.Duration);
            DateTime = DateTime.Now;
        }

        public PlayRecord() { }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Album { get; set; }

        public string Artist { get; set; }

        public int Year { get; set; }

        public int Genre { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime DateTime { get; set; }

        /// <summary>
        /// 解析常见的时长字符串（支持 "mm:ss", "m:ss", "hh:mm:ss", "ss" 等），
        /// 对 "mm:ss" 明确转换为 minutes/seconds，解析失败返回 TimeSpan.Zero。
        /// </summary>
        public static TimeSpan ParseDurationString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return TimeSpan.Zero;

            s = s.Trim();
            var parts = s.Split(':');

            try
            {
                if (parts.Length == 1)
                {
                    // 只有秒（可能包含小数）
                    if (double.TryParse(parts[0], out double sec))
                        return TimeSpan.FromSeconds(sec);
                }
                else if (parts.Length == 2)
                {
                    // mm:ss 或 m:ss（明确为 minutes + seconds）
                    if (int.TryParse(parts[0], out int minutes) && double.TryParse(parts[1], out double seconds))
                        return new TimeSpan(0, minutes, 0).Add(TimeSpan.FromSeconds(seconds));
                }
                else if (parts.Length == 3)
                {
                    // hh:mm:ss
                    if (int.TryParse(parts[0], out int hours) &&
                        int.TryParse(parts[1], out int minutes) &&
                        double.TryParse(parts[2], out double seconds))
                        return new TimeSpan(hours, minutes, 0).Add(TimeSpan.FromSeconds(seconds));
                }
            }
            catch
            {
                // fall through to TryParse below
            }

            if (TimeSpan.TryParse(s, out TimeSpan result))
                return result;

            return TimeSpan.Zero;
        }
    }

    public class PlayRecordHelper
    {
        //public static bool PlayRecordEnabled { get; private set; }

        //public static void StartPlayRecord()
        //{
        //    if (PlayRecordEnabled)
        //        StopPlayRecord();
            
        //    PlayRecordEnabled = true;
        //}

        //public static void StopPlayRecord()
        //{

        //}

        static string GetCreateTableSQL()
        {
            return "create table if not exists T" + DateTime.Now.Day + " (" +
                    "id integer primary key autoincrement," +
                    "title varchar(200) not null," +
                    "artist varchar(50)," +
                    "album varchar(50)," +
                    "year smallint," +
                    "genre tinyint unsigned," +
                    "duration time," +
                    "datetime DateTime" +
                    ");";
        }

        static string GetInsertSQL(PlayRecord playRecord)
        {
            return $"insert into T{DateTime.Now.Day} (title, artist, album, year, genre, duration, datetime) values" +
                $"            ('{playRecord.Title.Replace("'","''")}', '{playRecord.Artist.Replace("'", "''")}', '{playRecord.Album.Replace("'", "''")}', {playRecord.Genre}, {playRecord.Year}, '{playRecord.Duration.ToString(@"hh\:mm\:ss")}', '{DateTime.Now.ToString()}');";
        }

        static string GetQueryPlayRecordSQLByDay(int day)
        {
            return "select * from T" + day + ";";
        }

        public static StorageFolder RecordDataFolder { get; private set; }

        public static async Task InitAsync()
        {
            StorageFolder dataFolder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            RecordDataFolder = await StorageHelper.GetStorageFolderFromStorageFolderAsync(dataFolder, "PlayRecord");
        }

        public static async Task TestAsync()
        {
            var now = DateTime.Now.Date;
            await GetByYearMonthDayAsync(now.Year,now.Month,now.Day);
        }

        public static async Task<List<PlayRecord>> GetByYearMonthDayAsync(int year, int month, int day)
        {
            StorageFile dbFile = await GetDataBaseFileByYearMonthAsync(year,month);
            SQLiteConnection SQLiteConnection = new SQLiteConnection(dbFile.Path);
            List<PlayRecord> playRecords = await SQLiteConnection.QueryListAsync<PlayRecord>(GetQueryPlayRecordSQLByDay(day), reader => new PlayRecord
            {
                Id = Convert.ToInt32(reader["id"]),
                Title = reader["title"].ToString(),
                Artist = reader["artist"].ToString(),
                Album = reader["album"].ToString(),
                Year = Convert.ToInt32(reader["year"]),
                Genre = Convert.ToInt32(reader["genre"]),
                Duration = PlayRecord.ParseDurationString(reader["duration"].ToString()),
                DateTime = DateTime.Parse(reader["datetime"].ToString())
            });
            SQLiteConnection.Dispose();
            return playRecords;
        }

        private static async Task<StorageFile> GetDataBaseFileByYearMonthAsync(int year, int month)
        {
            return await StorageHelper.GetStorageFileFromStorageFolderAsync(RecordDataFolder,GetDataBaseFileNameByYearMonth(year,month));
        }

        public static string GetDataBaseFileNameByYearMonth(int year, int month)
        {
            if(month >= 10)
                return $"{year}-{month}.db";
            else
                return $"{year}-0{month}.db";
        }

        public static async Task InsertDataAsync(PlayRecord playRecord)
        {
            Debug.WriteLine($"插入播放记录：{playRecord.Title}, {playRecord.Artist}, {playRecord.Album}, {playRecord.Year}, {playRecord.Genre}, {playRecord.Duration}, {playRecord.DateTime}");
            StorageFile dbFile = await GetDataBaseFileByYearMonthAsync(DateTime.Now.Year,DateTime.Now.Month);
            SQLiteConnection SQLiteConnection = new SQLiteConnection(dbFile.Path);
            await SQLiteConnection.ExecuteNonQueryAsync(GetCreateTableSQL());
            await SQLiteConnection.ExecuteNonQueryAsync(GetInsertSQL(playRecord));
            SQLiteConnection.Dispose();
        }

        public static string GetTableNameByDay(int day)
        {
            return $"T{day}";
        }
    }
}
