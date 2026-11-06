using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static MethodDefinition SoulstoneFilter(ModuleDefinition module) => module.GetType("NGame2.NUI.NWindow.SelectFilter")
        .Methods.Single(m => m.Name == "CheckAddSoulStoneItem");

    static void PatchSoulstoneOwnership(ModuleDefinition module)
    {
        var method = SoulstoneFilter(module);
        var old = method.Body.Instructions.SingleOrDefault(i => i.Operand is MethodReference { Name: "CheckExistUniqueItem" });
        if (old != null) {
            var native = method.DeclaringType.Methods.Single(m => m.Name == "CheckAddEquipItemByUniqueWeapon");
            MethodReference Call(string name) => (MethodReference)native.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == name).Operand;
            // Replace only the equipment predicate. Retain the native class and
            // All/Owned/Not Owned branches and the soulstone's hero-index mapping.
            var first = old.Previous.Previous.Previous.Previous;
            if (first.OpCode != OpCodes.Ldarg_0 || first.Next.OpCode != OpCodes.Ldloc_0 || old.Next.OpCode != OpCodes.Stloc_1)
                throw new InvalidOperationException("Unexpected soulstone ownership predicate.");
            first.OpCode = OpCodes.Call; first.Operand = Call("get_instance");
            first.Next.OpCode = OpCodes.Callvirt; first.Next.Operand = Call("get_HeroManager");
            first.Next.Next.OpCode = OpCodes.Ldloc_0; first.Next.Next.Operand = null;
            old.Previous.OpCode = OpCodes.Callvirt; old.Previous.Operand = Call("Get");
            old.OpCode = OpCodes.Ldnull; old.Operand = null;
            method.Body.GetILProcessor().InsertAfter(old, Instruction.Create(OpCodes.Cgt_Un));
        }
        VerifySoulstoneOwnership(module);
    }

    static void VerifySoulstoneOwnership(ModuleDefinition module)
    {
        var method = SoulstoneFilter(module);
        var calls = method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().ToArray();
        foreach (var name in new[] { "GetValueInt", "get_instance", "get_HeroManager", "Get", "get_MainComboBoxCurrentKey" })
            if (calls.Count(m => m.Name == name) != 1) throw new InvalidOperationException("Missing soulstone roster lookup: " + name);
        if (calls.Any(m => m.Name == "CheckExistUniqueItem") || calls.Count(m => m.Name == "CheckSameSoulStoneTagType") != 3)
            throw new InvalidOperationException("Soulstone ownership/class filter regression.");
        var get = method.Body.Instructions.Single(i => i.Operand is MethodReference { Name: "Get" });
        if (get.Previous.OpCode != OpCodes.Ldloc_0 || get.Next.OpCode != OpCodes.Ldnull || get.Next.Next.OpCode != OpCodes.Cgt_Un)
            throw new InvalidOperationException("Soulstone must test roster membership for its hero.");
        VerifyArenaResultStack(method);
    }

    static void InstallSoulstoneOwnership(string root, bool staged, bool verifyOnly = false)
    {
        var dll = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        var stage = dll + ".soulstone-ownership-staged";
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        if (!verifyOnly) {
            using (var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
                PatchSoulstoneOwnership(game.MainModule);
                game.Write(stage);
            }
            if (!staged) {
                if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
                    throw new InvalidOperationException("Soulstone fix staged. Close client processes before installing.");
                var backup = Path.Combine("target", "soulstone-ownership-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
                Directory.CreateDirectory(backup); File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
                File.Move(stage, dll, true);
            }
        }
        using var installed = AssemblyDefinition.ReadAssembly(staged ? stage : dll);
        VerifySoulstoneOwnership(installed.MainModule);
        TestSoulstoneOwnership(installed.MainModule);
        Console.WriteLine("PASS: soulstone roster ownership, native class branches and idempotence.");
    }

    static void TestSoulstoneOwnership(ModuleDefinition module)
    {
        var method = SoulstoneFilter(module);
        var count = method.Body.Instructions.Count;
        PatchSoulstoneOwnership(module);
        if (count != method.Body.Instructions.Count) throw new Exception("Soulstone patch is not idempotent.");
        // Execute the installed method's control flow. Substitute only native
        // Unity/account boundaries, allowing regression checks without loading Unity.
        foreach (int main in new[] { 0, 4, 5 })
        foreach (bool owned in new[] { false, true })
        foreach (bool sameClass in new[] { false, true }) {
            var stack = new Stack<object?>(); var locals = new object?[3];
            var pc = method.Body.Instructions[0]; bool? actual = null;
            while (actual == null) {
                var next = pc.Next;
                switch (pc.OpCode.Code) {
                    case Code.Ldarg_0: stack.Push("filter"); break;
                    case Code.Ldarg_1: stack.Push(123); break;
                    case Code.Ldc_I4_0: stack.Push(0); break;
                    case Code.Ldc_I4_1: stack.Push(1); break;
                    case Code.Ldc_I4_4: stack.Push(4); break;
                    case Code.Ldc_I4_5: stack.Push(5); break;
                    case Code.Ldnull: stack.Push(null); break;
                    case Code.Ldloc_0: stack.Push(locals[0]); break;
                    case Code.Ldloc_1: stack.Push(locals[1]); break;
                    case Code.Ldloc_2: stack.Push(locals[2]); break;
                    case Code.Stloc_0: locals[0] = stack.Pop(); break;
                    case Code.Stloc_1: locals[1] = stack.Pop(); break;
                    case Code.Stloc_2: locals[2] = stack.Pop(); break;
                    case Code.Cgt_Un: var right = stack.Pop(); stack.Push(Equals(stack.Pop(), right) ? 0 : 1); break;
                    case Code.Call: case Code.Callvirt:
                        var call = (MethodReference)pc.Operand;
                        switch (call.Name) {
                            case "GetValueInt": if (!Equals(stack.Pop(), 0)) throw new Exception("Wrong hero field"); stack.Pop(); stack.Push(123); break;
                            case "get_instance": stack.Push("account"); break;
                            case "get_HeroManager": stack.Pop(); stack.Push("roster"); break;
                            case "Get": if (!Equals(stack.Pop(), 123)) throw new Exception("Wrong hero lookup"); stack.Pop(); stack.Push(owned ? "hero" : null); break;
                            case "get_MainComboBoxCurrentKey": stack.Pop(); stack.Push(main); break;
                            case "CheckSameSoulStoneTagType": stack.Pop(); stack.Pop(); stack.Push(sameClass ? 1 : 0); break;
                            default: throw new Exception("Unexpected native dependency: " + call.FullName);
                        }
                        break;
                    case Code.Br_S: case Code.Br: next = (Instruction)pc.Operand; break;
                    case Code.Brfalse_S: case Code.Brfalse: if (Equals(stack.Pop(), 0)) next = (Instruction)pc.Operand; break;
                    case Code.Brtrue_S: case Code.Brtrue: if (!Equals(stack.Pop(), 0)) next = (Instruction)pc.Operand; break;
                    case Code.Beq_S: case Code.Beq: if (Equals(stack.Pop(), stack.Pop())) next = (Instruction)pc.Operand; break;
                    case Code.Ret: actual = !Equals(stack.Pop(), 0); break;
                    default: throw new Exception("Unhandled soulstone IL: " + pc);
                }
                pc = next;
            }
            if (actual != (sameClass && (main == 0 || (main == 5 ? owned : !owned))))
                throw new Exception($"Soulstone filter failed: mode={main}, owned={owned}, class={sameClass}");
        }
    }
}
