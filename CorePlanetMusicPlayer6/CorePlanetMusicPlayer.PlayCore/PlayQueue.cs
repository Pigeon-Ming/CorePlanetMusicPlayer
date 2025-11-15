using CorePlanetMusicPlayer.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Storage;

namespace CorePlanetMusicPlayer.PlayCore
{
    public class PlayQueue
    {
        private static StorageFile LastPlayQueueFile { get; set; }

        private IPlayEngine playEngine { get; set; }

        public static async Task Init()
        {
            StorageFolder storageFolder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            LastPlayQueueFile = await StorageHelper.GetStorageFileFromStorageFolderAsync(storageFolder,"LastPlayQueue.json");
        }


        public PlayQueue(IPlayEngine playEngine)
        {
            PlayModes.Add(new PlayMode.RepeatAll(this));
            PlayModes.Add(new PlayMode.Shuffle(this));
            PlayModes.Add(new PlayMode.RepeatOne(this));
            PlayModes.Add(new PlayMode.Reverse(this));

            this.playEngine = playEngine;
        }
        
        private int currentIndex = -1;

        public int CurrentIndex { get { return currentIndex; } 
            set 
            { 
                currentIndex = value; 
            } 
        }

        public List<IMusic> NormalQueue { get;private set; } = new List<IMusic>();

        public List<IMusic> ShuffleQueue { get;private set; } = new List<IMusic>();

        public enum PlayModeEnum
        {
            RepeatAll, Shuffle, RepeatOne, Reverse
        }

        public List<IPlayMode> PlayModes { get; set; } = new List<IPlayMode>();

        public PlayModeEnum CurrentPlayModeEnum { get;private set; } = PlayModeEnum.RepeatAll;



        public event EventHandler PlayQueueChanged;

        public event EventHandler CurrentIndexChanging;

        public event EventHandler CurrentIndexChanged;

        public event EventHandler CurrentPlayModeChanged;

        //播放
        public IMusic Next()
        {
            int newIndex = -1;
            switch(CurrentPlayModeEnum)
            {
                case PlayModeEnum.RepeatAll:
                    newIndex = PlayModes[0].Next(currentIndex);
                    break;
                case PlayModeEnum.Shuffle:
                    newIndex = PlayModes[1].Next(currentIndex);
                    break;
                case PlayModeEnum.Reverse:
                    newIndex = PlayModes[3].Next(currentIndex);
                    break;
                default:
                    newIndex = PlayModes[2].Next(currentIndex);
                    break;
                
            }
            SetCurrentIndex(newIndex);
            return GetCurrentMusic();
        }

        public IMusic Previous()
        {
            int newIndex = -1;
            switch (CurrentPlayModeEnum)
            {
                case PlayModeEnum.RepeatAll:
                    newIndex = PlayModes[0].Previous(currentIndex);
                    break;
                case PlayModeEnum.Shuffle:
                    newIndex = PlayModes[1].Previous(currentIndex);
                    break;
                case PlayModeEnum.Reverse:
                    newIndex = PlayModes[3].Previous(currentIndex);
                    break;
                default:
                    newIndex = PlayModes[2].Previous(currentIndex);
                    break;

            }
            SetCurrentIndex(newIndex);
            return GetCurrentMusic();
        }

        public void NextPlayMode()
        {
            if (CurrentPlayModeEnum == PlayModeEnum.RepeatAll)
                CurrentPlayModeEnum = PlayModeEnum.Shuffle;
            else if (CurrentPlayModeEnum == PlayModeEnum.Shuffle)
                CurrentPlayModeEnum = PlayModeEnum.RepeatOne;
            else if (CurrentPlayModeEnum == PlayModeEnum.RepeatOne)
                CurrentPlayModeEnum = PlayModeEnum.Reverse;
            else
                CurrentPlayModeEnum = PlayModeEnum.RepeatAll;
            Debug.WriteLine("当前播放模式：" + CurrentPlayModeEnum.ToString());
            SetPlayMode(CurrentPlayModeEnum);
        }

        //列表管理
        public void ClearPlayQueue()
        {
            NormalQueue.Clear();
            ShuffleQueue.Clear();
            CurrentIndex = -1;
        }
        public void SetQueue(List<IMusic>musicList)//设置播放队列
        {
            switch(CurrentPlayModeEnum)
            {
                case PlayModeEnum.RepeatAll:
                case PlayModeEnum.RepeatOne:
                    NormalQueue = musicList.ToList();
                    break;
                case PlayModeEnum.Reverse:
                    NormalQueue = musicList.ToList();
                    NormalQueue.Reverse();
                    break;
                case PlayModeEnum.Shuffle:
                    NormalQueue = musicList.ToList();
                    ShuffleQueue = CreateShuffleQueue(NormalQueue);
                    break;
            }
            //if(true)//To-Do: To-Do: 添加判断是否存储恢复播放列表的数据
                SavePlayQueueToStorage();
        }

        public List<IMusic> GetQueue()
        {
            if (CurrentPlayModeEnum == PlayModeEnum.Shuffle)
                return ShuffleQueue;
            else
                return NormalQueue;
        }

