namespace AvaFunc.App

module QuickNoteDetail =
    open System
    open LiteDB
    open Elmish
    open Avalonia.Controls
    open Avalonia.Media
    open Avalonia.Layout
    open Avalonia.FuncUI.DSL
    open AvaFunc.Core.AvaFuncTypes
    open AvaFunc.Core.QuickNoteHelpers

    type State =
        { Note: QuickNote }

    type Msg =
        | SaveNote
        | DeleteNote
        | GoBack

    let private emptyNote =
        { Id = ObjectId.Empty
          Title = ""
          Content = ""
          CreatedAt = DateTime.Today }

    let update (msg: Msg) (state: State) =
        match msg with
        | SaveNote -> state, Cmd.none
        | DeleteNote -> state, Cmd.none
        (* This is meant to be captured in the parent view hence why we don't take action *)
        | GoBack -> state, Cmd.none

    let init = { Note = emptyNote }

    let private quickNoteForm note dispatch =
        StackPanel.create
            [ StackPanel.classes [ "quicknoteform" ]
              StackPanel.dock Dock.Left
              StackPanel.children
                  [ TextBox.create
                      [ TextBox.maxLength 140
                        TextBox.text note.Title ]
                    TextBox.create
                        [ TextBox.acceptsReturn true
                          TextBox.text note.Content ]
                    Button.create
                        [ Button.isEnabled (note.Title.Length >= 3)
                          Button.content "Save Note" ] ] ]

    let private notificationContent note dispatch =
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
              StackPanel.children
                  [ TextBlock.create [ TextBlock.text noteMsg ]
                    Button.create [ Button.content "Yes, Delete" ]
                    Button.create [ Button.content "Cancel" ] ] ]

    let view (state: State) dispatch =
        DockPanel.create
            [ DockPanel.horizontalAlignment HorizontalAlignment.Stretch
              DockPanel.verticalAlignment VerticalAlignment.Stretch
              DockPanel.children [ yield quickNoteForm state.Note dispatch ] ]
