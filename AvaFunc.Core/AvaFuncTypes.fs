namespace AvaFunc.Core

open LiteDB
open System

module AvaFuncTypes =
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
