Imports System.Collections.Generic
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Windows.Controls

''' <summary>
''' 运行时界面多语言模块。
''' PCL 的界面文字全部硬编码在 XAML 与代码里，无法走 resx 资源，
''' 因此这里采用「字符串映射表 + 遍历逻辑树替换」的方式实现即时切换语言。
''' </summary>
Public Module ModLanguage

#Region "语言状态"

    ''' <summary>
    ''' 当前界面语言。0=跟随首次系统语言 1=简体中文 2=繁體中文 3=English 4=日本語
    ''' </summary>
    Public ReadOnly Property CurrentLanguage As Integer
        Get
            Try
                Dim Value As Integer = Settings.Get(Of Integer)("UiLanguage")
                If Value >= 0 AndAlso Value <= 4 Then Return Value
            Catch ex As Exception
                '设置项尚未注册或读取失败时按简体中文处理
            End Try
            Return 1
        End Get
    End Property

    ''' <summary>
    ''' 本次翻译过程使用的语言编号（避免遍历时反复读取设置）。
    ''' </summary>
    Private _ApplyLanguage As Integer = 1

    ''' <summary>
    ''' 语言显示名。
    ''' </summary>
    Public Function LanguageName(Id As Integer) As String
        Select Case Id
            Case 0
                Return "跟随系统"
            Case 2
                Return "繁體中文"
            Case 3
                Return "English"
            Case 4
                Return "日本語"
            Case Else
                Return "简体中文"
        End Select
    End Function

    ''' <summary>
    ''' 首次启动时按系统语言决定默认值并写入设置 UiLanguage；非首次直接返回。
    ''' 默认语言只由第一次打开软件时的系统语言决定，之后不再自动改动。
    ''' </summary>
    Public Sub LanguageInitOnce()
        Try
            If Settings.Get(Of Boolean)("UiLanguageInited") Then Return
        Catch ex As Exception
            '读取失败时按未初始化处理
        End Try
        Try
            Settings.Set("UiLanguage", DetectSystemLanguage())
            Settings.Set("UiLanguageInited", True)
        Catch ex As Exception
            '写入失败不影响启动
        End Try
    End Sub

    ''' <summary>
    ''' 根据系统界面语言推断默认的界面语言编号。
    ''' </summary>
    Private Function DetectSystemLanguage() As Integer
        Try
            Dim Cult As CultureInfo = CultureInfo.CurrentUICulture
            Dim Name As String = If(Cult.Name, "").ToLowerInvariant()
            Dim Two As String = If(Cult.TwoLetterISOLanguageName, "").ToLowerInvariant()
            '日语
            If Two = "ja" OrElse Name.StartsWith("ja") Then Return 4
            '中文：先判定繁体
            If Two = "zh" OrElse Name.StartsWith("zh") Then
                If Name.Contains("tw") OrElse Name.Contains("hk") OrElse Name.Contains("mo") OrElse Name.Contains("hant") Then Return 2
                Return 1
            End If
            '英语
            If Two = "en" Then Return 3
        Catch ex As Exception
            '任何异常都回落到简体中文
        End Try
        Return 1
    End Function

#End Region

#Region "翻译表"

    ''' <summary>
    ''' 中文原文 → {简中, 繁中, English, 日本語}
    ''' </summary>
    Private ReadOnly Dict As New Dictionary(Of String, String())
    Private ReadOnly DynamicKeys As New List(Of String)

    ''' <summary>
    ''' 登记一条翻译。简中列固定等于原文。
    ''' </summary>
    Private Sub Add(Key As String, Tw As String, En As String, Ja As String)
        Dict(Key) = New String() {Key, Tw, En, Ja}
        If Key.Contains("{0}") AndAlso Not DynamicKeys.Contains(Key) Then DynamicKeys.Add(Key)
    End Sub

    Sub New()
        '导航 / 页签
        Add("原版游戏", "原版遊戲", "Vanilla", "バニラ")
        Add("社区资源", "社群資源", "Community", "コミュニティ")
        Add("Mod", "Mod", "Mods", "MOD")
        Add("模组", "模組", "Mods", "MOD")
        Add("整合包", "整合包", "Modpacks", "MODパック")
        Add("数据包", "資料包", "Data packs", "データパック")
        Add("资源包", "資源包", "Resource packs", "リソースパック")
        Add("光影包", "光影包", "Shaders", "シェーダー")
        Add("设置", "設定", "Settings", "設定")
        Add("个性化", "個人化", "Appearance", "カスタマイズ")
        Add("其他", "其他", "Misc", "その他")
        Add("任务管理", "任務管理", "Tasks", "タスク管理")
        Add("下载", "下載", "Download", "ダウンロード")

        '下载页欢迎区
        Add("PCL 下载器", "Mine Downloader", "Mine Downloader", "Mine Downloader")
        Add("MD下载器", "MD下載器", "MD Downloader", "MD ダウンローダー")
        Add("为 minecraft 服务", "為 minecraft 服務", "For Minecraft", "Minecraft のために")
        Add("在这里下载游戏版本、模组加载器与社区资源，安装完成后即可用 PCL 本体启动游戏。", "在這裡下載遊戲版本、模組載入器與社群資源，安裝完成後即可用 PCL 本體啟動遊戲。", "Download game versions, mod loaders and community resources here, then launch the installed game with PCL.", "ここからゲームバージョン、MODローダー、コミュニティリソースをダウンロードし、インストール後に PCL 本体でゲームを起動できます。")
        Add("快照版", "快照版", "Snapshot", "スナップショット")
        Add("下载游戏版本", "下載遊戲版本", "Download game", "ゲームをダウンロード")
        Add("最新版本", "最新版本", "Latest", "最新版")
        Add("最新正式版", "最新正式版", "Latest release", "最新リリース")
        Add("最新预览版", "最新預覽版", "Latest snapshot", "最新プレビュー")
        Add("在这里下载游戏版本、模组加载器与社区资源。",
            "在這裡下載遊戲版本、模組載入器與社群資源。",
            "Download game versions, mod loaders and community resources here.",
            "ここからゲームバージョン、MODローダー、コミュニティリソースをダウンロードできます。")

        '设置 - 下载
        Add("文件下载源", "檔案下載源", "File download source", "ファイルのダウンロード元")
        Add("版本列表源", "版本列表源", "Version list source", "バージョンリストの取得元")
        Add("最大线程数", "最大執行緒數", "Max threads", "最大スレッド数")
        Add("速度限制", "速度限制", "Speed limit", "速度制限")
        Add("游戏文件夹", "遊戲資料夾", "Game folder", "ゲームフォルダー")
        Add("更改…", "變更…", "Change…", "変更…")
        Add("打开文件夹", "開啟資料夾", "Open folder", "フォルダーを開く")
        Add("恢复默认", "恢復預設", "Reset", "初期化")
        Add("尽量使用镜像源", "盡量使用鏡像源", "Prefer mirror", "ミラーを優先")
        Add("尽量使用镜像源（可能缺少刚刚更新的版本）",
            "盡量使用鏡像源（可能缺少剛剛更新的版本）",
            "Prefer mirror (may miss newly released versions)",
            "ミラーを優先（最新版が無い場合あり）")
        Add("优先使用官方源，在加载缓慢时换用镜像源",
            "優先使用官方源，在載入緩慢時換用鏡像源",
            "Prefer official, switch to mirror when slow",
            "公式を優先し、遅い場合はミラーに切り替え")
        Add("尽量使用官方源", "盡量使用官方源", "Prefer official", "公式を優先")
        Add("仅在官方源加载缓慢时改用镜像源",
            "僅在官方源載入緩慢時改用鏡像源",
            "Mirror only when official source is slow",
            "公式が遅い場合のみミラーを使用")

        '设置 - 社区资源
        Add("来源", "來源", "Source", "取得元")
        Add("文件名格式", "檔名格式", "File name format", "ファイル名の形式")
        Add("Mod 管理样式", "Mod 管理樣式", "Mod list style", "MOD の表示形式")

        '设置 - 辅助功能
        Add("辅助功能", "輔助功能", "Accessibility", "アクセシビリティ")
        Add("游戏语言", "遊戲語言", "Game language", "ゲーム言語")
        Add("自动设置为中文", "自動設定為中文", "Set to Chinese automatically", "自動的に中国語に設定")

        '版本分类与动态文案；{0} 之后的路径、日期等保持原样
        Add("正式版", "正式版", "Release", "正式版")
        Add("预览版", "預覽版", "Snapshot", "プレビュー版")
        Add("远古版", "遠古版", "Legacy", "旧バージョン")
        Add("愚人节版", "愚人節版", "April Fools'", "エイプリルフール版")
        Add("游戏文件夹：{0}", "遊戲資料夾：{0}", "Game folder: {0}", "ゲームフォルダー：{0}")
        Add("背景图片（{0} 张）", "背景圖片（{0} 張）", "Background images ({0})", "背景画像（{0} 枚）")
        Add("{0}（已手动指定）", "{0}（已手動指定）", "{0} (manually selected)", "{0}（手動指定）")
        Add("{0}（自动识别：与启动器同目录的 .minecraft 文件夹）", "{0}（自動識別：與啟動器同目錄的 .minecraft 資料夾）", "{0} (auto-detected: .minecraft folder beside the launcher)", "{0}（自動検出：ランチャーと同じディレクトリの .minecraft フォルダー）")
        Add("已恢复默认主题：跳票红", "已恢復預設主題：跳票紅", "Default theme restored: Delayed Red", "デフォルトテーマに戻しました：延期レッド")
        Add("所有主题已解锁，再次右键版本号恢复默认", "所有主題已解鎖，再次右鍵版本號恢復預設", "All themes unlocked. Right-click the version number again to restore the default.", "すべてのテーマを解除しました。バージョン番号をもう一度右クリックすると元に戻ります。")
        Add("最新正式版，发布于 {0}", "最新正式版，發佈於 {0}", "Latest release, released on {0}", "最新リリース、公開日：{0}")
        Add("最新预览版，发布于 {0}", "最新預覽版，發佈於 {0}", "Latest snapshot, released on {0}", "最新プレビュー、公開日：{0}")
        Add("发布于 {0}", "發佈於 {0}", "Released on {0}", "公開日：{0}")
        Add("正式版，发布于 {0}", "正式版，發佈於 {0}", "Release, released on {0}", "正式版、公開日：{0}")
        Add("预览版，发布于 {0}", "預覽版，發佈於 {0}", "Snapshot, released on {0}", "プレビュー版、公開日：{0}")

        '设置 - 个性化
        Add("龙猫蓝", "龍貓藍", "Totoro Blue", "トトロブルー")
        Add("甜柠青", "甜檸青", "Lemon Cyan", "レモンシアン")
        Add("小草绿", "小草綠", "Grass Green", "グラスグリーン")
        Add("菠萝黄", "菠蘿黃", "Pineapple Yellow", "パイナップルイエロー")
        Add("橡木棕", "橡木棕", "Oak Brown", "オークブラウン")
        Add("玄素黑", "玄素黑", "Pure Black", "ピュアブラック")
        Add("滑稽彩", "滑稽彩", "Funny Rainbow", "ユーモアレインボー")
        Add("铁杆粉", "鐵桿粉", "Fan Pink", "ファンピンク")
        Add("神秘紫", "神秘紫", "Mystic Purple", "ミスティックパープル")
        Add("欧皇彩", "歐皇彩", "Lucky Rainbow", "ラッキーレインボー")
        Add("秋仪金", "秋儀金", "Autumn Gold", "オータムゴールド")
        Add("活跃橙", "活躍橙", "Vibrant Orange", "ビビッドオレンジ")
        Add("跳票红", "跳票紅", "Delayed Red", "延期レッド")
        Add("极客蓝", "極客藍", "Geek Blue", "ギークブルー")
        Add("灰色是个谎言", "灰色是個謊言", "Gray is a lie", "グレーは嘘")
        Add("滑稽树上滑稽果，滑稽树下你和我", "滑稽樹上滑稽果，滑稽樹下你和我", "A funny rainbow for you and me", "あなたと私のユーモアレインボー")
        Add("用下面的色相/饱和度/亮度自己调", "用下面的色相/飽和度/亮度自行調整", "Adjust the hue, saturation and brightness below", "下の色相・彩度・明度で調整")
        Add("打开启动器时显示 MD 图标", "開啟啟動器時顯示 MD 圖示", "Show the MD logo when opening the launcher", "起動時に MD ロゴを表示")
        Add("标题栏居左", "標題列靠左", "Left-align the title bar", "タイトルバーを左揃え")
        Add("左上 (保持长宽比)", "左上 (保持長寬比)", "Top left (keep aspect ratio)", "左上（縦横比を維持）")
        Add("左中 (保持长宽比)", "左中 (保持長寬比)", "Middle left (keep aspect ratio)", "左中央（縦横比を維持）")
        Add("左下 (保持长宽比)", "左下 (保持長寬比)", "Bottom left (keep aspect ratio)", "左下（縦横比を維持）")
        Add("中上 (保持长宽比)", "中上 (保持長寬比)", "Top center (keep aspect ratio)", "上中央（縦横比を維持）")
        Add("居中 (保持长宽比)", "置中 (保持長寬比)", "Center (keep aspect ratio)", "中央（縦横比を維持）")
        Add("中下 (保持长宽比)", "中下 (保持長寬比)", "Bottom center (keep aspect ratio)", "下中央（縦横比を維持）")
        Add("右上 (保持长宽比)", "右上 (保持長寬比)", "Top right (keep aspect ratio)", "右上（縦横比を維持）")
        Add("右中 (保持长宽比)", "右中 (保持長寬比)", "Middle right (keep aspect ratio)", "右中央（縦横比を維持）")
        Add("右下 (保持长宽比)", "右下 (保持長寬比)", "Bottom right (keep aspect ratio)", "右下（縦横比を維持）")
        Add("当图片较小时选择平铺，较大时选择居中 (保持长宽比)", "圖片較小時選擇並排，較大時選擇置中 (保持長寬比)", "Tile small images; center larger images while keeping their aspect ratio", "小さい画像はタイル表示、大きい画像は縦横比を維持して中央に配置")
        Add("拉伸图片使其恰好填满整个窗口，很可能会导致长宽比的改变", "拉伸圖片以填滿整個視窗，可能會改變長寬比", "Stretch the image to fill the window, which may change its aspect ratio", "ウィンドウ全体に画像を拡大します。縦横比が変わる場合があります")
        Add("随机加载文件夹内的一张背景图片", "隨機載入資料夾內的一張背景圖片", "Load a random background image from the folder", "フォルダーから背景画像をランダムに読み込む")
        Add("将背景图片放到该文件夹，PCL 将会在每次打开时随机加载其中的一张图片", "將背景圖片放入此資料夾，PCL 每次開啟時會隨機載入其中一張", "Place background images in this folder; PCL will load one randomly at startup", "このフォルダーに背景画像を入れると、PCL は起動時にランダムに読み込みます")
        Add("基础", "基礎", "Basic", "基本")
        Add("不透明度", "不透明度", "Opacity", "不透明度")
        Add("色调", "色調", "Hue", "色相")
        Add("色调渐变", "色調漸變", "Hue shift", "色相の変化")
        Add("饱和度", "飽和度", "Saturation", "彩度")
        Add("亮度", "亮度", "Brightness", "明度")
        Add("主题", "主題", "Theme", "テーマ")
        Add("更多主题", "更多主題", "More themes", "その他のテーマ")
        Add("自定义", "自訂", "Custom", "カスタム")
        Add("标题栏", "標題列", "Title bar", "タイトルバー")
        Add("背景图片", "背景圖片", "Background image", "背景画像")
        Add("自适应方式", "自適應方式", "Image fit", "配置方法")
        Add("背景模糊", "背景模糊", "Blur background", "背景のぼかし")
        Add("叠加彩色背景", "疊加彩色背景", "Colorful overlay", "カラフルな背景")
        Add("刷新背景图片", "重新整理背景圖片", "Refresh image", "背景を更新")
        Add("清空背景图片", "清空背景圖片", "Clear background", "背景を削除")
        Add("标题栏文本", "標題列文字", "Title bar text", "タイトルバーの文字")
        Add("更改图片", "變更圖片", "Change image", "画像を変更")
        Add("清空图片", "清空圖片", "Clear image", "画像を削除")
        Add("语言", "語言", "Language", "言語")
        Add("跟随系统（首次打开时的语言）",
            "跟隨系統（首次開啟時的語言）",
            "Follow system (language at first launch)",
            "システムに従う（初回起動時の言語）")
        Add("简体中文", "簡體中文", "Simplified Chinese", "簡体字中国語")
        Add("繁體中文", "繁體中文", "Traditional Chinese", "繁体字中国語")
        Add("English", "English", "English", "英語")
        Add("日本語", "日本語", "Japanese", "日本語")

        '设置 - 其他
        Add("关于", "關於", "About", "このソフトについて")
        Add("关于与鸣谢", "關於與鳴謝", "About and credits", "このソフトについて")
        Add("版本", "版本", "Version", "バージョン")
        Add("作者", "作者", "Author", "作者")
        Add("项目主页", "專案主頁", "Project page", "プロジェクトページ")
        Add("原项目", "原專案", "Upstream", "オリジナルプロジェクト")
        Add("开源协议", "開源協議", "License", "ライセンス")
        Add("检查更新", "檢查更新", "Check for updates", "アップデートを確認")
        Add("本项目", "本專案", "This project", "このプロジェクト")
        Add("打开原项目主页", "開啟原專案主頁", "Open upstream page", "オリジナルのページを開く")
        Add("查看开源协议", "檢視開源協議", "View license", "ライセンスを見る")
        Add("一款专注于 Minecraft 资源下载的启动器，基于 PCL 开源项目改造。", "一款專注於 Minecraft 資源下載的啟動器，基於 PCL 開源專案改造。", "A launcher focused on downloading Minecraft resources, adapted from the open-source PCL project.", "Minecraft リソースのダウンロードに特化した、オープンソースの PCL をベースにしたランチャーです。")
        Add("此精简版由本地改造生成，不对外发布。", "此精簡版由本機修改產生，不對外發佈。", "This locally customized lightweight edition is not publicly distributed.", "この軽量版はローカルで改造したもので、一般公開していません。")
        Add("标题显示译名，详情显示文件名", "標題顯示譯名，詳情顯示檔名", "Translated title, file name in details", "タイトルに翻訳名、詳細にファイル名を表示")
        Add("标题显示文件名，详情显示译名", "標題顯示檔名，詳情顯示譯名", "File name as title, translated name in details", "タイトルにファイル名、詳細に翻訳名を表示")
        Add("初始化本页设置", "初始化本頁設定", "Reset this page's settings", "このページの設定を初期化")

        '通用按钮 / 提示
        Add("确定", "確定", "OK", "OK")
        Add("取消", "取消", "Cancel", "キャンセル")
        Add("取消更改", "取消變更", "Cancel changes", "変更をキャンセル")
        Add("返回", "返回", "Back", "戻る")
        Add("返回顶部", "返回頂部", "Back to top", "トップへ戻る")
        Add("正在刷新……", "正在重新整理……", "Refreshing…", "更新中…")
        Add("已刷新！", "已重新整理！", "Refreshed!", "更新しました！")
        Add("关闭", "關閉", "Close", "閉じる")
        Add("正在加载", "正在載入", "Loading", "読み込み中")
        Add("开始下载", "開始下載", "Start download", "ダウンロード開始")
        Add("自动", "自動", "Auto", "自動")
        Add("平铺", "並排", "Tile", "タイル")
        Add("居中", "置中", "Center", "中央")
        Add("拉伸", "拉伸", "Stretch", "拡大")
        Add("无", "無", "None", "なし")
        Add("默认", "預設", "Default", "デフォルト")
        Add("文本", "文字", "Text", "テキスト")
        Add("图片", "圖片", "Image", "画像")
        Add("游戏设置", "遊戲設定", "Game settings", "ゲーム設定")
        Add("版本设置", "版本設定", "Version settings", "バージョン設定")
        Add("启动游戏", "啟動遊戲", "Launch game", "ゲームを起動")
        Add("启动", "啟動", "Launch", "起動")
        Add("已安装", "已安裝", "Installed", "インストール済み")
        Add("下载中", "下載中", "Downloading", "ダウンロード中")

        '异步下载结果及折叠卡片的内容可能晚于页面翻译才进入树。
        '使用类处理器，不持有页面实例，也不依赖修改各个结果生成器。
        EventManager.RegisterClassHandler(GetType(FrameworkElement), FrameworkElement.LoadedEvent, New RoutedEventHandler(AddressOf LanguageElementLoaded))
    End Sub

    Private Sub LanguageElementLoaded(Sender As Object, E As RoutedEventArgs)
        If Not Object.ReferenceEquals(Sender, E.OriginalSource) Then Return
        _ApplyLanguage = CurrentLanguage
        If _ApplyLanguage <= 0 Then _ApplyLanguage = 1
        Try
            ApplySelf(DirectCast(Sender, DependencyObject))
        Catch ex As Exception
            '加载阶段的单个翻译失败不影响页面显示
        End Try
    End Sub

    '仅匹配已登记的中文前后缀，占位部分（路径、数量、日期）逐字符保留。
    Private Function DynamicKey(Text As String) As String
        For Each Key As String In DynamicKeys
            Dim Marker As Integer = Key.IndexOf("{0}", StringComparison.Ordinal)
            Dim Prefix As String = Key.Substring(0, Marker)
            Dim Suffix As String = Key.Substring(Marker + 3)
            If Text.Length >= Prefix.Length + Suffix.Length AndAlso
                Text.StartsWith(Prefix, StringComparison.Ordinal) AndAlso Text.EndsWith(Suffix, StringComparison.Ordinal) Then Return Key
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' 查表翻译。语言为 0 或 1 时直接返回原文。
    ''' </summary>
    Private Function L(Text As String) As String
        If String.IsNullOrEmpty(Text) Then Return Text
        Dim Idx As Integer = _ApplyLanguage
        If Idx <= 1 Then Return Text
        Try
            Dim Arr As String() = Nothing
            If Dict.TryGetValue(Text, Arr) AndAlso Arr IsNot Nothing Then
                Dim Pos As Integer = Idx - 1
                If Pos >= 1 AndAlso Pos < Arr.Length Then
                    Dim Value As String = Arr(Pos)
                    If Not String.IsNullOrEmpty(Value) Then Return Value
                End If
            End If
            Dim Key As String = DynamicKey(Text)
            If Key IsNot Nothing Then
                Dim PrefixLength As Integer = Key.IndexOf("{0}", StringComparison.Ordinal)
                Dim SuffixLength As Integer = Key.Length - PrefixLength - 3
                Dim Value As String = Text.Substring(PrefixLength, Text.Length - PrefixLength - SuffixLength)
                Dim Template As String = Dict(Key)(Idx - 1)
                Dim Marker As Integer = Template.IndexOf("{0}", StringComparison.Ordinal)
                Return Template.Substring(0, Marker) & Value & Template.Substring(Marker + 3)
            End If
        Catch ex As Exception
            '任何异常都按原文返回
        End Try
        Return Text
    End Function

