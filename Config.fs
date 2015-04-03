module Config

open System
open System.IO

open FSharp.Data
let jsonConfig = JsonValue.Load(__SOURCE_DIRECTORY__ + "/config.json")

let useConfigOrDefault (path: string, folderName: string) = if (not (String.IsNullOrWhiteSpace(path)) && Directory.Exists(path)) then path 
                                                            else Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, folderName)).FullName

let scriptsPath = useConfigOrDefault(jsonConfig.["scriptsPath"].AsString(), "Recordings")
let screenshotBasePath = useConfigOrDefault(jsonConfig.["screenshotsPath"].AsString(), "Screenshots")

//ui testing config
let configCanopy() =
    canopy.configuration.compareTimeout <- 3.0
    canopy.configuration.elementTimeout <- 3.0
    canopy.configuration.pageTimeout <- 3.0
    canopy.configuration.chromeDir <- Environment.CurrentDirectory
    canopy.configuration.ieDir <- Environment.CurrentDirectory
    canopy.configuration.phantomJSDir <- Environment.CurrentDirectory
    canopy.configuration.failScreenshotPath <- Path.Combine(screenshotBasePath, "Failed")