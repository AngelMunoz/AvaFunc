namespace AvaFunc.App

open Elmish
open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts
open Avalonia.Media

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "AvaFunc"
        base.Width <- 800.0
        base.Height <- 600.0
        base.MinWidth <- 526.0
        base.MinHeight <- 526.0
        base.FontFamily <-
            FontFamily.Parse
                "Segoe UI, San Francisco, Helvetica Neue, Lucida Grande, Roboto, Oxygen-Sans, Ubuntu, Cantarell, Segoe UI Emoji, Apple Color Emoji"
        //this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
        //this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true

        Program.mkProgram (fun () -> Shell.init, Cmd.none) Shell.update Shell.view
        |> Program.withHost this
        |> Program.withConsoleTrace
        |> Program.run

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
        this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"
        this.Styles.Load "avares://AvaFunc.App/Styles.xaml"

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> desktopLifetime.MainWindow <- MainWindow()
        | _ -> ()

module Program =

    [<EntryPoint>]
    let main (args: string []) =
        AppBuilder.Configure<App>().UsePlatformDetect().UseSkia().StartWithClassicDesktopLifetime(args)
