using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Database;
using CorePlanetMusicPlayer.Data.Mapping;
using CorePlanetMusicPlayer.Data.Repositories.Sqlite;
using CorePlanetMusicPlayer.Services.Artwork;
using CorePlanetMusicPlayer.Services.Library;
using CorePlanetMusicPlayer.Services.Library.MusicQueries;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Services.Settings;
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
        private bool _isLoadingMusicTagEngine;

        public TestControl()
        {
            this.InitializeComponent();
        }

        private void ShowTestOutput(
            string title,
            string message)
        {
            string normalizedTitle =
                string.IsNullOrWhiteSpace(title)
                    ? "验证输出"
                    : title.Trim();

            TestOutputTextBox.Text =
                "[" + normalizedTitle + "]" +
                Environment.NewLine +
                (message ?? string.Empty);

            TestOutputTextBox.SelectionStart =
                TestOutputTextBox.Text.Length;
        }

        private void AppendTestOutput(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!string.IsNullOrEmpty(TestOutputTextBox.Text))
            {
                TestOutputTextBox.Text += Environment.NewLine;
            }

            TestOutputTextBox.Text += message;
            TestOutputTextBox.SelectionStart =
                TestOutputTextBox.Text.Length;
        }

        private async void TestControl_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await LoadMusicTagEngineAsync();
        }

        private async Task LoadMusicTagEngineAsync()
        {
            var services = AppRuntime.Services;

            if (services == null ||
                services.SettingsService == null)
            {
                MusicTagEngineComboBox.IsEnabled = false;
                ShowTestOutput(
                    "标签引擎设置",
                    "设置服务不可用。");
                return;
            }

            _isLoadingMusicTagEngine = true;

            try
            {
                var settings =
                    await services.SettingsService.LoadAsync();

                bool isTagLibAvailable =
                    services.MusicTagCapabilities != null &&
                    services.MusicTagCapabilities.IsTagLibAvailable;

                TagLibTagEngineItem.Visibility =
                    isTagLibAvailable
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                HybridTagEngineItem.Visibility =
                    isTagLibAvailable
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                MusicTagEngine engine =
                    settings.Metadata.TagEngine;

                if (!isTagLibAvailable &&
                    engine != MusicTagEngine.WindowsApi)
                {
                    engine = MusicTagEngine.WindowsApi;
                    settings.Metadata.TagEngine = engine;

                    await services.SettingsService.SaveAsync(
                        settings);
                }

                SelectMusicTagEngine(engine);
                MusicTagEngineComboBox.IsEnabled = true;

                if (isTagLibAvailable)
                {
                    ShowTestOutput(
                        "标签引擎设置",
                        "TagLib 扩展可用。");
                }
                else
                {
                    string reason =
                        services.MusicTagCapabilities == null
                            ? string.Empty
                            : services.MusicTagCapabilities
                                .UnavailableReason;

                    ShowTestOutput(
                        "标签引擎设置",
                        string.IsNullOrWhiteSpace(reason)
                            ? "当前环境不支持 TagLib 扩展。"
                            : reason);
                }
            }
            catch (Exception ex)
            {
                MusicTagEngineComboBox.IsEnabled = false;
                ShowTestOutput(
                    "标签引擎设置",
                    "读取标签引擎设置失败：" + ex.Message);
            }
            finally
            {
                _isLoadingMusicTagEngine = false;
            }
        }

        private void SelectMusicTagEngine(
            MusicTagEngine engine)
        {
            switch (engine)
            {
                case MusicTagEngine.TagLib:
                    MusicTagEngineComboBox.SelectedItem =
                        TagLibTagEngineItem;
                    break;

                case MusicTagEngine.Hybrid:
                    MusicTagEngineComboBox.SelectedItem =
                        HybridTagEngineItem;
                    break;

                default:
                    MusicTagEngineComboBox.SelectedItem =
                        WindowsApiTagEngineItem;
                    break;
            }
        }

        private async void MusicTagEngineComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (_isLoadingMusicTagEngine)
            {
                return;
            }

            var services = AppRuntime.Services;

            if (services == null ||
                services.SettingsService == null)
            {
                return;
            }

            MusicTagEngine selectedEngine;

            if (MusicTagEngineComboBox.SelectedItem ==
                TagLibTagEngineItem)
            {
                selectedEngine = MusicTagEngine.TagLib;
            }
            else if (MusicTagEngineComboBox.SelectedItem ==
                     HybridTagEngineItem)
            {
                selectedEngine = MusicTagEngine.Hybrid;
            }
            else
            {
                selectedEngine = MusicTagEngine.WindowsApi;
            }

            bool isTagLibAvailable =
                services.MusicTagCapabilities != null &&
                services.MusicTagCapabilities.IsTagLibAvailable;

            if (!isTagLibAvailable &&
                selectedEngine != MusicTagEngine.WindowsApi)
            {
                _isLoadingMusicTagEngine = true;

                try
                {
                    MusicTagEngineComboBox.SelectedItem =
                        WindowsApiTagEngineItem;
                }
                finally
                {
                    _isLoadingMusicTagEngine = false;
                }

                ShowTestOutput(
                    "标签引擎设置",
                    "当前环境不支持所选 TagLib 标签引擎。");

                return;
            }

            try
            {
                var settings =
                    await services.SettingsService.LoadAsync();

                settings.Metadata.TagEngine = selectedEngine;

                await services.SettingsService.SaveAsync(settings);

                ShowTestOutput(
                    "标签引擎设置",
                    "标签读取引擎设置已保存：" +
                    GetMusicTagEngineText(selectedEngine) + "。");
            }
            catch (Exception ex)
            {
                ShowTestOutput(
                    "标签引擎设置",
                    "保存标签引擎设置失败：" + ex.Message);
            }
        }

        private async void CheckMusicRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            CheckMusicRepositoryButton.IsEnabled = false;
            ShowTestOutput(
                "歌曲数据库读写",
                "正在验证……");

            try
            {
                await CheckMusicRepositoryAsync();

                ShowTestOutput(
                    "歌曲数据库读写",
                    "验证通过：单条写入、按 ID 读取、批量新增和更新、列表读取。");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ShowTestOutput(
                    "歌曲数据库读写",
                    $"验证失败：{ex.Message}");
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

        private void CheckLyricsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            CheckLyricsButton.IsEnabled = false;
            ShowTestOutput(
                "歌词核心逻辑",
                "正在验证……");

            try
            {
                CheckLyrics();

                ShowTestOutput(
                    "歌词核心逻辑",
                    "验证通过：时间标签、翻译合并、多时间标签、无效输入、来源优先级和冲突处理。");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ShowTestOutput(
                    "歌词核心逻辑",
                    "验证失败：" + ex.Message);
            }
            finally
            {
                CheckLyricsButton.IsEnabled = true;
            }
        }

        private static void CheckLyrics()
        {
            var parser = new LrcParser();

            IReadOnlyList<LyricLine> sameTimeLines = parser.Parse(
                "[00:03.89]Original\n[00:03.890]翻译");

            VerifyLyricCondition(
                sameTimeLines.Count == 1 &&
                sameTimeLines[0].Text == "Original" &&
                sameTimeLines[0].TranslationText == "翻译",
                "相同时间标签的翻译行合并不正确。");

            IReadOnlyList<LyricLine> inlineTranslationLines = parser.Parse(
                "[00:10.00]Original 「行内翻译」");

            VerifyLyricCondition(
                inlineTranslationLines.Count == 1 &&
                inlineTranslationLines[0].Text == "Original" &&
                inlineTranslationLines[0].TranslationText == "行内翻译",
                "行尾括号翻译解析不正确。");

            IReadOnlyList<LyricLine> multipleTimeLines = parser.Parse(
                "[00:01.2][00:02.34]Repeat");

            VerifyLyricCondition(
                multipleTimeLines.Count == 2 &&
                multipleTimeLines[0].Time ==
                    TimeSpan.FromMilliseconds(1200) &&
                multipleTimeLines[1].Time ==
                    TimeSpan.FromMilliseconds(2340),
                "多时间标签或小数位时间解析不正确。");

            VerifyLyricCondition(
                !parser.CanParse("[ar:Artist]\n[ti:Title]"),
                "元数据标签不应被识别为同步歌词。");

            var searchService = new LyricSearchService();
            DateTimeOffset now = DateTimeOffset.Now;
            var documents = new List<LyricDocument>
            {
                CreateCheckLyricDocument(
                    parser,
                    "embedded",
                    LyricSourceType.Embedded,
                    false,
                    string.Empty,
                    now),
                CreateCheckLyricDocument(
                    parser,
                    "online-old",
                    LyricSourceType.Online,
                    false,
                    string.Empty,
                    now.AddMinutes(-2)),
                CreateCheckLyricDocument(
                    parser,
                    "external",
                    LyricSourceType.ExternalFile,
                    false,
                    @"Album\Song.lrc",
                    now.AddMinutes(-3)),
                CreateCheckLyricDocument(
                    parser,
                    "application-cache",
                    LyricSourceType.Embedded,
                    true,
                    string.Empty,
                    now.AddMinutes(-4)),
                CreateCheckLyricDocument(
                    parser,
                    "online-new",
                    LyricSourceType.Online,
                    false,
                    string.Empty,
                    now.AddMinutes(-1)),
                new LyricDocument
                {
                    Id = "invalid",
                    SourceType = LyricSourceType.Online,
                    RawText = string.Empty,
                    Lines = new List<LyricLine>(),
                    UpdatedAt = now
                }
            };

            IReadOnlyList<LyricDocument> orderedDocuments =
                searchService.OrderByPreference(documents);

            VerifyLyricCondition(
                orderedDocuments.Count == 4 &&
                orderedDocuments[0].Id == "application-cache" &&
                orderedDocuments[1].Id == "external" &&
                orderedDocuments[2].Id == "embedded" &&
                orderedDocuments[3].Id == "online-new",
                "歌词来源优先级、重复来源或无效来源处理不正确。");

            VerifyLyricCondition(
                ReferenceEquals(
                    searchService.SelectPreferred(documents),
                    orderedDocuments[0]),
                "播放器首选歌词选择不正确。");
        }

        private static LyricDocument CreateCheckLyricDocument(
            ILyricParser parser,
            string id,
            LyricSourceType sourceType,
            bool isUserOverride,
            string sourcePath,
            DateTimeOffset updatedAt)
        {
            string rawText = "[00:01.00]" + id;

            return new LyricDocument
            {
                Id = id,
                SourceType = sourceType,
                SourcePath = sourcePath ?? string.Empty,
                IsUserOverride = isUserOverride,
                RawText = rawText,
                Lines = new List<LyricLine>(parser.Parse(rawText)),
                UpdatedAt = updatedAt
            };
        }

        private static void VerifyLyricCondition(
            bool condition,
            string errorMessage)
        {
            if (!condition)
            {
                throw new InvalidOperationException(errorMessage);
            }
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

        private static UwpMusicFileReader CreateMusicFileReader(
            UwpStorageFileMapper mapper)
        {
            IUwpMusicTagAccessor tagAccessor =
                AppRuntime.Services?.MusicTagAccessor;

            if (tagAccessor == null)
            {
                throw new InvalidOperationException(
                    "歌曲标签访问器尚未初始化。");
            }

            return new UwpMusicFileReader(
                mapper,
                tagAccessor);
        }

        private async void CheckMusicFileButton_Click(object sender, RoutedEventArgs e)
        {
            string relativePath = MusicRelativePathTextBox.Text;

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                ShowTestOutput(
                    "单文件读取",
                    "请先输入音乐文件的相对路径。");
                return;
            }

            CheckMusicFileButton.IsEnabled = false;
            ShowTestOutput(
                "单文件读取",
                "请选择音乐来源目录。");

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
                    ShowTestOutput(
                        "单文件读取",
                        "已取消。");
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

                var reader = CreateMusicFileReader(mapper);

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

                ShowTestOutput(
                    "单文件读取",
                    $"读取及重新定位成功。\n" +
                    $"标题：{music.Title}\n" +
                    $"艺术家：{music.ArtistName}\n" +
                    $"专辑：{music.AlbumTitle}\n" +
                    $"流派：{music.Metadata.Genre}\n" +
                    $"年份：{music.Metadata.Year}\n" +
                    $"音轨号：{music.Metadata.TrackNumber}\n" +
                    $"碟片号：{music.Metadata.DiscNumber}\n" +
                    $"时长：{music.Duration}\n" +
                    $"大小：{music.FileInfo.Size} 字节\n" +
                    $"相对路径：{music.FileInfo.RelativePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ShowTestOutput(
                    "单文件读取",
                    $"验证失败：{ex.Message}");
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

        private async void CheckMusicTagEnginesButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string relativePath = MusicRelativePathTextBox.Text;

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                ShowTestOutput(
                    "三种标签读取引擎",
                    "请先输入音乐文件的相对路径。");
                return;
            }

            AppServices services = AppRuntime.Services;

            if (services == null ||
                services.SettingsService == null ||
                services.MusicTagAccessor == null)
            {
                ShowTestOutput(
                    "三种标签读取引擎",
                    "标签读取验收所需服务尚未就绪。");
                return;
            }

            CheckMusicTagEnginesButton.IsEnabled = false;
            CheckMusicFileButton.IsEnabled = false;
            CheckLibraryScanButton.IsEnabled = false;
            CheckLibraryRefreshButton.IsEnabled = false;
            MusicTagEngineComboBox.IsEnabled = false;
            ShowTestOutput(
                "三种标签读取引擎",
                "请选择音乐来源目录。");

            var mapper = new UwpStorageFileMapper();
            var accessService = new UwpStorageAccessService(
                new UwpFolderPickerService(),
                mapper);
            var output = new StringBuilder();

            LibraryFolder folder = null;
            AppSettings settings = null;
            MusicTagEngine originalEngine =
                MusicTagEngine.WindowsApi;
            bool shouldRestoreSettings = false;
            bool settingsRestored = true;

            try
            {
                folder = await accessService
                    .PickerAndCreateFutureAccessFolderAsync();

                if (folder == null)
                {
                    output.AppendLine("已取消。");
                    return;
                }

                StorageFile file = await accessService
                    .GetStorageFileByFutureAccessAsync(
                        folder,
                        relativePath);

                if (file == null)
                {
                    throw new InvalidOperationException(
                        "无法找到文件，请检查相对路径和所选目录。");
                }

                var basicProperties =
                    await file.GetBasicPropertiesAsync();

                if (basicProperties.Size == 0)
                {
                    throw new InvalidDataException(
                        "音乐文件为空。");
                }

                settings =
                    await services.SettingsService.LoadAsync();
                originalEngine = settings.Metadata.TagEngine;
                shouldRestoreSettings = true;

                MusicTagReadResult windowsResult =
                    await ReadTagsWithEngineAsync(
                        services,
                        settings,
                        file,
                        MusicTagEngine.WindowsApi);

                AppendTagReadResult(
                    output,
                    "Windows API",
                    windowsResult);

                bool canReadWithTagLib =
                    services.MusicTagCapabilities != null &&
                    services.MusicTagCapabilities
                        .IsTagLibAvailable &&
                    services.MusicTagCapabilities
                        .CanReadTagsWithTagLib;

                if (!canReadWithTagLib)
                {
                    output.AppendLine();
                    output.AppendLine(
                        "TagLib 当前不可用，仅完成 Windows API 验收。");
                    return;
                }

                MusicTagReadResult tagLibResult =
                    await ReadTagsWithEngineAsync(
                        services,
                        settings,
                        file,
                        MusicTagEngine.TagLib);

                AppendTagReadResult(
                    output,
                    "TagLib",
                    tagLibResult);

                MusicTagReadResult hybridResult =
                    await ReadTagsWithEngineAsync(
                        services,
                        settings,
                        file,
                        MusicTagEngine.Hybrid);

                AppendTagReadResult(
                    output,
                    "混合引擎",
                    hybridResult);

                VerifyHybridReadResult(
                    windowsResult,
                    tagLibResult,
                    hybridResult);

                output.AppendLine();
                output.AppendLine(
                    "验证通过：混合引擎的基础标签和时长来自 Windows API，" +
                    "碟片号和备注来自 TagLib。");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                output.AppendLine();
                output.AppendLine("验证失败：" + ex.Message);
            }
            finally
            {
                if (shouldRestoreSettings && settings != null)
                {
                    try
                    {
                        settings.Metadata.TagEngine = originalEngine;

                        await services.SettingsService.SaveAsync(
                            settings);
                    }
                    catch (Exception ex)
                    {
                        settingsRestored = false;
                        System.Diagnostics.Debug.WriteLine(ex);
                        output.AppendLine();
                        output.AppendLine(
                            "警告：恢复原标签引擎设置失败：" +
                            ex.Message);
                    }
                }

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

                if (shouldRestoreSettings && settingsRestored)
                {
                    _isLoadingMusicTagEngine = true;

                    try
                    {
                        SelectMusicTagEngine(originalEngine);
                    }
                    finally
                    {
                        _isLoadingMusicTagEngine = false;
                    }
                }

                ShowTestOutput(
                    "三种标签读取引擎",
                    output.ToString());
                CheckMusicTagEnginesButton.IsEnabled = true;
                CheckMusicFileButton.IsEnabled = true;
                CheckLibraryScanButton.IsEnabled = true;
                CheckLibraryRefreshButton.IsEnabled = true;
                MusicTagEngineComboBox.IsEnabled = settingsRestored;
            }
        }

        private static async Task<MusicTagReadResult>
            ReadTagsWithEngineAsync(
                AppServices services,
                AppSettings settings,
                StorageFile file,
                MusicTagEngine engine)
        {
            settings.Metadata.TagEngine = engine;

            await services.SettingsService.SaveAsync(settings);

            MusicTagReadResult result =
                await services.MusicTagAccessor.ReadAsync(file);

            if (result == null || result.Metadata == null)
            {
                throw new InvalidOperationException(
                    GetMusicTagEngineText(engine) +
                    " 没有返回有效的标签读取结果。");
            }

            return result;
        }

        private static void VerifyHybridReadResult(
            MusicTagReadResult windowsResult,
            MusicTagReadResult tagLibResult,
            MusicTagReadResult hybridResult)
        {
            MusicMetadata windows = windowsResult.Metadata;
            MusicMetadata tagLib = tagLibResult.Metadata;
            MusicMetadata hybrid = hybridResult.Metadata;

            bool baseTagsMatch =
                TextEquals(hybrid.Title, windows.Title) &&
                TextEquals(hybrid.ArtistName, windows.ArtistName) &&
                ValuesEqual(
                    hybrid.ArtistNames,
                    windows.ArtistNames) &&
                TextEquals(
                    hybrid.AlbumTitle,
                    windows.AlbumTitle) &&
                TextEquals(
                    hybrid.AlbumArtistName,
                    windows.AlbumArtistName) &&
                TextEquals(hybrid.Genre, windows.Genre) &&
                hybrid.Year == windows.Year &&
                hybrid.TrackNumber == windows.TrackNumber &&
                TextEquals(hybrid.Composer, windows.Composer) &&
                hybridResult.Duration == windowsResult.Duration;

            if (!baseTagsMatch)
            {
                throw new InvalidOperationException(
                    "混合引擎的基础标签或时长与 Windows API 结果不一致。");
            }

            if (hybrid.DiscNumber != tagLib.DiscNumber)
            {
                throw new InvalidOperationException(
                    "混合引擎的碟片号与 TagLib 结果不一致。");
            }

            if (!TextEquals(hybrid.Comment, tagLib.Comment))
            {
                throw new InvalidOperationException(
                    "混合引擎的备注与 TagLib 结果不一致。");
            }
        }

        private static void AppendTagReadResult(
            StringBuilder output,
            string engineName,
            MusicTagReadResult result)
        {
            MusicMetadata metadata = result.Metadata;

            output.AppendLine();
            output.AppendLine("[" + engineName + "]");
            output.AppendLine(
                "标题：" + DisplayText(metadata.Title));
            output.AppendLine(
                "艺术家：" + DisplayValues(metadata.ArtistNames));
            output.AppendLine(
                "专辑：" + DisplayText(metadata.AlbumTitle));
            output.AppendLine(
                "专辑艺术家：" +
                DisplayText(metadata.AlbumArtistName));
            output.AppendLine(
                "流派：" + DisplayText(metadata.Genre));
            output.AppendLine(
                "年份：" + DisplayNumber(metadata.Year));
            output.AppendLine(
                "音轨号：" + DisplayNumber(metadata.TrackNumber));
            output.AppendLine(
                "碟片号：" + DisplayNumber(metadata.DiscNumber));
            output.AppendLine(
                "作曲家：" + DisplayText(metadata.Composer));
            output.AppendLine(
                "备注：" + DisplayText(metadata.Comment));
            output.AppendLine(
                "时长：" + result.Duration);
        }

        private static bool TextEquals(
            string left,
            string right)
        {
            return string.Equals(
                NormalizeTagText(left),
                NormalizeTagText(right),
                StringComparison.Ordinal);
        }

        private static bool ValuesEqual(
            IEnumerable<string> left,
            IEnumerable<string> right)
        {
            string[] leftValues = NormalizeTagValues(left);
            string[] rightValues = NormalizeTagValues(right);

            return leftValues.SequenceEqual(
                rightValues,
                StringComparer.Ordinal);
        }

        private static string[] NormalizeTagValues(
            IEnumerable<string> values)
        {
            if (values == null)
            {
                return new string[0];
            }

            return values
                .Select(NormalizeTagText)
                .Where(value => value.Length > 0)
                .ToArray();
        }

        private static string NormalizeTagText(string value)
        {
            return value == null
                ? string.Empty
                : value.Trim();
        }

        private static string DisplayText(string value)
        {
            string normalized = NormalizeTagText(value);

            return normalized.Length == 0
                ? "（空）"
                : normalized;
        }

        private static string DisplayValues(
            IEnumerable<string> values)
        {
            string[] normalized = NormalizeTagValues(values);

            return normalized.Length == 0
                ? "（空）"
                : string.Join("; ", normalized);
        }

        private static string DisplayNumber(int? value)
        {
            return value.HasValue
                ? value.Value.ToString()
                : "（空）";
        }

        private static string GetMusicTagEngineText(
            MusicTagEngine engine)
        {
            switch (engine)
            {
                case MusicTagEngine.TagLib:
                    return "TagLib";

                case MusicTagEngine.Hybrid:
                    return "混合引擎";

                default:
                    return "Windows API";
            }
        }

        private async void CheckLibraryScanButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            CheckLibraryScanButton.IsEnabled = false;
            ShowTestOutput(
                "递归扫描",
                "请选择音乐来源目录。");

            var mapper = new UwpStorageFileMapper();

            var accessService = new UwpStorageAccessService(
                new UwpFolderPickerService(),
                mapper);

            var reader = CreateMusicFileReader(mapper);

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
                    ShowTestOutput(
                        "递归扫描",
                        "已取消。");
                    return;
                }

                ShowTestOutput(
                    "递归扫描",
                    "正在扫描目录及子目录……");

                var result = await scanner.ScanAsync(folder);

                var output = new StringBuilder();

                output.AppendLine(
                    result.IsComplete ? "扫描完整完成。" : "扫描未完成。");

                output.AppendLine($"已读取歌曲：{result.Items.Count} 首");

                foreach (string error in result.Errors)
                {
                    output.AppendLine($"错误：{error}");
                }

                foreach (string warning in result.Warnings)
                {
                    output.AppendLine($"警告：{warning}");
                }

                output.AppendLine();
                output.AppendLine("前 10 首歌曲：");

                foreach (var music in result.Items.Take(10))
                {
                    output.AppendLine(
                        $"{music.Title} | {music.FileInfo.RelativePath}");
                }

                ShowTestOutput(
                    "递归扫描",
                    output.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ShowTestOutput(
                    "递归扫描",
                    $"验证失败：{ex.Message}");
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
                ShowTestOutput(
                    "音乐库刷新",
                    "所需服务尚未就绪。");
                return;
            }

            CheckLibraryRefreshButton.IsEnabled = false;
            ShowTestOutput(
                "音乐库刷新",
                "正在扫描并保存……");

            try
            {
                var folders = await service.GetAllFoldersAsync();

                if (folders.Count == 0)
                {
                    ShowTestOutput(
                        "音乐库刷新",
                        "请先在音乐库来源控件中添加测试目录。");
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
                    $"保存歌曲：{result.SavedMusicCount}，" +
                    $"跳过文件：{result.SkippedMusicCount}");

                foreach (string error in result.Errors)
                {
                    output.AppendLine(error);
                }

                foreach (string warning in result.Warnings)
                {
                    output.AppendLine("警告：" + warning);
                }

                // 先显示刷新结果，避免后续查询失败时丢失这些信息。
                ShowTestOutput(
                    "音乐库刷新",
                    output.ToString());

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

                ShowTestOutput(
                    "音乐库刷新",
                    output.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                AppendTestOutput(
                    $"验证未完整完成：{ex.Message}");
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

            if (musicQueryService == null ||
                services.ArtworkService == null)
            {
                ShowTestOutput(
                    "封面加载",
                    "封面加载所需服务尚未就绪。");
                return;
            }

            LoadArtworkButton.IsEnabled = false;
            ShowTestOutput(
                "封面加载",
                "正在加载第一首歌曲的封面……");

            try
            {
                var musicList =
                    await musicQueryService.GetAllAsync();

                if (musicList.Count == 0)
                {
                    ShowTestOutput(
                        "封面加载",
                        "音乐库中没有可用于验证封面的歌曲。");
                    return;
                }

                var reference =
                    await services.ArtworkService
                        .GetArtworkAsync(musicList[0]);

                PreviewArtworkControl.Request = ArtworkRequest.FromReference(reference);

                ShowTestOutput(
                    "封面加载",
                    "已提交第一首歌曲的封面加载请求：" +
                    musicList[0].Title);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ShowTestOutput(
                    "封面加载",
                    "验证失败：" + ex.Message);
            }
            finally
            {
                LoadArtworkButton.IsEnabled = true;
            }
        }
    }
}
