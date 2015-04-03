module CanopyExtensions

//overwrite and extend canopy functions here
open System
open canopy
open runner
open configuration
open OpenQA.Selenium

//functs
let getBrowser (name:string) =
    match name.ToString().ToLowerInvariant() with
    | "chrome" -> chrome
    | "firefox" -> firefox
    | "ie" 
    | "internet explorer" 
    | "internetexplorer" -> ie
    | "phantomjs" -> phantomJS
    | _ -> chrome //use chrome as default for now

let startBrowsers (browsers:string list) =
    browsers 
    |> List.map (fun f -> 
        start (getBrowser f)
        browser)

let getBrowserFriendlyName browser = 
    let raw = browser.ToString().Split('.')
    match raw.Length with
    | 0 -> "Undefined"
    | i -> raw.[i-1].Replace("Driver", "")

let runBrowserTest (inst:OpenQA.Selenium.IWebDriver) = 
    switchTo inst
    printfn "Running in %s" (getBrowserFriendlyName inst)
    run()
    inst

let cleanup (inst:OpenQA.Selenium.IWebDriver) = 
    switchTo inst
    printfn "Closed %s" (getBrowserFriendlyName inst)
    quit()

