[<EntryPoint>]
let main _ = 
  App.run AppConfig.Default
  stdin.ReadLine() |> ignore
  0
