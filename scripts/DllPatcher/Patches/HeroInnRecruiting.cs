using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static bool PatchHeroInnRecruiting(TypeDefinition heroInnViewType)
    {
        Console.WriteLine("\n=== Patching HeroInnView.InitUIBtnGroup ===");
        var initUIBtnGroup = heroInnViewType.Methods.FirstOrDefault(m => m.Name == "InitUIBtnGroup");
        if (initUIBtnGroup == null)
        {
            Console.WriteLine("ERROR: InitUIBtnGroup method not found!");
            return false;
        }

        Console.WriteLine($"Found InitUIBtnGroup with {initUIBtnGroup.Body.Instructions.Count} instructions");

        // Find the EGroupButtonType enum (nested in HeroInnView)
        var groupButtonTypeEnum = heroInnViewType.NestedTypes.FirstOrDefault(t => t.Name == "EGroupButtonType");
        if (groupButtonTypeEnum != null)
        {
            Console.WriteLine($"Found EGroupButtonType: {groupButtonTypeEnum.FullName}");
            foreach (var field in groupButtonTypeEnum.Fields)
            {
                if (field.HasConstant)
                    Console.WriteLine($"  {field.Name} = {field.Constant}");
            }
        }

        // The IL code pattern is:
        //   brtrue.s IL_0015   <- if flag2 (IsSelectedHero) is true, jump to load Friendly
        //   ldc.i4.0           <- else, load 0 (None)  <- WE WANT TO CHANGE THIS TO 1
        //   br.s IL_0019       <- skip to store
        //   ldc.i4.1           <- load 1 (Friendly)
        //   ...
        //   stloc.1            <- store gType

        var instructions = initUIBtnGroup.Body.Instructions;
        var il = initUIBtnGroup.Body.GetILProcessor();
        bool patched = false;

        Console.WriteLine("Looking for None -> Friendly patch opportunity...");

        // The pattern is: brtrue.s followed by ldc.i4.0 followed by br.s
        for (int i = 0; i < instructions.Count - 2; i++)
        {
            var instr = instructions[i];
            if (instr.OpCode == OpCodes.Brtrue || instr.OpCode == OpCodes.Brtrue_S)
            {
                var next = instructions[i + 1];
                var afterNext = instructions[i + 2];

                // Check if next is ldc.i4.0 and afterNext is br
                if ((next.OpCode == OpCodes.Ldc_I4_0 ||
                     (next.OpCode == OpCodes.Ldc_I4 && (int)next.Operand == 0) ||
                     (next.OpCode == OpCodes.Ldc_I4_S && (sbyte)next.Operand == 0)) &&
                    (afterNext.OpCode == OpCodes.Br || afterNext.OpCode == OpCodes.Br_S))
                {
                    Console.WriteLine($"Found pattern at IL_{i:X4}:");
                    Console.WriteLine($"  {instr}");
                    Console.WriteLine($"  {next} <- Changing this from 0 (None) to 1 (Friendly)");
                    Console.WriteLine($"  {afterNext}");

                    // Replace ldc.i4.0 with ldc.i4.1
                    var newInstr = il.Create(OpCodes.Ldc_I4_1);
                    il.Replace(next, newInstr);
                    patched = true;
                    Console.WriteLine("  Patched successfully!");
                    break;
                }
            }
        }

        if (!patched)
        {
            Console.WriteLine("WARNING: Could not find None -> Friendly patch point. Checking all ldc.i4.0...");
            // Print all instructions for debugging
            for (int i = 0; i < instructions.Count && i < 30; i++)
            {
                Console.WriteLine($"  IL_{i:X4}: {instructions[i]}");
            }
        }


        return true;
    }
}
