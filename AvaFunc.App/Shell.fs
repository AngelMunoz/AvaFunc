namespace AvaFunc.App



module Shell =
    open Elmish
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout

    type AppView =
        | Home
        | About
        | QuickNotes
        | QuickNoteDetail

    type State =
        { PageStack: List<AppView>
          CurrentView: AppView
          HomeState: Home.State
          AboutState: About.State
          QuickNotesState: QuickNotes.State
          QuickNoteDetailState: QuickNoteDetail.State }

    let init =
        { PageStack = [ QuickNotes ]
          CurrentView = QuickNotes
          HomeState = Home.init
          AboutState = About.init
          QuickNotesState = QuickNotes.init
          QuickNoteDetailState = QuickNoteDetail.init }

    type Msg =
        | NavigateBack
        | HomeMsg of Home.Msg
        | AboutMsg of About.Msg
        | QuickNotesMsg of QuickNotes.Msg
        | QuickNoteDetailMsg of QuickNoteDetail.Msg
        | NavigateTo of AppView

    module private NavHelpers =
        let navigateTo page state =
            match page = state.CurrentView with
            | true -> state, Cmd.none
            | false ->
                { state with
                      PageStack = page :: state.PageStack
                      CurrentView = page }, Cmd.none

        let navigateBack state =
            let stack = state.PageStack.[1..]
            let backPage = stack.Head
            { state with
                  PageStack = stack
                  CurrentView = backPage }, Cmd.none

    let update (msg: Msg) (state: State) =
        match msg with
        | NavigateTo page -> NavHelpers.navigateTo page state
        | NavigateBack -> NavHelpers.navigateBack state
        | HomeMsg msg ->
            let s, cmd = Home.update msg state.HomeState
            { state with HomeState = s }, Cmd.batch [ cmd; Cmd.none ]
        | AboutMsg msg ->
            let s, cmd = About.update msg state.AboutState
            { state with AboutState = s }, Cmd.batch [ cmd; Cmd.none ]
        | QuickNoteDetailMsg msg ->
            let s, cmd = QuickNoteDetail.update msg state.QuickNoteDetailState
            { state with QuickNoteDetailState = s }, Cmd.batch [ cmd; Cmd.none ]
        | QuickNotesMsg quicknotesMsg ->
            match quicknotesMsg with
            | QuickNotes.Msg.NavigateToNote note ->
                { state with QuickNoteDetailState = { state.QuickNoteDetailState with Note = note } },
                Cmd.batch
                    [ Cmd.ofMsg (NavigateTo QuickNoteDetail)
                      Cmd.none ]
            | quicknotesMsg ->
                let s, cmd = QuickNotes.update quicknotesMsg state.QuickNotesState
                { state with QuickNotesState = s }, Cmd.map QuickNotesMsg cmd


    let private getMenu showGoBack dispatch =
        Menu.create
            [ Menu.row 0
              Menu.horizontalAlignment HorizontalAlignment.Stretch
              Menu.viewItems
                  [ if showGoBack then
                      yield MenuItem.create
                                [ MenuItem.onClick (fun _ -> dispatch NavigateBack)
                                  MenuItem.header "Go Back" ]
                    yield MenuItem.create
                              [ MenuItem.onClick (fun _ -> dispatch (NavigateTo Home))
                                MenuItem.header "Home" ]
                    yield MenuItem.create
                              [ MenuItem.onClick (fun _ -> dispatch (NavigateTo About))
                                MenuItem.header "About" ]
                    yield MenuItem.create
                              [ MenuItem.onClick (fun _ -> dispatch (NavigateTo QuickNotes))
                                MenuItem.header "Quick Notes" ] ] ]

    /// <summary>Creates a Grid that wraps the current page being shown in the main window</summary>
    /// <param name="state">This is the Shell <see cref="AvaFunc.App.Shell.State">State</see></param>
    /// <param name="dispatch">The shell dispatch function</param>
    let private pageContent state dispatch =
        Grid.create
            [ Grid.row 1
              Grid.horizontalAlignment HorizontalAlignment.Stretch
              Grid.verticalAlignment VerticalAlignment.Stretch
              Grid.children
                  [ match state.CurrentView with
                    | Home -> Home.view state.HomeState (HomeMsg >> dispatch)
                    | About -> About.view state.AboutState (AboutMsg >> dispatch)
                    | QuickNotes -> QuickNotes.view state.QuickNotesState (QuickNotesMsg >> dispatch)
                    | QuickNoteDetail ->
                        QuickNoteDetail.view state.QuickNoteDetailState (QuickNoteDetailMsg >> dispatch) ] ]

    /// <summary>The main view for this shell</summary>
    let view (state: State) dispatch =
        Grid.create
            [ Grid.rowDefinitions (RowDefinitions("Auto,Auto"))
              Grid.verticalAlignment VerticalAlignment.Stretch
              Grid.horizontalAlignment HorizontalAlignment.Stretch
              Grid.children
                  [ yield getMenu (state.PageStack.Length > 1) dispatch
                    yield pageContent state dispatch ] ]
