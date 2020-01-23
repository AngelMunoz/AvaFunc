namespace AvaFunc.Core


module AvaFuncTypes =
    open LiteDB
    open System

    [<CLIMutable>]
    type QuickNote =
        { [<BsonId(true)>]
          Id: ObjectId
          Title: string
          Content: string
          CreatedAt: DateTime }

    type Pagination<'T> =
        { Page: int
          Limit: int
          Count: int
          Where: ('T -> bool) option }

    type NoteSection =
        | Title of string
        | Content of String

    [<RequireQualifiedAccess>]
    type Link =
        | Avalonia
        | AvaloniaFuncUI
        | FSharp
        | Dotnet
        | Twitter
