copy ..\bin\Microsoft.Automata.dll .
csc /target:exe /platform:x86 /out:run.exe Program.cs /r:Microsoft.Automata.dll 