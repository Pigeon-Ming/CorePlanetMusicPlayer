# 歌词编辑与持久化

歌词编辑由 [`LyricEditService`](../../../CorePlanetMusicPlayer.Services/Lyrics/LyricEditService.cs) 统一处理。UI 不直接操作数据库、外部歌词文件或 TagLibSharp。

## 保存目标

`LyricSaveTarget` 定义三个保存目标：

| 目标 | 行为 |
| --- | --- |
| `ExternalFile` | 写回当前外部 `.lrc` 文件 |
| `Embedded` | 写入音频文件的内嵌歌词标签 |
| `DatabaseOverride` | 保存为当前歌曲的应用缓存 |

## 保存前验证

所有保存目标共享以下检查：

1. 音乐 ID 有效。
2. 歌曲仍存在于音乐库。
3. 当前编辑文档属于这首歌曲。
4. 歌词文本不为空。
5. 文本规范化后仍不为空。
6. 能识别有效 LRC 时间标签。
7. 能解析出至少一条同步歌词。

只有验证通过后才会执行外部写入。

## 默认保存行为

- 当前编辑的是具有有效路径的外部歌词时，“保存”直接写回该文件。
- 其他来源保存时，编辑器询问用户写入音频文件还是仅保存在应用中。
- 外部歌词工具栏另外提供“写入内嵌歌词”和“保存至应用中”，允许显式选择其他目标。

来源 `ComboBox` 只决定正在编辑哪一份歌词，不改变播放器的来源优先级。

## 写回外部歌词

[`UwpExternalLyricWriter`](../../../CorePlanetMusicPlayer.Uwp/Platform/Lyrics/UwpExternalLyricWriter.cs) 使用音乐库目录和歌词相对路径重新定位原文件。

安全写入流程：

1. 确认原文件仍然存在且扩展名为 `.lrc`。
2. 在同一目录创建临时文件。
3. 以 UTF-8 写入临时文件。
4. 重新读取临时文件并比较内容。
5. 验证成功后使用临时文件替换原文件。
6. 再次读取最终文件进行校验。
7. 失败时保留原文件并清理临时文件。

成功写回后会删除该歌曲原有的应用缓存，使播放器重新按照正常来源顺序选择歌词。

## 写入内嵌歌词

[`UwpEmbeddedLyricAccessor`](../../../CorePlanetMusicPlayer.Uwp/Platform/Lyrics/UwpEmbeddedLyricAccessor.cs) 使用 TagLibSharp 修改 `Tag.Lyrics`。

为了避免直接修改音频原文件，写入流程为：

1. 将原音频复制为临时文件。
2. 通过 TagLibSharp 修改临时文件。
3. 重新读取临时文件验证歌词。
4. 验证成功后替换原音频文件。
5. 重新读取最终文件再次验证。
6. 失败时清理临时文件，不替换原文件。

读取和写入结果能够区分无权限、格式不支持、文件占用、标签或音频损坏以及普通失败。

成功写入音频后同样会删除旧应用缓存。

## 应用缓存

无法写入文件或用户主动选择“仅保存在应用中”时，调用 `SaveUserOverrideAsync` 保存数据库覆盖。

应用缓存：

- 使用 `IsUserOverride = true` 标记。
- 保留当前歌词原先的 `SourceType`。
- 外部来源可以保留其相对路径。
- 在所有 UI 中统一显示为“应用缓存”。
- 在播放器来源选择中拥有最高优先级。

数据库通过部分唯一索引保证每首歌曲最多只有一份用户覆盖：

```sql
CREATE UNIQUE INDEX IF NOT EXISTS idx_lyrics_user_override_music_id
ON lyrics(music_id)
WHERE is_user_override = 1;
```

## 写入失败后的回退

外部文件或内嵌歌词写入失败时，服务通过 `CanFallbackToDatabase` 告知 UI 可以回退。编辑器向用户展示失败原因，并询问是否保存为应用缓存。

回退不是自动进行的，必须由用户确认。

## 恢复来源歌词

“恢复来源歌词”仅对应用缓存可用。操作会：

1. 要求用户确认。
2. 删除当前歌曲的用户覆盖。
3. 重新发现外部和内嵌歌词。
4. 重新加载数据库中的其他来源。
5. 按正常来源优先级选取歌词。

## 数据库存储策略

歌词表保存原始文本、来源信息、用户覆盖标记和时间信息。

`LyricDataMapper` 从数据库恢复 `LyricDocument` 时不会恢复解析行；`LyricService` 会在服务边界重新解析 `RawText`。因此解析规则调整后不需要迁移旧的歌词行序列化结构。

