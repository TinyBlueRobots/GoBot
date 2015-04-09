module App

open FSharp.Data
open System
open System.Collections.Generic
open System.Timers

type CCTrayXmlProvider = XmlProvider< "CCTray.xml" >

let deployments = (appSettings "Deployments").Split ',' |> Array.toList
let builds = (appSettings "Builds").Split ',' |> Array.toList
let deployedMessage projectName = appSettings "DeployedMessage" |> sprintf """{"text": "%s %s"}""" projectName
let buildStatusChangedMessage = sprintf """{"text": "%s %O"}"""

let createProject (project : CCTrayXmlProvider.Project) = 
  let projectName (name : string) = 
    name.Split([| "::" |], StringSplitOptions.RemoveEmptyEntries)
    |> Seq.map (fun x -> x.Trim())
    |> Seq.distinct
    |> fun x -> String.Join(" : ", x)
  { Name = projectName project.Name
    BuildStatus = project.LastBuildStatus |> BuildStatus.Create
    Activity = project.Activity |> Activity.Create }

let ProjectReadFromCCTray = Event<Project>()
let ProjectStatusChanged = Event<string>()
let projectState : Dictionary<_, _> = Dictionary()
let projectIsInDeployments project = Seq.exists (fun x -> x = project.Name) deployments
let projectIsInBuilds project = Seq.exists (fun x -> x = project.Name) builds
let projectIsInDeploymentsOrBuilds project = (deployments @ builds) |> List.exists (fun x -> x = project.Name)
let projectHasBeenSeenBefore project = projectState.ContainsKey project.Name

let readCCTray ccTraySource _ = 
  let projects = 
    ccTraySource() |> function 
    | None -> Seq.empty
    | Some xml -> 
      xml
      |> CCTrayXmlProvider.Parse
      |> fun ps -> 
        ps.Projects
        |> Seq.map createProject
        |> Seq.distinctBy (fun p -> p.Name)
        |> Seq.filter projectIsInDeploymentsOrBuilds
  if projectState.Count = 0 then projects |> Seq.iter (fun x -> projectState.Add(x.Name, x))
  projects |> Seq.iter ProjectReadFromCCTray.Trigger

let createTimer (appConfig : AppConfig) = 
  let timer = new Timer(appConfig.Interval, AutoReset = true)
  readCCTray appConfig.GetCCTray |> timer.Elapsed.Add
  timer.Start()

let (|Deployed|BuildStatusChanged|NoChange|) = 
  function 
  | project when projectIsInDeployments project && projectHasBeenSeenBefore project 
                 && projectState.[project.Name].Activity = Building && project.Activity = Sleeping 
                 && project.BuildStatus = Success -> Deployed
  | project when projectHasBeenSeenBefore project && projectIsInBuilds project 
                 && projectState.[project.Name].BuildStatus <> project.BuildStatus -> BuildStatusChanged
  | _ -> NoChange

let checkProjectStatus project = 
  match project with
  | Deployed -> deployedMessage project.Name |> ProjectStatusChanged.Trigger
  | BuildStatusChanged -> buildStatusChangedMessage project.Name project.BuildStatus |> ProjectStatusChanged.Trigger
  | NoChange -> ()
  projectState.[project.Name] <- project

let run appConfig = 
  ProjectReadFromCCTray.Publish
  |> Event.add checkProjectStatus
  ProjectStatusChanged.Publish |> Event.add appConfig.PostNotification
  readCCTray appConfig.GetCCTray None
  createTimer appConfig
