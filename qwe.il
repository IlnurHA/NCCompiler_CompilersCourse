// Type: P 
// Assembly: NCCompiler_CompilersCourse, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F258731D-9963-4A00-AE08-8E1526129929
// Location: C:\Users\almet\RiderProjects\NCCompiler_CompilersCourse\bin\Debug\net7.0\NCCompiler_CompilersCourse.dll
// Sequence point data and variable names from C:\Users\almet\RiderProjects\NCCompiler_CompilersCourse\bin\Debug\net7.0\NCCompiler_CompilersCourse.pdb

.class private auto ansi beforefieldinit
  P
    extends [System.Runtime]System.Object
{

  .class nested private sealed sequential ansi beforefieldinit
    S
      extends [System.Runtime]System.ValueType
  {

    .field public int32 a

    .field public int32 b
  } // end of class S

  .class nested private sealed sequential ansi beforefieldinit
    T
      extends [System.Runtime]System.ValueType
  {

    .field public valuetype P/S e

    .field public bool b
  } // end of class T

  .method public hidebysig static void
    Print(
      object[] p
    ) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(unsigned int8)
      = (01 00 01 00 00 ) // .....
      // unsigned int8(1) // 0x01
    .param [1]
      .custom instance void [System.Runtime]System.ParamArrayAttribute::.ctor()
        = (01 00 00 00 )
    .maxstack 2
    .locals init (
      [0] object[] V_0,
      [1] int32 V_1,
      [2] object param
    )

    // [16 5 - 16 6]
    IL_0000: nop

    // [17 9 - 17 16]
    IL_0001: nop

    // [17 31 - 17 32]
    IL_0002: ldarg.0      // p
    IL_0003: stloc.0      // V_0
    IL_0004: ldc.i4.0
    IL_0005: stloc.1      // V_1

    IL_0006: br.s         IL_001f
    // start of loop, entry point: IL_001f

      // [17 18 - 17 27]
      IL_0008: ldloc.0      // V_0
      IL_0009: ldloc.1      // V_1
      IL_000a: ldelem.ref
      IL_000b: stloc.2      // param

      // [18 9 - 18 10]
      IL_000c: nop

      // [19 13 - 19 26]
      IL_000d: ldloc.2      // param
      IL_000e: call         void P::Print(object)
      IL_0013: nop

      // [20 13 - 20 33]
      IL_0014: call         void [System.Console]System.Console::WriteLine()
      IL_0019: nop

      // [21 9 - 21 10]
      IL_001a: nop

      IL_001b: ldloc.1      // V_1
      IL_001c: ldc.i4.1
      IL_001d: add
      IL_001e: stloc.1      // V_1

      // [17 28 - 17 30]
      IL_001f: ldloc.1      // V_1
      IL_0020: ldloc.0      // V_0
      IL_0021: ldlen
      IL_0022: conv.i4
      IL_0023: blt.s        IL_0008
    // end of loop

    // [22 5 - 22 6]
    IL_0025: ret

  } // end of method P::Print

  .method public hidebysig static void
    Print(
      object o
    ) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(unsigned int8)
      = (01 00 01 00 00 ) // .....
      // unsigned int8(1) // 0x01
    .maxstack 3
    .locals init (
      [0] bool V_0,
      [1] bool V_1,
      [2] bool V_2,
      [3] class [System.Runtime]System.Reflection.FieldInfo[] fields,
      [4] int32 i,
      [5] class [System.Runtime]System.Reflection.FieldInfo fieldInfo,
      [6] bool V_6,
      [7] bool V_7,
      [8] bool V_8,
      [9] class [System.Runtime]System.Type elementType,
      [10] class [System.Runtime]System.Array list,
      [11] int32 i_V_11,
      [12] bool V_12,
      [13] bool V_13
    )

    // [25 5 - 25 6]
    IL_0000: nop

    // [26 9 - 26 40]
    IL_0001: ldarg.0      // o
    IL_0002: isinst       [System.Runtime]System.Int32
    IL_0007: brtrue.s     IL_001b
    IL_0009: ldarg.0      // o
    IL_000a: isinst       [System.Runtime]System.Float32
    IL_000f: brtrue.s     IL_001b
    IL_0011: ldarg.0      // o
    IL_0012: isinst       [System.Runtime]System.Boolean
    IL_0017: brtrue.s     IL_001b
    IL_0019: br.s         IL_001f
    IL_001b: ldc.i4.1
    IL_001c: stloc.0      // V_0
    IL_001d: br.s         IL_0021
    IL_001f: ldc.i4.0
    IL_0020: stloc.0      // V_0
    IL_0021: ldloc.0      // V_0
    IL_0022: stloc.1      // V_1

    IL_0023: ldloc.1      // V_1
    IL_0024: brfalse.s    IL_0034

    // [27 9 - 27 10]
    IL_0026: nop

    // [28 13 - 28 30]
    IL_0027: ldarg.0      // o
    IL_0028: call         void [System.Console]System.Console::Write(object)
    IL_002d: nop

    // [29 9 - 29 10]
    IL_002e: nop

    IL_002f: br           IL_018c

    // [29 16 - 29 55]
    IL_0034: ldarg.0      // o
    IL_0035: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()
    IL_003a: callvirt     instance class [System.Runtime]System.Reflection.FieldInfo[] [System.Runtime]System.Type::GetFields()
    IL_003f: ldlen
    IL_0040: ldc.i4.0
    IL_0041: cgt.un
    IL_0043: stloc.2      // V_2

    IL_0044: ldloc.2      // V_2
    IL_0045: brfalse      IL_00e7

    // [30 9 - 30 10]
    IL_004a: nop

    // [32 13 - 32 59]
    IL_004b: ldarg.0      // o
    IL_004c: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()
    IL_0051: callvirt     instance string [System.Runtime]System.Object::ToString()
    IL_0056: ldstr        " { "
    IL_005b: call         string [System.Runtime]System.String::Concat(string, string)
    IL_0060: call         void [System.Console]System.Console::Write(string)
    IL_0065: nop

    // [33 13 - 33 50]
    IL_0066: ldarg.0      // o
    IL_0067: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()
    IL_006c: callvirt     instance class [System.Runtime]System.Reflection.FieldInfo[] [System.Runtime]System.Type::GetFields()
    IL_0071: stloc.3      // fields

    // [34 18 - 34 27]
    IL_0072: ldc.i4.0
    IL_0073: stloc.s      i

    IL_0075: br.s         IL_00c9
    // start of loop, entry point: IL_00c9

      // [35 13 - 35 14]
      IL_0077: nop

      // [36 17 - 36 43]
      IL_0078: ldloc.3      // fields
      IL_0079: ldloc.s      i
      IL_007b: ldelem.ref
      IL_007c: stloc.s      fieldInfo

      // [37 17 - 37 55]
      IL_007e: ldloc.s      fieldInfo
      IL_0080: callvirt     instance string [System.Runtime]System.Reflection.MemberInfo::get_Name()
      IL_0085: ldstr        " = "
      IL_008a: call         string [System.Runtime]System.String::Concat(string, string)
      IL_008f: call         void [System.Console]System.Console::Write(string)
      IL_0094: nop

      // [38 17 - 38 46]
      IL_0095: ldloc.s      fieldInfo
      IL_0097: ldarg.0      // o
      IL_0098: callvirt     instance object [System.Runtime]System.Reflection.FieldInfo::GetValue(object)
      IL_009d: call         void P::Print(object)
      IL_00a2: nop

      // [39 17 - 39 44]
      IL_00a3: ldloc.s      i
      IL_00a5: ldloc.3      // fields
      IL_00a6: ldlen
      IL_00a7: conv.i4
      IL_00a8: ldc.i4.1
      IL_00a9: sub
      IL_00aa: ceq
      IL_00ac: ldc.i4.0
      IL_00ad: ceq
      IL_00af: stloc.s      V_6

      IL_00b1: ldloc.s      V_6
      IL_00b3: brfalse.s    IL_00c2

      // [40 17 - 40 18]
      IL_00b5: nop

      // [41 21 - 41 41]
      IL_00b6: ldstr        ", "
      IL_00bb: call         void [System.Console]System.Console::Write(string)
      IL_00c0: nop

      // [42 17 - 42 18]
      IL_00c1: nop

      // [43 13 - 43 14]
      IL_00c2: nop

      // [34 48 - 34 51]
      IL_00c3: ldloc.s      i
      IL_00c5: ldc.i4.1
      IL_00c6: add
      IL_00c7: stloc.s      i

      // [34 29 - 34 46]
      IL_00c9: ldloc.s      i
      IL_00cb: ldloc.3      // fields
      IL_00cc: ldlen
      IL_00cd: conv.i4
      IL_00ce: clt
      IL_00d0: stloc.s      V_7

      IL_00d2: ldloc.s      V_7
      IL_00d4: brtrue.s     IL_0077
    // end of loop

    // [44 13 - 44 33]
    IL_00d6: ldstr        " }"
    IL_00db: call         void [System.Console]System.Console::Write(string)
    IL_00e0: nop

    // [45 9 - 45 10]
    IL_00e1: nop

    IL_00e2: br           IL_018c

    // [46 14 - 46 38]
    IL_00e7: ldarg.0      // o
    IL_00e8: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()
    IL_00ed: callvirt     instance bool [System.Runtime]System.Type::get_IsArray()
    IL_00f2: stloc.s      V_8

    IL_00f4: ldloc.s      V_8
    IL_00f6: brfalse.s    IL_0177

    // [47 9 - 47 10]
    IL_00f8: nop

    // [48 13 - 48 60]
    IL_00f9: ldarg.0      // o
    IL_00fa: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()
    IL_00ff: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Type::GetElementType()
    IL_0104: stloc.s      elementType

    // [49 13 - 49 33]
    IL_0106: ldarg.0      // o
    IL_0107: castclass    [System.Runtime]System.Array
    IL_010c: stloc.s      list

    // [50 13 - 50 33]
    IL_010e: ldstr        "[ "
    IL_0113: call         void [System.Console]System.Console::Write(string)
    IL_0118: nop

    // [52 18 - 52 27]
    IL_0119: ldc.i4.0
    IL_011a: stloc.s      i_V_11

    IL_011c: br.s         IL_0158
    // start of loop, entry point: IL_0158

      // [53 13 - 53 14]
      IL_011e: nop

      // [54 17 - 54 41]
      IL_011f: ldloc.s      list
      IL_0121: ldloc.s      i_V_11
      IL_0123: callvirt     instance object [System.Runtime]System.Array::GetValue(int32)
      IL_0128: call         void P::Print(object)
      IL_012d: nop

      // [55 17 - 55 42]
      IL_012e: ldloc.s      i_V_11
      IL_0130: ldloc.s      list
      IL_0132: callvirt     instance int32 [System.Runtime]System.Array::get_Length()
      IL_0137: ldc.i4.1
      IL_0138: sub
      IL_0139: ceq
      IL_013b: ldc.i4.0
      IL_013c: ceq
      IL_013e: stloc.s      V_12

      IL_0140: ldloc.s      V_12
      IL_0142: brfalse.s    IL_0151

      // [56 17 - 56 18]
      IL_0144: nop

      // [57 21 - 57 41]
      IL_0145: ldstr        ", "
      IL_014a: call         void [System.Console]System.Console::Write(string)
      IL_014f: nop

      // [58 17 - 58 18]
      IL_0150: nop

      // [59 13 - 59 14]
      IL_0151: nop

      // [52 46 - 52 49]
      IL_0152: ldloc.s      i_V_11
      IL_0154: ldc.i4.1
      IL_0155: add
      IL_0156: stloc.s      i_V_11

      // [52 29 - 52 44]
      IL_0158: ldloc.s      i_V_11
      IL_015a: ldloc.s      list
      IL_015c: callvirt     instance int32 [System.Runtime]System.Array::get_Length()
      IL_0161: clt
      IL_0163: stloc.s      V_13

      IL_0165: ldloc.s      V_13
      IL_0167: brtrue.s     IL_011e
    // end of loop

    // [61 13 - 61 33]
    IL_0169: ldstr        " ]"
    IL_016e: call         void [System.Console]System.Console::Write(string)
    IL_0173: nop

    // [62 9 - 62 10]
    IL_0174: nop

    IL_0175: br.s         IL_018c

    // [64 9 - 64 10]
    IL_0177: nop

    // [65 13 - 65 55]
    IL_0178: ldarg.0      // o
    IL_0179: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()
    IL_017e: callvirt     instance class [System.Runtime]System.Reflection.FieldInfo[] [System.Runtime]System.Type::GetFields()
    IL_0183: ldc.i4.0
    IL_0184: ldelem.ref
    IL_0185: call         void [System.Console]System.Console::Write(object)
    IL_018a: nop

    // [66 9 - 66 10]
    IL_018b: nop

    // [67 5 - 67 6]
    IL_018c: ret

  } // end of method P::Print

  .method public hidebysig static void
    Main() cil managed
  {
    .entrypoint
    .maxstack 2
    .locals init (
      [0] valuetype P/S s,
      [1] valuetype P/S V_1
    )

    // [70 5 - 70 6]
    IL_0000: nop

    // [71 9 - 71 40]
    IL_0001: ldloca.s     V_1
    IL_0003: initobj      P/S
    IL_0009: ldloca.s     V_1
    IL_000b: ldc.i4.1
    IL_000c: stfld        int32 P/S::a
    IL_0011: ldloca.s     V_1
    IL_0013: ldc.i4.2
    IL_0014: stfld        int32 P/S::b
    IL_0019: ldloc.1      // V_1
    IL_001a: stloc.0      // s

    // [72 9 - 72 20]
    IL_001b: ldc.r8       5.5
    IL_0024: box          [System.Runtime]System.Float32
    IL_0029: call         void P::Print(object)
    IL_002e: nop

    // [73 5 - 73 6]
    IL_002f: ret

  } // end of method P::Main

  .method public hidebysig specialname rtspecialname instance void
    .ctor() cil managed
  {
    .maxstack 8

    IL_0000: ldarg.0      // this
    IL_0001: call         instance void [System.Runtime]System.Object::.ctor()
    IL_0006: nop
    IL_0007: ret

  } // end of method P::.ctor
} // end of class P
