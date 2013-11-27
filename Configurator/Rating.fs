module Ratings

open System

type RatingSource = 
    | SnP
    | Moody's
    | Fitch

    member this.Name = 
        match this with
            | SnP -> "Standard and Poor's"
            | Moody's -> "Moody's Investor Service"
            | Fitch -> "Fitch Rating"

    member this.Abbr = 
        match this with
            | SnP -> "SnP"
            | Moody's -> "Moodys"
            | Fitch -> "Fitch"

type Notch = 
    | AAA 
    | Aa1  | Aa2  | Aa3
    | A1   | A2   | A3
    | Baa1 | Baa2 | Baa3
    | Ba1  | Ba2  | Ba3
    | B1   | B2   | B3
    | Caa1 | Caa2 | Caa3
    | Ca   | C
    
    static member Notches = [ 
        (AAA,  1000, ["AAA"]);
        (Aa1,  990,  ["AA+"; "Aa1"]);     (Aa2, 980, ["AA"; "Aa2"]);     (Aa3, 970, ["AA-"; "Aa3"]);
        (A1,   960,  ["A+"; "A1"]);        (A2, 950, ["A"; "A2"]);        (A3, 940, ["A-"; "A3"]);
        (Baa1, 930,  ["BBB+"; "Baa1"]);  (Baa2, 920, ["BBB"; "Baa2"]);  (Baa3, 910, ["BBB-"; "Baa3"]);
        (Ba1,  900,  ["BB+"; "Ba1"]);     (Ba2, 890, ["BB"; "Ba2"]);     (Ba3, 880, ["BB-"; "Ba3"]);
        (B1,   870,  ["B+"; "B1"]);        (B2, 860, ["B"; "B2"]);        (B3, 850, ["B-"; "B3"]);
        (Caa1, 840,  ["CCC+"; "Caa1"]);  (Caa2, 830, ["CCC"; "Caa2"]);  (Caa3, 820, ["CCC-"; "Caa3"]);
        (Ca,   810,  ["CC"; "Ca"]);
        (C,    800,  ["C"])
    ]    

    static member FromName (name : string) =
        let filter notch = 
            match notch with 
                | (_, name, _) -> true
                | _ -> false
        
        match List.tryFind filter Notch.Notches with
            | Some(n, _, _) -> Some(n)
            | _ -> None
    
type Rating = {
    Source : RatingSource; 
    Raing : string; 
    Date : DateTime
}