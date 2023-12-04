// Type: Program 
// Assembly: NCCompiler_CompilersCourse, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D51FDEFC-63DB-440D-8773-7CE62B5854F8
// Location: C:\Users\almet\RiderProjects\NCCompiler_CompilersCourse\bin\Debug\net7.0\NCCompiler_CompilersCourse.dll
// Sequence point data and variable names from c:\users\almet\riderprojects\nccompiler_compilerscourse\bin\debug\net7.0\nccompiler_compilerscourse.pdb

.class private auto ansi beforefieldinit
  Program
    extends [System.Runtime]System.Object
{
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
    = (01 00 00 00 )

  .method private hidebysig static void
    '<Main>$'(
      string[] args
    ) cil managed
  {
    .entrypoint
    .maxstack 8

    IL_0000: nop
    IL_0001: ret

  } // end of method Program::'<Main>$'

  .method public hidebysig specialname rtspecialname instance void
    .ctor() cil managed
  {
    .maxstack 8

    IL_0000: ldarg.0      // this
    IL_0001: call         instance void [System.Runtime]System.Object::.ctor()
    IL_0006: nop
    IL_0007: ret

  } // end of method Program::.ctor

  .method assembly hidebysig static void
    '<<Main>$>g__Mai|0_0'() cil managed
  {
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
      = (01 00 00 00 )
    .maxstack 2
    .locals init (
      [0] int32 i,
      [1] bool V_1
    )

    // [2 1 - 2 2]
    IL_0000: nop

    // [3 5 - 3 15]
    IL_0001: ldc.i4.0
    IL_0002: stloc.0      // i

    IL_0003: br.s         IL_000b
    // start of loop, entry point: IL_000b

      // [5 5 - 5 6]
      IL_0005: nop

      // [6 9 - 6 16]
      IL_0006: ldloc.0      // i
      IL_0007: ldc.i4.1
      IL_0008: add
      IL_0009: stloc.0      // i

      // [7 5 - 7 6]
      IL_000a: nop

      // [4 5 - 4 18]
      IL_000b: ldloc.0      // i
      IL_000c: ldc.i4.5
      IL_000d: clt
      IL_000f: stloc.1      // V_1

      IL_0010: ldloc.1      // V_1
      IL_0011: brtrue.s     IL_0005
    // end of loop

    // [8 1 - 8 2]
    IL_0013: ret

  } // end of method Program::'<<Main>$>g__Mai|0_0'
} // end of class Program
