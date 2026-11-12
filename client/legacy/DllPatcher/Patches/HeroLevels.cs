using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void PatchLegacyHeroLevels(ModuleDefinition module) {
        var hero=module.GetType("NShared.HeroInfo");
        // CreatureStarTable retains the legacy awakening/transcendence caps,
        // ending at 100. Do not add the modern LimitBreakLevel to that cap.
        var max=hero.Methods.Single(m=>m.Name=="GetMaxLevel");
        var baseCap=max.Body.Instructions.Single(i=>i.Operand is MethodReference call &&
            call.DeclaringType.FullName=="NShared.CreatureStarDataContainer" && call.Name=="GetHeroMaxLevel");
        var processor=max.Body.GetILProcessor();
        while(baseCap.Next!=null)processor.Remove(baseCap.Next);
        processor.Append(Instruction.Create(OpCodes.Ret));
        foreach(var name in new[]{"CanIncLimitBreakLevel","CanIncLimitBreakExp","IsMaxLimitBreakLevel"}) {
            var method=hero.Methods.Single(m=>m.Name==name);
            method.Body=new MethodBody(method);
            method.Body.Instructions.Add(Instruction.Create(name=="IsMaxLimitBreakLevel"?OpCodes.Ldc_I4_1:OpCodes.Ldc_I4_0));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        // HeroStatPanel now takes its native fully-transcended branch: no Limit
        // Break controls/materials, and the class/stats grid moves up naturally.
    }
}
