# 歌词模块

歌词模块负责发现、读取、解析、选择、展示和编辑当前歌曲的同步歌词。当前初版已经建立从外部 LRC、音频内嵌标签和数据库到播放控件与编辑控件的完整链路。

## 当前能力

- 解析标准 LRC 时间标签。
- 支持一个文本行包含多个时间标签。
- 支持相同时间的原文与翻译行合并。
- 支持使用 `「」` 标记的行内翻译。
- 在歌曲所在目录自动发现同名 `.lrc` 文件。
- 支持使用用户设置的正则表达式补充匹配外部歌词文件名。
- 使用 TagLibSharp 读取和写入音频内嵌歌词。
- 按确定的优先级选择播放器使用的歌词。
- 在编辑器中查看不同来源，而不改变播放器的全局读取顺序。
- 将用户修改写回外部文件、写入音频标签，或保存为应用缓存。
- 播放时高亮和滚动当前歌词，支持手动浏览、恢复跟随和点击跳转。

## 模块边界

歌词功能跨越以下项目：

| 项目 | 职责 |
| --- | --- |
| `CorePlanetMusicPlayer.Core` | 歌词文档、歌词行和来源枚举 |
| `CorePlanetMusicPlayer.Data` | SQLite 实体、映射和仓储 |
| `CorePlanetMusicPlayer.Services` | LRC 解析、来源选择、歌词读取编排和编辑保存流程 |
| `CorePlanetMusicPlayer.Uwp` | UWP 文件访问、外部 LRC 读写和 TagLibSharp 适配 |
| `CorePlanetMusicPlayer6` | 滚动歌词控件、歌词编辑控件和运行时服务注册 |

主要代码入口：

- [`LyricDocument`](../../../CorePlanetMusicPlayer.Core/Lyrics/LyricDocument.cs)
- [`LrcParser`](../../../CorePlanetMusicPlayer.Services/Lyrics/LrcParser.cs)
- [`LyricService`](../../../CorePlanetMusicPlayer.Services/Lyrics/LyricService.cs)
- [`LyricEditService`](../../../CorePlanetMusicPlayer.Services/Lyrics/LyricEditService.cs)
- [`UwpExternalLyricReader`](../../../CorePlanetMusicPlayer.Uwp/Platform/Lyrics/UwpExternalLyricReader.cs)
- [`UwpEmbeddedLyricAccessor`](../../../CorePlanetMusicPlayer.Uwp/Platform/Lyrics/UwpEmbeddedLyricAccessor.cs)
- [`LyricControl`](../../../CorePlanetMusicPlayer6/Controls/Dev/LyricControl.xaml)
- [`LyricEditControl`](../../../CorePlanetMusicPlayer6/Controls/Dev/LyricEditControl.xaml)

## 核心数据模型

`LyricSourceType` 表示歌词的实际来源：

```csharp
Unknown = 0
Embedded = 1
ExternalFile = 2
Online = 3
```

“应用缓存”不是独立的来源枚举。用户在应用内保存的覆盖版本使用 `LyricDocument.IsUserOverride` 标记，并保留其原始来源类型。UI 对所有用户覆盖统一显示为“应用缓存”。

`LyricDocument` 保存：

- 歌曲 ID
- 来源类型
- 外部歌词相对路径
- 用户覆盖标记
- 原始 LRC 文本
- 解析后的歌词行
- 创建和更新时间

`LyricLine` 保存时间、原文和翻译。翻译与原文在模型中分开，显示层不需要再次解析文本。

## 主读取流程

```text
数据库中的用户覆盖 ─┐
音频同目录外部 LRC ──┼─→ LyricService
音频内嵌歌词 ────────┤        ↓
数据库在线歌词 ──────┘   校验、解析和去重
                              ↓
                       LyricSearchService
                              ↓
                     播放器首选或全部来源
```

`LyricService.GetPreferredByMusicIdAsync` 返回播放器应使用的首选歌词；`GetAllByMusicIdAsync` 返回编辑器可以查看的全部有效来源。

详细设计：

- [来源发现与选择](source-resolution.md)
- [LRC 解析](lrc-parsing.md)
- [编辑与持久化](editing-and-persistence.md)
- [UI 控件](ui-controls.md)
- [测试](testing.md)

## 错误与降级原则

- 某个来源读取失败不能阻止其他来源继续参与选择。
- 文件写入失败不能直接覆盖或损坏原文件。
- 写入文件失败后，由用户决定是否改为保存应用缓存。
- 快速切歌、控件卸载和异步滚动后，旧任务不得更新新的 UI 状态。
- 无原始文本、无有效歌词行或来源信息不完整的文档不参与选择。

## 已知限制与后续扩展

- `Online` 来源模型和保存入口已经存在，但尚未接入实际在线歌词提供商。
- 当前只支持整行同步 LRC，不支持逐字歌词或卡拉 OK 时间轴。
- 已支持解析翻译文本，但还没有独立的翻译获取、匹配或下载服务。
- 外部歌词正则已经进入设置模型，但还需要正式的用户设置界面。
- 歌词控件仍位于 `Controls/Dev`，尚未集成到正式播放页面。
- TagLibSharp 对不同格式、文件占用、只读介质和旧平台的行为仍需持续进行真机验证。

