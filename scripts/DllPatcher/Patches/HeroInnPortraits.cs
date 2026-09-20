using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchHeroInnPortraits(TypeDefinition heroInnViewType)
    {
        // PATCH InitHeroPortraits to fix array bounds issue
        // The original code crashes when heroSelectedIndices.Length < HeroInnPortraits.Length
        // Strategy: Find the loop's back-edge (where it checks i < HeroInnPortraits.Length)
        // and add an additional check: i < heroSelectedIndices.Length
        // ================================================================
        Console.WriteLine("\n=== Patching HeroInnView.InitHeroPortraits ===");
        var initHeroPortraits = heroInnViewType.Methods.FirstOrDefault(m => m.Name == "InitHeroPortraits");
        if (initHeroPortraits != null)
        {
            Console.WriteLine($"Found InitHeroPortraits with {initHeroPortraits.Body.Instructions.Count} instructions");
            var initIL = initHeroPortraits.Body.GetILProcessor();
            var initInstrs = initHeroPortraits.Body.Instructions;

            // Print original IL for debugging
            Console.WriteLine("Original IL (first 60 instructions):");
            for (int i = 0; i < Math.Min(initInstrs.Count, 60); i++)
            {
                Console.WriteLine($"  IL_{i:D4}: {initInstrs[i]}");
            }

            // Find the heroSelectedIndices variable (int[] type, should be V_0)
            VariableDefinition heroIndicesVar = null;
            foreach (var v in initHeroPortraits.Body.Variables)
            {
                if (v.VariableType.IsArray && v.VariableType.GetElementType().FullName == "System.Int32")
                {
                    heroIndicesVar = v;
                    Console.WriteLine($"  Found heroSelectedIndices: {v}");
                    break;
                }
            }

            // The loop condition usually looks like:
            // ldloc.X (loop counter)
            // ldarg.0
            // ldfld NGame2.NUI.NWindow.HeroInnView::HeroInnPortraits
            // ldlen
            // conv.i4
            // blt.s IL_xxxx (jump to loop start)
            // We want to insert BEFORE this sequence:
            //   ldloc.s V_3 (i)
            //   ldloc.0 (heroSelectedIndices)
            //   ldlen
            //   conv.i4
            //   bge <after loop>  // if i >= heroSelectedIndices.Length, exit loop

            // Actually, simpler: find where the loop ENDS and add our own check there
            // Look for pattern: ldloc (i), ldarg.0, ldfld HeroInnPortraits, ldlen, conv.i4, blt/clt

            bool patchedInitHP = false;
            for (int i = 0; i < initInstrs.Count - 5; i++)
            {
                var instr = initInstrs[i];

                // Look for: ldarg.0, ldfld HeroInnPortraits, ldlen, conv.i4, blt.s <target>
                if (instr.OpCode == OpCodes.Ldarg_0 &&
                    i + 4 < initInstrs.Count)
                {
                    var next1 = initInstrs[i + 1]; // ldfld HeroInnPortraits
                    var next2 = initInstrs[i + 2]; // ldlen
                    var next3 = initInstrs[i + 3]; // conv.i4
                    var next4 = initInstrs[i + 4]; // blt.s or clt

                    if (next1.OpCode == OpCodes.Ldfld &&
                        next1.Operand is FieldReference fr && fr.Name == "HeroInnPortraits" &&
                        next2.OpCode == OpCodes.Ldlen &&
                        next3.OpCode == OpCodes.Conv_I4)
                    {
                        Console.WriteLine($"  Found loop condition at IL_{i:D4}");
                        Console.WriteLine($"    {next1}");
                        Console.WriteLine($"    {next2}");
                        Console.WriteLine($"    {next3}");
                        Console.WriteLine($"    {next4}");

                        // The branch target of next4 (blt.s) is the loop body
                        // We need to add our check right before next4

                        if ((next4.OpCode == OpCodes.Blt_S || next4.OpCode == OpCodes.Blt) &&
                            heroIndicesVar != null)
                        {
                            var loopBodyTarget = (Instruction)next4.Operand;

                            // After the original blt.s, we need to also check heroSelectedIndices.Length
                            // Actually, easier approach: change the loop to use the minimum length

                            // Since we can't easily call Math.Min, let's insert a second check
                            // AFTER the blt.s branches to loop body:
                            // At the loop body start, add:
                            //   ldloc.s V_3 (i)
                            //   ldloc.0 (heroSelectedIndices)
                            //   ldlen
                            //   conv.i4
                            //   bge <after loop>

                            // Find the instruction AFTER the loop (where blt.s falls through)
                            // This should be right after next4
                            var afterLoopInstr = initInstrs[i + 5];
                            Console.WriteLine($"  After loop: {afterLoopInstr}");

                            // Find the loop variable - look for what's loaded before ldarg.0
                            // It should be: ldloc.s V_3, ldarg.0, ldfld...
                            VariableDefinition loopVarDef = null;
                            if (i > 0)
                            {
                                var prevInstr = initInstrs[i - 1];
                                if (prevInstr.OpCode == OpCodes.Ldloc_S || prevInstr.OpCode == OpCodes.Ldloc)
                                {
                                    loopVarDef = (VariableDefinition)prevInstr.Operand;
                                    Console.WriteLine($"  Loop variable: V_{loopVarDef.Index}");
                                }
                                else if (prevInstr.OpCode == OpCodes.Ldloc_0)
                                {
                                    loopVarDef = initHeroPortraits.Body.Variables[0];
                                    Console.WriteLine($"  Loop variable: V_0");
                                }
                                else if (prevInstr.OpCode == OpCodes.Ldloc_1)
                                {
                                    loopVarDef = initHeroPortraits.Body.Variables[1];
                                    Console.WriteLine($"  Loop variable: V_1");
                                }
                                else if (prevInstr.OpCode == OpCodes.Ldloc_2)
                                {
                                    loopVarDef = initHeroPortraits.Body.Variables[2];
                                    Console.WriteLine($"  Loop variable: V_2");
                                }
                                else if (prevInstr.OpCode == OpCodes.Ldloc_3)
                                {
                                    loopVarDef = initHeroPortraits.Body.Variables[3];
                                    Console.WriteLine($"  Loop variable: V_3");
                                }
                            }

                            if (loopVarDef != null)
                            {
                                // Insert bounds check at the START of the loop body
                                // (right at loopBodyTarget)

                                // Create new instructions for the bounds check
                                // ldloc.s loopVar
                                // ldloc heroIndicesVar
                                // ldlen
                                // conv.i4
                                // bge afterLoopInstr

                                var loadLoopVar = loopVarDef.Index switch {
                                    0 => initIL.Create(OpCodes.Ldloc_0),
                                    1 => initIL.Create(OpCodes.Ldloc_1),
                                    2 => initIL.Create(OpCodes.Ldloc_2),
                                    3 => initIL.Create(OpCodes.Ldloc_3),
                                    _ => initIL.Create(OpCodes.Ldloc_S, loopVarDef)
                                };

                                var loadHeroIndices = heroIndicesVar.Index switch {
                                    0 => initIL.Create(OpCodes.Ldloc_0),
                                    1 => initIL.Create(OpCodes.Ldloc_1),
                                    2 => initIL.Create(OpCodes.Ldloc_2),
                                    3 => initIL.Create(OpCodes.Ldloc_3),
                                    _ => initIL.Create(OpCodes.Ldloc_S, heroIndicesVar)
                                };

                                var ldlenInstr = initIL.Create(OpCodes.Ldlen);
                                var convI4Instr = initIL.Create(OpCodes.Conv_I4);
                                var bgeInstr = initIL.Create(OpCodes.Bge, afterLoopInstr);

                                // Insert BEFORE loopBodyTarget
                                initIL.InsertBefore(loopBodyTarget, loadLoopVar);
                                initIL.InsertBefore(loopBodyTarget, loadHeroIndices);
                                initIL.InsertBefore(loopBodyTarget, ldlenInstr);
                                initIL.InsertBefore(loopBodyTarget, convI4Instr);
                                initIL.InsertBefore(loopBodyTarget, bgeInstr);

                                // Update the blt.s to point to our new check
                                next4.Operand = loadLoopVar;

                                patchedInitHP = true;
                                Console.WriteLine("  Successfully patched InitHeroPortraits with bounds check!");
                            }
                        }

                        break;
                    }
                }
            }

            if (!patchedInitHP)
            {
                Console.WriteLine("  WARNING: Could not patch InitHeroPortraits bounds check");
            }
        }
        else
        {
            Console.WriteLine("WARNING: InitHeroPortraits method not found");
        }
    }
}
