using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Storage;
using Windows.System;

namespace CorePlanetMusicPlayer.App
{
    public class DataBaseManager
    {
        // 创建LocalMusic表 语句
        const string CreateLocalMusicTableSQL = "create table if not exists local_music (" +
                             //"id integer primary key autoincrement,"+
                             "filepath varchar(255) primary key not null unique," +
                             "title varchar(200)," +
                             "artist varchar(50)," +
                             "album varchar(50)," +
                             "duration varchar(8)," +
                             "bitrate int unsigned," +
                             "track smallint," +
                             "disc tinyint," +
                             "year smallint," +
                             "genre tinyint unsigned" +
                             ");";

        // 从LocalMusic表中查询 语句
        const string QueryLocalMusicTableSQL = "select filepath, title, artist, album, duration, bitrate, track, disc, year, genre from local_music;";

        // 插入LocalMusic表数据
        const string InsertLocalMusicTableSQL = "insert or replace into local_music (filepath, title, artist, album, duration, bitrate, track, disc, year, genre) VALUES ";

        // 创建StreamMusic表 语句
        const string CreateStreamMusicTableSQL = "create table if not exists stream_music (" +
                             "url varchar(1024) primary key not null unique," +
                             "cover_url varchar(1024)," +
                             "title varchar(200)," +
                             "artist varchar(50)," +
                             "album varchar(50)," +
                             "duration varchar(8)," +
                             "bitrate int unsigned," +
                             "track smallint," +
                             "disc tinyint," +
                             "year smallint," +
                             "genre tinyint unsigned" +
                             ");";

        // 从StreamMusic表中查询 语句
        const string QueryStreamMusicTableSQL = "select url, cover_url, title, artist, album, duration, bitrate, track, disc, year, genre from stream_music;";

        // 插入StreamMusic表数据
        const string InsertStreamMusicTableSQL = "insert or replace into stream_music (url, cover_url, title, artist, album, duration, bitrate, track, disc, year, genre) VALUES";

        // 删除StreamMusic表数据
        const string DeleteStreamMusicSQL = "delete from stream_music where url=";

        // 创建Artists表 语句
        const string CreateArtistTableSQL = "create table if not exists artists (" +
                            "name varchar(255) primary key not null," +
                            "profile_path varchar(255)," +
                            "description varchar(5000)" +
                            ");";

        // 从Artists表中查询 语句
        const string QueryArtistsTableSQL = "select name, profile_path, description from artists;";

        // 插入Artists表数据
        const string InsertArtistTableSQL = "insert or replace into artists (name, profile_path, description) VALUES";

        // 创建Albums表 语句
        const string CreateAlbumTableSQL = "create table if not exists albums (" +
                            "title varchar(255) not null," +
                            "artists varchar(255) not null," + // 这个字段要不要？如果该字段不为空，则证明指定的艺术家是专辑的主要艺术家，否则专辑的艺术家就是专辑内所有歌曲的艺术家集合
                            "description varchar(255)," +
                            "cover_path varchar(255)," +
                            "release_year smallint," +
                            "primary key (title, artists)" +
                            ");";

        // 从Albums表中查询 语句
        const string QueryAlbumsTableSQL = "select title, artists, description, cover_path, release_year from albums;";

        // 插入Albums表数据
        const string InsertAlbumTableSQL = "insert or replace into albums (title, artists, description, cover_path, release_year) VALUES";

        // 初始化所有要用到的数据库
        public static async Task InitDataBasesAsync()
        {
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            // 创建LocalMusic表
            await SQLiteConnection.ExecuteNonQueryAsync(CreateLocalMusicTableSQL);
            // 创建StreamMusic表
            await SQLiteConnection.ExecuteNonQueryAsync(CreateStreamMusicTableSQL);
            // 创建Artists表
            await SQLiteConnection.ExecuteNonQueryAsync(CreateArtistTableSQL);
            // 创建Albums表
            await SQLiteConnection.ExecuteNonQueryAsync(CreateAlbumTableSQL);
            SQLiteConnection.Dispose();
        }

        // 从LocalMusic表中获取数据
        public static async Task<List<Music>> GetLocalMusicDataAsync()
        {
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            List<Music> musicList = await SQLiteConnection.QueryListAsync<Music>(QueryLocalMusicTableSQL, reader => new Music // 映射函数定义
            {
                Token = reader["filepath"].ToString(),
                Title = reader["title"].ToString(),
                Artist = reader["artist"].ToString(),
                Album = reader["album"].ToString(),
                Duration = reader["duration"].ToString(),
                Bitrate = Convert.ToUInt32(reader["bitrate"]),
                TrackNumber = Convert.ToUInt32(reader["track"]),
                DiscNumber = Convert.ToUInt32(reader["disc"]),
                Year = Convert.ToUInt32(reader["year"]),
                Genre = Convert.ToUInt32(reader["genre"])
            });
            SQLiteConnection.Dispose();
            return musicList;
        }

        // 更新LocalMusic表数据
        public static async Task UpdateLocalMusicDataAsync(List<LocalMusic> localMusicList)
        {
            if (localMusicList.Count == 0)
                return;
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(InsertLocalMusicTableSQL);
            foreach (LocalMusic localMusic in localMusicList)
            {
                stringBuilder.Append($"('{localMusic.StorageFile.Path.Replace("'", "''")}', '{localMusic.Title.Replace("'", "''")}', '{localMusic.Artist.Replace("'", "''")}', '{localMusic.Album.Replace("'", "''")}', '{localMusic.Duration.Replace("'", "''")}'," +
                    $" {localMusic.Bitrate}, {localMusic.TrackNumber}, {localMusic.DiscNumber}, {localMusic.Year}, {localMusic.Genre}),");
            }
            string SQLCommand = stringBuilder.ToString();
            SQLCommand = SQLCommand.Substring(0, SQLCommand.Length - 1);
            SQLCommand += ";";
            await SQLiteConnection.ExecuteNonQueryAsync(SQLCommand);
            SQLiteConnection.Dispose();
        }


