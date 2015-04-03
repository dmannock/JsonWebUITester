module RunnerConsole

open System
open System.IO
open canopy
open runner
open configuration
open reporters
open types
open Config
open CanopyExtensions
open Common

let Start (browsers:string List) = 
    printfn "Runner Started."
    configCanopy()
    //use default browser is not provided
    let browsers = if browsers.Length.Equals(0) then [""] else browsers //[browser;"chrome";"phantomjs"]               
    let instances = startBrowsers (browsers |> sortByDesc)
    //define tests
    Scripts.runAll()
    instances
        |> List.map runBrowserTest 
        |> List.iter cleanup