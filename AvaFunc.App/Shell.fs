namespace AvaFunc.App

module Shell =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    type AppView = Home

    type State = 
      { PageStack: List<AppView>
        HomeState: Home.State }

    let init = { PageStack = [Home]; HomeState = Home.init }

    type Msg = 
      | Clicked
      | HomeMsg of Home.Msg

    let update (msg: Msg) (state: State) : State =
        match msg with
        | Clicked -> state
        | HomeMsg msg -> 
          let s = Home.update msg state.HomeState
          { state with HomeState = s }
    
    let view (state: State) (dispatch) =
      Grid.create  [
        Grid.children [ 
          for page in state.PageStack do
          match page with
          | Home -> yield Home.view state.HomeState (HomeMsg >> dispatch)
        ]
      ]
      