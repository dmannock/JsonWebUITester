namespace JsonWebUITester

open System

module Program =

    [<EntryPoint>]
    let main(argv) = 
        RunnerConsole.Start (List.ofArray argv)
        0
