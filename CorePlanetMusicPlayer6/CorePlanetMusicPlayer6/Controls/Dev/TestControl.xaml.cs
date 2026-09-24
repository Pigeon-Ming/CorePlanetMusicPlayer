using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Database;
using CorePlanetMusicPlayer.Data.Mapping;
using CorePlanetMusicPlayer.Data.Repositories.Sqlite;
using CorePlanetMusicPlayer.Services.Artwork;
using CorePlanetMusicPlayer.Services.Library;
using CorePlanetMusicPlayer.Services.Library.MusicQueries;
using CorePlanetMusicPlayer.Uwp.Platform.Library;
using CorePlanetMusicPlayer.Uwp.Platform.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using CorePlanetMusicPlayer6.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.Dev
{
    public sealed partial class TestControl : UserControl
    {

        public TestControl()
        {
            this.InitializeComponent();
        }

        private async void CheckMusicRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            CheckMusicRepositoryButton.IsEnabled = false;
            MusicRepositoryCheckTextBlock.Text = "正在验证……";

            try
            {
                await CheckMusicRepositoryAsync();

                MusicRepositoryCheckTextBlock.Text = "验证通过：单条写入、按 ID 读取、批量新增和更新、列表读取。";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                MusicRepositoryCheckTextBlock.Text = $"验证失败：{ex.Message}";
            }
            finally
            {
                CheckMusicRepositoryButton.IsEnabled = true;
            }
        }

        private static async Task CheckMusicRepositoryAsync()
        {
            var options = new LibraryDatabaseOptions
            {
                DatabaseDirectory = ApplicationData.Current.TemporaryFolder.Path,
                DatabaseFileName = $"music-check-{Guid.NewGuid():N}.db",
                TargetVersion = 1
            };

            var database = new LibraryDatabase(
                options,
                new DatabaseConnectionFactory(options),
                new DatabaseMigrator());

            database.Initialize();

            var repository = new SqliteMusicRepository(database);

            string folderId = Guid.NewGuid().ToString();

            var first = CreateCheckMusic(folderId, "first.mp3");
            var second = CreateCheckMusic(folderId, "second.mp3");

            // 验证单条写入和按 ID 读取。
            await repository.UpsertAsync(first);

            var loadedFirst = await repository.GetByIdAsync(first.Id);

            VerifyCheckMusic(first, loadedFirst);

            // 用同一个 ID 更新第一首，同时批量新增第二首。
            first.Title = "更新后的标题";
            first.Metadata.Genre = "Jazz";
            first.Metadata.Year = 2025;

            await repository.UpsertRangeAsync(
                new[] { first, second });

            var allMusic = await repository.GetAllAsync();

            if (allMusic.Count != 2)
            {
                throw new InvalidOperationException(
                    $"预期共 2 首歌曲，实际为 {allMusic.Count} 首。");
            }

            VerifyCheckMusic(
                first,
                allMusic.SingleOrDefault(music => music.Id == first.Id));

            VerifyCheckMusic(
                second,
                allMusic.SingleOrDefault(music => music.Id == second.Id));

            var folderMusic = await repository.GetByLibraryFolderIdAsync(
                new CorePlanetMusicPlayer.Core.Library.LibraryFolderId(folderId));

            if (folderMusic.Count != 2)
            {
                throw new InvalidOperationException("按来源查询的歌曲数量不正确。");
            }

            await CheckFolderReplacementAsync(repository);
        }

        private static Music CreateCheckMusic(
            string folderId,
            string fileName)
        {
            var timestamp = new DateTimeOffset(
                2026, 9, 12, 10, 0, 0, TimeSpan.Zero);

            var fileInfo = new MusicFileInfo
            {
                LibraryFolderId = folderId,
                Path = @"C:\MusicRepositoryCheck\Album\" + fileName,
                RelativePath = @"Album\" + fileName,
                FileName = fileName,
                Extension = ".mp3",
                Size = 123456,
                LastModifiedAt = timestamp
            };

            var music = Music.CreateLocal(
                fileName,
                "测试艺术家",
                "测试专辑",
                TimeSpan.FromSeconds(180),
                fileInfo);

            music.Metadata = new MusicMetadata
            {
                Genre = "Pop",
                Year = 2024
            };

            music.AddedAt = timestamp;
            music.LastPlayedAt = null;

            return music;
        }

        private static void VerifyCheckMusic(
            Music expected,
            Music actual)
        {
            if (actual == null ||
                actual.Metadata == null ||
                actual.FileInfo == null)
            {
                throw new InvalidOperationException("歌曲或关联信息未正确读回。");
            }

            bool matches =
                actual.Id == expected.Id &&
                actual.Title == expected.Title &&
                actual.ArtistName == expected.ArtistName &&
                actual.AlbumTitle == expected.AlbumTitle &&
                actual.Duration == expected.Duration &&
                actual.SourceType == expected.SourceType &&
                actual.Metadata.Genre == expected.Metadata.Genre &&
                actual.Metadata.Year == expected.Metadata.Year &&
                actual.FileInfo.LibraryFolderId == expected.FileInfo.LibraryFolderId &&
                actual.FileInfo.Path == expected.FileInfo.Path &&
                actual.FileInfo.RelativePath == expected.FileInfo.RelativePath &&
                actual.FileInfo.FileName == expected.FileInfo.FileName &&
                actual.FileInfo.Extension == expected.FileInfo.Extension &&
                actual.FileInfo.Size == expected.FileInfo.Size &&
                actual.FileInfo.LastModifiedAt == expected.FileInfo.LastModifiedAt &&
                actual.AddedAt == expected.AddedAt &&
                actual.LastPlayedAt == expected.LastPlayedAt;

            if (!matches)
            {
                throw new InvalidOperationException(
                    $"歌曲 {expected.Id} 写入与读回的数据不一致。");
            }
        }

        private async void CheckMusicFileButton_Click(object sender, RoutedEventArgs e)
        {
            string relativePath = MusicRelativePathTextBox.Text;

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                MusicFileCheckTextBlock.Text = "请先输入音乐文件的相对路径。";
                return;
            }

            CheckMusicFileButton.IsEnabled = false;
            MusicFileCheckTextBlock.Text = "请选择音乐来源目录。";

            var mapper = new UwpStorageFileMapper();

            var accessService = new UwpStorageAccessService(
                new UwpFolderPickerService(),
                mapper);

            LibraryFolder folder = null;

            try
            {
                folder = await accessService.PickerAndCreateFutureAccessFolderAsync();

                if (folder == null)
                {
                    MusicFileCheckTextBlock.Text = "已取消。";
                    return;
                }

                var file = await accessService.GetStorageFileByFutureAccessAsync(
                    folder,
                    relativePath);

                if (file == null)
                {
                    throw new InvalidOperationException(
                        "无法找到文件，请检查相对路径和所选目录。");
                }

                var reader = new UwpMusicFileReader(mapper);

                var music = await reader.ReadAsync(
                    file,
                    folder,
                    relativePath);

                // 检查经过数据库映射后，文件定位信息仍可使用。
                var restored = MusicDataMapper.ToModel(
                    MusicDataMapper.ToEntity(music));

                if (restored.FileInfo == null ||
                    restored.FileInfo.LibraryFolderId != folder.Id.ToString() ||
                    restored.FileInfo.RelativePath != music.FileInfo.RelativePath)
                {
                    throw new InvalidOperationException("文件定位信息映射不正确。");
                }

                var reopenedFile = await accessService.GetStorageFileAsync(
                    restored,
                    folder);

                if (reopenedFile == null)
                {
                    throw new InvalidOperationException(
                        "音乐信息已读取，但无法根据来源和相对路径重新打开文件。");
                }

                MusicFileCheckTextBlock.Text =
                    $"读取及重新定位成功。\n" +
                    $"标题：{music.Title}\n" +
                    $"艺术家：{music.ArtistName}\n" +
                    $"专辑：{music.AlbumTitle}\n" +
                    $"流派：{music.Metadata.Genre}\n" +
                    $"年份：{music.Metadata.Year}\n" +
                    $"时长：{music.Duration}\n" +
                    $"大小：{music.FileInfo.Size} 字节\n" +
                    $"相对路径：{music.FileInfo.RelativePath}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MusicFileCheckTextBlock.Text = $"验证失败：{ex.Message}";
            }
            finally
            {
                // 本次只是临时验证，没有把来源保存进音乐库。
                if (folder != null)
                {
                    try
                    {
                        accessService.RemoveFutureAccess(folder);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }

                CheckMusicFileButton.IsEnabled = true;
            }
        }

        private async void CheckLibraryScanButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            CheckLibraryScanButton.IsEnabled = false;
            LibraryScanCheckTextBlock.Text = "请选择音乐来源目录。";

            var mapper = new UwpStorageFileMapper();

            var accessService = new UwpStorageAccessService(
                new UwpFolderPickerService(),
                mapper);

            var reader = new UwpMusicFileReader(mapper);

            var scanner = new UwpLibraryScanner(
                accessService,
                mapper,
                reader);

            LibraryFolder folder = null;

            try
            {
                folder = await accessService.PickerAndCreateFutureAccessFolderAsync();

                if (folder == null)
                {
                    LibraryScanCheckTextBlock.Text = "已取消。";
                    return;
                }

                LibraryScanCheckTextBlock.Text = "正在扫描目录及子目录……";

                var result = await scanner.ScanAsync(folder);

                var output = new StringBuilder();

                output.AppendLine(
                    result.IsComplete ? "扫描完整完成。" : "扫描未完成。");

                output.AppendLine($"已读取歌曲：{result.Items.Count} 首");

                foreach (string error in result.Errors)
                {
                    output.AppendLine($"错误：{error}");
                }

                output.AppendLine();
                output.AppendLine("前 10 首歌曲：");

                foreach (var music in result.Items.Take(10))
                {
                    output.AppendLine(
                        $"{music.Title} | {music.FileInfo.RelativePath}");
                }

                LibraryScanCheckTextBlock.Text = output.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                LibraryScanCheckTextBlock.Text = $"验证失败：{ex.Message}";
            }
            finally
            {
                // 临时测试来源未写入数据库，结束后清理本次授权。
                if (folder != null)
                {
                    try
                    {
                        accessService.RemoveFutureAccess(folder);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }

                CheckLibraryScanButton.IsEnabled = true;
            }
        }


        private static async Task CheckFolderReplacementAsync(
    SqliteMusicRepository repository)
        {
            var folderA = LibraryFolderId.NewId();
            var folderB = LibraryFolderId.NewId();

            var oldMusic = CreateCheckMusic(
                folderA.ToString(),
                "old.mp3");

            var otherFolderMusic = CreateCheckMusic(
                folderB.ToString(),
                "other.mp3");

            await repository.UpsertRangeAsync(
                new[] { oldMusic, otherFolderMusic });

            // 先验证正常替换。
            await repository.ReplaceByLibraryFolderIdAsync(
                folderA,
                new[] { oldMusic });

            VerifyCheckMusic(
                oldMusic,
                await repository.GetByIdAsync(oldMusic.Id));

            var firstNewMusic = CreateCheckMusic(
                folderA.ToString(),
                "new.mp3");

            var conflictingMusic = CreateCheckMusic(
                folderA.ToString(),
                "conflict.mp3");

            // 故意与另一个来源的已有歌曲发生主键冲突。
            conflictingMusic.Id = otherFolderMusic.Id;

            bool failedAsExpected = false;

            try
            {
                await repository.ReplaceByLibraryFolderIdAsync(
                    folderA,
                    new[] { firstNewMusic, conflictingMusic });
            }
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                failedAsExpected = true;
            }

            if (!failedAsExpected)
            {
                throw new InvalidOperationException(
                    "预期主键冲突失败，但替换没有报错。");
            }

            var remaining = await repository.GetByLibraryFolderIdAsync(folderA);

            if (remaining.Count != 1)
            {
                throw new InvalidOperationException("回滚后歌曲数量不正确。");
            }

            VerifyCheckMusic(oldMusic, remaining[0]);

            if (await repository.GetByIdAsync(firstNewMusic.Id) != null)
            {
                throw new InvalidOperationException("回滚后仍残留本次新增歌曲。");
            }

            VerifyCheckMusic(
                otherFolderMusic,
                await repository.GetByIdAsync(otherFolderMusic.Id));

            // 完整空结果应当能清空指定来源。
            await repository.ReplaceByLibraryFolderIdAsync(
                folderA,
                new Music[0]);

            remaining = await repository.GetByLibraryFolderIdAsync(folderA);

            if (remaining.Count != 0)
            {
                throw new InvalidOperationException("空结果没有清空指定来源。");
            }

            VerifyCheckMusic(
                otherFolderMusic,
                await repository.GetByIdAsync(otherFolderMusic.Id));
        }

        private async void CheckLibraryRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckLibararyRefresh();
        }

        private async Task CheckLibararyRefresh()
        {
            var services = AppRuntime.Services;

            var service = services?.MusicLibraryService;
            var musicQueryService = services?.MusicQueryService;
            var albumService = services?.AlbumService;
            var artistService = services?.ArtistService;

            if (service == null || musicQueryService == null || albumService == null || artistService == null)
            {
                LibraryRefreshCheckTextBlock.Text = "所需服务尚未就绪。";
                return;
            }

            CheckLibraryRefreshButton.IsEnabled = false;
            LibraryRefreshCheckTextBlock.Text = "正在扫描并保存……";

            try
            {
                var folders = await service.GetAllFoldersAsync();

                if (folders.Count == 0)
                {
                    LibraryRefreshCheckTextBlock.Text =
                        "请先在音乐库来源控件中添加测试目录。";
                    return;
                }

                var result = await service.RefreshAsync();

                var output = new StringBuilder();

                output.AppendLine(
                    result.HasError ? "刷新存在错误。" : "刷新完成。");

                output.AppendLine(
                    $"尝试目录：{result.FolderCount}，" +
                    $"成功更新：{result.UpdatedFolderCount}");

                output.AppendLine(
                    $"扫描歌曲：{result.ScannedMusicCount}，" +
                    $"保存歌曲：{result.SavedMusicCount}");

                foreach (string error in result.Errors)
                {
                    output.AppendLine(error);
                }

                // 先显示刷新结果，避免后续查询失败时丢失这些信息。
                LibraryRefreshCheckTextBlock.Text = output.ToString();

                var music = await musicQueryService.GetAllAsync();
                var albums = await albumService.GetAllAsync();
                var artists = await artistService.GetAllAsync();

                output.AppendLine();
                output.AppendLine(
                    $"当前数据库：{music.Count} 首歌曲，" +
                    $"{albums.Count} 张专辑，{artists.Count} 位艺术家");

                foreach (var item in music.Take(10))
                {
                    output.AppendLine(
                        $"歌曲：{item.Title} | ID：{item.Id}");

                    output.AppendLine(
                        $"来源：{item.FileInfo?.LibraryFolderId} | " +
                        $"路径：{item.FileInfo?.RelativePath}");
                }

                foreach (var album in albums.Take(10))
                {
                    output.AppendLine(
                        $"专辑：{album.Title} | " +
                        $"艺术家：{album.ArtistName} | " +
                        $"歌曲数：{album.MusicIds.Count}");
                }

                foreach (var artist in artists.Take(10))
                {
                    output.AppendLine(
                        $"艺术家：{artist.Name} | " +
                        $"歌曲数：{artist.MusicIds.Count} | " +
                        $"专辑数：{artist.AlbumIds.Count}");
                }

                LibraryRefreshCheckTextBlock.Text = output.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                LibraryRefreshCheckTextBlock.Text +=
                    $"\n验证未完整完成：{ex.Message}";
            }
            finally
            {
                CheckLibraryRefreshButton.IsEnabled = true;
            }
        }

        private async void LoadArtworkButton_Click(object sender, RoutedEventArgs e)
        {
            var services = AppRuntime.Services;

            var musicQueryService = services?.MusicQueryService;

            var musicList = await musicQueryService.GetAllAsync();

            if (musicList.Count > 0)
            {
                var reference = await services.ArtworkService.GetArtworkAsync(musicList[0]);

                PreviewArtworkControl.Request = ArtworkRequest.FromReference(reference);
            }
        }
    }
}
