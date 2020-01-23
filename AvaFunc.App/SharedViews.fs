namespace AvaFunc.App

[<RequireQualifiedAccess>]
module SharedViews =
    open AvaFunc.Core
    open Avalonia.Controls
    open Avalonia.Layout
    open Avalonia.FuncUI.DSL

    let notificationContent (note: AvaFuncTypes.QuickNote) dispatch proceedMsg cancelMsg =
        let getContent =
            let str = (sprintf "%s - %s" note.Title note.Content)

            let getLength =
                if str.Length <= 26 then (str.Length - 1) else 25

            sprintf "%s..." (str.Substring(0, getLength))

        let noteMsg = sprintf "Delete \"%s\"?" getContent

        StackPanel.create
            [ StackPanel.dock Dock.Top
              StackPanel.verticalAlignment VerticalAlignment.Top
              StackPanel.horizontalAlignment HorizontalAlignment.Center
              StackPanel.spacing 8.0
              StackPanel.margin 8.0
              StackPanel.classes []
              StackPanel.children
                  [ TextBlock.create [ TextBlock.text noteMsg ]
                    Button.create
                        [ Button.content "Yes, Delete"
                          Button.onClick (fun _ -> dispatch proceedMsg) ]
                    Button.create
                        [ Button.content "Cancel"
                          Button.onClick (fun _ -> dispatch cancelMsg) ] ] ]
