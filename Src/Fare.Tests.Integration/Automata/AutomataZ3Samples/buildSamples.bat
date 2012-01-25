copy ..\bin\Microsoft.Automata.dll .
copy ..\bin\Microsoft.Automata.Z3.dll .
copy ..\bin\Microsoft.Z3.dll .
csc /target:exe /platform:x86 /out:run.exe Program.cs /r:Microsoft.Automata.dll /r:Microsoft.Automata.Z3.dll /r:Microsoft.Z3.dll