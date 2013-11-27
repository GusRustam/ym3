namespace DataManager

open System.IO

type Tag = string list * string
type Chain = string * Tag list
type List = string * string list * Tag list

module X =
    let tags : Tag list = [
        (["russia"], "Bonds issued by Russian companies")
        (["usa"], "Bonds issued by US companies")
        (["rouble"; "rub"; "rur"], "Bonds denominated in roubles")
        (["eur"; "euro"], "Bonds denominated in euro")
        (["issuer"], "Bonds grouped by issuer")
    ]

    // todo store this bullshit in XML, and implement commands so that to rollback

//
//type ITaggedItem = 
//    abstract Name : string
//    abstract Tags : string list
//
//type ITaggedList = 
//    inherit ITaggedItem
//    abstract Items : string List
//
//type IGlossary = 
//    abstract Tags : string list
//    abstract Chains : ITaggedItem list
//    abstract Lists : ITaggedList list
//
//type ISaverLoader = 
//    abstract Name : string with get
//    abstract Saved : bool with get
//    abstract Save : string -> unit
//    abstract SaveAs : string * string -> unit
//    abstract Load : string -> ISaverLoader
//    abstract Import : string -> ISaverLoader
//
//type JsonSaverLoader =
//    val fileName : string
//    val saved : bool
//    
//    interface ISaverLoader with
//        override self.Name with get() = self.fileName
//        override self.Saved with get() = self.saved
//        
//        // saving does nothing
//        override self.Save data = ()
//        override self.SaveAs (data, name) = ()
//            
//        // loading and importing does nothing
//        override self.Load name = JsonSaverLoader(name) :> ISaverLoader
//        override self.Import name = self :> ISaverLoader
// 
//    new (name) =  
//        {
//            fileName = name
//            saved = true
//        }
//
//type Glossary(storage : ISaverLoader) =
//    interface IGlossary with
//        override self.Tags = []
//        override self.Lists = []
//        override self.Chains = [] 
//
//    member self.Name with get() = storage.Name
//    member self.Saved with get() = storage.Saved
//    member self.Save = storage.Save
//    member self.SaveAs name = storage.SaveAs name
//    member self.Load name = storage.Load name
//    member self.Import name = storage.Import name