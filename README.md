# CorePlanetMusicPlayer Version6.1

### <u>*该版本核心功能已开发完毕，相关测试页面正在完善</u>

CorePlanetMusicPlayer（以下简称CorePMP），是一个UWP平台下的开源音乐播放器。

相关连接：

- [CorePMP介绍网页](http://pigeonming.top/CorePlanetMusicPlayer)

- 基于CorePMP开发的闭源项目：[PlanetMusicPlayer](http://pigeonming.top/PlanetMusicPlayer)（未开源，且暂未更新至最新的CorePMP6.1）

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

| 项目名称及链接 | 功能  |
| ------- | --- |
|         |     |
|         |     |

---

### 项目架构

- CorePlanetMusicPlayer6：实际运行的应用程序
  
  - 仅保留核心功能的PlanetMusicPlayer，主要供开发使用。包含应用页面与控件，并负责各层服务的组装、初始化与应用生命周期管理。

- CorePlanetMusicPlayer.Services：业务服务层
  
  - 音乐库扫描与分类索引、播放列表管理、歌词解析与查找、封面获取、元数据编辑、播放历史与统计、应用设置等业务逻辑与服务封装。

- CorePlanetMusicPlayer.Playback：播放核心
  
  - 播放控制、播放状态与进度管理、播放队列与播放模式逻辑。通过 IAudioPlayer 接口解耦媒体播放引擎，具体实现由平台层提供。

- CorePlanetMusicPlayer.Data：数据持久化层
  
  - SQLite数据库初始化与版本迁移、数据实体与领域模型的映射；通过仓储接口及其实现，提供音乐、专辑、艺术家、播放列表、歌词、音乐库目录与播放历史的存储和查询。

- CorePlanetMusicPlayer.Uwp：UWP平台适配层

  - 提供媒体播放引擎及与 Windows.Media/SMTC 的交互，封装文件与目录选择、存储访问、音乐库扫描、元数据读写、封面与缩略图加载、设置存储等平台功能；负责将 StorageFile 等平台对象映射到领域模型。

- CorePlanetMusicPlayer.Core：领域模型与基础类型

  - 表示音乐、元数据、专辑、艺术家、播放列表、歌词、音乐库目录与播放历史等领域对象，并提供统一标识、结果与参数校验等基础类型，供其他各层使用。

---

#### 在寻找过去的CorePlanetMusicPlayer版本?

[Version4](https://github.com/Pigeon-Ming/CorePlanetMusicPlayer/tree/Version4)

[Version5](https://github.com/Pigeon-Ming/CorePlanetMusicPlayer/tree/Version5)

[[Version6](https://github.com/Pigeon-Ming/CorePlanetMusicPlayer/tree/Version6)](https://github.com/Pigeon-Ming/CorePlanetMusicPlayer/tree/Version6)


