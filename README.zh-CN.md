# Mine Downloader（MD）

简体中文 | [English](README.md)

本项目是第三方基于 [Meloong-Git/PCL](https://github.com/Meloong-Git/PCL) 独立修改的衍生作品，原作者为 **龙腾猫跃（LTCat / Meloong-Git）**，并非 PCL 官方发布版本。

Mine Downloader 保留 Minecraft 下载、安装和资源浏览流程，在应用界面移除游戏启动和账号登录入口。源码仍保留下载组件所需的上游公共能力，并非删除了所有与启动相关的辅助代码。

## 功能

- 下载 Minecraft 版本、支持的加载器、Mod、资源包、光影包、数据包和整合包。
- 仅下载界面、MD 标志、外观设置与多语言界面文本。
- 使用 MD 专属配置和数据路径，避免与 PCL 设置混用。
- 主题快捷操作：在“设置 → 系统”连续右键版本号 11 次解锁所有主题，再右键一次恢复默认主题状态。

## 数据路径

- 程序本地配置和数据：可执行文件旁的 `MD\`。
- 日志：`MD\Lite\`；背景：`MD\Pictures\`；音乐：`MD\Musics\`；自定义标志：`MD\Logo.png`。
- 默认临时缓存：`%TEMP%\MD\`；任务临时文件可能位于 `%ProgramData%\MD\TaskTemp\`。
- 实例配置：`<实例目录>\MD\Setup.ini`。为兼容旧实例，部分 PCL 元数据仍可读取。

## Windows 构建

安装 Visual Studio 2022 或 Build Tools，选择 **.NET 桌面生成工具**、WPF/VB 支持、.NET Framework 4.8 targeting pack，并使用支持 **C# 13** 的较新编译器及 .NET SDK。恢复依赖需要访问 NuGet。实际验证环境为 VS 2022 amd64 MSBuild，SDK 8.0.424。

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Debug
```

脚本先恢复和构建 MeloongCore、PCLCS 实际源码，再串行构建 VB WPF 主项目，并关闭共享编译。默认使用 `bin\` 输出是为了匹配主项目资源文件中的 `PCLCS\bin\` 嵌入依赖路径。产物为 `Plain Craft Launcher 2\bin\Mine Downloader.exe` 及配套文件。构建无需签名私钥。

### 源码结构

- `Plain Craft Launcher 2/`：VB WPF 主项目、图片与构建必需的第三方资源库。
- `PCLCS/`：下载、版本、Java 和资源辅助代码。
- `MeloongCore/Shared/`、`MeloongCore/Wpf/`：实际依赖源码，而非 submodule 指针。
- `Plain Craft Launcher 2.sln`、`build.ps1`：解决方案与可重复执行的构建入口。

此源码快照排除 Git 元数据、构建产物、私有签名材料、个人配置、测试运行目录、日志及图片临时素材。主项目资源目录中的 DLL/JAR/ZIP 是构建输入，不是编译好的 Mine Downloader 发布产品。

## 署名和许可

上游：[Meloong-Git/PCL](https://github.com/Meloong-Git/PCL)。原作者：**龙腾猫跃**；[支持原作者](https://meloong.com/afd/a/LTCat)。

原始 [LICENCE](LICENCE) 保持不变，其中包含 PCL 分发条款链接；本项目未将其替换为 MIT 等统一许可。MeloongCore 保留其 [Apache-2.0 许可](MeloongCore/LICENSE.txt)。第三方库和素材遵循各自的上游条款。
