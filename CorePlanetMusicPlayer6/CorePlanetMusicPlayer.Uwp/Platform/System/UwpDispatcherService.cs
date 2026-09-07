using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace CorePlanetMusicPlayer.Uwp.Platform.System
{
    /// <summary>
    /// 将代码切回UI线程的工具类
    /// </summary>
    public sealed class UwpDispatcherService
    {
        private readonly CoreDispatcher _dispatcher;

        public UwpDispatcherService()
            : this(GetDefaultDispatcher())
        {
        }

        public UwpDispatcherService(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public bool HasDispatcher
        {
            get { return _dispatcher != null; }
        }

        public bool HasThreadAccess
        {
            get
            {
                return _dispatcher != null && _dispatcher.HasThreadAccess;
            }
        }

        public async Task RunAsync(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (_dispatcher == null)
            {
                action();
                return;
            }

            if (_dispatcher.HasThreadAccess)
            {
                action();
                return;
            }

            await _dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () => action());
        }

        public async Task<T> RunAsync<T>(Func<T> action)
        {
            if (action == null)
            {
                return default(T);
            }

            if (_dispatcher == null)
            {
                return action();
            }

            if (_dispatcher.HasThreadAccess)
            {
                return action();
            }

            var taskCompletionSource = new TaskCompletionSource<T>();

            await _dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    try
                    {
                        taskCompletionSource.SetResult(action());
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(ex);
                    }
                });

            return await taskCompletionSource.Task;
        }

        private static CoreDispatcher GetDefaultDispatcher()
        {
            try
            {
                var mainView = CoreApplication.MainView;

                if (mainView == null ||
                    mainView.CoreWindow == null)
                {
                    return null;
                }

                return mainView.CoreWindow.Dispatcher;
            }
            catch
            {
                return null;
            }
        }
    }
}
