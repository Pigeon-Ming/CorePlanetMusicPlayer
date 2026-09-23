using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library
{

    /// <summary>
    /// 刷新音乐库的结果类
    /// </summary>
    public sealed class LibraryRefreshResult
    {
        private readonly List<string> _errors = new List<string>();

        /// <summary>
        /// 本次尝试刷新的目录数
        /// </summary>
        public int FolderCount { get; private set; }

        /// <summary>
        /// 本次成功用扫描结果更新的目录数
        /// </summary>
        public int UpdatedFolderCount { get; private set; }

        public int ScannedMusicCount { get; private set; }

        public int SavedMusicCount { get; private set; }

        public int SkippedMusicCount { get; private set; }

        public IReadOnlyList<string> Errors
        {
            get { return _errors.AsReadOnly(); }
        }

        public bool HasError
        {
            get { return _errors.Count > 0; }
        }

        public void AddFolder()
        {
            FolderCount++;
        }

        public void AddUpdatedFolder()
        {
            UpdatedFolderCount++;
        }

        public void AddScannedMusic(int count)
        {
            if (count > 0)
            {
                ScannedMusicCount += count;
            }
        }

        public void AddSavedMusic(int count)
        {
            if (count > 0)
            {
                SavedMusicCount += count;
            }
        }

        public void AddSkippedMusic(int count)
        {
            if (count > 0)
            {
                SkippedMusicCount += count;
            }
        }

        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _errors.Add(message);
            }
        }
    }
}
