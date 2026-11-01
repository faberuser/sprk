using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string GodkingWeeklyRechargeMessage = "You have already used your weekly Ruby recharge for Trial of the God King. You can recharge again after the weekly reset on Monday (00:00 UTC).";

    static TypeDefinition GodkingRechargeCallbacks(ModuleDefinition module) => module
        .GetType("ContentUIGodkingTrialDungeonListComponent").NestedTypes.Single(t => t.Name == "<>c");

    static string GodkingRechargeMessage(string result) => result switch
    {
        "ResetLimitExceeded" => GodkingWeeklyRechargeMessage,
        "MaxStamina" => "You already have the maximum number of Trial of the God King entries. Use an entry before recharging.",
        "NotEnoughGem" => "You do not have enough Rubies to recharge a Trial of the God King entry.",
        _ => "The Trial of the God King entry could not be recharged. Please try again later."
    };

    static void PatchGodkingRechargePrompt(ModuleDefinition module)
    {
        var popup = module.GetType("NGame2.NUI.NWindow.MessagePopup").Methods.Single(m =>
            m.Name == "OpenInform" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.String");
        var callback = GodkingRechargeCallbacks(module).Methods.Single(m => m.Name == "<ConsumeCashPopup>b__34_2");
        var results = module.GetType("NShared.RechargeStamina.ResultType");
        int Code(string name) => Convert.ToInt32(results.Fields.Single(f => f.Name == name).Constant);
        callback.Body = new MethodBody(callback);
        var il = callback.Body.GetILProcessor();
        var done = Instruction.Create(OpCodes.Ret);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldc_I4, Code("Success"));
        il.Emit(OpCodes.Beq, done);
        foreach (var result in new[] { "ResetLimitExceeded", "MaxStamina", "NotEnoughGem" })
        {
            var next = Instruction.Create(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4, Code(result));
            il.Emit(OpCodes.Bne_Un, next);
            il.Emit(OpCodes.Ldstr, GodkingRechargeMessage(result));
            il.Emit(OpCodes.Call, popup);
            il.Emit(OpCodes.Ret);
            il.Append(next);
        }
        il.Emit(OpCodes.Ldstr, GodkingRechargeMessage("Fail"));
        il.Emit(OpCodes.Call, popup);
        il.Append(done);

        // The local preflight gate has the same limit, before any request is sent.
        var preflight = module.GetType("ContentUIGodkingTrialDungeonListComponent").Methods.Single(m => m.Name == "ConsumeCashPopup");
        foreach (var text in preflight.Body.Instructions.Where(i => Equals(i.Operand, "GODKING_TRIAL.NOT_ENTER")))
        {
            var localize = text.Next;
            if (localize?.Operand is not MethodReference method || method.Name != "GetStringDirect")
                throw new InvalidOperationException("Unrecognized God King recharge preflight localization.");
            text.Operand = GodkingWeeklyRechargeMessage;
            localize.OpCode = OpCodes.Nop;
            localize.Operand = null;
        }
        VerifyGodkingRechargePrompt(module);
    }

    static void VerifyGodkingRechargePrompt(ModuleDefinition module)
    {
        var callback = GodkingRechargeCallbacks(module).Methods.Single(m => m.Name == "<ConsumeCashPopup>b__34_2");
        var instructions = callback.Body.Instructions;
        foreach (var result in new[] { "ResetLimitExceeded", "MaxStamina", "NotEnoughGem", "Fail" })
            if (!instructions.Any(i => Equals(i.Operand, GodkingRechargeMessage(result))))
                throw new InvalidOperationException("Missing distinct God King recharge error: " + result);
        if (instructions.Any(i => i.Operand is MethodReference m && (m.Name == "OpenNewPayShop" || m.Name == "GetString")) ||
            !instructions.Any(i => i.OpCode == OpCodes.Beq && i.Operand == instructions.Last()))
            throw new InvalidOperationException("God King recharge must leave success silent and show accurate failures.");
        var preflight = module.GetType("ContentUIGodkingTrialDungeonListComponent").Methods.Single(m => m.Name == "ConsumeCashPopup");
        if (!preflight.Body.Instructions.Any(i => Equals(i.Operand, GodkingWeeklyRechargeMessage)) ||
            !preflight.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "CanRechargeTicket"))
            throw new InvalidOperationException("God King preflight must explain the weekly recharge limit.");
        VerifyArenaResultStack(callback);
        VerifyArenaResultStack(preflight);
        // Execute the actual rewritten callback for every native result. In
        // particular, ResetLimitExceeded must not reach the Ruby-shortage path.
        var results = module.GetType("NShared.RechargeStamina.ResultType");
        foreach (var result in results.Fields.Where(f => f.HasConstant))
        {
            var stack = new Stack<object>();
            string? message = null;
            var pc = instructions[0];
            var budget = 100;
            while (pc != null && --budget > 0)
            {
                var next = pc.Next;
                if (pc.OpCode == OpCodes.Ldarg_1) stack.Push(Convert.ToInt32(result.Constant));
                else if (pc.OpCode == OpCodes.Ldc_I4) stack.Push((int)pc.Operand);
                else if (pc.OpCode == OpCodes.Ldstr) stack.Push((string)pc.Operand);
                else if (pc.OpCode == OpCodes.Beq || pc.OpCode == OpCodes.Bne_Un)
                {
                    var equal = Equals(stack.Pop(), stack.Pop());
                    if (equal == (pc.OpCode == OpCodes.Beq)) next = (Instruction)pc.Operand;
                }
                else if (pc.OpCode == OpCodes.Call && pc.Operand is MethodReference called && called.Name == "OpenInform")
                {
                    if (message != null) throw new InvalidOperationException("Duplicate God King failure popup.");
                    message = (string)stack.Pop();
                }
                else if (pc.OpCode == OpCodes.Ret) break;
                else if (pc.OpCode != OpCodes.Nop) throw new InvalidOperationException("Unexpected God King callback instruction.");
                pc = next;
            }
            if (budget <= 0 || stack.Count != 0 || message != (result.Name == "Success" ? null : GodkingRechargeMessage(result.Name)))
                throw new InvalidOperationException("Incorrect God King recharge callback behavior for " + result.Name);
        }
    }

    static void InstallGodkingRechargePrompt(string root, bool verifyOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            if (verifyOnly) VerifyGodkingRechargePrompt(assembly.MainModule);
            else
            {
                PatchGodkingRechargePrompt(assembly.MainModule);
                PatchGodkingRechargePrompt(assembly.MainModule);
                var backup = Path.Combine("target", "godking-prompt-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
                Directory.CreateDirectory(backup);
                File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
                assembly.Write(dll + ".godking-prompt-staged");
            }
        }
        // Resolving companion types can cache another handle to the game DLL.
        // Release those handles before replacing the file on Windows.
        resolver.Dispose();
        if (!verifyOnly) File.Move(dll + ".godking-prompt-staged", dll, true);
        Console.WriteLine("Verified God King weekly limit, full-entry and Ruby shortage prompts and IL stacks.");
    }
}
