namespace AvaFunc.App

module Shell =
    open Elmish
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL

    type AppView =
        | Home
        | About
        | QuickNotes

    type State =
        { PageStack: List<AppView>
          CurrentView: AppView
          HomeState: Home.State
          AboutState: About.State
          QuickNotesState: QuickNotes.State }

    let init =
        { PageStack = [ Home ]
          CurrentView = Home
          HomeState = Home.init
          AboutState = About.init
          QuickNotesState = QuickNotes.init }

    type Msg =
        | HomeMsg of Home.Msg
        | AboutMsg of About.Msg
        | QuickNotesMsg of QuickNotes.Msg
        | NavigateTo of AppView
        | NavigateBack

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
        | QuickNotesMsg quicknotesMsg ->
            let s, cmd = QuickNotes.update quicknotesMsg state.QuickNotesState
            { state with QuickNotesState = s }, Cmd.map QuickNotesMsg cmd


    let private getMenu state dispatch =
        Menu.create
            [ Menu.viewItems
                [ if state.PageStack.Length > 1 then
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

    let view (state: State) dispatch =
        StackPanel.create
            [ StackPanel.spacing 8.0
              StackPanel.children
                  [ yield getMenu state dispatch
                    match state.CurrentView with
                    | Home -> yield Home.view state.HomeState (HomeMsg >> dispatch)
                    | About -> yield About.view state.AboutState (AboutMsg >> dispatch)
                    | QuickNotes -> yield QuickNotes.view state.QuickNotesState (QuickNotesMsg >> dispatch) ] ]
