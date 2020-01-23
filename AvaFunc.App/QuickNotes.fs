namespace AvaFunc.App


module QuickNotes =
    open System
    open LiteDB
    open Elmish
    open Avalonia.Controls
    open Avalonia.Media
    open Avalonia.Layout
    open Avalonia.Controls.Primitives
    open Avalonia.FuncUI.DSL
    open AvaFunc.Core
    open AvaFunc.Core.AvaFuncTypes

    type State =
        { Pagination: Pagination<QuickNote>
          Notes: seq<QuickNote>
          CurrentNote: QuickNote
          IsLoading: bool
          IsShowingDelete: bool * QuickNote
          IsHoveringOver: bool * QuickNote }

    type Msg =
        | LoadQuickNotes
        | FetchNotes
        | NextPage
        | PreviousPage
        | AddNote
        | HideDelete
        | NavigateToNote of QuickNote
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
        (* This is meant to be captured in the parent view hence why we don't take action *)
        | NavigateToNote note -> failwith "Capture me in the parent component please"
        | ShowDelete note -> { state with IsShowingDelete = true, note }, Cmd.none
        | HideDelete -> { state with IsShowingDelete = false, emptyNote }, Cmd.none
        | DeleteNote note ->
            QuickNoteHelpers.delete note.Id |> ignore
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
            let notes, count =
                QuickNoteHelpers.find (Some state.Pagination.Page) (Some state.Pagination.Limit) state.Pagination.Where
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
            QuickNoteHelpers.create note |> ignore
            state,
            Cmd.batch
                [ Cmd.ofMsg FetchNotes
                  Cmd.none ]

    let init =
        let pagination =
            { Page = 1
              Limit = 9
              Count = 0
              Where = None }

        let notes, count = QuickNoteHelpers.find (Some pagination.Page) (Some pagination.Limit) pagination.Where
        { Pagination = { pagination with Count = count }
          Notes = notes
          CurrentNote = emptyNote
          IsShowingDelete = false, emptyNote
          IsHoveringOver = false, emptyNote
          IsLoading = false }

    let private quickNoteForm note dispatch =
        StackPanel.create
            [ StackPanel.classes [ "quicknoteform" ]
              StackPanel.dock Dock.Left
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

    let private quickNoteTemplate (note: QuickNote) dispatch =
        Grid.create
            [ Grid.classes [ "quicknoteitem" ]
              Grid.rowDefinitions (RowDefinitions("25,25,50"))
              Grid.columnDefinitions (ColumnDefinitions("50,50"))
              Grid.horizontalAlignment HorizontalAlignment.Stretch
              Grid.verticalAlignment VerticalAlignment.Stretch
              Grid.children
                  [ TextBlock.create
                      [ TextBlock.row 0
                        TextBlock.columnSpan 2
                        TextBlock.classes [ "title" ]
                        TextBlock.text note.Title
                        TextBlock.textWrapping TextWrapping.Wrap ]
                    TextBlock.create
                        [ TextBlock.row 1
                          TextBlock.columnSpan 2
                          TextBlock.classes [ "subtitle" ]
                          TextBlock.text note.Content
                          TextBlock.textWrapping TextWrapping.NoWrap ]
                    Button.create
                        [ Button.row 2
                          Button.column 1
                          Button.content "🗑"
                          Button.classes [ "deletebtn"; "tobottom" ]
                          Button.onClick (fun _ -> dispatch (ShowDelete note)) ]
                    Button.create
                        [ Button.row 2
                          Button.column 0
                          Button.content "📃"
                          Button.classes [ "detailbtn"; "tobottom" ]
                          Button.onClick (fun _ -> dispatch (NavigateToNote note)) ] ] ]

    let private notesList notes dispatch =
        ScrollViewer.create
            [ ScrollViewer.dock Dock.Top
              ScrollViewer.verticalScrollBarVisibility ScrollBarVisibility.Auto
              ScrollViewer.content
                  (UniformGrid.create
                      [ UniformGrid.children
                          [ for note in notes do
                              yield quickNoteTemplate note dispatch ] ]) ]


    let private paginationMenu (pagination: Pagination<QuickNote>) dispatch =
        Menu.create
            [ Menu.dock Dock.Top
              Menu.verticalAlignment VerticalAlignment.Center
              Menu.horizontalAlignment HorizontalAlignment.Right
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

    let view (state: State) dispatch =
        let isShowing, note = state.IsShowingDelete
        DockPanel.create
            [ DockPanel.horizontalAlignment HorizontalAlignment.Stretch
              DockPanel.verticalAlignment VerticalAlignment.Stretch
              DockPanel.children
                  [ yield paginationMenu state.Pagination dispatch
                    yield quickNoteForm state.CurrentNote dispatch
                    if isShowing then
                        yield SharedViews.notificationContent note dispatch (DeleteNote note) HideDelete
                    yield notesList state.Notes dispatch ] ]
