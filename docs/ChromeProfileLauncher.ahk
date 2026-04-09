#Requires AutoHotkey v2.0
#SingleInstance Force

chromePath := "C:\Program Files\Google\Chrome\Application\chrome.exe"
userDataDir := EnvGet("LOCALAPPDATA") "\Google\Chrome\User Data"
profileWindows := Map()

profiles := Map()
localStatePath := userDataDir "\Local State"

if FileExist(localStatePath) {
    json := FileRead(localStatePath, "UTF-8")
    pos := 1
    while RegExMatch(json, '"(Profile \d+|Default)"\s*:\s*\{', &mBlock, pos) {
        profileKey := mBlock[1]
        blockStart := mBlock.Pos + mBlock.Len
        depth := 1
        i := blockStart
        while (i <= StrLen(json) && depth > 0) {
            ch := SubStr(json, i, 1)
            if (ch = "{")
                depth++
            else if (ch = "}")
                depth--
            i++
        }
        block := SubStr(json, blockStart, i - blockStart - 1)
        if RegExMatch(block, '"name"\s*:\s*"(.*?)"', &mName)
            profiles[profileKey] := mName[1]
        pos := i
    }
}

logFile := A_ScriptDir "\debug.log"
if FileExist(logFile)
    FileDelete(logFile)
WriteLog("=== スクリプト起動 [" FormatTime(, "yyyy-MM-dd HH:mm:ss") "] ===`n")

myGui := Gui("+Resize", "Chromeプロファイルランチャ")
myGui.MarginX := 10
myGui.MarginY := 10

iconSize := 32
profileCount := 0
iconTempDir := A_Temp "\ChromeProfileIcons"
DirCreate(iconTempDir)

Loop Files, userDataDir "\*", "D" {
    dir := A_LoopFileName
    if (dir = "Default" || RegExMatch(dir, "Profile \d+")) {
        displayName := profiles.Has(dir) ? profiles[dir] : dir
        WriteLog("プロファイル検出: " dir " (表示名: " displayName ")`n")

        icoPath := userDataDir "\" dir "\Google Profile.ico"

        yOpt := (profileCount > 0) ? "xs" : ""
        iconLoaded := false

        if (FileExist(icoPath)) {
            cropImg := iconTempDir "\" dir "_crop.png"
            needCrop := !FileExist(cropImg)
            if (!needCrop) {
                icoTime := FileGetTime(icoPath, "M")
                cropTime := FileGetTime(cropImg, "M")
                if (icoTime > cropTime) {
                    needCrop := true
                    WriteLog("  ICO更新検出: 再生成`n")
                }
            }
            if (needCrop) {
                psCmd := 'powershell.exe -NoProfile -Command "'
                    . 'Add-Type -AssemblyName System.Drawing;'
                    . "$ico=[System.Drawing.Icon]::new('" icoPath "',128,128);"
                    . '$bmp=$ico.ToBitmap();$w=$bmp.Width;'
                    . '$sz=[int]($w*0.42);$x=$w-$sz-[int]($w*0.02);$y=[int]($w*0.02);'
                    . '$crop=[System.Drawing.Bitmap]::new(64,64);'
                    . '$g=[System.Drawing.Graphics]::FromImage($crop);'
                    . '$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;'
                    . '$g.DrawImage($bmp,[System.Drawing.Rectangle]::new(0,0,64,64),[System.Drawing.Rectangle]::new($x,$y,$sz,$sz),[System.Drawing.GraphicsUnit]::Pixel);'
                    . "$g.Dispose();$crop.Save('" cropImg "',[System.Drawing.Imaging.ImageFormat]::Png);"
                    . '$ico.Dispose();$bmp.Dispose();$crop.Dispose()"'
                try {
                    RunWait(psCmd,, "Hide")
                    WriteLog("  ICO切り抜き成功: " cropImg "`n")
                } catch {
                    WriteLog("  ICO切り抜き失敗`n")
                }
            }
            if (FileExist(cropImg)) {
                try {
                    myGui.AddPicture(yOpt " w" iconSize " h" iconSize, cropImg)
                    iconLoaded := true
                } catch {
                    WriteLog("  切り抜き画像表示失敗: " cropImg "`n")
                }
            }
        }
        if (!iconLoaded) {
            try {
                myGui.AddPicture(yOpt " w" iconSize " h" iconSize " Icon1", chromePath)
            } catch {
                myGui.AddText(yOpt " w" iconSize " h" iconSize " Center", "●")
            }
        }

        btn := myGui.AddButton("x+8 yp w180 h" iconSize, displayName)
        btn.OnEvent("Click", MakeClickHandler(dir, displayName))
        profileCount++
    }
}

myGui.Show()

MakeClickHandler(profileDir, displayName) {
    return (*) => LaunchProfile(profileDir, displayName)
}

WriteLog(msg) {
    global logFile
    try {
        FileAppend(msg, logFile, "UTF-8")
    }
}

LaunchProfile(profileDir, displayName) {
    global chromePath, profileWindows
    
    timestamp := FormatTime(, "yyyy-MM-dd HH:mm:ss")
    WriteLog("`n=== [" timestamp "] ボタンクリック ===`n")
    WriteLog("プロファイルディレクトリ: " profileDir "`n")
    WriteLog("表示名: " displayName "`n")
    
    if (profileWindows.Has(profileDir)) {
        storedHwnd := profileWindows[profileDir]
        WriteLog("  [追跡] 保存済みhwnd=" storedHwnd "`n")
        if WinExist("ahk_id " storedHwnd) {
            title := WinGetTitle(storedHwnd)
            WriteLog("  [追跡] ウィンドウ有効: title=" title "`n")
            WriteLog("→ 保存済みウィンドウをアクティブ化`n")
            WinActivate(storedHwnd)
            return
        } else {
            WriteLog("  [追跡] ウィンドウ無効（閉じられた）。削除`n")
            profileWindows.Delete(profileDir)
        }
    } else {
        WriteLog("  [追跡] 保存済みウィンドウなし`n")
    }
    
    existingSet := Map()
    try {
        for hwnd in WinGetList("ahk_class Chrome_WidgetWin_1") {
            existingSet[hwnd] := true
        }
    }
    WriteLog("  [起動前] 既存Chromeウィンドウ数=" existingSet.Count "`n")
    
    cmd := '"' chromePath '" --profile-directory="' profileDir '"'
    WriteLog("→ 新規起動: " cmd "`n")
    try {
        Run(cmd)
    } catch as err {
        WriteLog("→ Chrome起動エラー: " err.Message "`n")
        MsgBox("Chrome起動エラー: " err.Message, "エラー", 16)
        return
    }
    
    newHwnd := 0
    Loop 50 {
        Sleep(100)
        try {
            for hwnd in WinGetList("ahk_class Chrome_WidgetWin_1") {
                if (!existingSet.Has(hwnd)) {
                    newHwnd := hwnd
                    break 2
                }
            }
        }
    }
    
    if (newHwnd) {
        profileWindows[profileDir] := newHwnd
        title := WinGetTitle(newHwnd)
        WriteLog("→ 新規ウィンドウ検出: hwnd=" newHwnd " title=" title "`n")
        WinActivate(newHwnd)
    } else {
        WriteLog("→ 新規ウィンドウを検出できず（5秒タイムアウト）`n")
    }
}