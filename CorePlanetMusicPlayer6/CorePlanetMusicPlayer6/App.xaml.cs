using CorePlanetMusicPlayer6.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CorePlanetMusicPlayer6
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            this.EnteredBackground += OnEnteredBackground;
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }


            // 初始化CorePMP
            if (AppRuntime.Bootstrapper == null)
            {
                AppRuntime.Bootstrapper = new AppBootstrapper();

                await AppRuntime.Bootstrapper.InitializeAsync();
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            try
            {
                // 真正挂起前暂停计时，并保存当前阶段数据。
                await SavePlaybackStateAsync(suspendHistory: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                deferral.Complete();
            }
        }


        private async void OnEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();

            try
            {
                // 后台音乐可能继续播放，因此只保存，不暂停计时。
                await SavePlaybackStateAsync(suspendHistory: false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void OnResuming(object sender, object e)
        {
            try
            {
                AppRuntime.Services?
                    .PlaybackService?
                    .ResumeHistoryTracking();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"恢复播放历史跟踪失败：{ex}");
            }
        }


        /// <summary>
        /// 采集当前历史快照，并保存历史及播放队列。
        /// </summary>
        private async Task SavePlaybackStateAsync(bool suspendHistory)
        {
            var services = AppRuntime.Services;

            if (services == null)
            {
                return;
            }

            try
            {
                var playbackService = services.PlaybackService;

                if (playbackService != null)
                {
                    if (suspendHistory)
                    {
                        playbackService.SuspendHistoryTracking();
                    }
                    else
                    {
                        playbackService.CaptureHistorySnapshot();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"采集播放历史快照失败：{ex}");
            }

            // 两项保存分别处理异常，避免其中一项失败阻断另一项。
            await Task.WhenAll(
                SavePlaybackHistoryAsync(services),
                SavePlaybackQueueAsync(services));
        }

        private async Task SavePlaybackHistoryAsync(AppServices services)
        {
            try
            {
                var recorder = services.PlaybackHistoryRecorder;

                if (recorder != null)
                {
                    await recorder.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                // 未成功写入的快照仍保留在内存队列中。
                Debug.WriteLine($"保存播放历史失败：{ex}");
            }
        }

        private async Task SavePlaybackQueueAsync(AppServices services)
        {
            try
            {
                var sessionService = services.PlaybackSessionService;

                if (sessionService != null)
                {
                    await sessionService.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存播放队列失败：{ex}");
            }
        }
    }
}
