# Mine Downloader (MD)

[简体中文](README.zh-CN.md) | English

A third-party derivative of [Plain Craft Launcher (PCL)](https://github.com/Meloong-Git/PCL), originally created by **龙腾猫跃 (LTCat / Meloong-Git)**. This project is independently modified and is not an official PCL release.

Mine Downloader keeps the Minecraft download and installation workflow and resource browsing, while removing game-launch and account-login entry points from the application UI. The repository retains supporting upstream code needed by the download components; this is not a promise that every upstream launch-related helper has been deleted.

## Features

- Download Minecraft versions and supported loaders, mods, resource packs, shader packs, data packs, and modpacks.
- Download-only UI, an MD logo, appearance settings, and multilingual interface strings.
- MD-specific settings and data paths instead of sharing PCL settings.
- Theme shortcut: in Settings → System, right-click the version number 11 times to unlock all themes; right-click it again to restore the default theme state.

## Data paths

- Application-local settings and data: `MD\` next to the executable.
- Logs: `MD\Lite\`; backgrounds: `MD\Pictures\`; music: `MD\Musics\`; custom logo: `MD\Logo.png`.
- Default temporary cache: `%TEMP%\MD\`; task temporary files may use `%ProgramData%\MD\TaskTemp\`.
- Per-instance settings: `<instance>\MD\Setup.ini`. Some legacy PCL instance metadata remains readable for compatibility.

## Build on Windows

Install Visual Studio 2022 / Build Tools with the **.NET desktop build tools**, WPF/VB support, the .NET Framework 4.8 targeting pack, a recent C# compiler supporting **C# 13**, and a .NET SDK. Restore requires NuGet access. The tested machine used VS 2022 MSBuild (amd64) with SDK 8.0.424.

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Debug
```

The script restores and builds the actual MeloongCore and PCLCS sources first, then builds the VB WPF application serially with shared compilation disabled. Default `bin\` output is intentional: the application resource file embeds dependencies from `PCLCS\bin\`. The result is `Plain Craft Launcher 2\bin\Mine Downloader.exe` and its companion files. No signing key is required.

### Source layout

- `Plain Craft Launcher 2/`: VB WPF application, images, and required third-party resource libraries.
- `PCLCS/`: C# download, version, Java, and resource helpers.
- `MeloongCore/Shared/` and `MeloongCore/Wpf/`: actual dependency sources, not a submodule pointer.
- `Plain Craft Launcher 2.sln`, `build.ps1`: solution and repeatable build entry point.

This source snapshot excludes Git metadata, build outputs, private signing material, personal configurations, test runtime folders, logs, and temporary artwork. DLL/JAR/ZIP files under the application resources are required source inputs, not compiled Mine Downloader release artifacts.

## Attribution and license

Upstream: [Meloong-Git/PCL](https://github.com/Meloong-Git/PCL). Original author: **龙腾猫跃**; [support the original author](https://meloong.com/afd/a/LTCat).

The original [LICENCE](LICENCE) is retained verbatim and links to the upstream PCL distribution terms. This project does not replace those terms with an MIT or other blanket license. MeloongCore retains its [Apache-2.0 license](MeloongCore/LICENSE.txt). Third-party libraries and assets remain subject to their respective upstream terms.
