using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static readonly string[] AwakeningSortFields = {
        "_ReqLevelSortType", "_GradeSortType", "_StarSortType",
        "_HeroLevelSortType", "_HeroGradeSortType"
    };

    static (MethodDefinition Constructor, MethodDefinition Reset, FieldDefinition[] Fields)
        AwakeningSortContext(ModuleDefinition module)
    {
        var view = module.GetType("NGame2.NUI.NWindow.HeroEquipAwaken")
            ?? throw new InvalidOperationException("Missing awakening window.");
        var fields = AwakeningSortFields.Select(name => view.Fields.Single(f => f.Name == name)).ToArray();
        var sortType = view.FullName + "/ESortType";
        if (fields.Any(f => f.IsStatic || f.FieldType.FullName != sortType) ||
            view.Fields.Count(f => f.FieldType.FullName == sortType) != fields.Length)
            throw new InvalidOperationException("Unexpected awakening sort fields.");
        var ctor = view.Methods.Single(m => m.IsConstructor && !m.IsStatic);
        var reset = view.Methods.Single(m => m.Name == "ResetHeroEquipAwaken");
        foreach (var method in new[] { ctor, reset }) {
            if (method.IsStatic || method.Parameters.Count != 0 || !method.HasBody ||
                method.ReturnType.MetadataType != MetadataType.Void || method.Body.HasExceptionHandlers ||
                method.Body.Instructions.Count == 0 || method.Body.Instructions.Last().OpCode != OpCodes.Ret ||
                method.Body.Instructions.Count(i => i.OpCode == OpCodes.Ret) != 1 ||
                method.Body.Instructions.Any(i => i.OpCode.FlowControl is FlowControl.Branch or
                    FlowControl.Cond_Branch or FlowControl.Throw))
                throw new InvalidOperationException("Unexpected awakening sort initialization control flow: " + method.FullName);
            VerifyArenaResultStack(method);
        }
        // Reset is the native source of truth, but a changed default requires review.
        foreach (var field in fields) {
            var references = reset.Body.Instructions.Where(i => i.Operand is FieldReference f && f.FullName == field.FullName).ToArray();
            if (references.Length != 1 || references[0].OpCode != OpCodes.Stfld ||
                references[0].Previous?.OpCode != OpCodes.Ldc_I4_1 ||
                references[0].Previous.Previous?.OpCode != OpCodes.Ldarg_0)
                throw new InvalidOperationException("Unexpected awakening reset default: " + field.Name);
        }
        return (ctor, reset, fields);
    }

    static void PatchAwakeningSortInitialization(ModuleDefinition module)
    {
        var (ctor, _, fields) = AwakeningSortContext(module);
        if (ctor.Body.Instructions.Any(i => i.Operand is FieldReference f && fields.Any(field => field.FullName == f.FullName))) {
            // Never repair a partial, duplicated or conflicting patch silently.
            VerifyAwakeningSortInitialization(module);
            return;
        }
        VerifyAwakeningSortBaseConstructor(ctor, ctor.Body.Instructions.Count - 1);
        var original = ctor.Body.Instructions.Select(i => (i.OpCode, i.Operand)).ToArray();
        var ret = ctor.Body.Instructions.Last();
        var il = ctor.Body.GetILProcessor();
        foreach (var field in fields) {
            il.InsertBefore(ret, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(ret, Instruction.Create(OpCodes.Ldc_I4_1));
            il.InsertBefore(ret, Instruction.Create(OpCodes.Stfld, field));
        }
        if (!original.SequenceEqual(ctor.Body.Instructions.Take(original.Length - 1)
                .Append(ret).Select(i => (i.OpCode, i.Operand))))
            throw new InvalidOperationException("Awakening sort patch changed native constructor initialization.");
        VerifyAwakeningSortInitialization(module);
    }

    static void VerifyAwakeningSortBaseConstructor(MethodDefinition ctor, int end)
    {
        var body = ctor.Body.Instructions;
        if (end < 2 || body[end - 2].OpCode != OpCodes.Ldarg_0 || body[end - 1].OpCode != OpCodes.Call ||
            body[end - 1].Operand is not MethodReference baseCtor || baseCtor.Name != ".ctor" ||
            !baseCtor.HasThis || baseCtor.Parameters.Count != 0 ||
            baseCtor.ReturnType.MetadataType != MetadataType.Void ||
            baseCtor.DeclaringType.FullName != ctor.DeclaringType.BaseType.FullName ||
            body.Take(end).Count(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference m && m.Name == ".ctor") != 1)
            throw new InvalidOperationException("Unexpected awakening base constructor boundary.");
    }

    static void VerifyAwakeningSortInitialization(ModuleDefinition module)
    {
        var (ctor, reset, fields) = AwakeningSortContext(module);
        var body = ctor.Body.Instructions;
        int start = body.Count - 1 - fields.Length * 3;
        VerifyAwakeningSortBaseConstructor(ctor, start);
        for (int index = 0; index < fields.Length; index++) {
            var field = fields[index];
            var references = body.Where(i => i.Operand is FieldReference f && f.FullName == field.FullName).ToArray();
            int offset = start + index * 3;
            if (body[offset].OpCode != OpCodes.Ldarg_0 || body[offset + 1].OpCode != OpCodes.Ldc_I4_1 ||
                body[offset + 2].OpCode != OpCodes.Stfld || references.Length != 1 || references[0] != body[offset + 2])
                throw new InvalidOperationException("Missing, duplicated or conflicting awakening constructor default: " + field.Name);
        }
        VerifyArenaResultStack(ctor);
        TestAwakeningSortInitialization(ctor, reset, fields);
    }

    static void TestAwakeningSortInitialization(MethodDefinition ctor, MethodDefinition reset, FieldDefinition[] fields)
    {
        // Interpret the actual IL, not a second implementation of the assignments.
        // Unity objects, collection constructors and native cleanup/UI calls are
        // opaque boundaries; none is executed or requires loading the game runtime.
        var cold = fields.ToDictionary(f => f.FullName, _ => 0);
        var reopened = fields.ToDictionary(f => f.FullName, _ => 0);
        ExecuteAwakeningSortInstructions(ctor, cold);

        ExecuteAwakeningSortInstructions(reset, reopened);
        if (fields.Any(f => cold[f.FullName] != 1 || cold[f.FullName] != reopened[f.FullName]))
            throw new InvalidOperationException("Cold awakening sort state differs from close/reset state.");

        foreach (var name in new[] { "SortAwakeInvenItemList", "SortAwakeEquipItemList" }) {
            var toggle = ctor.DeclaringType.Methods.Single(m => m.Name == name && m.Parameters.Count == 2 &&
                m.Parameters[0].ParameterType.FullName == fields[0].FieldType.FullName + "&");
            VerifyArenaResultStack(toggle);
            foreach (var field in fields)
            foreach (int initial in new[] { 0, 1 }) {
                var state = fields.ToDictionary(f => f.FullName, _ => 1);
                state[field.FullName] = initial;
                for (int click = 1; click <= 4; click++) {
                    int sprite = ExecuteAwakeningSortInstructions(toggle, state, field.FullName);
                    int expected = (initial + click) % 2;
                    if (state[field.FullName] != expected || sprite != expected ||
                        fields.Where(f => f != field).Any(f => state[f.FullName] != 1))
                        throw new InvalidOperationException("Awakening sort toggle regression: " + name + "/" + field.Name);
                }
            }
        }
    }

    static int ExecuteAwakeningSortInstructions(MethodDefinition method, Dictionary<string, int> state, string? sortField = null)
    {
        var stack = new Stack<object>();
        var opaque = new object();
        int sprite = -1, spriteCalls = 0, steps = 0;
        var pc = method.Body.Instructions[0];
        while (pc != null && ++steps <= method.Body.Instructions.Count * 4) {
            var next = pc.Next;
            switch (pc.OpCode.Code) {
                case Code.Nop: break;
                case Code.Ldarg_0: stack.Push(state); break;
                case Code.Ldarg_1: stack.Push(sortField ?? throw new InvalidOperationException("Unexpected awakening argument.")); break;
                case Code.Ldarg_2: stack.Push(opaque); break;
                case Code.Ldnull: stack.Push(opaque); break;
                case Code.Ldc_I4_0: stack.Push(0); break;
                case Code.Ldc_I4_1: stack.Push(1); break;
                case Code.Ldc_I4_2: stack.Push(2); break;
                case Code.Ldc_I4: stack.Push((int)pc.Operand); break;
                case Code.Ldc_I4_S: stack.Push((int)(sbyte)pc.Operand); break;
                case Code.Ldftn: stack.Push(opaque); break;
                case Code.Add: { int right = (int)stack.Pop(); stack.Push((int)stack.Pop() + right); break; }
                case Code.Ldind_I4: stack.Push(state[(string)stack.Pop()]); break;
                case Code.Stind_I4: { int value = (int)stack.Pop(); state[(string)stack.Pop()] = value; break; }
                case Code.Bne_Un:
                case Code.Bne_Un_S: { var right = stack.Pop(); if (!Equals(stack.Pop(), right)) next = (Instruction)pc.Operand; break; }
                case Code.Stfld: {
                    var value = stack.Pop();
                    if (!ReferenceEquals(stack.Pop(), state)) throw new InvalidOperationException("Unexpected awakening field receiver.");
                    var field = (FieldReference)pc.Operand;
                    if (state.ContainsKey(field.FullName)) state[field.FullName] = (int)value;
                    break;
                }
                case Code.Call:
                case Code.Callvirt:
                case Code.Newobj: {
                    var called = (MethodReference)pc.Operand;
                    var arguments = new object[called.Parameters.Count];
                    for (int index = arguments.Length - 1; index >= 0; index--) arguments[index] = stack.Pop();
                    if (called.HasThis && pc.OpCode != OpCodes.Newobj) stack.Pop();
                    if (called.Name == "SetSprite") {
                        if (called.DeclaringType.FullName != method.DeclaringType.FullName + "/UpDownSpriteInfo" || arguments.Length != 1)
                            throw new InvalidOperationException("Unexpected awakening sort sprite call.");
                        sprite = (int)arguments[0];
                        spriteCalls++;
                    }
                    if (pc.OpCode == OpCodes.Newobj || called.ReturnType.MetadataType != MetadataType.Void) stack.Push(opaque);
                    break;
                }
                case Code.Ret:
                    if (stack.Count != 0 || (sortField != null && spriteCalls != 1))
                        throw new InvalidOperationException("Unbalanced awakening sort regression or missing sprite update.");
                    return sprite;
                default: throw new InvalidOperationException("Unexpected awakening sort regression IL: " + pc);
            }
            pc = next;
        }
        throw new InvalidOperationException("Awakening sort regression did not return.");
    }
}
