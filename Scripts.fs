module Scripts

//#r "../packages/FSharp.Data.2.2.0/lib/net40/FSharp.Data.dll"
open canopy
open runner
open FSharp.Data
open Config
open Common
open System.IO

let [<Literal>] sampleScript = __SOURCE_DIRECTORY__ + "\Recording_sample.json.provider"
type ScriptType = JsonProvider<sampleScript>

let dataDir = 
        if (not (Directory.Exists(scriptsPath))) then failwith @"scripts path doesn't exist"
        new DirectoryInfo(scriptsPath)

let scriptFiles =
    [
        for file in dataDir.EnumerateFiles("*.json", SearchOption.AllDirectories) do
            let subDir = if file.Directory.FullName.Equals(Config.scriptsPath, System.StringComparison.InvariantCultureIgnoreCase) then "" else file.Directory.Name 
            yield (file.Name, subDir, ScriptType.Load(file.FullName))
    ]

type scriptActions =
    | Empty
    | Get of Option:string
    | SetText of ScriptType.Locator * string
    | Click of ScriptType.Locator
    | SetSelected of ScriptType.Locator
    | VerifyTextPresent of string
    | VerifyElementPresent of ScriptType.Locator

let convertScriptStepToAction (step:ScriptType.Step) =
    match step.Type, step.Text, step.Locator, step.Url with
    | ("get", _, _, Some u) -> Get u
    | ("setElementText", Some t, Some l,  _) -> SetText(l, t)
    | ("clickElement", _, Some l,  _) -> Click l
    | ("setElementSelected", _, Some l,  _) -> SetSelected l
    //no implemented
    //| ("verifyTextPresent", _, Some l,  _) -> VerifyTextPresent l
    | ("verifyElementPresent", _, Some l,  _) -> VerifyElementPresent l
    | _ -> Empty

let localiseFileUri subDir u = 
    //TODO: replace exceptions with types
    if System.String.IsNullOrWhiteSpace(u) then failwith "url is an invalid"
    else
        let uri = new System.Uri(u)
        match uri.IsFile with
        | true when File.Exists(uri.LocalPath) -> uri.LocalPath
        | true -> uri.AbsoluteUri.Replace(@"file://", @"file://" + Path.Combine(Config.scriptsPath, subDir))
        | false -> uri.AbsoluteUri

let buildSelector (locator:ScriptType.Locator) = 
    match locator.Type with
    | "id" -> sprintf "#%s" locator.Value
    | "class" -> sprintf ".%s" locator.Value
    | "link text" -> sprintf "a:contains(%s)" locator.Value
    | "text" -> sprintf ":contains(%s)" locator.Value //internal use
    | "name" -> sprintf "[name='%s']" locator.Value
    //| "xpath" //not implemented
    | "css selector"
    | _ -> locator.Value

let buildDescription = function
    | Get u -> sprintf "Navigating to url %s" (sanitizeFileText u)
    | SetText(l, t) -> sprintf "Setting text of %s to %s" (sanitizeFileText l.Value) (sanitizeFileText t)
    | Click l -> sprintf "Clicking %s" (sanitizeFileText l.Value)
    | SetSelected l -> sprintf "Selecting %s" (sanitizeFileText l.Value)
    | VerifyTextPresent t -> sprintf "Verify Text Present %s" (sanitizeFileText t)
    | VerifyElementPresent l -> sprintf "Verify Element Present %s" (sanitizeFileText l.Value)
    | _ -> ""   
           
let scriptOfCanopy subDir = function
    | Get u -> url (localiseFileUri subDir u)
    | SetText(l, t) -> (buildSelector l) << t
    | Click l -> click (buildSelector l)
    | SetSelected l -> check (buildSelector l)
    | VerifyTextPresent t -> waitFor (fun _ -> (new ScriptType.Locator("text", t) |> buildSelector |> element).Displayed)
    | VerifyElementPresent l -> waitFor (fun _ ->  (buildSelector l |> element).Displayed)
    | _ -> ()


let private convertScript (fileName, subDir, json) = 
    let parseJson (json:ScriptType.Root) =
        let convertToScriptActions (item:ScriptType.Root) =  item.Steps |> Array.map convertScriptStepToAction
        if (json.FormatVersion = 2 && json.SeleniumVersion = 2) then convertToScriptActions json
        else [|Empty|]
    let createScriptResult scriptActs =
        let websiteName = match (scriptActs: scriptActions[]) |> Array.tryPick (function | Get u -> Some u | _ -> None) with
                            | Some u when u.StartsWith(@"file://") -> subDir
                            | Some u -> u
                            | None -> subDir
        let descriptors = scriptActs
                        |> List.ofArray
                        |> List.map (fun f -> TestDescriptor.Create (buildDescription f) (fun () -> scriptOfCanopy subDir f))
        ScriptResult.Create websiteName subDir fileName descriptors
    json |> parseJson |> createScriptResult


let testWrap websiteName pageName testName steps =
    let ss = Common.takeScreenshot websiteName pageName testName
    testName &&& fun _ -> steps |> List.iter (fun step -> Common.doStep step.Description step.Step ss)

let runForScript script = 
    script  |> convertScript |> (fun converted -> testWrap converted.WebsiteName converted.Group converted.Name converted.Descriptors)

//entry point
let runAll() =
       scriptFiles |> List.iter runForScript

// custom operators for writing tests in f# 
// and utilising the same test wrapper features (screenshotting, etc)
// over using generated json files from a recording tool

//let convertToTestDescriptor steps =
//    steps |> List.map (fun f -> TestDescriptor.Create (fst f) (snd f))
//
////basically wraps with post screenshot process after each step
//let (!&&&!) (pageName:string) (testName:string) (steps:(string * (Unit -> Unit)) list) = 
//    testWrap pageName testName (convertToTestDescriptor steps)
//
//let (@!) step a nextStep b =
//    (step a)::[(nextStep, b)]


