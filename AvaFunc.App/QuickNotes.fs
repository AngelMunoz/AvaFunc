namespace AvaFunc.App

open LiteDB
open Avalonia.Controls.Primitives

module QuickNotes =
    open System
    open Elmish
    open Avalonia.Controls
    open Avalonia.Media
    open Avalonia.Layout
    open Avalonia.FuncUI.DSL
    open AvaFunc.Core.AvaFuncTypes
    open AvaFunc.Core.QuickNoteHelpers

    type State =
        { Pagination: Pagination<QuickNote>
          Notes: seq<QuickNote>
          CurrentNote: QuickNote
          IsLoading: bool
          IsShowingDelete: bool * QuickNote }

    type Msg =
        | LoadQuickNotes
        | FetchNotes
        | NextPage
        | PreviousPage
        | AddNote
        | HideDelete
        | ShowDelete of QuickNote
        | DeleteNote of QuickNote
        | UpdateNoteTitle of string
        | UpdateNoteContent of string

    let private emptyNote =
        { Id = ObjectId.Empty
          Title = ""
          Content = ""
          CreatedAt = DateTime.Today }

    let update (msg: Msg) (state: State) =
        match msg with
        | ShowDelete note -> { state with IsShowingDelete = true, note }, Cmd.none
        | HideDelete -> { state with IsShowingDelete = false, emptyNote }, Cmd.none
        | DeleteNote note ->
            delete note.Id |> ignore
            state,
            Cmd.batch
                [ Cmd.ofMsg HideDelete
                  Cmd.ofMsg LoadQuickNotes ]
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
                  Notes = notes
                  IsLoading = false }, Cmd.none
        | UpdateNoteContent content ->
            { state with CurrentNote = { state.CurrentNote with Content = content } }, Cmd.none
        | UpdateNoteTitle title -> { state with CurrentNote = { state.CurrentNote with Title = title } }, Cmd.none
        | AddNote ->
            let note =
                { state.CurrentNote with
                      CreatedAt = DateTime.Now
                      Id = ObjectId.NewObjectId() }
            create note |> ignore
            state,
            Cmd.batch
                [ Cmd.ofMsg FetchNotes
                  Cmd.none ]

    let init =
        let pagination =
            { Page = 1
              Limit = 5
              Count = 0
              Where = None }

        let notes, count = find (Some pagination.Page) (Some pagination.Limit) pagination.Where
        { Pagination = { pagination with Count = count }
          Notes = notes
          CurrentNote = emptyNote
          IsShowingDelete = false, emptyNote
          IsLoading = false }

    let private quickNoteTemplate (note: QuickNote) dispatch =
        StackPanel.create
            [ StackPanel.spacing 12.0
              StackPanel.margin (12.0, 0.0, 0.0, 8.0)
              StackPanel.orientation Orientation.Horizontal
              StackPanel.children
                  [ StackPanel.create
                      [ StackPanel.width 162.0
                        StackPanel.height 82.0
                        StackPanel.spacing 8.0
                        StackPanel.children
                            [ TextBlock.create
                                [ TextBlock.text note.Title
                                  TextBlock.textWrapping TextWrapping.Wrap
                                  TextBlock.maxHeight 224.0 ]
                              TextBlock.create
                                  [ TextBlock.text note.Content
                                    TextBlock.textWrapping TextWrapping.NoWrap
                                    TextBlock.maxHeight 128.0 ] ] ]
                    Button.create
                        [ Button.content "Delete Note"
                          Button.onClick (fun _ -> dispatch (ShowDelete note)) ] ] ]

    let private quickNotesList notes dispatch =
        ItemsControl.create
            [ ItemsControl.horizontalAlignment HorizontalAlignment.Stretch
              ItemsControl.dock Dock.Right
              ItemsControl.viewItems
                  [ for note in notes do
                      yield quickNoteTemplate note dispatch ] ]

    let private quickNoteForm note dispatch =
        StackPanel.create
            [ StackPanel.dock Dock.Left
              StackPanel.spacing 8.0
              StackPanel.width 162.0
              StackPanel.children
                  [ TextBox.create
                      [ TextBox.maxLength 140
                        TextBox.onTextChanged (fun text ->
                            match String.IsNullOrEmpty(text) with
                            | true -> ()
                            | false -> dispatch (UpdateNoteTitle text)) ]
                    TextBox.create
                        [ TextBox.acceptsReturn true
                          TextBox.onTextChanged (fun text ->
                              match String.IsNullOrEmpty(text) with
                              | true -> ()
                              | false -> dispatch (UpdateNoteContent text)) ]
                    Button.create
                        [ Button.onClick (fun _ -> dispatch AddNote)
                          Button.isEnabled (note.Title.Length >= 3)
                          Button.content "Save Note" ] ] ]

    let private scrollContent notes dispatch =
        ScrollViewer.create
            [ ScrollViewer.dock Dock.Top
              ScrollViewer.verticalAlignment VerticalAlignment.Stretch
              ScrollViewer.verticalScrollBarVisibility ScrollBarVisibility.Auto
              ScrollViewer.content (quickNotesList notes dispatch) ]

    let private bottomMenu (pagination: Pagination<QuickNote>) dispatch =
        Menu.create
            [ Menu.dock Dock.Bottom
              Menu.verticalAlignment VerticalAlignment.Bottom
              Menu.viewItems
                  [ MenuItem.create
                      [ MenuItem.onClick (fun _ -> dispatch PreviousPage)
                        MenuItem.isEnabled (PaginationHelpers.canGoBack pagination.Page)
                        MenuItem.header "Back" ]
                    MenuItem.create
                        [ MenuItem.isEnabled false
                          MenuItem.header
                              (sprintf "Page: %i of %i" pagination.Page
                                   (PaginationHelpers.getPageCount pagination.Limit pagination.Count)) ]
                    MenuItem.create
                        [ MenuItem.onClick (fun _ -> dispatch NextPage)
                          MenuItem.isEnabled
                              (PaginationHelpers.canGoNext pagination.Page pagination.Limit pagination.Count)
                          MenuItem.header "Next" ] ] ]

    let private notificationContent note dispatch =
        let getContent =
            let str = (sprintf "%s - %s" note.Title note.Content)

            let getLength =
                if str.Length <= 26 then (str.Length - 1) else 25

            sprintf "%s..." (str.Substring(0, getLength))

        let noteMsg = sprintf "Delete \"%s\"?" getContent

        StackPanel.create
            [ StackPanel.dock Dock.Right
              StackPanel.verticalAlignment VerticalAlignment.Top
              StackPanel.horizontalAlignment HorizontalAlignment.Right
              StackPanel.spacing 8.0
              StackPanel.children
                  [ TextBlock.create [ TextBlock.text noteMsg ]
                    Button.create
                        [ Button.onClick (fun _ -> dispatch (DeleteNote note))
                          Button.content "Yes, Delete" ]
                    Button.create
                        [ Button.onClick (fun _ -> dispatch HideDelete)
                          Button.content "Cancel" ] ] ]

    let view (state: State) dispatch =
        let isShowing, note = state.IsShowingDelete
        DockPanel.create
            [ DockPanel.margin 12.0
              DockPanel.horizontalAlignment HorizontalAlignment.Stretch
              DockPanel.verticalAlignment VerticalAlignment.Stretch
              DockPanel.children
                  [ yield quickNoteForm state.CurrentNote dispatch
                    yield scrollContent state.Notes dispatch
                    if isShowing then yield notificationContent note dispatch
                    yield bottomMenu state.Pagination dispatch ] ]
