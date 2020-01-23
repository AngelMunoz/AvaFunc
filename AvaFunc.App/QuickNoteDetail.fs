namespace AvaFunc.App

module QuickNoteDetail =
    open System
    open LiteDB
    open Elmish
    open Avalonia.Controls
    open Avalonia.Layout
    open Avalonia.FuncUI.DSL
    open AvaFunc.Core
    open AvaFunc.Core.AvaFuncTypes

    type State =
        { Note: QuickNote
          IsDeleting: bool }

    type Msg =
        | SaveNote
        | DeleteNote
        | UpdateNote of NoteSection
        | SetIsDeleting of bool
        | SetNote of ObjectId option
        | GoBack of string option

    let private emptyNote =
        { Id = ObjectId.Empty
          Title = ""
          Content = ""
          CreatedAt = DateTime.Today }

    let update (msg: Msg) (state: State) =
        match msg with
        | SetIsDeleting isDeleting -> { state with IsDeleting = isDeleting }, Cmd.none
        | SetNote noteid ->
            let note =
                match noteid with
                | Some noteid -> QuickNoteHelpers.findOne noteid
                | None -> None
            match note with
            | Some note -> { state with Note = note }, Cmd.none
            | None -> state, Cmd.ofMsg (GoBack(Some("Record Not Found")))
        | UpdateNote section ->
            let note =
                match section with
                | Title title -> { state.Note with Title = title }
                | Content content -> { state.Note with Content = content }
            { state with Note = note }, Cmd.none

        | SaveNote ->
            QuickNoteHelpers.update state.Note |> ignore
            state, Cmd.none
        | DeleteNote ->
            let deleted = QuickNoteHelpers.delete state.Note.Id
            state,
            Cmd.batch
                [ Cmd.ofMsg (GoBack None)
                  Cmd.ofMsg (SetIsDeleting false) ]
        (* This is meant to be captured in the parent view hence why we don't take action *)
        | GoBack textMsg ->
            match textMsg with
            | Some text -> failwith text
            | None -> failwith "Capture me in the parent component please"

    let init =
        { Note = emptyNote
          IsDeleting = false }

    let private quickNoteForm note dispatch =
        StackPanel.create
            [ StackPanel.classes [ "quicknoteform" ]
              StackPanel.dock Dock.Left
              StackPanel.children
                  [ TextBox.create
                      [ TextBox.maxLength 140
                        TextBox.text note.Title
                        TextBox.onTextChanged (fun text -> dispatch (UpdateNote(Title text))) ]
                    TextBox.create
                        [ TextBox.acceptsReturn true
                          TextBox.text note.Content
                          TextBox.onTextChanged (fun text -> dispatch (UpdateNote(Content text))) ]
                    StackPanel.create
                        [ StackPanel.orientation Orientation.Horizontal
                          StackPanel.spacing 12.0
                          StackPanel.children
                              [ Button.create
                                  [ Button.isEnabled (note.Title.Length >= 3)
                                    Button.content "Save Note"
                                    Button.onClick (fun _ -> dispatch SaveNote) ]
                                Button.create
                                    [ Button.isEnabled (note.Title.Length >= 3)
                                      Button.content "Delete Note"
                                      Button.onClick (fun _ -> dispatch (SetIsDeleting true)) ] ] ] ] ]


    let view (state: State) dispatch =
        DockPanel.create
            [ DockPanel.horizontalAlignment HorizontalAlignment.Stretch
              DockPanel.verticalAlignment VerticalAlignment.Stretch
              DockPanel.children
                  [ if state.IsDeleting then
                      yield SharedViews.notificationContent state.Note dispatch DeleteNote (SetIsDeleting false)
                    yield quickNoteForm state.Note dispatch ] ]
