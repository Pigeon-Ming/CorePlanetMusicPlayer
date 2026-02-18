# CorePlanetMusicPlayer Version6

### <u>*该版本主要功能仍在开发！！！</u>

CorePlanetMusicPlayer（以下简称CorePMP），是一个UWP平台下的开源音乐播放器。

相关连接：

- [CorePMP介绍网页](http://pigeonming.top/index.php/coreplanetmusicplayer)

- 基于CorePMP开发的完整项目：[PlanetMusicPlayer](http://pigeonming.top/index.php/planetmusicplayer)（未开源，且暂未更新至最新的CorePMP6）

---

### 功能

- 音乐播放
  
  - 支持.mp3、.flac、.wma、.ac3、.aac、.wav格式音频文件的播放
  - 播放队列管理，支持正序、循环、单曲循环、随机、倒序播放

- 音乐管理
  
  - 查看设备上指定目录中的音乐，并以SQLite缓存歌曲信息
  
  - 自动以专辑、艺术家、年份、流派分类音乐
  
  - 创建自定义播放列表

- 滚动歌词（LRC格式）

- 播放统计

### 引用

| 项目名称及链接                                                         | 功能        |
| --------------------------------------------------------------- | --------- |
| [mono/taglib-sharp](https://github.com/mono/taglib-sharp)       | 读取音乐文件信息  |
| [Pigeon-Ming/UWPTools](https://github.com/Pigeon-Ming/UWPTools) | UWP C#帮助类 |

---

### 项目架构

- CorePlanetMusicPlayer6：实际运行的应用程序
  
  - 仅保留核心功能的PlanetMusicPlayer，主要供开发使用。

- CorePlanetMusicPlayer.App：抽象应用/服务层
  
  * 应用范围的业务逻辑、状态管理、设置持久化、全局事件与服务封装。

- CorePlanetMusicPlayer.PlayCore：播放核心
  
  - 媒体播放引擎、播放队列与播放模式逻辑、与 Windows.Media/SMTC 的交互。通过接口解耦引擎实现（可替换实现）。

- CorePlanetMusicPlayer.Modals：
  
  - 表示音乐、元数据、歌词等领域对象；提供将平台对象（如 StorageFile）映射到领域模型的适配器（例如 UwpStorageFileAbstraction）。

---

#### 在寻找过去的CorePlanetMusicPlayer版本?

[Version4](https://github.com/Pigeon-Ming/CorePlanetMusicPlayer/tree/Version4)

[Version5](https://github.com/Pigeon-Ming/CorePlanetMusicPlayer/tree/Version5)
