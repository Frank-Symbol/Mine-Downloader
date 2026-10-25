Public Class PageSetupSystem


    Private Sub PageSetupSystem_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        '重复加载部分
        PanBack.ScrollToHome()

        If BuildType = BuildTypes.Release Then
        End If

        '非重复加载部分
        Static IsLoaded As Boolean = False
        If IsLoaded Then Return
        IsLoaded = True

        AniControlEnabled += 1
        Reload()
        SliderLoad()
        AniControlEnabled -= 1

    End Sub
    Public Sub Reload()
        SettingService.RefreshSettings(Me)
        RefreshFolderPath()
        RefreshAbout()
    End Sub

    Private ThemeUnlockClicks As Integer

    Private Sub AboutVersion_RightClick(sender As Object, e As MouseButtonEventArgs) Handles LabAboutVersion.MouseRightButtonUp
        e.Handled = True
        If Settings.Get(Of Boolean)("MdThemesUnlocked") Then
            Settings.Set("MdThemesUnlocked", False)
            ThemeUnlockClicks = 0
            ThemeCheckAll(True)
            Hint("已恢复默认主题：跳票红", HintType.Green)
            Return
        End If
        ThemeUnlockClicks += 1
        If ThemeUnlockClicks < 11 Then Return
        ThemeUnlockClicks = 0
        Settings.Set("MdThemesUnlocked", True)
        ThemeCheckAll(True)
        Hint("所有主题已解锁，再次右键版本号恢复默认", HintType.Green)
    End Sub

    '关于
    ''' <summary>
    ''' 刷新「关于」卡片中的版本与项目信息。
    ''' </summary>
    Private Sub RefreshAbout()
        If LabAboutName Is Nothing Then Return
        Try
            LabAboutName.Text = "Mine Downloader"
            LabAboutVersion.Text = VersionDisplay
            LabAboutUpstream.Text = "Plain Craft Launcher（Meloong-Git/PCL）"
        Catch ex As Exception
            Logger.Warn(ex, "刷新关于信息失败")
        End Try
    End Sub
    Private Sub BtnAboutUpstream_Click(sender As Object, e As EventArgs) Handles BtnAboutUpstream.Click
        OpenWebsite("https://github.com/Meloong-Git/PCL")
    End Sub
    Private Sub BtnAboutLicense_Click(sender As Object, e As EventArgs) Handles BtnAboutLicense.Click
        Try
            OpenWebsite("https://github.com/Meloong-Git/PCL/blob/main/LICENCE")
        Catch ex As Exception
            Logger.Error(ex, "打开开源协议失败")
        End Try
    End Sub

    '游戏文件夹（.minecraft）
    ''' <summary>
    ''' 刷新界面显示的游戏文件夹路径。
    ''' </summary>
    Public Sub RefreshFolderPath()
        If LabFolderPath Is Nothing Then Return
        Try
            Dim IsCustom As Boolean = Not String.IsNullOrWhiteSpace(Settings.Get(Of String)("McFolder"))
            LabFolderPath.Text = $"{McFolderSelected}（" & If(IsCustom, "已手动指定", "自动识别：与启动器同目录的 .minecraft 文件夹") & "）"
            LabFolderPath.ToolTip = McFolderSelected
        Catch ex As Exception
            LabFolderPath.Text = "读取游戏文件夹失败"
        End Try
    End Sub
    Private Sub BtnFolderBrowse_Click(sender As Object, e As EventArgs) Handles BtnFolderBrowse.Click
        Dim Target As String = Dialogs.SelectFolder("选择游戏文件夹（.minecraft）", False).FirstOrDefault()
        If String.IsNullOrWhiteSpace(Target) Then Return
        Try
            Target = PathUtils.AddSlashSuffix(PathUtils.ForCompare(Target))
            If Not DirectoryUtils.Exists(Target) Then
                Hint("所选文件夹不存在！", HintType.Red)
                Return
            End If
            If Not CheckPermission(Target) Then
                Hint($"没有对文件夹 {Target} 的写入权限，请换一个位置或以管理员身份运行！", HintType.Red)
                Return
            End If
            Settings.Set("McFolder", Target)
            McFolderSelected = Target
            McFolderListLoader.Start(IsForceRestart:=True)
            RefreshFolderPath()
            Hint($"游戏文件夹已更改为：{Target}", HintType.Green)
            Logger.Info($"用户手动更改了游戏文件夹：{Target}")
        Catch ex As Exception
            Logger.Error(ex, "更改游戏文件夹失败", LogBehavior.Alert)
        End Try
    End Sub
    Private Sub BtnFolderOpen_Click(sender As Object, e As EventArgs) Handles BtnFolderOpen.Click
        Try
            DirectoryUtils.Create(McFolderSelected)
            OpenExplorer(McFolderSelected)
        Catch ex As Exception
            Logger.Error(ex, "打开游戏文件夹失败", LogBehavior.Alert)
        End Try
    End Sub
    Private Sub BtnFolderReset_Click(sender As Object, e As EventArgs) Handles BtnFolderReset.Click
        Settings.Set("McFolder", "")
        McFolderListLoader.Start(IsForceRestart:=True)
        RefreshFolderPath()
        Hint("已恢复为自动识别的游戏文件夹", HintType.Green)
    End Sub
    Public Sub Reset()
        Try
            SettingService.ResetSettings(Me)
            Logger.Info("已初始化启动器页设置")
            Hint("已初始化启动器页设置！", HintType.Green, False)
        Catch ex As Exception
            Logger.Error(ex, "初始化启动器页设置失败", LogBehavior.Alert)
        End Try
        Reload()
    End Sub

    '滑动条
    Private Sub SliderLoad()
        SliderDownloadThread.GetHintText = Function(v) v + 1
        SliderDownloadSpeed.GetHintText =
        Function(v)
            Select Case v
                Case Is <= 14
                    Return (v + 1) * 0.1 & " M/s"
                Case Is <= 31
                    Return (v - 11) * 0.5 & " M/s"
                Case Is <= 41
                    Return (v - 21) & " M/s"
                Case Else
                    Return "无限制"
            End Select
        End Function
    End Sub
    Private Sub SliderDownloadThread_PreviewChange(sender As Object, e As RouteEventArgs) Handles SliderDownloadThread.PreviewChange
        If SliderDownloadThread.Value < 100 Then Return
        If Not Settings.Get(Of Boolean)("HintDownloadThread") Then
            Settings.Set("HintDownloadThread", True)
            MyMsgBox("如果设置过多的下载线程，可能会导致下载时出现非常严重的卡顿。" & vbCrLf &
                     "一般设置 64 线程即可满足大多数下载需求，除非你知道你在干什么，否则不建议设置更多的线程数！", "警告", "我知道了", IsWarn:=True)
        End If
    End Sub


    '自动更新
    ''' <summary>
    ''' 启动器是否已经是最新版？
    ''' 若返回 Nothing，则代表无更新缓存文件或出错。
    ''' </summary>
    Public Shared Function IsLauncherNewest() As Boolean?
        Try
            '确认服务器公告是否正常
            Dim ServerContent As String = If(FileUtils.TryReadAsString(PathTemp & "Cache\Notice.cfg"), "")
            If ServerContent.Split("|").Count < 3 Then Return Nothing
            '确认是否为最新
            Return ServerContent.Split("|")(If(BuildType = BuildTypes.Release, 2, 1)) <= VersionCode
        Catch ex As Exception
            Logger.Error(ex, "确认启动器更新失败")
            Return Nothing
        End Try
    End Function


End Class
