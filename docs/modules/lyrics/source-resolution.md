# 歌词来源发现与选择

## 来源类型与应用缓存

歌词来源由 `LyricSourceType` 描述：内嵌歌词、外部文件和在线歌词。`Unknown` 只表示无法识别的来源，不参与正常选择。

应用数据库中的用户修改使用 `IsUserOverride` 表示。它不是新的歌词来源，也不另外设置“手动歌词”或“应用存储”来源枚举。这样可以分别回答两个问题：

- 歌词最初来自哪里？
- 当前内容是否由用户在应用中覆盖？

所有 `IsUserOverride = true` 的歌词在 UI 中统一显示为“应用缓存”。

## 外部歌词发现

外部歌词读取由 [`UwpExternalLyricReader`](../../../CorePlanetMusicPlayer.Uwp/Platform/Lyrics/UwpExternalLyricReader.cs) 实现。

读取步骤：

1. 根据歌曲的音乐库目录 ID 和相对路径重新定位音频文件。
2. 获取音频所在目录。
3. 枚举目录中的 `.lrc` 文件。
4. 首先查找文件名主体与音频相同的歌词。
5. 未找到时，使用设置中的正则表达式继续匹配。
6. 读取歌词，并记录相对于音乐库目录的路径。

例如：

```text
Album/Song.mp3
Album/Song.lrc
```

默认优先选择 `Song.lrc`。

正则表达式可以使用 `{FileName}` 占位符。匹配时该占位符会替换为经过转义的音频文件名主体；正则匹配设置了超时，避免复杂表达式长时间阻塞。

系统不为每个外部歌词文件写入 `FutureAccessList`。歌词访问依赖用户已经授权的音乐库目录，并在每次读取时从音频所在目录重新发现文件，因此歌词重命名、替换或新增后能够在重新加载时被识别。

## 内嵌歌词发现

内嵌歌词由 [`UwpEmbeddedLyricAccessor`](../../../CorePlanetMusicPlayer.Uwp/Platform/Lyrics/UwpEmbeddedLyricAccessor.cs) 读取。

它通过已经授权的音乐库目录定位音频文件，再使用 TagLibSharp 读取 `Tag.Lyrics`。TagLibSharp 通过 `UwpTagLibFileAbstraction` 使用 UWP 提供的文件流，不要求业务服务直接依赖 UWP API。

只有具有可访问库目录信息的本地或临时音乐参与外部和内嵌歌词读取。

## 数据库来源

数据库可能包含：

- 每首歌曲最多一份用户覆盖歌词。
- 在线歌词缓存。
- 外部或内嵌歌词的最近读取缓存。

服务读取时会重新解析数据库中的 `RawText`。外部和内嵌歌词以当前文件内容为准，不直接使用数据库中的旧缓存替代实时读取结果。

## 选择优先级

[`LyricSearchService`](../../../CorePlanetMusicPlayer.Services/Lyrics/LyricSearchService.cs) 使用以下顺序：

```text
应用缓存
  ↓
外部歌词
  ↓
内嵌歌词
  ↓
在线歌词
```

同一优先级中优先使用更新时间较新的记录，再使用来源路径和 ID 保证排序结果稳定。

## 有效性检查与去重

以下文档不会参与选择：

- 文档为空。
- 原始文本为空。
- 没有解析后的歌词行。
- 来源不是外部、内嵌或在线歌词。
- 外部歌词没有来源路径。

去重规则：

- 所有用户覆盖视为同一来源，每首歌曲只保留一个候选项。
- 外部歌词使用不区分大小写的相对路径区分。
- 内嵌歌词和在线歌词各自只保留最新候选项。

## 播放器与编辑器的区别

播放器通过 `GetPreferredByMusicIdAsync` 只取得排序后的第一个来源。

编辑器通过 `GetAllByMusicIdAsync` 取得全部有效来源，并允许用户在 `ComboBox` 中切换查看。这个选择只改变编辑器状态，不会修改全局优先级，也不会改变播放器正在使用的来源。