#End Region

#Region "控件原文缓存"

    ''' <summary>
    ''' 单个控件上各文本属性的原文与上次翻译所用的语言编号。
    ''' </summary>
    Private Class TextCache
        Public ReadOnly Values(5) As String
        Public ReadOnly Outputs(5) As String
        Public ReadOnly Langs(5) As Integer
    End Class

    ''' <summary>
    ''' 控件 → 原文缓存。弱引用表不会阻止控件被回收。
    ''' </summary>
    Private ReadOnly Origs As New ConditionalWeakTable(Of DependencyObject, TextCache)

    ''' <summary>
    ''' 文本属性槽位，避免同一控件的不同属性互相覆盖。
    ''' </summary>
    Private Enum TextSlot
        Text = 0
        Content = 1
        ToolTip = 2
        Title = 3
        Hint = 4
        Info = 5
    End Enum

    ''' <summary>
    ''' 取出（或建立）某个控件的原文缓存。
    ''' </summary>
    Private Function GetCache(Ctl As DependencyObject) As TextCache
        Dim Cache As TextCache = Nothing
        If Origs.TryGetValue(Ctl, Cache) AndAlso Cache IsNot Nothing Then Return Cache
        Cache = New TextCache()
        Try
            Origs.Add(Ctl, Cache)
        Catch ex As Exception
            '并发情况下忽略，直接使用临时缓存
        End Try
        Return Cache
    End Function

    ''' <summary>
    ''' 计算某个文本属性在当前语言下应当显示的内容。
    ''' 只处理字典里存在的文案，其余原样返回；重复调用保持幂等，且支持来回切换语言。
    ''' </summary>
    Private Function TranslateValue(Ctl As DependencyObject, Slot As Integer, Current As String) As String
        Try
            Dim Cache As TextCache = GetCache(Ctl)
            Dim Cur As Integer = _ApplyLanguage
            If Cur <= 0 Then Cur = 1
            Dim Orig As String = Nothing
            '仅当前值仍是上次输出时复用缓存，避免路径/日期更新后恢复旧值。
            If Cache.Langs(Slot) <> 0 AndAlso String.Equals(Current, Cache.Outputs(Slot), StringComparison.Ordinal) Then
                Orig = Cache.Values(Slot)
            ElseIf Not String.IsNullOrEmpty(Current) AndAlso (Dict.ContainsKey(Current) OrElse DynamicKey(Current) IsNot Nothing) Then
                Orig = Current
            End If
            If Orig Is Nothing Then
                Cache.Values(Slot) = Nothing
                Cache.Outputs(Slot) = Nothing
                Cache.Langs(Slot) = 0
                Return Current
            End If
            Dim Result As String = L(Orig)
            Cache.Values(Slot) = Orig
            Cache.Outputs(Slot) = Result
            Cache.Langs(Slot) = Cur
            Return Result
        Catch ex As Exception
            Return Current
        End Try
    End Function

