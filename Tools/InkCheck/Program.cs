// Uso: dotnet run --project Tools/InkCheck -- Game/Assets/_Project/Narrative/Ink Prologo Capitulo1 ...
// Sale con el número de fallos (0 = todo bien).
using Ink;
using Ink.Runtime;
var dir = System.IO.Path.GetFullPath(args[0]); Directory.SetCurrentDirectory(dir);
int fails = 0;
foreach (var name in args.Skip(1))
{
    var errors = new List<string>();
    var compiler = new Compiler(File.ReadAllText(System.IO.Path.Combine(dir, name + ".ink")), new Compiler.Options
    {
        sourceFilename = name + ".ink", countAllVisits = true,
        fileHandler = new DefaultFileHandler(),
        errorHandler = (msg, type) => { if (type != ErrorType.Author) errors.Add(type + ": " + msg); },
    });
    var story = compiler.Compile();
    if (story == null || errors.Count > 0) { fails++; Console.WriteLine($"[{name}] NO COMPILA"); errors.ForEach(e => Console.WriteLine("   " + e)); continue; }
    foreach (var decision in new[] { "None", "Confess", "StaySilent", "HangUp" })
    foreach (var pick in new[] { 0, -1 })
    {
        var s = new Story(story.ToJson());
        s.allowExternalFunctionFallbacks = true;
        s.variablesState["DECISION_C3"] = decision;
        s.variablesState["SENALES"] = pick == 0 ? 17 : 1;
        s.variablesState["LENTE"] = pick == 0 ? 3 : 0;
        s.variablesState["VALE"] = pick == 0 ? 6 : 0;
        var lines = 0; var steps = 0; string err = null;
        s.onError += (m, t) => { if (t == ErrorType.Error) err = m; };
        try
        {
            while (steps++ < 5000)
            {
                while (s.canContinue) { s.Continue(); lines++; }
                if (s.currentChoices.Count == 0) break;
                s.ChooseChoiceIndex(pick == 0 ? 0 : s.currentChoices.Count - 1);
            }
        }
        catch (Exception e) { err = e.Message; }
        if (err != null || steps >= 5000) { fails++; Console.WriteLine($"[{name}] {decision}/{(pick==0?"primera":"última")}: {err ?? "bucle"}"); }
        else if (decision == "None" && pick == 0) Console.WriteLine($"[{name}] OK ({lines} líneas)");
    }
}
return fails;