        public void AddNextMusic(IMusic music)//添加歌曲至下一首播放
        {
            if (CurrentPlayModeEnum == PlayModeEnum.Shuffle)
                ShuffleQueue.Insert(CurrentIndex, music);
            else
                NormalQueue.Insert(CurrentIndex, music);
        }

        public void AddNextMusicList(List<IMusic>musicList)//添加歌曲列表至本曲之后
        {
            if (CurrentPlayModeEnum == PlayModeEnum.Shuffle)
                ShuffleQueue.InsertRange(CurrentIndex, musicList.ToList());
            else
                NormalQueue.InsertRange(CurrentIndex, musicList.ToList());
        }

        public void AddMusic(IMusic music)//添加歌曲至播放队列尾部
        {
            if (CurrentPlayModeEnum == PlayModeEnum.Shuffle)
                ShuffleQueue.Add(music);
            else
                NormalQueue.Add(music);
        }

        public void AddMusicList(List<IMusic>musicList)//添加歌曲列表至播放队列尾部
        {
            if(CurrentPlayModeEnum == PlayModeEnum.Shuffle)
                ShuffleQueue.AddRange(musicList.ToList());
            else
                NormalQueue.AddRange(musicList.ToList());
        }

        public void RemoveAt(int index)//移除对应索引歌曲
        {
            if (CurrentPlayModeEnum == PlayModeEnum.Shuffle)
            {
                NormalQueue.Remove(ShuffleQueue[index]);
                ShuffleQueue.RemoveAt(index);
            }
            else
            {
                ShuffleQueue.Remove(NormalQueue[index]);
                NormalQueue.RemoveAt(index);
            }
        }

        public void Remove(Music music)//移除对应歌曲
        {
            NormalQueue.Remove(music);
            ShuffleQueue.Remove(music);
        }

        public List<IMusic> CreateShuffleQueue(List<IMusic>sourceData)//创建随机播放列表
        {
            Random random = new Random();
            List<IMusic> musicList = sourceData.ToList();
            List<IMusic> newQueue = new List<IMusic>();
            int index;
            while (musicList.Count != 0)
            {
                index = random.Next(musicList.Count);
                newQueue.Add(musicList[index]);
                musicList.RemoveAt(index);
            }
            return newQueue;
        }

        public void SetCurrentIndex(int newIndex)//设置正在播放索引
        {
            CurrentIndexChanging?.Invoke(this, null);
            CurrentIndex = newIndex;
            //if()//To-Do: 添加判断是否存储恢复播放列表的数据
            SavePlayQueueIndexToStorage();
            CurrentIndexChanged?.Invoke(this,null);
        }

        public void SetPlayMode(PlayModeEnum newPlayModeEnum)//设置播放模式
        {
            CurrentPlayModeEnum = newPlayModeEnum;
            if (newPlayModeEnum == PlayModeEnum.Shuffle)
                ShuffleQueue = CreateShuffleQueue(NormalQueue);
            CurrentPlayModeChanged?.Invoke(this, null);
        }

        public IMusic GetCurrentMusic()//获取正在播放的歌曲
        {
            switch (CurrentPlayModeEnum)
            {
                case PlayModeEnum.RepeatAll:
                case PlayModeEnum.RepeatOne:
                case PlayModeEnum.Reverse:
                    if (NormalQueue.Count > 0 && currentIndex != -1 && NormalQueue.Count > CurrentIndex)
                        return NormalQueue[CurrentIndex];
                    else
                        return null;
                case PlayModeEnum.Shuffle:
                    if (ShuffleQueue.Count > 0 && currentIndex != -1 && ShuffleQueue.Count > CurrentIndex)
                        return ShuffleQueue[CurrentIndex];
                    else
                        return null;
            }
            return null;
        }

        public void SavePlayQueueToStorage()
        {
            List<JMusic> jMusicList;
            if (CurrentPlayModeEnum == PlayModeEnum.Shuffle)
                jMusicList = JMusicHelper.GetJMusicList(ShuffleQueue);
            else
                jMusicList = JMusicHelper.GetJMusicList(NormalQueue);
            string json = JMusicHelper.GetJMusicListJsonString(jMusicList);
            //Debug.WriteLine($"设置上次播放列表：{json}");
            _ = StorageHelper.WriteStringToFileAsync(LastPlayQueueFile,json);
        }

        public void SavePlayQueueIndexToStorage()
        {
            SettingsManager.SetSetting("CorePlanetMusicPlayer_LastPlayQueueIndex", CurrentIndex.ToString());
            Debug.WriteLine($"设置上次播放列表播放项索引：{currentIndex}");
        }

        public async Task ResumePlayQueueAsync(List<LocalMusic> localMusicList)
        {
            string json = await StorageHelper.ReadFileAsStringAsync(LastPlayQueueFile);
            if (String.IsNullOrEmpty(json))
                return;
            JArray jArray = JArray.Parse(json);
            List<IMusic> queue= JMusicHelper.GetMusicFromJArray(jArray, localMusicList);
            SetQueue(queue);
            string indexStr = SettingsManager.GetSettingContent("CorePlanetMusicPlayer_LastPlayQueueIndex").ToString();
            int index = Convert.ToInt32(indexStr);
            if (index >= localMusicList.Count || index < 0)
                index = 0;
            SetCurrentIndex(index);
            playEngine.SetMediaSource(index, queue);
        }
    }
}
