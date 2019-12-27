namespace AvaFunc.App

module PaginationHelpers =
    open System

    let getPageCount limit count = Math.Ceiling((count |> float) / (limit |> float)) |> int
    let canGoBack page = page > 1
    let canGoNext page limit count = page < getPageCount limit count
