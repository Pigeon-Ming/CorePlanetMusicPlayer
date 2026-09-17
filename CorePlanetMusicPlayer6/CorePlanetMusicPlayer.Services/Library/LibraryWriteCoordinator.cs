using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library
{
    /// <summary>
    /// 协调分类索引重建与分类资料编辑。
    /// 所有参与协调的服务必须使用同一个实例。
    /// </summary>
    public sealed class LibraryWriteCoordinator
    {
        private readonly SemaphoreSlim _gate =
            new SemaphoreSlim(1, 1);

        public async Task ExecuteAsync(Func<Task> operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            await _gate.WaitAsync();

            try
            {
                await operation();
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