#End Region

#Region "界面替换"

    ''' <summary>
    ''' 把某个控件树里的中文按当前语言替换（用于页面 Loaded 时调用）。
    ''' </summary>
    Public Sub LanguageApply(Target As DependencyObject)
        If Target Is Nothing Then Return
        _ApplyLanguage = CurrentLanguage
        If _ApplyLanguage <= 0 Then _ApplyLanguage = 1
        Try
            ApplyCore(Target)
        Catch ex As Exception
            '单个控件出错绝不影响界面
        End Try
    End Sub

    ''' <summary>
    ''' 语言变化后立即刷新整个已加载界面。
    ''' </summary>
    Public Sub LanguageApplyAll()
        Try
            If FrmMain Is Nothing Then Return
            _ApplyLanguage = CurrentLanguage
            If _ApplyLanguage <= 0 Then _ApplyLanguage = 1
            LanguageApply(FrmMain.PanMainLeft)
            LanguageApply(FrmMain.PanMainRight)
            LanguageApply(FrmMain.PanTitle)
            LanguageApply(FrmMain.PanHint)
        Catch ex As Exception
            '主窗口尚未初始化完成时直接忽略
        End Try
        LanguageRefreshTitle()
    End Sub

    ''' <summary>
    ''' 标题栏品牌名不翻译，并刷新页面名。
    ''' </summary>
    Private Sub LanguageRefreshTitle()
        Try
            If FrmMain Is Nothing Then Return
            If FrmMain.LabTitleLogo IsNot Nothing Then FrmMain.LabTitleLogo.Text = "Mine Downloader"
        Catch ex As Exception
        End Try
        Try
            FrmMain.PageNameRefresh()
        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>
    ''' 递归处理单个控件及其子控件。
    ''' </summary>
    Private Sub ApplyCore(Ctl As DependencyObject)
        '1. 控件自身
        Try
            ApplySelf(Ctl)
        Catch ex As Exception
            '单个属性出错跳过
        End Try

        '2. 先快照子节点，避免翻译过程改动树结构导致枚举异常
        Dim Children As New List(Of DependencyObject)
        Try
            For Each Child As Object In LogicalTreeHelper.GetChildren(Ctl)
                Dim Node As DependencyObject = TryCast(Child, DependencyObject)
                If Node IsNot Nothing Then Children.Add(Node)
            Next
        Catch ex As Exception
        End Try

        '3. 递归
        For Each Node As DependencyObject In Children
            Try
                ApplyCore(Node)
            Catch ex As Exception
            End Try
        Next
    End Sub

    ''' <summary>
    ''' 按控件类型替换对应文本属性。
    ''' </summary>
    Private Sub ApplySelf(Ctl As DependencyObject)
        '语言名称固定使用各自的原生写法；也跳过条目模板内的 TextBlock。
        Dim LanguageNode As DependencyObject = Ctl
        While LanguageNode IsNot Nothing
            Dim LanguageCombo = TryCast(LanguageNode, MyComboBox)
            If LanguageCombo IsNot Nothing AndAlso LanguageCombo.Name = "ComboLanguage" Then Return
            Dim LanguageItem = TryCast(LanguageNode, MyComboBoxItem)
            If LanguageItem IsNot Nothing AndAlso LanguageItem.Tag IsNot Nothing Then
                Dim NativeNames = New String() {"跟随系统（首次打开时的语言）", "简体中文", "繁體中文", "English", "日本語"}
                Dim LanguageId As Integer
                If Integer.TryParse(CStr(LanguageItem.Tag), LanguageId) AndAlso LanguageId >= 0 AndAlso LanguageId < NativeNames.Length Then
                    If String.Equals(TryCast(LanguageItem.Content, String), NativeNames(LanguageId), StringComparison.Ordinal) Then Return
                End If
            End If
            LanguageNode = LogicalTreeHelper.GetParent(LanguageNode)
        End While
        '通用提示（包括普通 TextBlock、Slider 等非自绘文字控件）。
        Dim Element As FrameworkElement = TryCast(Ctl, FrameworkElement)
        If Element IsNot Nothing Then
            Dim Tip As String = TryCast(Element.ToolTip, String)
            If Tip IsNot Nothing Then Element.ToolTip = TranslateValue(Ctl, TextSlot.ToolTip, Tip)
        End If

        '纯文本；保留绑定，避免翻译时切断后续动态更新。
        If TypeOf Ctl Is TextBlock Then
            Dim Lab As TextBlock = DirectCast(Ctl, TextBlock)
            Dim Translated As String = TranslateValue(Ctl, TextSlot.Text, Lab.Text)
            If Not String.Equals(Lab.Text, Translated, StringComparison.Ordinal) Then Lab.SetCurrentValue(TextBlock.TextProperty, Translated)
        End If

        '自绘按钮
        If TypeOf Ctl Is MyButton Then
            Dim Btn As MyButton = DirectCast(Ctl, MyButton)
            Btn.Text = TranslateValue(Ctl, TextSlot.Text, Btn.Text)
            Dim Tip As String = TryCast(Btn.ToolTip, String)
            If Tip IsNot Nothing Then Btn.ToolTip = TranslateValue(Ctl, TextSlot.ToolTip, Tip)
        End If

        '附加按钮 / 图标按钮：没有文字属性，只处理提示
        If TypeOf Ctl Is MyExtraButton Then
            Dim Btn As MyExtraButton = DirectCast(Ctl, MyExtraButton)
            Dim Tip As String = TryCast(Btn.ToolTip, String)
            If Tip IsNot Nothing Then Btn.ToolTip = TranslateValue(Ctl, TextSlot.ToolTip, Tip)
        End If
        If TypeOf Ctl Is MyIconButton Then
            Dim Btn As MyIconButton = DirectCast(Ctl, MyIconButton)
            Dim Tip As String = TryCast(Btn.ToolTip, String)
            If Tip IsNot Nothing Then Btn.ToolTip = TranslateValue(Ctl, TextSlot.ToolTip, Tip)
        End If

        '单选框 / 复选框
        If TypeOf Ctl Is MyRadioBox Then
            Dim Box As MyRadioBox = DirectCast(Ctl, MyRadioBox)
            Box.Text = TranslateValue(Ctl, TextSlot.Text, Box.Text)
            Dim Tip As String = TryCast(Box.ToolTip, String)
            If Tip IsNot Nothing Then Box.ToolTip = TranslateValue(Ctl, TextSlot.ToolTip, Tip)
        End If
        If TypeOf Ctl Is MyRadioButton Then
            Dim Rdo As MyRadioButton = DirectCast(Ctl, MyRadioButton)
            Rdo.Text = TranslateValue(Ctl, TextSlot.Text, Rdo.Text)
            Dim Tip As String = TryCast(Rdo.ToolTip, String)
            If Tip IsNot Nothing Then Rdo.ToolTip = TranslateValue(Ctl, TextSlot.ToolTip, Tip)
        End If
        If TypeOf Ctl Is MyCheckBox Then
            Dim Chk As MyCheckBox = DirectCast(Ctl, MyCheckBox)
            Chk.Text = TranslateValue(Ctl, TextSlot.Text, Chk.Text)
            Dim Tip As String = TryCast(Chk.ToolTip, String)
            If Tip IsNot Nothing Then Chk.ToolTip = TranslateValue(Ctl, TextSlot.ToolTip, Tip)
        End If

        '左栏列表项
        If TypeOf Ctl Is MyListItem Then
            Dim Item As MyListItem = DirectCast(Ctl, MyListItem)
            Item.Title = TranslateValue(Ctl, TextSlot.Title, Item.Title)
            Item.Info = TranslateValue(Ctl, TextSlot.Info, Item.Info)
        End If

        '下拉框条目
        If TypeOf Ctl Is MyComboBoxItem Then
            Dim Item As MyComboBoxItem = DirectCast(Ctl, MyComboBoxItem)
            Dim Body As String = TryCast(Item.Content, String)
            If Body IsNot Nothing Then Item.Content = TranslateValue(Ctl, TextSlot.Content, Body)
            Dim Tip As String = TryCast(Item.ToolTip, String)
            If Tip IsNot Nothing Then Item.ToolTip = TranslateValue(Ctl, TextSlot.ToolTip, Tip)
        End If

        '输入框提示
        If TypeOf Ctl Is MyTextBox Then
            Dim Txt As MyTextBox = DirectCast(Ctl, MyTextBox)
            Txt.HintText = TranslateValue(Ctl, TextSlot.Hint, Txt.HintText)
        End If

        '文字按钮（Label 派生，文字走 Text 属性，Content 随动画同步）
        If TypeOf Ctl Is MyTextButton Then
            Dim Btn As MyTextButton = DirectCast(Ctl, MyTextButton)
            Btn.Text = TranslateValue(Ctl, TextSlot.Text, Btn.Text)
        End If

        '卡片标题
        If TypeOf Ctl Is MyCard Then
            Dim Card As MyCard = DirectCast(Ctl, MyCard)
            Card.Title = TranslateValue(Ctl, TextSlot.Title, Card.Title)
        End If

        '提示条
        If TypeOf Ctl Is MyHint Then
            Dim Bar As MyHint = DirectCast(Ctl, MyHint)
            Bar.Text = TranslateValue(Ctl, TextSlot.Text, Bar.Text)
        End If
    End Sub

#End Region

End Module
