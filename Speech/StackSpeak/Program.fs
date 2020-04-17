﻿open System
open System.Speech.Recognition
    
let sr_SpeechDetected sender (e:SpeechDetectedEventArgs)  =
    printfn "Speech detected..."

let sr_SpeechHypothesized sender (e:SpeechHypothesizedEventArgs)  =
    printfn "Speech hypothesized... (Confidence: %A) %s" e.Result.Confidence e.Result.Text

let sr_SpeechRecognized (interpreterState:Interpreter.State) sender (e:SpeechRecognizedEventArgs)  =
    let previousColor = Console.ForegroundColor
    if e.Result.Confidence > 0.9f then
        Console.ForegroundColor <- ConsoleColor.Green
    else
        Console.ForegroundColor <- ConsoleColor.Cyan

    printfn "Speech recognized... (Confidence: %A) %s" e.Result.Confidence e.Result.Text

    Console.ForegroundColor <- ConsoleColor.Red
    if e.Result.Confidence > 0.9f then
        match Parser.runParser e.Result.Text with
        | Some result -> Interpreter.processCommand interpreterState result
        | None -> ()

    Console.ForegroundColor <- ConsoleColor.Yellow
    printfn "Stack: %A" interpreterState.Stack
    Console.ForegroundColor <- previousColor
    ()

[<EntryPoint>]
let main argv =
    let document = Grammar.createGrammarDocument

    // Create a Grammar object, initializing it with the root rule.
    let g = new Grammar(document, document.Root.Id)
    
    // Create an in-process speech recognizer for the en-US locale.  
    use recognizer =  new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"))

    //recognizer.EndSilenceTimeout <- TimeSpan.FromSeconds 1.0
    //recognizer.InitialSilenceTimeout <- TimeSpan.FromSeconds 3.0
    //recognizer.BabbleTimeout <- TimeSpan.FromSeconds 4.0
    recognizer.EndSilenceTimeoutAmbiguous <- TimeSpan.FromSeconds 2.0

    let interpreterState = Interpreter.State()

    recognizer.SpeechDetected.AddHandler(new EventHandler<SpeechDetectedEventArgs>(sr_SpeechDetected))
    recognizer.SpeechHypothesized.AddHandler(new EventHandler<SpeechHypothesizedEventArgs>(sr_SpeechHypothesized))
    recognizer.SpeechRecognized.AddHandler(new EventHandler<SpeechRecognizedEventArgs>(sr_SpeechRecognized interpreterState))

    // Load the Grammar object into the recognizer.
    recognizer.LoadGrammar(g);

    // Configure input to the speech recognizer.  
    recognizer.SetInputToDefaultAudioDevice();  
  
    // Start asynchronous, continuous speech recognition.  
    recognizer.RecognizeAsync(RecognizeMode.Multiple);  

    // Produce an XML file that contains the grammar.
    //let writer = System.Xml.XmlWriter.Create("srgsDocument.xml")
    //document.WriteSrgs(writer)
    //writer.Close()

    while (true) do
        Console.Read() |> ignore
    0 // return an integer exit code