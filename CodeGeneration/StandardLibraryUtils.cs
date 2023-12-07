﻿namespace NCCompiler_CompilersCourse.CodeGeneration;

public class StandardLibraryUtils
{
    public static string getPrintFunction()
    {
        return ".method public hidebysig static void\n    Print(\n      object[] p\n    ) cil managed\n  {\n    .param [1]\n      .custom instance void [System.Runtime]System.ParamArrayAttribute::.ctor()\n        = (01 00 00 00 )\n    .maxstack 2\n    .locals init (\n      [0] object[] V_0,\n      [1] int32 V_1,\n      [2] object param\n    )\n\n    // [16 5 - 16 6]\n    IL_0000: nop\n\n    // [17 9 - 17 16]\n    IL_0001: nop\n\n    // [17 31 - 17 32]\n    IL_0002: ldarg.0      // p\n    IL_0003: stloc.0      // V_0\n    IL_0004: ldc.i4.0\n    IL_0005: stloc.1      // V_1\n\n    IL_0006: br.s         IL_001f\n    // start of loop, entry point: IL_001f\n\n      // [17 18 - 17 27]\n      IL_0008: ldloc.0      // V_0\n      IL_0009: ldloc.1      // V_1\n      IL_000a: ldelem.ref\n      IL_000b: stloc.2      // param\n\n      // [18 9 - 18 10]\n      IL_000c: nop\n\n      // [19 13 - 19 26]\n      IL_000d: ldloc.2      // param\n      IL_000e: call         void Program::Print(object)\n      IL_0013: nop\n\n      // [20 13 - 20 33]\n      IL_0014: call         void [System.Console]System.Console::WriteLine()\n      IL_0019: nop\n\n      // [21 9 - 21 10]\n      IL_001a: nop\n\n      IL_001b: ldloc.1      // V_1\n      IL_001c: ldc.i4.1\n      IL_001d: add\n      IL_001e: stloc.1      // V_1\n\n      // [17 28 - 17 30]\n      IL_001f: ldloc.1      // V_1\n      IL_0020: ldloc.0      // V_0\n      IL_0021: ldlen\n      IL_0022: conv.i4\n      IL_0023: blt.s        IL_0008\n    // end of loop\n\n    // [22 5 - 22 6]\n    IL_0025: ret\n\n  } // end of method Program::Print\n\n  .method public hidebysig static void\n    Print(\n      object o\n    ) cil managed\n  {\n  .maxstack 3\n    .locals init (\n      [0] bool V_0,\n      [1] bool V_1,\n      [2] bool V_2,\n      [3] class [System.Runtime]System.Reflection.FieldInfo[] fields,\n      [4] int32 i,\n      [5] class [System.Runtime]System.Reflection.FieldInfo fieldInfo,\n      [6] bool V_6,\n      [7] bool V_7,\n      [8] bool V_8,\n      [9] class [System.Runtime]System.Type elementType,\n      [10] class [System.Runtime]System.Array list,\n      [11] int32 i_V_11,\n      [12] bool V_12,\n      [13] bool V_13\n    )\n\n    // [25 5 - 25 6]\n    IL_0000: nop\n\n    // [26 9 - 26 40]\n    IL_0001: ldarg.0      // o\n    IL_0002: isinst       [System.Runtime]System.Int32\n    IL_0007: brtrue.s     IL_001b\n    IL_0009: ldarg.0      // o\n    IL_000a: isinst       [System.Runtime]System.Double\n    IL_000f: brtrue.s     IL_001b\n    IL_0011: ldarg.0      // o\n    IL_0012: isinst       [System.Runtime]System.Boolean\n    IL_0017: brtrue.s     IL_001b\n    IL_0019: br.s         IL_001f\n    IL_001b: ldc.i4.1\n    IL_001c: stloc.0      // V_0\n    IL_001d: br.s         IL_0021\n    IL_001f: ldc.i4.0\n    IL_0020: stloc.0      // V_0\n    IL_0021: ldloc.0      // V_0\n    IL_0022: stloc.1      // V_1\n\n    IL_0023: ldloc.1      // V_1\n    IL_0024: brfalse.s    IL_0034\n\n    // [27 9 - 27 10]\n    IL_0026: nop\n\n    // [28 13 - 28 30]\n    IL_0027: ldarg.0      // o\n    IL_0028: call         void [System.Console]System.Console::Write(object)\n    IL_002d: nop\n\n    // [29 9 - 29 10]\n    IL_002e: nop\n\n    IL_002f: br           IL_0175\n\n    // [29 16 - 29 55]\n    IL_0034: ldarg.0      // o\n    IL_0035: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()\n    IL_003a: callvirt     instance class [System.Runtime]System.Reflection.FieldInfo[] [System.Runtime]System.Type::GetFields()\n    IL_003f: ldlen\n    IL_0040: ldc.i4.0\n    IL_0041: cgt.un\n    IL_0043: stloc.2      // V_2\n\n    IL_0044: ldloc.2      // V_2\n    IL_0045: brfalse      IL_00e7\n\n    // [30 9 - 30 10]\n    IL_004a: nop\n\n    // [32 13 - 32 59]\n    IL_004b: ldarg.0      // o\n    IL_004c: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()\n    IL_0051: callvirt     instance string [System.Runtime]System.Object::ToString()\n    IL_0056: ldstr        \" { \"\n    IL_005b: call         string [System.Runtime]System.String::Concat(string, string)\n    IL_0060: call         void [System.Console]System.Console::Write(string)\n    IL_0065: nop\n\n    // [33 13 - 33 50]\n    IL_0066: ldarg.0      // o\n    IL_0067: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()\n    IL_006c: callvirt     instance class [System.Runtime]System.Reflection.FieldInfo[] [System.Runtime]System.Type::GetFields()\n    IL_0071: stloc.3      // fields\n\n    // [34 18 - 34 27]\n    IL_0072: ldc.i4.0\n    IL_0073: stloc.s      i\n\n    IL_0075: br.s         IL_00c9\n    // start of loop, entry point: IL_00c9\n\n      // [35 13 - 35 14]\n      IL_0077: nop\n\n      // [36 17 - 36 43]\n      IL_0078: ldloc.3      // fields\n      IL_0079: ldloc.s      i\n      IL_007b: ldelem.ref\n      IL_007c: stloc.s      fieldInfo\n\n      // [37 17 - 37 55]\n      IL_007e: ldloc.s      fieldInfo\n      IL_0080: callvirt     instance string [System.Runtime]System.Reflection.MemberInfo::get_Name()\n      IL_0085: ldstr        \" = \"\n      IL_008a: call         string [System.Runtime]System.String::Concat(string, string)\n      IL_008f: call         void [System.Console]System.Console::Write(string)\n      IL_0094: nop\n\n      // [38 17 - 38 46]\n      IL_0095: ldloc.s      fieldInfo\n      IL_0097: ldarg.0      // o\n      IL_0098: callvirt     instance object [System.Runtime]System.Reflection.FieldInfo::GetValue(object)\n      IL_009d: call         void Program::Print(object)\n      IL_00a2: nop\n\n      // [39 17 - 39 44]\n      IL_00a3: ldloc.s      i\n      IL_00a5: ldloc.3      // fields\n      IL_00a6: ldlen\n      IL_00a7: conv.i4\n      IL_00a8: ldc.i4.1\n      IL_00a9: sub\n      IL_00aa: ceq\n      IL_00ac: ldc.i4.0\n      IL_00ad: ceq\n      IL_00af: stloc.s      V_6\n\n      IL_00b1: ldloc.s      V_6\n      IL_00b3: brfalse.s    IL_00c2\n\n      // [40 17 - 40 18]\n      IL_00b5: nop\n\n      // [41 21 - 41 41]\n      IL_00b6: ldstr        \", \"\n      IL_00bb: call         void [System.Console]System.Console::Write(string)\n      IL_00c0: nop\n\n      // [42 17 - 42 18]\n      IL_00c1: nop\n\n      // [43 13 - 43 14]\n      IL_00c2: nop\n\n      // [34 48 - 34 51]\n      IL_00c3: ldloc.s      i\n      IL_00c5: ldc.i4.1\n      IL_00c6: add\n      IL_00c7: stloc.s      i\n\n      // [34 29 - 34 46]\n      IL_00c9: ldloc.s      i\n      IL_00cb: ldloc.3      // fields\n      IL_00cc: ldlen\n      IL_00cd: conv.i4\n      IL_00ce: clt\n      IL_00d0: stloc.s      V_7\n\n      IL_00d2: ldloc.s      V_7\n      IL_00d4: brtrue.s     IL_0077\n    // end of loop\n\n    // [44 13 - 44 33]\n    IL_00d6: ldstr        \" }\"\n    IL_00db: call         void [System.Console]System.Console::Write(string)\n    IL_00e0: nop\n\n    // [45 9 - 45 10]\n    IL_00e1: nop\n\n    IL_00e2: br           IL_0175\n\n    // [46 14 - 46 38]\n    IL_00e7: ldarg.0      // o\n    IL_00e8: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()\n    IL_00ed: callvirt     instance bool [System.Runtime]System.Type::get_IsArray()\n    IL_00f2: stloc.s      V_8\n\n    IL_00f4: ldloc.s      V_8\n    IL_00f6: brfalse.s    IL_0175\n\n    // [47 9 - 47 10]\n    IL_00f8: nop\n\n    // [48 13 - 48 60]\n    IL_00f9: ldarg.0      // o\n    IL_00fa: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()\n    IL_00ff: callvirt     instance class [System.Runtime]System.Type [System.Runtime]System.Type::GetElementType()\n    IL_0104: stloc.s      elementType\n\n    // [49 13 - 49 33]\n    IL_0106: ldarg.0      // o\n    IL_0107: castclass    [System.Runtime]System.Array\n    IL_010c: stloc.s      list\n\n    // [50 13 - 50 33]\n    IL_010e: ldstr        \"[ \"\n    IL_0113: call         void [System.Console]System.Console::Write(string)\n    IL_0118: nop\n\n    // [52 18 - 52 27]\n    IL_0119: ldc.i4.0\n    IL_011a: stloc.s      i_V_11\n\n    IL_011c: br.s         IL_0158\n    // start of loop, entry point: IL_0158\n\n      // [53 13 - 53 14]\n      IL_011e: nop\n\n      // [54 17 - 54 41]\n      IL_011f: ldloc.s      list\n      IL_0121: ldloc.s      i_V_11\n      IL_0123: callvirt     instance object [System.Runtime]System.Array::GetValue(int32)\n      IL_0128: call         void Program::Print(object)\n      IL_012d: nop\n\n      // [55 17 - 55 42]\n      IL_012e: ldloc.s      i_V_11\n      IL_0130: ldloc.s      list\n      IL_0132: callvirt     instance int32 [System.Runtime]System.Array::get_Length()\n      IL_0137: ldc.i4.1\n      IL_0138: sub\n      IL_0139: ceq\n      IL_013b: ldc.i4.0\n      IL_013c: ceq\n      IL_013e: stloc.s      V_12\n\n      IL_0140: ldloc.s      V_12\n      IL_0142: brfalse.s    IL_0151\n\n      // [56 17 - 56 18]\n      IL_0144: nop\n\n      // [57 21 - 57 41]\n      IL_0145: ldstr        \", \"\n      IL_014a: call         void [System.Console]System.Console::Write(string)\n      IL_014f: nop\n\n      // [58 17 - 58 18]\n      IL_0150: nop\n\n      // [59 13 - 59 14]\n      IL_0151: nop\n\n      // [52 46 - 52 49]\n      IL_0152: ldloc.s      i_V_11\n      IL_0154: ldc.i4.1\n      IL_0155: add\n      IL_0156: stloc.s      i_V_11\n\n      // [52 29 - 52 44]\n      IL_0158: ldloc.s      i_V_11\n      IL_015a: ldloc.s      list\n      IL_015c: callvirt     instance int32 [System.Runtime]System.Array::get_Length()\n      IL_0161: clt\n      IL_0163: stloc.s      V_13\n\n      IL_0165: ldloc.s      V_13\n      IL_0167: brtrue.s     IL_011e\n    // end of loop\n\n    // [61 13 - 61 33]\n    IL_0169: ldstr        \" ]\"\n    IL_016e: call         void [System.Console]System.Console::Write(string)\n    IL_0173: nop\n\n    // [62 9 - 62 10]\n    IL_0174: nop\n\n    // [63 5 - 63 6]\n    IL_0175: ret\n\n  } // end of method Program::Print\n";
    }
}