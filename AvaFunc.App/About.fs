namespace AvaFunc.App

open Avalonia.Media
open Avalonia.Input

module About =
    open Elmish
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open AvaFunc.Core
    open AvaFunc.Core.AvaFuncTypes

    type State =
        { Greeting: string }

    type Msg = OpenLink of Link

    let update (msg: Msg) (state: State) =
        match msg with
        | OpenLink link ->
            match link with
            | Link.Avalonia -> Helpers.openUrl "https://github.com/AvaloniaUI/Avalonia" |> ignore
            | Link.AvaloniaFuncUI -> Helpers.openUrl "https://github.com/AvaloniaCommunity/Avalonia.FuncUI" |> ignore
            | Link.FSharp -> Helpers.openUrl "https://fsharp.org/" |> ignore
            | Link.Dotnet -> Helpers.openUrl "https://dotnet.microsoft.com/" |> ignore
            | Link.Twitter -> Helpers.openUrl "https://twitter.com/daniel_tuna" |> ignore
            state, Cmd.none

    let init = { Greeting = "Hello There" }

    let text =
        "My Name is Angel Munoz and Sometimes I do stuff like this.\n"
        + "Please consider visiting and taking a look at the awesome\n" + "projects that allow this app to exist"

    let view (state: State) dispatch =
        DockPanel.create
            [ DockPanel.row 1
              DockPanel.children
                  [ StackPanel.create
                      [ StackPanel.dock Dock.Left
                        StackPanel.margin (12.0, 4.0)
                        StackPanel.spacing 12.0
                        StackPanel.classes []
                        StackPanel.children
                            [ TextBlock.create
                                [ TextBlock.text "Avalonia"
                                  TextBlock.classes [ "link" ]
                                  TextBlock.onTapped (fun _ -> dispatch (OpenLink Link.Avalonia)) ]
                              TextBlock.create
                                  [ TextBlock.text "Avalonia.FuncUI"
                                    TextBlock.classes [ "link" ]
                                    TextBlock.onTapped (fun _ -> dispatch (OpenLink Link.AvaloniaFuncUI)) ]
                              TextBlock.create
                                  [ TextBlock.text "F#"
                                    TextBlock.classes [ "link" ]
                                    TextBlock.onTapped (fun _ -> dispatch (OpenLink Link.Dotnet)) ]
                              TextBlock.create
                                  [ TextBlock.text ".Netcore"
                                    TextBlock.classes [ "link" ]
                                    TextBlock.onTapped (fun _ -> dispatch (OpenLink Link.FSharp)) ] ] ]
                    StackPanel.create
                        [ StackPanel.dock Dock.Top
                          StackPanel.margin (12.0, 4.0)
                          StackPanel.spacing 8.0
                          StackPanel.classes []
                          StackPanel.children
                              [ TextBlock.create [ TextBlock.text text ]
                                TextBlock.create [ TextBlock.text "You can Find me on Twitter" ]
                                TextBlock.create
                                    [ TextBlock.text "@daniel_tuna"
                                      TextBlock.classes [ "link" ]
                                      TextBlock.onTapped (fun _ -> dispatch (OpenLink Link.Twitter)) ] ] ] ] ]
