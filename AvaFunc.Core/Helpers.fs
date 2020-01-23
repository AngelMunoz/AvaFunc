namespace AvaFunc.Core

[<RequireQualifiedAccess>]
module Helpers =
    open System.Runtime.InteropServices
    open System.Diagnostics

    let openUrl (url: string) =
        if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
            let urlProcess = ProcessStartInfo("cmd", sprintf "/c start %s" url)
            urlProcess.CreateNoWindow <- true
            Process.Start(urlProcess) |> ignore
        else if RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then
            Process.Start("xdg-open", url) |> ignore
        else if RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
            Process.Start("open", url) |> ignore
