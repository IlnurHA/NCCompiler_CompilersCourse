# NCCompiler_CompilersCourse

This is compiler for toy language NCC.

```
routine main() is
  var a is [1, 2, 3]
  print(a)
end
```

## How to run

1. Clone repository

``` sh
git clone https://github.com/IlnurHA/NCCompiler_CompilersCourse.git
```

1.5 Installing ILASM

``` sh
cd NCCompiler_CompilersCourse
dotnet add package ILAsm --version 4.700.2
cd ..
```

2. Installing GPPG
   
Clonning GPPG repository: https://github.com/k-john-gough/gppg

``` sh
git clone https://github.com/k-john-gough/gppg.git
```

Building using `dotnet`: https://dotnet.microsoft.com/en-us/)https://dotnet.microsoft.com/en-us/

``` sh
cd gppg
dotnet build --output .\GPPG_build
cp .\GPPG_build\Gppg.exe ..\NCCompiler_CompilersCourse
```

3. Generating code from GPPG

Execute command inside root folder of NCCompiler project
``` sh
.\Gppg.exe /nolines /conflicts ./Parser/Parser.y > ./Parser/ParserGenerated.cs
```

4. Building Compiler and compiling program
Inside root folder of NCCompiler project
``` sh
dotnet build --output .\compiler_build
cd compiler_build
.\NCCompiler_CompilersCourse.exe
```

You will execute Compiler. Next you need to specify path to the program. Then it will generate `compiledProgram.il` in the current directory.

5. Transform to `exe` file using `ilasm`
``` sh
ilasm \exe .\compiledProgram.il
```
Then you can execute program using binary `compiledProgram.exe`

P.S.
You can usually find `ilasm` using the following path (in Windows):
> C:\Users\<user_name>\.nuget\packages\microsoft.netcore.ilasm\<dotnet_version>\runtimes\native\
or
> C:\Users\<user_name>\.nuget\packages\ilasm\4.700.2\bin\Win.x64 (or Win.x32)

So, the command above could be:
``` sh
C:\Users\<user_name>\.nuget\packages\ilasm\4.700.2\bin\Win.x64\ilasm.exe /exe .\compiledProgram.il
```
