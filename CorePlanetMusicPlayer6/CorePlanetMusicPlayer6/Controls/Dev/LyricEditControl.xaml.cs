using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using CorePlanetMusicPlayer.Playback.Player;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer6.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer6.Controls.Dev
{
    public sealed partial class LyricEditControl : UserControl
    {
        private AppServices _services;
        private IPlaybackService _playbackService;

        private bool _isAttached;
        private bool _isBusy;
        private bool _isDirty;
        private bool _isApplyingEditorText;
        private bool _isApplyingSourceSelection;

        private int _lifetimeVersion;
        private int _loadVersion;

        private MusicId? _currentMusicId;
        private Music _currentMusic;
        private LyricDocument _currentDocument;
        private LyricDocument _preferredDocument;

        public LyricEditControl()
        {
            this.InitializeComponent();
        }

        private async void UserControl_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Detach();

            _services = AppRuntime.Services;
            _playbackService = _services?.PlaybackService;

            if (_services == null)
            {
                ApplyNoMusicState();
                StatusTextBlock.Text = "服务尚未就绪，请检查应用初始化。";
                return;
            }

            if (_playbackService == null ||
                _services.MusicQueryService == null ||
                _services.LyricService == null ||
                _services.LyricEditService == null ||
                _services.LyricSearchService == null)
            {
                ApplyNoMusicState();
                StatusTextBlock.Text = "歌词编辑所需服务尚未就绪。";
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
            _currentDocument = null;
            _preferredDocument = null;

            ClearSourceOptions("尚未加载歌词");
            SetEditorText(string.Empty);
            UpdateOperationState();
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

        private async void ReloadLyricsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_isAttached ||
                _isBusy ||
                _playbackService == null)
            {
                return;
            }

            if (_isDirty && !await ConfirmDiscardChangesAsync())
            {
                return;
            }

            await ReloadCurrentMusicAsync(
                _playbackService.State.CurrentMusicId,
                null,
                _currentDocument);
        }

        private async void SaveLyricsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await SaveLyricsAsync(null);
        }

        private async void WriteEmbeddedLyricsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await SaveLyricsAsync(LyricSaveTarget.Embedded);
        }

        private async void SaveToApplicationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await SaveLyricsAsync(LyricSaveTarget.DatabaseOverride);
        }

        private async Task SaveLyricsAsync(
            LyricSaveTarget? requestedTarget)
        {
            if (!_isAttached ||
                _isBusy ||
                !_currentMusicId.HasValue ||
                _currentMusicId.Value.IsEmpty ||
                _services?.LyricEditService == null)
            {
                return;
            }

            MusicId musicId = _currentMusicId.Value;
            int lifetime = _lifetimeVersion;
            string rawText = LyricEditorTextBox.Text;
            LyricDocument sourceDocument = _currentDocument;

            _isBusy = true;
            UpdateOperationState();

            try
            {
                LyricSaveTarget? selectedTarget = requestedTarget;

                if (!selectedTarget.HasValue)
                {
                    selectedTarget =
                        await SelectSaveTargetAsync(_currentDocument);
                }

                if (!selectedTarget.HasValue ||
                    !CanContinueInteraction(musicId, lifetime))
                {
                    return;
                }

                StatusTextBlock.Text = "正在保存歌词……";

                LyricSaveResult result =
                    await _services.LyricEditService.SaveAsync(
                        musicId,
                        rawText,
                        selectedTarget.Value,
                        sourceDocument);

                if (!CanContinueInteraction(musicId, lifetime))
                {
                    return;
                }

                if (result != null &&
                    !result.Succeeded &&
                    result.CanFallbackToDatabase &&
                    await ConfirmDatabaseFallbackAsync(
                        result.ErrorMessage))
                {
                    if (!CanContinueInteraction(musicId, lifetime))
                    {
                        return;
                    }

                    result = await _services.LyricEditService.SaveAsync(
                        musicId,
                        rawText,
                        LyricSaveTarget.DatabaseOverride,
                        sourceDocument);
                }

                if (!CanContinueInteraction(musicId, lifetime))
                {
                    return;
                }

                if (result == null || !result.Succeeded)
                {
                    StatusTextBlock.Text = result == null
                        ? "保存歌词失败：保存服务没有返回结果。"
                        : "保存歌词失败：" + result.ErrorMessage;
                    return;
                }

                _isDirty = false;
                string successMessage = result.Message;

                _isBusy = false;
                UpdateOperationState();

                await ReloadCurrentMusicAsync(
                    musicId,
                    result.Target,
                    sourceDocument);

                if (CanContinueInteraction(musicId, lifetime))
                {
                    StatusTextBlock.Text =
                        GetSaveSuccessMessage(successMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (CanContinueInteraction(musicId, lifetime))
                {
                    StatusTextBlock.Text =
                        "保存歌词失败：" + ex.Message;
                }
            }
            finally
            {
                if (CanContinueInteraction(musicId, lifetime))
                {
                    _isBusy = false;
                    UpdateOperationState();
                }
            }
        }

        private async void RestoreSourceLyricsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_isAttached ||
                _isBusy ||
                !_currentMusicId.HasValue ||
                _currentMusicId.Value.IsEmpty ||
                _currentDocument == null ||
                !_currentDocument.IsUserOverride ||
                _services?.LyricService == null)
            {
                return;
            }

            if (!await ConfirmRestoreSourceAsync())
            {
                return;
            }

            MusicId musicId = _currentMusicId.Value;
            int lifetime = _lifetimeVersion;

            _isBusy = true;
            UpdateOperationState();
            StatusTextBlock.Text = "正在恢复来源歌词……";

            try
            {
                await _services.LyricService.DeleteUserOverrideAsync(
                    musicId);

                if (!CanContinueInteraction(musicId, lifetime))
                {
                    return;
                }

                _isDirty = false;
                _isBusy = false;
                UpdateOperationState();

                await ReloadCurrentMusicAsync(musicId);

                if (CanContinueInteraction(musicId, lifetime))
                {
                    StatusTextBlock.Text = "已恢复来源歌词。";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (CanContinueInteraction(musicId, lifetime))
                {
                    StatusTextBlock.Text =
                        "恢复来源歌词失败：" + ex.Message;
                }
            }
            finally
            {
                if (CanContinueInteraction(musicId, lifetime))
                {
                    _isBusy = false;
                    UpdateOperationState();
                }
            }
        }

        private void LyricEditorTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (_isApplyingEditorText)
            {
                return;
            }

            _isDirty = true;
            UpdateOperationState();
        }

        private async void LyricSourceComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (_isApplyingSourceSelection)
            {
                return;
            }

            LyricDocument selectedDocument =
                GetSelectedSourceDocument();

            if (IsSameDocument(selectedDocument, _currentDocument))
            {
                return;
            }

            if (!_isAttached ||
                _isBusy ||
                !_currentMusicId.HasValue ||
                _currentMusicId.Value.IsEmpty)
            {
                SelectSourceDocument(_currentDocument);
                return;
            }

            MusicId musicId = _currentMusicId.Value;
            int lifetime = _lifetimeVersion;
            LyricDocument previousDocument = _currentDocument;

            _isBusy = true;
            UpdateOperationState();

            try
            {
                if (_isDirty &&
                    !await ConfirmChangeSourceAsync())
                {
                    SelectSourceDocument(previousDocument);
                    return;
                }

                if (!CanContinueInteraction(musicId, lifetime))
                {
                    return;
                }

                ApplySelectedDocument(selectedDocument);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (CanContinueInteraction(musicId, lifetime))
                {
                    SelectSourceDocument(previousDocument);
                    StatusTextBlock.Text =
                        "切换歌词来源失败：" + ex.Message;
                }
            }
            finally
            {
                if (CanContinueInteraction(musicId, lifetime))
                {
                    _isBusy = false;
                    UpdateOperationState();
                }
            }
        }

        private async Task ReloadCurrentMusicAsync(
            MusicId? musicId,
            LyricSaveTarget? savedTarget = null,
            LyricDocument previouslySelectedDocument = null)
        {
            int version = ++_loadVersion;
            int lifetime = _lifetimeVersion;
            AppServices services = _services;

            _currentMusicId = musicId;
            _currentMusic = null;
            _currentDocument = null;
            _preferredDocument = null;
            _isDirty = false;

            if (!_isAttached || services == null)
            {
                return;
            }

            if (!musicId.HasValue || musicId.Value.IsEmpty)
            {
                _isBusy = false;
                ApplyNoMusicState();
                return;
            }

            _isBusy = true;
            ApplyLoadingState();
            UpdateOperationState();

            try
            {
                Task<Music> musicTask =
                    services.MusicQueryService.GetByIdAsync(musicId.Value);
                Task<IReadOnlyList<LyricDocument>> lyricsTask =
                    services.LyricService.GetAllByMusicIdAsync(
                        musicId.Value);

                await Task.WhenAll(musicTask, lyricsTask);

                Music music = await musicTask;
                IReadOnlyList<LyricDocument> documents =
                    await lyricsTask;
                bool previousSourceUnavailable =
                    previouslySelectedDocument != null &&
                    FindMatchingDocument(
                        documents,
                        previouslySelectedDocument) == null;
                LyricDocument document = SelectDocumentForEditor(
                    documents,
                    services.LyricSearchService,
                    savedTarget,
                    previouslySelectedDocument);

                if (!CanApplyResult(
                    musicId.Value,
                    version,
                    lifetime,
                    services))
                {
                    return;
                }

                ApplyLoadedData(music, documents, document);

                if (previousSourceUnavailable &&
                    !savedTarget.HasValue)
                {
                    StatusTextBlock.Text = document == null
                        ? "原先选择的歌词来源已不可用。"
                        : "原先选择的歌词来源已不可用，已切换到：" +
                            GetLyricSourceOptionText(document);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (CanApplyResult(
                    musicId.Value,
                    version,
                    lifetime,
                    services))
                {
                    SetEditorText(string.Empty);
                    StatusTextBlock.Text =
                        "加载歌词失败：" + ex.Message;
                }
            }
            finally
            {
                if (CanApplyResult(
                    musicId.Value,
                    version,
                    lifetime,
                    services))
                {
                    _isBusy = false;
                    UpdateOperationState();
                }
            }
        }

        private void ApplyNoMusicState()
        {
            _currentMusicId = null;
            _currentMusic = null;
            _currentDocument = null;
            _preferredDocument = null;
            _isDirty = false;

            MusicTitleTextBlock.Text = "尚未播放";
            MusicArtistTextBlock.Text = string.Empty;
            ClearSourceOptions("尚未加载歌词");
            SetEditorText(string.Empty);
            StatusTextBlock.Text = "请先从音乐列表播放一首歌曲。";

            UpdateOperationState();
        }

        private void ApplyLoadingState()
        {
            MusicTitleTextBlock.Text = "正在读取歌曲资料……";
            MusicArtistTextBlock.Text = string.Empty;
            ClearSourceOptions("正在加载歌词……");
            SetEditorText(string.Empty);
            StatusTextBlock.Text = "正在读取当前歌曲和歌词。";
        }

        private void ApplyLoadedData(
            Music music,
            IReadOnlyList<LyricDocument> documents,
            LyricDocument document)
        {
            _currentMusic = music;
            _currentDocument = document;
            _preferredDocument =
                _services?.LyricSearchService?.SelectPreferred(documents);
            _isDirty = false;

            SetSourceOptions(documents, document);

            if (music == null)
            {
                MusicTitleTextBlock.Text = "歌曲资料已不在音乐库中";
                MusicArtistTextBlock.Text = string.Empty;
            }
            else
            {
                MusicTitleTextBlock.Text = string.IsNullOrWhiteSpace(
                    music.Title)
                    ? "未知标题"
                    : music.Title;
                MusicArtistTextBlock.Text = string.IsNullOrWhiteSpace(
                    music.ArtistName)
                    ? "未知艺术家"
                    : music.ArtistName;
            }

            if (document == null)
            {
                SetEditorText(string.Empty);
                StatusTextBlock.Text =
                    "可以粘贴带时间标签的 LRC 歌词。";
                return;
            }

            SetEditorText(document.RawText ?? string.Empty);
            StatusTextBlock.Text = "歌词已加载，可以开始编辑。";
        }

        private void SetEditorText(string text)
        {
            _isApplyingEditorText = true;

            try
            {
                LyricEditorTextBox.Text = text ?? string.Empty;
            }
            finally
            {
                _isApplyingEditorText = false;
            }
        }

        private void UpdateOperationState()
        {
            bool hasMusic = _currentMusicId.HasValue &&
                !_currentMusicId.Value.IsEmpty;
            bool canOperate = _isAttached && !_isBusy;
            bool hasEditorText = !string.IsNullOrWhiteSpace(
                LyricEditorTextBox.Text);
            bool isExternalSource = IsExternalSource(_currentDocument);
            bool canWriteEmbedded =
                isExternalSource && CanWriteEmbeddedLyrics(_currentMusic);

            ReloadLyricsButton.IsEnabled = canOperate && hasMusic;
            SaveLyricsButton.IsEnabled =
                canOperate && hasMusic && hasEditorText;
            WriteEmbeddedLyricsButton.Visibility = canWriteEmbedded
                ? Visibility.Visible
                : Visibility.Collapsed;
            WriteEmbeddedLyricsButton.IsEnabled =
                canOperate && hasEditorText && canWriteEmbedded;
            SaveToApplicationButton.Visibility = isExternalSource
                ? Visibility.Visible
                : Visibility.Collapsed;
            SaveToApplicationButton.IsEnabled =
                canOperate && hasMusic && hasEditorText && isExternalSource;
            RestoreSourceLyricsButton.IsEnabled =
                canOperate &&
                hasMusic &&
                _currentDocument != null &&
                _currentDocument.IsUserOverride;
            LyricSourceComboBox.IsEnabled =
                canOperate && LyricSourceComboBox.Items.Count > 0;
            LyricEditorTextBox.IsEnabled = canOperate && hasMusic;
        }

        private void ApplySelectedDocument(
            LyricDocument document)
        {
            _currentDocument = document;
            _isDirty = false;

            SetEditorText(document?.RawText ?? string.Empty);

            StatusTextBlock.Text = document == null
                ? "当前歌曲暂无歌词。"
                : "正在编辑：" + GetLyricSourceOptionText(document);
        }

        private void SetSourceOptions(
            IReadOnlyList<LyricDocument> documents,
            LyricDocument selectedDocument)
        {
            _isApplyingSourceSelection = true;

            try
            {
                LyricSourceComboBox.Items.Clear();
                ComboBoxItem selectedItem = null;

                if (documents != null)
                {
                    for (int i = 0; i < documents.Count; i++)
                    {
                        LyricDocument document = documents[i];

                        if (document == null)
                        {
                            continue;
                        }

                        var item = new ComboBoxItem
                        {
                            Content = GetLyricSourceOptionText(
                                document,
                                IsSameDocument(
                                    document,
                                    _preferredDocument)),
                            Tag = document
                        };

                        LyricSourceComboBox.Items.Add(item);

                        if (IsSameDocument(document, selectedDocument))
                        {
                            selectedItem = item;
                        }
                    }
                }

                LyricSourceComboBox.PlaceholderText =
                    LyricSourceComboBox.Items.Count == 0
                        ? "当前歌曲暂无歌词"
                        : "选择歌词来源";
                LyricSourceComboBox.SelectedItem = selectedItem;
            }
            finally
            {
                _isApplyingSourceSelection = false;
            }
        }

        private void ClearSourceOptions(string placeholderText)
        {
            _isApplyingSourceSelection = true;

            try
            {
                LyricSourceComboBox.SelectedItem = null;
                LyricSourceComboBox.Items.Clear();
                LyricSourceComboBox.PlaceholderText =
                    placeholderText ?? string.Empty;
            }
            finally
            {
                _isApplyingSourceSelection = false;
            }
        }

        private LyricDocument GetSelectedSourceDocument()
        {
            var item = LyricSourceComboBox.SelectedItem as ComboBoxItem;

            return item?.Tag as LyricDocument;
        }

        private void SelectSourceDocument(LyricDocument document)
        {
            _isApplyingSourceSelection = true;

            try
            {
                ComboBoxItem selectedItem = null;

                for (int i = 0;
                    i < LyricSourceComboBox.Items.Count;
                    i++)
                {
                    var item =
                        LyricSourceComboBox.Items[i] as ComboBoxItem;
                    var itemDocument = item?.Tag as LyricDocument;

                    if (IsSameDocument(itemDocument, document))
                    {
                        selectedItem = item;
                        break;
                    }
                }

                LyricSourceComboBox.SelectedItem = selectedItem;
            }
            finally
            {
                _isApplyingSourceSelection = false;
            }
        }

        private static LyricDocument SelectDocumentForEditor(
            IReadOnlyList<LyricDocument> documents,
            LyricSearchService searchService,
            LyricSaveTarget? savedTarget,
            LyricDocument previouslySelectedDocument)
        {
            if (documents == null || documents.Count == 0)
            {
                return null;
            }

            LyricDocument document = FindSavedDocument(
                documents,
                savedTarget,
                previouslySelectedDocument);

            if (document != null)
            {
                return document;
            }

            if (previouslySelectedDocument != null)
            {
                document = FindMatchingDocument(
                    documents,
                    previouslySelectedDocument);

                if (document != null)
                {
                    return document;
                }
            }

            document = searchService?.SelectPreferred(documents);

            if (document != null)
            {
                return document;
            }

            for (int i = 0; i < documents.Count; i++)
            {
                if (documents[i] != null)
                {
                    return documents[i];
                }
            }

            return null;
        }

        private static LyricDocument FindSavedDocument(
            IReadOnlyList<LyricDocument> documents,
            LyricSaveTarget? savedTarget,
            LyricDocument previouslySelectedDocument)
        {
            if (!savedTarget.HasValue)
            {
                return null;
            }

            for (int i = 0; i < documents.Count; i++)
            {
                LyricDocument document = documents[i];

                if (document == null)
                {
                    continue;
                }

                switch (savedTarget.Value)
                {
                    case LyricSaveTarget.DatabaseOverride:
                        if (document.IsUserOverride)
                        {
                            return document;
                        }
                        break;

                    case LyricSaveTarget.Embedded:
                        if (!document.IsUserOverride &&
                            document.SourceType ==
                                LyricSourceType.Embedded)
                        {
                            return document;
                        }
                        break;

                    case LyricSaveTarget.ExternalFile:
                        if (!document.IsUserOverride &&
                            document.SourceType ==
                                LyricSourceType.ExternalFile &&
                            HasSameSourcePath(
                                document,
                                previouslySelectedDocument))
                        {
                            return document;
                        }
                        break;
                }
            }

            return null;
        }

        private static LyricDocument FindMatchingDocument(
            IReadOnlyList<LyricDocument> documents,
            LyricDocument expectedDocument)
        {
            for (int i = 0; i < documents.Count; i++)
            {
                if (IsSameDocument(documents[i], expectedDocument))
                {
                    return documents[i];
                }
            }

            return null;
        }

        private static bool IsSameDocument(
            LyricDocument left,
            LyricDocument right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(left.Id) &&
                !string.IsNullOrWhiteSpace(right.Id))
            {
                if (string.Equals(
                    left.Id,
                    right.Id,
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return left.IsUserOverride == right.IsUserOverride &&
                left.SourceType == right.SourceType &&
                HasSameSourcePath(left, right);
        }

        private static bool HasSameSourcePath(
            LyricDocument left,
            LyricDocument right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            return string.Equals(
                left.SourcePath ?? string.Empty,
                right.SourcePath ?? string.Empty,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool CanWriteEmbeddedLyrics(Music music)
        {
            if (_services?.EmbeddedLyricAccessor == null ||
                music == null ||
                music.FileInfo == null)
            {
                return false;
            }

            if (music.SourceType != MusicSourceType.Local &&
                music.SourceType != MusicSourceType.Temporary)
            {
                return false;
            }

            return music.FileInfo.HasLibraryFolder &&
                (music.FileInfo.HasRelativePath ||
                 music.FileInfo.HasPath);
        }

        private static bool IsExternalSource(
            LyricDocument document)
        {
            return document != null &&
                document.SourceType == LyricSourceType.ExternalFile &&
                !string.IsNullOrWhiteSpace(document.SourcePath);
        }

        private bool CanApplyResult(
            MusicId musicId,
            int version,
            int lifetime,
            AppServices services)
        {
            return _isAttached &&
                   lifetime == _lifetimeVersion &&
                   version == _loadVersion &&
                   ReferenceEquals(services, _services) &&
                   _playbackService != null &&
                   Nullable.Equals(
                       _playbackService.State.CurrentMusicId,
                       (MusicId?)musicId);
        }

        private bool CanContinueInteraction(
            MusicId musicId,
            int lifetime)
        {
            return _isAttached &&
                   lifetime == _lifetimeVersion &&
                   _playbackService != null &&
                   Nullable.Equals(_currentMusicId, (MusicId?)musicId) &&
                   Nullable.Equals(
                       _playbackService.State.CurrentMusicId,
                       (MusicId?)musicId);
        }

        private async Task<LyricSaveTarget?> SelectSaveTargetAsync(
            LyricDocument document)
        {
            if (document != null &&
                document.SourceType == LyricSourceType.ExternalFile &&
                !string.IsNullOrWhiteSpace(document.SourcePath))
            {
                return LyricSaveTarget.ExternalFile;
            }

            var dialog = new ContentDialog
            {
                Title = "保存歌词",
                Content = CreateDialogText(
                    "是否将歌词写入音频文件？写入后，其他支持歌词标签的软件也可以读取这份歌词。"),
                PrimaryButtonText = "写入音频",
                SecondaryButtonText = "仅保存在应用中",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return LyricSaveTarget.Embedded;
            }

            if (result == ContentDialogResult.Secondary)
            {
                return LyricSaveTarget.DatabaseOverride;
            }

            return null;
        }

        private static async Task<bool> ConfirmDatabaseFallbackAsync(
            string errorMessage)
        {
            string reason = string.IsNullOrWhiteSpace(errorMessage)
                ? "无法写入歌词文件。"
                : errorMessage;

            var dialog = new ContentDialog
            {
                Title = "无法写入歌词文件",
                Content = CreateDialogText(
                    reason + "\n\n是否改为保存为应用缓存？"),
                PrimaryButtonText = "保存到应用",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            };

            return await dialog.ShowAsync() ==
                ContentDialogResult.Primary;
        }

        private static async Task<bool> ConfirmRestoreSourceAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "恢复来源歌词",
                Content = CreateDialogText(
                    "这会删除应用缓存中的歌词修改，并重新读取外部、内嵌或在线歌词。"),
                PrimaryButtonText = "恢复",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close
            };

            return await dialog.ShowAsync() ==
                ContentDialogResult.Primary;
        }

        private static async Task<bool> ConfirmDiscardChangesAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "放弃未保存的修改？",
                Content = CreateDialogText(
                    "重新加载会丢弃编辑框中尚未保存的内容。"),
                PrimaryButtonText = "重新加载",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close
            };

            return await dialog.ShowAsync() ==
                ContentDialogResult.Primary;
        }

        private static async Task<bool> ConfirmChangeSourceAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "放弃未保存的修改？",
                Content = CreateDialogText(
                    "切换歌词来源会丢弃编辑框中尚未保存的内容。"),
                PrimaryButtonText = "切换来源",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close
            };

            return await dialog.ShowAsync() ==
                ContentDialogResult.Primary;
        }

        private static TextBlock CreateDialogText(string text)
        {
            return new TextBlock
            {
                Text = text ?? string.Empty,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private string GetSaveSuccessMessage(string successMessage)
        {
            string message = string.IsNullOrWhiteSpace(successMessage)
                ? "歌词保存成功。"
                : successMessage.Trim();

            if (_currentDocument == null ||
                _preferredDocument == null ||
                IsSameDocument(_currentDocument, _preferredDocument))
            {
                return message;
            }

            message = message.TrimEnd('。', '；');

            return message + "；播放器仍优先使用" +
                GetLyricSourceText(_preferredDocument) + "。";
        }

        private static string GetLyricSourceText(
            LyricDocument document)
        {
            if (document == null)
            {
                return "未知来源";
            }

            if (document.IsUserOverride)
            {
                return "应用缓存";
            }

            string sourceText;

            switch (document.SourceType)
            {
                case LyricSourceType.ExternalFile:
                    sourceText = "外部歌词";
                    break;
                case LyricSourceType.Embedded:
                    sourceText = "内嵌歌词";
                    break;
                case LyricSourceType.Online:
                    sourceText = "在线歌词";
                    break;
                default:
                    sourceText = "未知来源";
                    break;
            }

            return sourceText;
        }

        private static string GetLyricSourceOptionText(
            LyricDocument document,
            bool isPreferred = false)
        {
            string sourceText = GetLyricSourceText(document);

            if (document != null &&
                !document.IsUserOverride &&
                document.SourceType == LyricSourceType.ExternalFile &&
                !string.IsNullOrWhiteSpace(document.SourcePath))
            {
                sourceText += " — " + document.SourcePath;
            }

            return isPreferred
                ? sourceText + "（播放器首选）"
                : sourceText;
        }
    }
}