        // 从StreamMusic表中获取数据
        public static async Task<List<StreamMusic>> GetStreamMusicDataAsync()
        {
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            List<StreamMusic> musicList = await SQLiteConnection.QueryListAsync<StreamMusic>(QueryStreamMusicTableSQL, reader => new StreamMusic // 映射函数定义
            {
                Url = reader["url"].ToString(),
                CoverUrl = reader["cover_url"].ToString(),
                Title = reader["title"].ToString(),
                Artist = reader["artist"].ToString(),
                Album = reader["album"].ToString(),
                Duration = reader["duration"].ToString(),
                Bitrate = Convert.ToUInt32(reader["bitrate"]),
                TrackNumber = Convert.ToUInt32(reader["track"]),
                DiscNumber = Convert.ToUInt32(reader["disc"]),
                Year = Convert.ToUInt32(reader["year"]),
                Genre = Convert.ToUInt32(reader["genre"])
            });
            SQLiteConnection.Dispose();
            return musicList;
        }

        // 更新Stream表数据
        public static async Task UpdateStreamMusicDataAsync(List<StreamMusic> streamMusicList)
        {
            if (streamMusicList.Count == 0)
                return;
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(InsertStreamMusicTableSQL);
            foreach (StreamMusic streamMusic in streamMusicList)
            {
                stringBuilder.Append($"('{streamMusic.Url.Replace("'", "''")}', '{streamMusic.CoverUrl.Replace("'", "''")}', '{streamMusic.Title.Replace("'", "''")}', '{streamMusic.Artist.Replace("'", "''")}', '{streamMusic.Album.Replace("'", "''")}', '{streamMusic.Duration.Replace("'", "''")}'," +
                    $" {streamMusic.Bitrate}, {streamMusic.TrackNumber}, {streamMusic.DiscNumber}, {streamMusic.Year}, {streamMusic.Genre}),");
            }
            string SQLCommand = stringBuilder.ToString();
            SQLCommand = SQLCommand.Substring(0, SQLCommand.Length - 1);
            SQLCommand += ";";
            await SQLiteConnection.ExecuteNonQueryAsync(SQLCommand);
            SQLiteConnection.Dispose();
        }
        // 删除Stream表数据
        public static async Task DeleteStreamMusicDataAsync(List<StreamMusic> delateData)
        {
            if (delateData.Count == 0)
                return;
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            foreach (StreamMusic streamMusic in delateData)
            {
                await SQLiteConnection.ExecuteNonQueryAsync($"{DeleteStreamMusicSQL}'{streamMusic.Url}';");
            }
            SQLiteConnection.Dispose();
        }

        // 从Artists表中获取数据
        public static async Task<List<Artist>> GetArtistsDataAsync()
        {
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            List<Artist> artistList = await SQLiteConnection.QueryListAsync<Artist>(QueryArtistsTableSQL, reader => new Artist // 映射函数定义
            {
                Name = reader["name"].ToString(),
                ProfilePath = reader["profile_path"].ToString(),
                Description = reader["description"].ToString()
            });
            SQLiteConnection.Dispose();
            return artistList;
        }

        // 更新Artists表数据
        public static async Task UpdateArtistsDataAsync(List<Artist> artistList)
        {
            if (artistList.Count == 0)
                return;
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(InsertArtistTableSQL);
            foreach (Artist artist in artistList)
            {
                stringBuilder.Append($"('{artist.Name.Replace("'", "''")}', '{artist.ProfilePath.Replace("'", "''")}', '{artist.Description.Replace("'", "''")}'),");
            }
            string SQLCommand = stringBuilder.ToString();
            SQLCommand = SQLCommand.Substring(0, SQLCommand.Length - 1);
            SQLCommand += ";";
            await SQLiteConnection.ExecuteNonQueryAsync(SQLCommand);
            SQLiteConnection.Dispose();
        }

        // 从Albums表中获取数据
        public static async Task<List<Album>> GetAlbumsDataAsync()
        {
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            List<Album> albumList = await SQLiteConnection.QueryListAsync<Album>(QueryAlbumsTableSQL, reader => new Album // 映射函数定义
            {
                Name = reader["title"].ToString(),
                ArtistsString = reader["artists"].ToString(),
                CoverPath = reader["cover_path"].ToString(),
                ReleaseYear = Convert.ToUInt32(reader["release_year"]),
                Description = reader["description"].ToString()
            });
            SQLiteConnection.Dispose();
            return albumList;
        }

        // 更新Albums表数据
        public static async Task UpdateAlbumsDataAsync(List<Album> albumList)
        {
            if (albumList.Count == 0)
                return;
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile file = await StorageHelper.GetStorageFileFromStorageFolderAsync(folder, "Library.db");
            SQLiteConnection SQLiteConnection = new SQLiteConnection(file.Path);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(InsertAlbumTableSQL);
            foreach (Album album in albumList)
            {
                stringBuilder.Append($"('{album.Name.Replace("'", "''")}', '{album.ArtistsString.Replace("'", "''")}', '{album.Description.Replace("'", "''")}', '{album.CoverPath.Replace("'", "''")}', {album.ReleaseYear}),");
            }
            string SQLCommand = stringBuilder.ToString();
            SQLCommand = SQLCommand.Substring(0, SQLCommand.Length - 1);
            SQLCommand += ";";
            await SQLiteConnection.ExecuteNonQueryAsync(SQLCommand);
            SQLiteConnection.Dispose();
        }
    }
}
