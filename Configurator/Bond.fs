module Bond

open System
open Ratings

type BondMetadata = {
    shortName       : string
    description     : string

    issuerName      : string
    borrowerName    : string

    bondStructure   : string
    rateStructure   : string

    maturity        : DateTime option
    issue           : DateTime option
    coupon          : double option

    currency        : string 

    straight        : bool
    putable         : bool
    callable        : bool
    floater         : bool

    industry        : string
    subIndustry     : string

    instrumentType  : string

    seniority       : string

    issuerCountry   : string
    borrowerCountry : string

    issueRating     : Rating option
    issuerRating    : Rating option
    rating          : Rating option

    faceValue       : double
}

type Bond(isin, meta) =
    member self.Isin = isin
    member self.Meta = meta