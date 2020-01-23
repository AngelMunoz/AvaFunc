namespace AvaFunc.App




module Shell =
    open Elmish
    open Avalonia.Controls
    open Avalonia.Layout
    open Avalonia.Media
    open Avalonia.FuncUI.DSL

    type AppView =
        | Home
        | About
        | QuickNotes
        | QuickNoteDetail

    type State =
        { PageStack: AppView list
          CurrentView: AppView
          ErrorTxt: string option
          HomeState: Home.State
          AboutState: About.State
          QuickNotesState: QuickNotes.State
          QuickNoteDetailState: QuickNoteDetail.State }

    let init =
        { PageStack = [ QuickNotes ]
          CurrentView = QuickNotes
          ErrorTxt = None
          HomeState = Home.init
          AboutState = About.init
          QuickNotesState = QuickNotes.init
          QuickNoteDetailState = QuickNoteDetail.init }

    type Msg =
        | HomeMsg of Home.Msg
        | AboutMsg of About.Msg
        | QuickNotesMsg of QuickNotes.Msg
        | QuickNoteDetailMsg of QuickNoteDetail.Msg
        | NavigateTo of AppView * Msg option
        | NavigateBack of Msg option

    module private NavHelpers =
        let navigateTo (page: AppView) (state: State) msg =
            let state =
                match page = state.CurrentView with
                | true -> state
                | false ->
                    { state with
                          PageStack = page :: state.PageStack
                          CurrentView = page }

            let cmd =
                match msg with
                | Some msg -> Cmd.ofMsg msg
                | None -> Cmd.none

            state, cmd


        let lastPage state =
            let stack state = state.PageStack.[1..]
            let page = stack state |> List.head
            page, stack state

        let navigateBack state msg =

            let cmd =
                match msg with
                | Some msg -> Cmd.ofMsg msg
                | None -> Cmd.none

            let (page, stack) = lastPage state
            { state with
                  PageStack = stack
                  CurrentView = page }, cmd

    let navigateToQuickNotes = NavigateTo(QuickNotes, Some(QuickNotesMsg QuickNotes.Msg.LoadQuickNotes))

    let update (msg: Msg) (state: State) =
        match msg with
        | NavigateTo(page, navMsg) -> NavHelpers.navigateTo page state navMsg
        | NavigateBack navMsg -> NavHelpers.navigateBack state navMsg
        | HomeMsg msg ->
            let s, cmd = Home.update msg state.HomeState
            { state with HomeState = s }, Cmd.batch [ cmd; Cmd.none ]
        | AboutMsg msg ->
            let s, cmd = About.update msg state.AboutState
            { state with AboutState = s }, Cmd.batch [ cmd; Cmd.none ]
        | QuickNoteDetailMsg quickNoteDetailMsg ->
            match quickNoteDetailMsg with
            | QuickNoteDetail.Msg.GoBack text -> { state with ErrorTxt = text }, Cmd.ofMsg navigateToQuickNotes
            | quickNoteDetailMsg ->
                let s, cmd = QuickNoteDetail.update quickNoteDetailMsg state.QuickNoteDetailState
                { state with QuickNoteDetailState = s }, Cmd.map QuickNoteDetailMsg cmd
        | QuickNotesMsg quicknotesMsg ->
            match quicknotesMsg with
            | QuickNotes.Msg.NavigateToNote note ->
                let detailMsg = (QuickNoteDetail.Msg.SetNote(Some(note.Id)))
                state, Cmd.ofMsg (NavigateTo(QuickNoteDetail, Some(QuickNoteDetailMsg(detailMsg))))
            | quicknotesMsg ->
                let s, cmd = QuickNotes.update quicknotesMsg state.QuickNotesState
                { state with QuickNotesState = s }, Cmd.map QuickNotesMsg cmd


    let private getMenu state dispatch =
        Menu.create
            [ Menu.row 0
              Menu.horizontalAlignment HorizontalAlignment.Stretch
              Menu.viewItems
                  [ yield MenuItem.create
                              [ MenuItem.onClick (fun _ -> dispatch (NavigateTo(Home, None)))
                                MenuItem.header "Home" ]
                    yield MenuItem.create
                              [ MenuItem.onClick (fun _ -> dispatch (NavigateTo(About, None)))
                                MenuItem.header "About" ]
                    yield MenuItem.create
                              [ MenuItem.onClick (fun _ -> dispatch navigateToQuickNotes)
                                MenuItem.header "Quick Notes" ]
                    if (state.PageStack.Length > 1) then
                        yield MenuItem.create
                                  [ MenuItem.onClick (fun _ ->
                                      let (page, _) = NavHelpers.lastPage state

                                      let msg =
                                          match page with
                                          | QuickNotes -> NavigateBack(Some(navigateToQuickNotes))
                                          | _ -> NavigateBack None
                                      dispatch msg)
                                    MenuItem.header "Go Back" ] ] ]

    let noticeText text =
        let text =
            match text with
            | Some text -> text
            | None -> ""
        TextBlock.create
            [ TextBlock.text text
              TextBlock.dock Dock.Top
              TextBlock.textAlignment TextAlignment.Center ]

    let showErrorText (text: string option) =
        match text with
        | Some text -> text.Length > 0
        | None -> false

    /// <summary>Creates a Grid that wraps the current page being shown in the main window</summary>
    /// <param name="state">This is the Shell <see cref="AvaFunc.App.Shell.State">State</see></param>
    /// <param name="dispatch">The shell dispatch function</param>
    let private pageContent state dispatch =
        DockPanel.create
            [ DockPanel.row 1
              DockPanel.horizontalAlignment HorizontalAlignment.Stretch
              DockPanel.verticalAlignment VerticalAlignment.Stretch
              DockPanel.children
                  [ if showErrorText state.ErrorTxt then yield noticeText state.ErrorTxt
                    match state.CurrentView with
                    | Home -> yield Home.view state.HomeState (HomeMsg >> dispatch)
                    | About -> yield About.view state.AboutState (AboutMsg >> dispatch)
                    | QuickNotes -> yield QuickNotes.view state.QuickNotesState (QuickNotesMsg >> dispatch)
                    | QuickNoteDetail ->
                        yield QuickNoteDetail.view state.QuickNoteDetailState (QuickNoteDetailMsg >> dispatch) ] ]

    /// <summary>The main view for this shell</summary>
    let view (state: State) dispatch =
        Grid.create
            [ Grid.rowDefinitions (RowDefinitions("Auto,Auto"))
              Grid.verticalAlignment VerticalAlignment.Stretch
              Grid.horizontalAlignment HorizontalAlignment.Stretch
              Grid.children
                  [ yield getMenu state dispatch
                    yield pageContent state dispatch ] ]
