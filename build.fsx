#r "packages/FAKE/tools/FakeLib.dll"

let solutionFile = "CCTray.sln "

open Fake

Target "Default" (fun _ -> 
    !!"src/**/bin/Release/" |> CleanDirs
    !!solutionFile
    |> MSBuildRelease "" "Build"
    |> ignore)
RunTargetOrDefault "Default"
