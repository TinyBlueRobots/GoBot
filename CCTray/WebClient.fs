module WebClient

open System.Net

let private webClient() = 
  let client = new WebClient(Credentials = NetworkCredential(appSettings "UserName", appSettings "Password"))
  client.Proxy.Credentials <- client.Credentials
  client

let postNotification message = 
  let endPoint = appSettings "WebHook"
  use client = webClient()
  try 
    client.UploadString(endPoint, message) |> ignore
  with ex -> printfn "%O" ex

let getCCTray() = 
  use client = webClient()
  try 
    printfn "%O : Read CCTray.xml" System.DateTime.UtcNow
    appSettings "CCTray"
    |> client.DownloadString
    |> Some
  with ex -> 
    printfn "%O" ex
    None
