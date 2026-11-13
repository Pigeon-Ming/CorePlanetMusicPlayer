using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using CorePlanetMusicPlayer.Playback.Player;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Services.Settings;
using CorePlanetMusicPlayer6.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer6.Controls.Dev
{
    /// <summary>
    /// 编辑当前播放歌曲的标签信息。
    /// </summary>
    public sealed partial class MusicTagEditControl
        : UserControl
    {
        private static readonly string[] GenreSuggestions =
        {
            "Alternative",
            "Ambient",
            "Blues",
            "Classical",
            "Country",
            "Dance",
            "Disco",
            "Electronic",
            "Folk",
            "Funk",
            "Game",
            "Hip-Hop",
            "House",
            "Instrumental",
            "Jazz",
            "J-Pop",
            "K-Pop",
            "Latin",
            "Metal",
            "New Age",
            "Pop",
            "Punk",
            "R&B",
            "Rap",
            "Reggae",
            "Rock",
            "Soul",
            "Soundtrack",
            "Techno",
            "Trance",
            "Vocal"
        };

        private AppServices _services;
        private IPlaybackService _playbackService;

        private bool _isAttached;
        private bool _isBusy;
        private bool _isDirty;
        private bool _isApplyingFields;
        private bool _canEditDiscNumber;
        private bool _canEditComment;

        private int _lifetimeVersion;
        private int _loadVersion;

        private MusicId? _currentMusicId;
        private Music _currentMusic;
        private MusicTagEngine _effectiveTagEngine;

        public MusicTagEditControl()
        {
            this.InitializeComponent();
            GenreAutoSuggestBox.ItemsSource = GenreSuggestions;
        }

        private async void UserControl_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Detach();

            _services = AppRuntime.Services;
            _playbackService = _services?.PlaybackService;

            if (_services == null ||
                _playbackService == null ||
                _services.MusicQueryService == null ||
                _services.MusicMetadataEditService == null ||
                _services.SettingsService == null)
            {
                ApplyNoMusicState();
                StatusTextBlock.Text =
                    "标签编辑所需服务尚未就绪。";
                return;
            }

            _isAttached = true;
            _playbackService.CurrentMusicChanged +=
                PlaybackService_CurrentMusicChanged;

            await ReloadCurrentMusicAsync(
                _playbackService.State.CurrentMusicId);
        }

        private void UserControl_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            Detach();
        }

        private void Detach()
        {
            _isAttached = false;
            _isBusy = false;
            _isDirty = false;
            _canEditDiscNumber = false;
            _canEditComment = false;

            _lifetimeVersion++;
            _loadVersion++;

            if (_playbackService != null)
            {
                _playbackService.CurrentMusicChanged -=
                    PlaybackService_CurrentMusicChanged;
            }

            _playbackService = null;
            _services = null;
            _currentMusicId = null;
            _currentMusic = null;

            ApplyNoMusicState();
        }

        private async void PlaybackService_CurrentMusicChanged(
            object sender,
            CurrentMusicChangedEventArgs e)
        {
            int lifetime = _lifetimeVersion;

            try
            {
                Action reload = () =>
                {
                    if (!_isAttached ||
                        lifetime != _lifetimeVersion ||
                        !ReferenceEquals(sender, _playbackService))
                    {
                        return;
                    }

                    MusicId? musicId = e == null
                        ? _playbackService.State.CurrentMusicId
                        : e.NewMusicId;

                    _ = ReloadCurrentMusicAsync(musicId);
                };

                if (Dispatcher.HasThreadAccess)
                {
                    reload();
                }
                else
                {
                    await Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        () => reload());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void ReloadButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_isAttached || _isBusy ||
                _playbackService == null)
            {
                return;
            }

            if (_isDirty && !await ConfirmDiscardChangesAsync())
            {
                return;
            }

            await ReloadCurrentMusicAsync(
                _playbackService.State.CurrentMusicId);
        }

        private async void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_isAttached || _isBusy ||
                _currentMusic == null ||
                _services?.MusicMetadataEditService == null)
            {
                return;
            }

            MusicMetadataUpdateRequest request;

            try
            {
                request = CreateUpdateRequest();
            }
            catch (FormatException ex)
            {
                StatusTextBlock.Text = ex.Message;
                return;
            }

            if (!request.HasAnyChange)
            {
                _isDirty = false;
                UpdateOperationState();
                StatusTextBlock.Text =
                    "没有需要保存的标签修改。";
                return;
            }

            MusicId musicId = _currentMusic.Id;
            int lifetime = _lifetimeVersion;
            int loadVersion = _loadVersion;

            _isBusy = true;
            UpdateOperationState();
            StatusTextBlock.Text = "正在保存标签……";

            try
            {
                Result<Music> result =
                    await _services.MusicMetadataEditService
                        .UpdateAsync(request);

                if (!CanContinueInteraction(
                    musicId,
                    lifetime,
                    loadVersion))
                {
                    return;
                }

                if (result == null || result.IsFailure)
                {
                    StatusTextBlock.Text =
                        "保存失败：" +
                        (result == null
                            ? "编辑服务没有返回结果。"
                            : result.ErrorMessage);
                    return;
                }

                _currentMusic = result.Value;
                _isDirty = false;
                ApplyMusicToFields(_currentMusic);
                StatusTextBlock.Text = "标签已保存。";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (CanContinueInteraction(
                    musicId,
                    lifetime,
                    loadVersion))
                {
                    StatusTextBlock.Text =
                        "保存失败：" + ex.Message;
                }
            }
            finally
            {
                if (CanContinueInteraction(
                    musicId,
                    lifetime,
                    loadVersion))
                {
                    _isBusy = false;
                    UpdateOperationState();
                }
            }
        }

        private void EditorField_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            MarkEditorDirty();
        }

        private void GenreAutoSuggestBox_TextChanged(
            AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.Reason ==
                AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = NormalizeText(sender.Text);

                sender.ItemsSource = query.Length == 0
                    ? GenreSuggestions
                    : GenreSuggestions
                        .Where(value =>
                            value.IndexOf(
                                query,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
            }

            MarkEditorDirty();
        }

        private void GenreAutoSuggestBox_SuggestionChosen(
            AutoSuggestBox sender,
            AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            string selectedGenre = e.SelectedItem as string;

            if (selectedGenre != null)
            {
                sender.Text = selectedGenre;
            }
        }

        private void MarkEditorDirty()
        {
            if (_isApplyingFields || !_isAttached ||
                _currentMusic == null)
            {
                return;
            }

            _isDirty = true;
            UpdateOperationState();
        }

        private async Task ReloadCurrentMusicAsync(
            MusicId? musicId)
        {
            AppServices services = _services;
            int lifetime = _lifetimeVersion;
            int version = ++_loadVersion;

            _isBusy = true;
            _isDirty = false;
            _currentMusicId = musicId;
            _currentMusic = null;
            _canEditDiscNumber = false;
            _canEditComment = false;

            if (!musicId.HasValue || musicId.Value.IsEmpty)
            {
                ApplyNoMusicState();
                StatusTextBlock.Text = "尚未播放音乐。";
                _isBusy = false;
                UpdateOperationState();
                return;
            }

            MusicTitleTextBlock.Text = "正在读取歌曲标签……";
            UpdateOperationState();

            try
            {
                Music music =
                    await services.MusicQueryService.GetByIdAsync(
                        musicId.Value);

                AppSettings settings =
                    await services.SettingsService.LoadAsync();

                if (!CanApplyLoad(
                    musicId,
                    lifetime,
                    version))
                {
                    return;
                }

                if (music == null)
                {
                    ApplyNoMusicState();
                    StatusTextBlock.Text =
                        "当前歌曲已不在音乐库中。";
                    return;
                }

                _currentMusic = music;
                _effectiveTagEngine = ResolveEffectiveTagEngine(
                    music,
                    settings);
                _canEditDiscNumber = CanEditDiscNumber(
                    music,
                    _effectiveTagEngine);
                _canEditComment = CanEditComment(
                    music,
                    _effectiveTagEngine);

                ApplyMusicContext(music);
                ApplyMusicToFields(music);

                StatusTextBlock.Text = IsFileBacked(music)
                    ? "已加载。保存时将先写入音乐文件，再更新数据库。"
                    : "已加载。非本地音乐的修改将直接保存到数据库。";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (CanApplyLoad(
                    musicId,
                    lifetime,
                    version))
                {
                    ApplyNoMusicState();
                    StatusTextBlock.Text =
                        "读取歌曲标签失败：" + ex.Message;
                }
            }
            finally
            {
                if (CanApplyLoad(
                    musicId,
                    lifetime,
                    version))
                {
                    _isBusy = false;
                    UpdateOperationState();
                }
            }
        }

        private void ApplyMusicContext(Music music)
        {
            MusicTitleTextBlock.Text =
                string.IsNullOrWhiteSpace(music.Title)
                    ? "未知标题"
                    : music.Title;

            SourceTypeTextBlock.Text =
                "来源：" + GetSourceTypeText(music.SourceType);

            if (!IsFileBacked(music))
            {
                TagEngineTextBlock.Text =
                    "保存目标：应用数据库";
                DiscNumberHintTextBlock.Text =
                    "非本地音乐不会写入文件，碟片号可以保存到数据库。";
                CommentHintTextBlock.Text =
                    "非本地音乐不会写入文件，备注可以保存到数据库。";
                return;
            }

            TagEngineTextBlock.Text =
                "标签引擎：" +
                GetTagEngineText(_effectiveTagEngine);

            DiscNumberHintTextBlock.Text =
                _canEditDiscNumber
                    ? "碟片号将由 TagLib 写入音乐文件。"
                    : "Windows API 不支持读取或写入碟片号。";

            CommentHintTextBlock.Text =
                _canEditComment
                    ? "备注将由 TagLib 写入音乐文件。"
                    : "Windows API 模式不支持读取或写入备注。";
        }

        private void ApplyMusicToFields(Music music)
        {
            MusicMetadata metadata =
                music.Metadata ?? MusicMetadata.Empty;

            List<string> artistNames =
                GetStoredArtistNames(music, metadata);

            _isApplyingFields = true;

            try
            {
                TitleTextBox.Text = music.Title ?? string.Empty;
                ArtistNamesTextBox.Text = string.Join(
                    Environment.NewLine,
                    artistNames);
                AlbumTitleTextBox.Text =
                    music.AlbumTitle ?? string.Empty;
                AlbumArtistNameTextBox.Text =
                    metadata.AlbumArtistName ?? string.Empty;
                GenreAutoSuggestBox.Text =
                    metadata.Genre ?? string.Empty;
                YearTextBox.Text = ToNullableText(metadata.Year);
                TrackNumberTextBox.Text =
                    ToNullableText(metadata.TrackNumber);
                DiscNumberTextBox.Text =
                    ToNullableText(metadata.DiscNumber);
                ComposerTextBox.Text =
                    metadata.Composer ?? string.Empty;
                CommentTextBox.Text =
                    metadata.Comment ?? string.Empty;
            }
            finally
            {
                _isApplyingFields = false;
            }

            UpdateOperationState();
        }

        private void ApplyNoMusicState()
        {
            _currentMusic = null;
            _currentMusicId = null;
            _canEditDiscNumber = false;
            _canEditComment = false;
            _isDirty = false;

            MusicTitleTextBlock.Text = "尚未播放";
            SourceTypeTextBlock.Text = string.Empty;
            TagEngineTextBlock.Text = string.Empty;
            DiscNumberHintTextBlock.Text = string.Empty;
            CommentHintTextBlock.Text = string.Empty;

            _isApplyingFields = true;

            try
            {
                TitleTextBox.Text = string.Empty;
                ArtistNamesTextBox.Text = string.Empty;
                AlbumTitleTextBox.Text = string.Empty;
                AlbumArtistNameTextBox.Text = string.Empty;
                GenreAutoSuggestBox.Text = string.Empty;
                YearTextBox.Text = string.Empty;
                TrackNumberTextBox.Text = string.Empty;
                DiscNumberTextBox.Text = string.Empty;
                ComposerTextBox.Text = string.Empty;
                CommentTextBox.Text = string.Empty;
            }
            finally
            {
                _isApplyingFields = false;
            }

            UpdateOperationState();
        }

        private MusicMetadataUpdateRequest CreateUpdateRequest()
        {
            Music music = _currentMusic;
            MusicMetadata metadata =
                music.Metadata ?? MusicMetadata.Empty;

            string title = NormalizeText(TitleTextBox.Text);
            List<string> artistNames = ReadArtistNamesFromEditor();
            string albumTitle = NormalizeText(
                AlbumTitleTextBox.Text);
            string albumArtistName = NormalizeText(
                AlbumArtistNameTextBox.Text);
            string genre = NormalizeText(
                GenreAutoSuggestBox.Text);
            int? year = ParseNullablePositiveInt(
                YearTextBox.Text,
                "年份");
            int? trackNumber = ParseNullablePositiveInt(
                TrackNumberTextBox.Text,
                "音轨号");
            int? discNumber = _canEditDiscNumber
                ? ParseNullablePositiveInt(
                    DiscNumberTextBox.Text,
                    "碟片号")
                : metadata.DiscNumber;
            string composer = NormalizeText(
                ComposerTextBox.Text);
            string comment = _canEditComment
                ? NormalizeText(CommentTextBox.Text)
                : NormalizeText(metadata.Comment);

            var request = MusicMetadataUpdateRequest.ForMusic(
                music.Id);

            if (title != NormalizeText(music.Title))
            {
                request.WithTitle(title);
            }

            List<string> storedArtists =
                GetStoredArtistNames(music, metadata);

            if (!artistNames.SequenceEqual(
                storedArtists,
                StringComparer.Ordinal))
            {
                request.WithArtistNames(artistNames);
            }

            if (albumTitle != NormalizeText(music.AlbumTitle))
            {
                request.WithAlbumTitle(albumTitle);
            }

            if (albumArtistName !=
                NormalizeText(metadata.AlbumArtistName))
            {
                request.WithAlbumArtistName(albumArtistName);
            }

            if (genre != NormalizeText(metadata.Genre))
            {
                if (genre.Length > 0 &&
                    genre.All(char.IsDigit))
                {
                    throw new FormatException(
                        "流派不能只包含数字；数字会被 ID3 " +
                        "解释为预定义流派编号，请直接输入流派名称。");
                }

                request.WithGenre(genre);
            }

            if (year != metadata.Year)
            {
                request.WithYear(year);
            }

            if (trackNumber != metadata.TrackNumber)
            {
                request.WithTrackNumber(trackNumber);
            }

            if (_canEditDiscNumber &&
                discNumber != metadata.DiscNumber)
            {
                request.WithDiscNumber(discNumber);
            }

            if (composer != NormalizeText(metadata.Composer))
            {
                request.WithComposer(composer);
            }

            if (_canEditComment &&
                comment != NormalizeText(metadata.Comment))
            {
                request.WithComment(comment);
            }

            return request;
        }

        private List<string> ReadArtistNamesFromEditor()
        {
            string[] lines =
                (ArtistNamesTextBox.Text ?? string.Empty)
                    .Split(
                        new[] { "\r\n", "\n", "\r" },
                        StringSplitOptions.RemoveEmptyEntries);

            return ArtistNameNormalizer.Normalize(lines);
        }

        private static List<string> GetStoredArtistNames(
            Music music,
            MusicMetadata metadata)
        {
            if (metadata.ArtistNames != null &&
                metadata.ArtistNames.Count > 0)
            {
                return ArtistNameNormalizer.Normalize(
                    metadata.ArtistNames);
            }

            if (!string.IsNullOrWhiteSpace(music.ArtistName))
            {
                return ArtistNameNormalizer.Normalize(
                    new[] { music.ArtistName });
            }

            return new List<string>();
        }

        private MusicTagEngine ResolveEffectiveTagEngine(
            Music music,
            AppSettings settings)
        {
            MusicTagEngine engine =
                settings?.Metadata == null
                    ? MusicTagEngine.WindowsApi
                    : settings.Metadata.TagEngine;

            if (!Enum.IsDefined(typeof(MusicTagEngine), engine))
            {
                engine = MusicTagEngine.WindowsApi;
            }

            if (!IsFileBacked(music))
            {
                return engine;
            }

            if ((engine == MusicTagEngine.TagLib ||
                 engine == MusicTagEngine.Hybrid) &&
                !CanWriteWithTagLib())
            {
                return MusicTagEngine.WindowsApi;
            }

            return engine;
        }

        private bool CanEditDiscNumber(
            Music music,
            MusicTagEngine engine)
        {
            if (!IsFileBacked(music))
            {
                return true;
            }

            return engine != MusicTagEngine.WindowsApi &&
                   CanWriteWithTagLib();
        }

        private bool CanEditComment(
            Music music,
            MusicTagEngine engine)
        {
            if (!IsFileBacked(music))
            {
                return true;
            }

            return engine != MusicTagEngine.WindowsApi &&
                   CanWriteWithTagLib();
        }

        private bool CanWriteWithTagLib()
        {
            MusicTagCapabilities capabilities =
                _services?.MusicTagCapabilities;

            return capabilities != null &&
                   capabilities.IsTagLibAvailable &&
                   capabilities.CanWriteTagsWithTagLib;
        }

        private void UpdateOperationState()
        {
            bool canEdit =
                _isAttached && !_isBusy &&
                _currentMusic != null;

            TitleTextBox.IsEnabled = canEdit;
            ArtistNamesTextBox.IsEnabled = canEdit;
            AlbumTitleTextBox.IsEnabled = canEdit;
            AlbumArtistNameTextBox.IsEnabled = canEdit;
            GenreAutoSuggestBox.IsEnabled = canEdit;
            YearTextBox.IsEnabled = canEdit;
            TrackNumberTextBox.IsEnabled = canEdit;
            DiscNumberTextBox.IsEnabled =
                canEdit && _canEditDiscNumber;
            ComposerTextBox.IsEnabled = canEdit;
            CommentTextBox.IsEnabled =
                canEdit && _canEditComment;

            SaveButton.IsEnabled = canEdit && _isDirty;
            ReloadButton.IsEnabled =
                _isAttached && !_isBusy &&
                _playbackService?.State.CurrentMusicId.HasValue == true;
        }

        private bool CanApplyLoad(
            MusicId? musicId,
            int lifetime,
            int version)
        {
            return _isAttached &&
                   lifetime == _lifetimeVersion &&
                   version == _loadVersion &&
                   _playbackService != null &&
                   Nullable.Equals(
                       _playbackService.State.CurrentMusicId,
                       musicId);
        }

        private bool CanContinueInteraction(
            MusicId musicId,
            int lifetime,
            int loadVersion)
        {
            return _isAttached &&
                   lifetime == _lifetimeVersion &&
                   loadVersion == _loadVersion &&
                   _currentMusic != null &&
                   _currentMusic.Id == musicId &&
                   Nullable.Equals(
                       _playbackService?.State.CurrentMusicId,
                       (MusicId?)musicId);
        }

        private static bool IsFileBacked(Music music)
        {
            return music != null &&
                   (music.SourceType == MusicSourceType.Local ||
                    music.SourceType == MusicSourceType.Temporary);
        }

        private static int? ParseNullablePositiveInt(
            string text,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            int value;

            if (!int.TryParse(text.Trim(), out value) || value <= 0)
            {
                throw new FormatException(
                    fieldName + "必须是正整数，留空表示清除。");
            }

            return value;
        }

        private static string ToNullableText(int? value)
        {
            return value.HasValue
                ? value.Value.ToString()
                : string.Empty;
        }

        private static string NormalizeText(string value)
        {
            return value == null
                ? string.Empty
                : value.Trim();
        }

        private static string GetTagEngineText(
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

        private static string GetSourceTypeText(
            MusicSourceType sourceType)
        {
            switch (sourceType)
            {
                case MusicSourceType.Local:
                    return "本地音乐";

                case MusicSourceType.Temporary:
                    return "临时音乐";

                case MusicSourceType.Stream:
                    return "流媒体";

                default:
                    return sourceType.ToString();
            }
        }

        private static async Task<bool>
            ConfirmDiscardChangesAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "放弃未保存的标签修改？",
                Content = "重新加载会丢弃当前尚未保存的修改。",
                PrimaryButtonText = "放弃修改",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close
            };

            return await dialog.ShowAsync() ==
                   ContentDialogResult.Primary;
        }
    }
}
