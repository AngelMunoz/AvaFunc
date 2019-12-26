namespace AvaFunc.Core

open LiteDB
open AvaFunc.Core.AvaFuncTypes
open AvaFunc.Core.Database


module QuickNoteHelpers =
    let private quicknotes (db: LiteDatabase) =
        let col = db.GetCollection<QuickNote>()
        col.EnsureIndex "Title" |> ignore
        col.EnsureIndex "Content" |> ignore
        col

    let find (page: int option) (limit: int option) (where: (QuickNote -> bool) option) =
        use db = getDatabase dbpath
        let col = quicknotes db

        let page = defaultArg page 0
        let limit = defaultArg limit 10
        let skip = limit * (page - 1)

        match where with
        | Some where ->
            let results = col.Find(where, skip, limit) |> Seq.toList
            let count = col.Count(where)
            (results, count)
        | None ->
            let results = col.Find(Query.All(), skip, limit) |> Seq.toList
            let count = col.Count()
            (results, count)

    let findOne (id: ObjectId) =
        use db = getDatabase dbpath
        let col = quicknotes db

        let results = col.Find(fun note -> note.Id = id)
        results |> Seq.tryHead

    let create (note: QuickNote) =
        use db = getDatabase dbpath
        let col = quicknotes db
        col.Insert note

    let delete (id: ObjectId) =
        use db = getDatabase dbpath
        let col = quicknotes db
        col.Delete(fun note -> note.Id = id)

    let update (note: QuickNote) =
        use db = getDatabase dbpath
        let col = quicknotes db
        col.Update note
