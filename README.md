# Markdown Monster 简体中文界面插件

为 Markdown Monster 4.7.x 提供部分简体中文界面翻译的社区插件。当前版本 **v0.1.0，实验性初版**。

**这是独立社区项目，与 West Wind Technologies 无隶属关系，也不是官方语言包。** 需要自行安装并遵守 Markdown Monster 的许可证。本仓库及发布包不包含 Markdown Monster、Weblog 或其他第三方插件的程序文件。

## 当前效果与限制

- 508 条翻译映射，覆盖主菜单、常用文件操作、部分工具提示、侧栏标题和窗口标签。
- 在 Windows 上的 Markdown Monster **4.7.1 / .NET 11** 实测：主菜单和文件菜单中文正常、Ctrl+N 可以新建空白文档。初版启动扫描记录替换了 208 处界面标签。
- 508 条映射不代表 508 项功能已逐项测试；其他版本、全部快捷键、各插件窗口和长期性能尚未全面验证。
- 动态绑定文本、未收录提示、WebView/Monaco 内部菜单可能仍显示英文；菜单访问键标记会保留，但未逐个验证。
- 使用运行时控件文字替换，不修改主程序文件。代码跳过编辑框、密码框、WebView 及直接数据绑定的属性。
- 这不是基于完整官方资源体系的本地化。自定义原生控件中恰好与词库相同的静态文本也可能被替换；遇到问题请移除插件并反馈。
- 不联网、不上传内容、不包含账号配置。可选的 `loaded.txt` 只记录加载时间、翻译计数和词库条数。

## 安装

1. 从 [Releases](https://github.com/SamueruTokeisou/markdown-monster-zh-cn/releases) 下载 ZIP 并解压。
2. 保存文档，完全关闭 Markdown Monster。
3. 把解压后的 **ChineseLocalization 文件夹**放到以下任意一个位置：
   - 推荐：`%APPDATA%\Markdown Monster\Addins\ChineseLocalization`
   - 或软件安装目录：`<Markdown Monster>\Addins\ChineseLocalization`
4. 文件夹中应直接包含 `ChineseAddin.dll`、`translations.tsv`、`version.json`，避免多嵌套一层。
5. 重新启动软件。**两个位置只安装一份**；如果之前已有手动安装版本，先将旧版移出 Addins 目录。

本机验证使用了软件安装目录的插件路径；用户目录路径遵循软件公开的插件安装约定，尚未单独实测。

### 恢复英文

关闭软件，把整个 `ChineseLocalization` 文件夹移到 **Addins 目录之外**，再启动软件。仅改文件夹名称可能仍被加载。重新安装或更新软件后，如有兼容问题，也可用此方法恢复。

## 修改翻译

编辑 UTF-8 编码的 `translations.tsv`：每行一条，英文与中文之间为一个真实 Tab。保存后重启软件。

```text
Save	保存
Open Folder	打开文件夹
```

匹配不区分英文大小写，移除英文菜单中的下划线访问键标记，并识别部分冒号、省略号和括号快捷键提示。不建议加入文件名、路径或用户文档内容。

## 从源码构建

需要 Windows、PowerShell 7（含 Roslyn 编译器）、.NET 11 桌面运行时和已安装的 Markdown Monster 4.7.x。无需将第三方 DLL 复制进仓库。

```powershell
pwsh -File .\scripts\build.ps1 -MarkdownMonsterPath '你的 Markdown Monster 安装目录'
```

输出在 `dist/`，包括插件 ZIP 和 SHA-256 校验文件。编译只引用本机软件及 .NET 运行时的程序集；这些文件不会打进发布包。

## 反馈与贡献

欢迎提交翻译修正和 Issues。请提供软件版本、菜单路径、英文原文和建议译法；截图请遮住文档内容、账号、网址中的凭据等私人信息。

源码和词库由 AI 辅助编写，已做有限的本机验证。欢迎帮助扩大测试覆盖。

## 许可与相关链接

本项目原创插件代码和翻译映射采用 [MIT License](LICENSE)。第三方产品及其商标属于各自所有者，MIT 许可不适用于 Markdown Monster 本身。

- [Markdown Monster 官方网站](https://markdownmonster.west-wind.com/)
- [官方插件开发与手动安装说明](https://markdownmonster.west-wind.com/docs/Creating-Markdown-Monster-Addins-with-NET/Creating-a-Markdown-Monster-Add--in.html)
