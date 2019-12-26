namespace AvaFunc.App

module Home =
    open Elmish
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL

    type State =
        { isGreeting: bool }

    type Msg =
        | Greet
        | NotGreet

    let update (msg: Msg) (state: State) =
        match msg with
        | Greet -> { state with isGreeting = true }, Cmd.none
        | NotGreet -> { state with isGreeting = false }, Cmd.none

    let init = { isGreeting = false }

    let view (state: State) dispatch =
        StackPanel.create
            [ StackPanel.spacing 8.0
              StackPanel.children
                  [ Button.create
                      [ Button.onClick (fun _ ->
                          dispatch (if state.isGreeting then NotGreet else Greet))
                        Button.content
                            (sprintf "Hey I'm %s" (if state.isGreeting then "Greeting" else "Not Greeting")) ] ] ]
