[<AutoOpen>]
module AppSettings

let appSettings (key : string) = System.Configuration.ConfigurationManager.AppSettings.[key]