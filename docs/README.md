# CorePlanetMusicPlayer6 文档

本目录用于记录 CorePlanetMusicPlayer6 的架构、业务模块、开发约定、设计决策和后续计划。

文档按业务能力组织，而不是简单按照项目文件夹拆分。一个业务模块可以同时涉及 Core、Data、Services、Playback、Uwp 和应用 UI。

## 文档导航

- [整体架构](architecture/README.md)
- [业务模块](modules/README.md)
- [开发指南](development/README.md)
- [设计决策](decisions/README.md)
- [后续计划](roadmap/README.md)

## 已完成的模块文档

- [歌词模块](modules/lyrics/README.md)

## 推荐阅读顺序

1. 阅读对应模块的 `README.md`，了解模块目标、边界和主要流程。
2. 按需要阅读该模块下的专项文档。
3. 查看 `architecture` 中的全局分层和依赖约束。
4. 查看 `decisions` 中与该模块相关的重要设计决策。
5. 开发和验证时参考 `development` 中的公共约定。

## 文档约定

- 文件和目录使用英文小写短横线命名。
- 正文使用中文，代码中的类型名、方法名和字段名保留英文。
- 文档区分“当前实现”和“后续计划”，未实现功能不能描述为现有能力。
- 模块文档说明功能如何工作；开发文档说明如何开发和验证项目。
- 重要设计取舍应记录为独立设计决策，而不是只保留在代码注释或提交历史中。
- 源码片段只用于说明关键约定，详细实现以仓库中的代码为准。

