[<AutoOpen>]
module Types

type BuildStatus = 
  | Exception
  | Success
  | Failure
  | Unknown
  
  override this.ToString() = 
    match this with
    | Exception -> "Exception"
    | Success -> appSettings "SuccessMessage"
    | Failure -> appSettings "FailureMessage"
    | _ -> "Unknown"
  
  static member Create = 
    function 
    | "Exception" -> Exception
    | "Success" -> Success
    | "Failure" -> Failure
    | _ -> Unknown

type Activity = 
  | Sleeping
  | Building
  | CheckingModifications
  
  override this.ToString() = 
    match this with
    | Sleeping -> "Sleeping"
    | Building -> "Building"
    | _ -> "CheckingModifications"
  
  static member Create = 
    function 
    | "Sleeping" -> Sleeping
    | "Building" -> Building
    | _ -> CheckingModifications

type Project = 
  { Name : string
    BuildStatus : BuildStatus
    Activity : Activity }

type AppConfig = 
  { GetCCTray : unit -> string option
    PostNotification : string -> unit
    Interval : float }
  static member Default = 
    { GetCCTray = WebClient.getCCTray
      PostNotification = WebClient.postNotification
      Interval = appSettings "Interval" |> float }
