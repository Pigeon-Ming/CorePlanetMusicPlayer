using System;
using System.IO;

namespace CorePlanetMusicPlayer.Uwp.Platform.Lyrics
{
    internal sealed class UwpTagLibFileAbstraction : TagLib.File.IFileAbstraction
    {
        private readonly Stream _stream;

        public UwpTagLibFileAbstraction(string name, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "文件名不能为空。",
                    nameof(name));
            }

            _stream = stream ??
                throw new ArgumentNullException(nameof(stream));

            Name = name;
        }

        public string Name { get; private set; }

        public Stream ReadStream
        {
            get
            {
                if (!_stream.CanRead)
                {
                    throw new NotSupportedException("文件流不支持读取。");
                }

                return _stream;
            }
        }

        public Stream WriteStream
        {
            get
            {
                if (!_stream.CanWrite)
                {
                    throw new NotSupportedException("文件流不支持写入。");
                }

                return _stream;
            }
        }

        public void CloseStream(Stream stream)
        {
            // 流由调用方管理。TagLib 在读取和保存之间会重复请求同一个流。
        }
    }
}
