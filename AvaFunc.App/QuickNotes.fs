namespace AvaFunc.App

open LiteDB

module QuickNotes =
    open System
    open Elmish
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open AvaFunc.Core.AvaFuncTypes
    open AvaFunc.Core.QuickNoteHelpers

    type State =
        { Pagination: Pagination<QuickNote>
          Notes: seq<QuickNote>
          CurrentNote: QuickNote
          IsLoading: bool }

    type Msg =
        | LoadQuickNotes
        | FetchNotes
        | NextPage
        | PreviousPage
        | AddNote
        | UpdateNoteTitle of string
        | UpdateNoteContent of string

    let update (msg: Msg) (state: State) =
        match msg with
        | LoadQuickNotes ->
            match state.IsLoading with
            | false -> { state with IsLoading = true }, Cmd.ofMsg FetchNotes
            | true -> state, Cmd.none
        | NextPage ->
            match state.IsLoading with
            | false ->
                { state with Pagination = { state.Pagination with Page = state.Pagination.Page + 1 } },
                Cmd.ofMsg FetchNotes
            | true -> state, Cmd.none
        | PreviousPage ->
            match state.IsLoading with
            | false ->
                { state with Pagination = { state.Pagination with Page = state.Pagination.Page - 1 } },
                Cmd.ofMsg FetchNotes
            | true -> state, Cmd.none
        | FetchNotes ->
            let notes, count = find (Some state.Pagination.Page) (Some state.Pagination.Limit) state.Pagination.Where
            { state with
                  Pagination = { state.Pagination with Count = count }
                  Notes = notes }, Cmd.none
        | UpdateNoteContent content ->
            { state with CurrentNote = { state.CurrentNote with Content = content } }, Cmd.none
        | UpdateNoteTitle title -> { state with CurrentNote = { state.CurrentNote with Title = title } }, Cmd.none
        | AddNote ->
            let note =
                { state.CurrentNote with
                      CreatedAt = DateTime.Now
                      Id = ObjectId.NewObjectId() }
            create note |> ignore
            { state with
                  CurrentNote =
                      { Id = ObjectId.Empty
                        Title = ""
                        Content = ""
                        CreatedAt = DateTime.Today } },
            Cmd.batch
                [ Cmd.ofMsg FetchNotes
                  Cmd.none ]

    let init =
        let pagination =
            { Page = 1
              Limit = 10
              Count = 0
              Where = None }

        let notes, count = find (Some pagination.Page) (Some pagination.Limit) pagination.Where
        { Pagination = { pagination with Count = count }
          Notes = notes
          CurrentNote =
              { Id = ObjectId.Empty
                Title = ""
                Content = ""
                CreatedAt = DateTime.Today }
          IsLoading = true }

    let view (state: State) dispatch =
        DockPanel.create
            [ DockPanel.margin 12.0
              DockPanel.children
                  [ StackPanel.create
                      [ StackPanel.dock Dock.Left
                        StackPanel.spacing 8.0
                        StackPanel.children
                            [ TextBox.create [ TextBox.onTextChanged (fun text -> dispatch (UpdateNoteTitle text)) ]
                              TextBox.create [ TextBox.onTextChanged (fun text -> dispatch (UpdateNoteContent text)) ]
                              Button.create
                                  [ Button.onClick (fun _ -> dispatch AddNote)
                                    Button.content "Save Note" ] ] ]
                    ItemsControl.create
                        [ ItemsControl.dock Dock.Right
                          ItemsControl.viewItems
                              [ for note in state.Notes do
                                  yield StackPanel.create
                                            [ StackPanel.spacing 12.0
                                              StackPanel.children
                                                  [ TextBlock.create [ TextBlock.text note.Title ]
                                                    TextBlock.create [ TextBlock.text note.Content ] ] ] ] ] ] ]
