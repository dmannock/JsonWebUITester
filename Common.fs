module Common

//anything that is useful globally
open System
open System.IO
open canopy
open Config
open CanopyExtensions

//types
type TestDescriptor =
    {
    Description:string;
    Step:(Unit -> Unit)
    }

let createTestDescriptor desc step = { Description = desc; Step= step }

type TestDescriptor
    with static member Create = createTestDescriptor

type ScriptResult =
    {
    Name:string;
    Group:string;
    WebsiteName:string;
    Descriptors:TestDescriptor list
    }

let createScriptResult websiteName group name descriptors =  
        {
            Name=name;
            Group=group;
            WebsiteName=websiteName;
            Descriptors=descriptors 
        }

type ScriptResult with
    static member Create = createScriptResult

let sortByDesc (list:string list) =
    list |> List.sortWith (fun x y -> x.CompareTo(y) * -1)

//actions
let sanitizeFileText (txt:string) =
    System.Text.RegularExpressions.Regex.Replace(txt.Replace("%2f",""), @"[\[\]\=\*\:\~\#\;\%\$\£\""\!\{\}\<\>\?\\\/\.\, ]+", "-")

//TODO: move
let testingStartedTime = DateTime.Now

//screenshoting
let screenshotPath websiteName browserName = Path.Combine(screenshotBasePath, sanitizeFileText(websiteName), testingStartedTime.ToString("yyyy-MM-dd_HHmm"), browserName)

let screenshotName count page context discriptors = 
    let hyphenate (s:string) = s.Replace(' ', '_')
    let standardiseDiscriptors (disc:string list) = 
        disc
        |> List.map (fun s -> s.Split(' ') |> List.ofArray)
        |> List.concat
        |> List.map hyphenate
    String.Format("{0:00}-{1}-{2}-{3}", 
        count, 
        (hyphenate page), 
        (hyphenate context), 
        String.Join("_", (standardiseDiscriptors discriptors)))

let takeScreenshot =
    let screenshotCount = ref 0
    fun websiteName page context (discriptors:string list) ->
        let path = screenshotPath websiteName (getBrowserFriendlyName browser)
        Directory.CreateDirectory(path) |> ignore
        screenshotCount := (!screenshotCount + 1)
        let name = screenshotName (!screenshotCount) page context discriptors
        screenshot path (sanitizeFileText name) |> ignore
//end screenshotting

let doStep (description:string) (action:Unit -> Unit) (postAction:string list -> Unit) = 
    action() |> ignore
    postAction [description]


    