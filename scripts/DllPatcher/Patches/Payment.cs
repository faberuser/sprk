using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    /// <summary>
    /// Continues login when Steam/payment initialization fails.
    /// </summary>
    static void PatchPaymentErrorPopup(ModuleDefinition module)
    {
        // The coRun method is compiled into a state machine class
        // Find the StateBase_InAppBilling type first
        var stateBaseType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NLogin.NState.StateBase_InAppBilling");
        if (stateBaseType == null)
        {
            Console.WriteLine("ERROR: StateBase_InAppBilling type not found!");
            return;
        }
        Console.WriteLine($"Found type: {stateBaseType.FullName}");

        // Find the nested state machine class (compiler-generated for the coroutine)
        // It's typically named something like "<coRun>d__X"
        TypeDefinition stateMachineType = null;
        foreach (var nestedType in stateBaseType.NestedTypes)
        {
            if (nestedType.Name.StartsWith("<coRun>"))
            {
                stateMachineType = nestedType;
                break;
            }
        }

        if (stateMachineType == null)
        {
            Console.WriteLine("ERROR: coRun state machine type not found!");
            Console.WriteLine("Available nested types:");
            foreach (var nt in stateBaseType.NestedTypes)
            {
                Console.WriteLine($"  - {nt.Name}");
            }
            return;
        }
        Console.WriteLine($"Found state machine: {stateMachineType.Name}");

        // Find the MoveNext method (this is where the actual coroutine code lives)
        var moveNextMethod = stateMachineType.Methods.FirstOrDefault(m => m.Name == "MoveNext");
        if (moveNextMethod == null)
        {
            Console.WriteLine("ERROR: MoveNext method not found!");
            return;
        }
        Console.WriteLine($"Found MoveNext method with {moveNextMethod.Body.Instructions.Count} instructions");

        // Find the EResultInit enum type
        var eResultInitType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.IAP.EResultInit");
        if (eResultInitType == null)
        {
            Console.WriteLine("WARNING: EResultInit type not found, trying to find it in nested namespaces...");
        }

        // Find the InAppBillingState enum (nested in StateBase_InAppBilling)
        var billingStateType = stateBaseType.NestedTypes.FirstOrDefault(t => t.Name == "InAppBillingState");
        if (billingStateType == null)
        {
            Console.WriteLine("WARNING: InAppBillingState enum not found!");
        }

        var il = moveNextMethod.Body.GetILProcessor();
        var instructions = moveNextMethod.Body.Instructions;

        // Strategy: Find the comparison with EResultInit.Success and the branch that leads to
        // the error popup code. We want to make the failure case jump to the success path instead.
        //
        // Looking for pattern:
        //   ... call get_TupleResultInit
        //   ... call get_Item1  (gets EResultInit value)
        //   ldc.i4.1  (EResultInit.Success = 1)
        //   beq/bne.un -> error_handling or success_path
        //
        // We'll change the branch so that when init fails, we still go to success path.

        Console.WriteLine("\nStrategy: Find all branch points that lead to WaitPopup error paths and redirect them");

        // Print out key IL instructions for debugging
        Console.WriteLine("\nKey IL instructions in MoveNext:");
        for (int i = 0; i < instructions.Count; i++)
        {
            var instr = instructions[i];
            // Print brfalse/brtrue/beq/bne instructions
            if (instr.OpCode.FlowControl == FlowControl.Cond_Branch)
            {
                Console.WriteLine($"  IL_{i:X4} (0x{instr.Offset:X4}): {instr}");
            }
            // Print WaitPopup constructions
            if (instr.OpCode == OpCodes.Newobj)
            {
                var methodRef = instr.Operand as MethodReference;
                if (methodRef != null && methodRef.DeclaringType.Name == "WaitPopup")
                {
                    Console.WriteLine($"  IL_{i:X4} (0x{instr.Offset:X4}): {instr} <-- WaitPopup");
                }
            }
            // Print ldstr for context
            if (instr.OpCode == OpCodes.Ldstr)
            {
                var str = instr.Operand as string;
                if (str != null && (str.Contains("Success") || str.Contains("BILLING") || str.Contains("FAILED")))
                {
                    Console.WriteLine($"  IL_{i:X4} (0x{instr.Offset:X4}): ldstr \"{str}\"");
                }
            }
        }

        // SIMPLER APPROACH: Find the "Success all" target and make all early error paths go there
        //
        // Find instruction: ldstr "[{0}] Success all!!!!"
        // That's the success path we want everything to go to

        Instruction successAllTarget = null;
        for (int i = 0; i < instructions.Count; i++)
        {
            if (instructions[i].OpCode == OpCodes.Ldstr)
            {
                var str = instructions[i].Operand as string;
                if (str != null && str.Contains("Success all"))
                {
                    successAllTarget = instructions[i];
                    Console.WriteLine($"\nFound success target at IL_{i:X4}: ldstr \"{str}\"");
                    break;
                }
            }
        }

        if (successAllTarget == null)
        {
            Console.WriteLine("ERROR: Could not find 'Success all' target instruction!");
        }
        else
        {
            // Strategy: Find ALL brfalse instructions that check for null/failure and redirect
            // the failing paths to successAllTarget
            //
            // Looking at the C# code:
            // - Line 77: if (CheckInAppBilling == null) -> show error
            // - Line 91: if (TupleResultInit.Item1 != Success) -> inner checks -> show error
            // - Line 118: if (TupleResultQueryInventory.Item1 == Success) -> success, else -> error
            //
            // The key insight is that the "else" block (starting at line 111) eventually leads to
            // the Success path at line 119-123 if everything goes well.
            //
            // Simplest approach: Find the first brfalse after TupleResultQueryInventory and
            // make it always branch to success

            int patchCount = 0;

            // First, patch the TupleResultInit check
            // Pattern: ldfld TupleResultInit, ldfld Item1, brfalse IL_02a0
            // We want to change brfalse to unconditional br (but need to pop the value)

            for (int i = 0; i < instructions.Count - 3; i++)
            {
                var instr = instructions[i];

                if (instr.OpCode == OpCodes.Ldfld)
                {
                    var fieldRef = instr.Operand as FieldReference;
                    if (fieldRef != null && fieldRef.Name == "TupleResultInit")
                    {
                        // Check if next is ldfld Item1 and then brfalse
                        if (i + 2 < instructions.Count &&
                            instructions[i + 1].OpCode == OpCodes.Ldfld &&
                            instructions[i + 2].OpCode == OpCodes.Brfalse)
                        {
                            var item1Field = instructions[i + 1].Operand as FieldReference;
                            if (item1Field != null && item1Field.Name == "Item1")
                            {
                                var brTarget = instructions[i + 2].Operand as Instruction;
                                Console.WriteLine($"\nFound TupleResultInit.Item1 check at IL_{i:X4}");
                                Console.WriteLine($"  brfalse target: {brTarget}");

                                // Change: pop the CheckInAppBilling, nop, unconditional br to success
                                il.Replace(instr, il.Create(OpCodes.Pop)); // pop CheckInAppBilling
                                il.Replace(instructions[i + 1], il.Create(OpCodes.Nop));
                                il.Replace(instructions[i + 2], il.Create(OpCodes.Br, successAllTarget));

                                Console.WriteLine($"  Patched: unconditional jump to Success all");
                                patchCount++;
                                break; // Only patch the first TupleResultInit check
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"\nTotal patches applied: {patchCount}");
        }

        // Now patch the QueryInventory check as well
        // This shows "Cannot make any payments" popup
        Console.WriteLine("\nSearching for QueryInventory check...");

        // Looking at the IL structure for QueryInventory:
        //   call get_instance()  <- pushes LoginManager
        //   ldfld CheckInAppBilling  <- pops LoginManager, pushes CheckInAppBilling
        //   ldfld TupleResultQueryInventory  <- pops CheckInAppBilling, pushes Tuple
        //   ldfld Item1  <- pops Tuple, pushes Item1 (EResultQueryInventory enum)
        //   brfalse IL_043e  <- when Item1 == 0 (Success), jump to success
        //
        // We want to unconditionally go to success
        // Strategy: Replace the ldfld Item1 with a pop (removes Tuple)
        //           Replace brfalse with br to success (no stack change needed)

        int queryPatchCount = 0;
        for (int i = 0; i < instructions.Count - 5; i++)
        {
            var instr = instructions[i];

            // Look for pattern: ldfld TupleResultQueryInventory followed by ldfld Item1 followed by brfalse
            if (instr.OpCode == OpCodes.Ldfld)
            {
                var fieldRef = instr.Operand as FieldReference;
                if (fieldRef != null && fieldRef.Name == "TupleResultQueryInventory")
                {
                    // Check next instructions
                    if (i + 2 < instructions.Count)
                    {
                        var item1Instr = instructions[i + 1];
                        var branchInstr = instructions[i + 2];

                        if (item1Instr.OpCode == OpCodes.Ldfld && branchInstr.OpCode == OpCodes.Brfalse)
                        {
                            var item1Field = item1Instr.Operand as FieldReference;
                            if (item1Field != null && item1Field.Name == "Item1")
                            {
                                Console.WriteLine($"Found QueryInventory check at IL_{i:X4}");
                                var successTarget = branchInstr.Operand as Instruction;
                                Console.WriteLine($"  brfalse target (success path): {successTarget}");

                                // Stack before our changes:
                                //   ... CheckInAppBilling on stack
                                //   ldfld TupleResultQueryInventory → pops CheckInAppBilling, pushes Tuple
                                //   ldfld Item1 → pops Tuple, pushes Item1
                                //   brfalse → pops Item1, maybe branches
                                //
                                // We want to unconditionally branch to success
                                // Replace:
                                //   ldfld TupleResultQueryInventory → pop (removes CheckInAppBilling from stack)
                                //   ldfld Item1 → nop
                                //   brfalse → br success

                                Console.WriteLine($"  Patching: Making unconditional jump to success path");

                                il.Replace(instr, il.Create(OpCodes.Pop)); // pop CheckInAppBilling
                                il.Replace(item1Instr, il.Create(OpCodes.Nop)); // nop
                                il.Replace(branchInstr, il.Create(OpCodes.Br, successTarget)); // unconditional branch

                                queryPatchCount++;
                                Console.WriteLine($"  Changed to unconditional jump to success at {successTarget}");

                                // Only patch once
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (queryPatchCount > 0)
        {
            Console.WriteLine($"Successfully patched {queryPatchCount} QueryInventory check(s)");
        }
        else
        {
            Console.WriteLine("QueryInventory patch skipped (handled by TupleResultInit patch)");
        }
    }
}
