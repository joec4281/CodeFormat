# CodeFormat
Take Command Console (TCC) has the LIBRARY command,
which provides the ability to   
* create
* modify
* delete
* or display

library functions for use with TCC.

Ref: https://jpsoft.com/help/library.htm?q=library

The **LIBRARY /F** option displays the loaded (matching) functions (the function name can contain wildcards).  
  
For example;

```dos
R:\>library /f Utils.Library$c#run
utils.library$c#run {
@setlocal
@echo off
if isdir r:\temp set temp=r:\temp
set _libraryfunction=%0
set csc="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
iff %# eq 0 then
echo USAGE: %_libraryfunction test.cs
quit
endiff
set theEXE=%temp\%@name[%1].exe
set theHASH=%theEXE.hash
iff defined rebuild then
iff exist %theEXE then
del /q %theEXE
endiff
iff exist %theHASH then
del /q %theHASH
endiff
endiff
iff not exist %theHASH then
Gosub CompileEXE
iff exist %theEXE then
Gosub CreateHash
Gosub RunEXE
endiff
else
Gosub ReadHash
Gosub CompareHash
iff "%CompareHash" eq "Y" then
Gosub RunEXE
else
Gosub CompileEXE
iff exist %theEXE then
Gosub CreateHash
Gosub RunEXE
endiff
endiff
endiff
if isalias csc unalias csc
endlocal
quit
:CreateHash
echo %@md5[%1] >! %theHASH
set HashValue=%@md5[%1]
Return
:ReadHash
set HashValue=%@line[%theHASH,0]
Return
:CompareHash
debugstring Hash File: %HashValue
debugstring Source Hash: %@md5[%1]
iff "%HashValue" eq "%@md5[%1]" then
set CompareHash=Y
else
set CompareHash=N
endiff
debugstring CompareHash: %CompareHash
Return
:RunEXE
iff exist %theEXE then
%theEXE %2$
endiff
Return
:CompileEXE
rem You can remove VBCSCompiler.exe from memory by doing:
rem taskkill.exe /im VBCSCompiler.exe /f
if %@pid[VBCSCompiler.exe] eq 0 echo /shared /keepalive:600 not active.
%csc -nologo /shared /keepalive:600 %1 -out:%theEXE
iff %? ne 0 then
echo Error compiling %theEXE
quit
endiff
if %@pid[VBCSCompiler.exe] eq 0 echo /shared /keepalive:600 not active.
Return
}
```

...but I would prefer that the library code output be formatted.
  
So, I have developed a C# .NET Framework program,  
**CodeFormat.exe**,   
which accepts the output from the **LIBRARY /F** command via a pipe,   
and outputs the library code formatted.  
  
```dos
R:\>library /f Utils.Library$c#run | CodeFormat.exe
utils.library$c#run {
@setlocal
@echo off
if isdir r:\temp set temp=r:\temp
set _libraryfunction=%0
set csc="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"

iff %# eq 0 then
    echo USAGE: %_libraryfunction test.cs
    quit
endiff

set theEXE=%temp\%@name[%1].exe
set theHASH=%theEXE.hash

iff defined rebuild then

    iff exist %theEXE then
        del /q %theEXE
    endiff

    iff exist %theHASH then
        del /q %theHASH
    endiff

endiff

iff not exist %theHASH then
    Gosub CompileEXE

    iff exist %theEXE then
        Gosub CreateHash
        Gosub RunEXE
    endiff

else
    Gosub ReadHash
    Gosub CompareHash

    iff "%CompareHash" eq "Y" then
        Gosub RunEXE
    else
        Gosub CompileEXE

        iff exist %theEXE then
            Gosub CreateHash
            Gosub RunEXE
        endiff

    endiff

endiff

if isalias csc unalias csc
endlocal
quit

:CreateHash
echo %@md5[%1] >! %theHASH
set HashValue=%@md5[%1]
Return

:ReadHash
set HashValue=%@line[%theHASH,0]
Return

:CompareHash
debugstring Hash File: %HashValue
debugstring Source Hash: %@md5[%1]

iff "%HashValue" eq "%@md5[%1]" then
    set CompareHash=Y
else
    set CompareHash=N
endiff

debugstring CompareHash: %CompareHash
Return

:RunEXE

iff exist %theEXE then
    %theEXE %2$
endiff

Return

:CompileEXE
rem You can remove VBCSCompiler.exe from memory by doing:
rem taskkill.exe /im VBCSCompiler.exe /f
if %@pid[VBCSCompiler.exe] eq 0 echo /shared /keepalive:600 not active.
%csc -nologo /shared /keepalive:600 %1 -out:%theEXE

iff %? ne 0 then
    echo Error compiling %theEXE
    quit
endiff

if %@pid[VBCSCompiler.exe] eq 0 echo /shared /keepalive:600 not active.
Return

}
```
