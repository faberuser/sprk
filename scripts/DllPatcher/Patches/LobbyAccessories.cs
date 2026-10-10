using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchLobbyAccessories(ModuleDefinition module, TypeDefinition bridge)
    {
        MethodReference Bridge(string name) => module.ImportReference(bridge.Methods.Single(m => m.Name == name));
        var map = module.GetType("NGame2.NUI.NWorldMap.MapLayout");
        var builder = map.Methods.Single(m => m.Name == "CreateAvatarHeroRenderFacadeBuilder");
        var clone = builder.Body.Instructions.Single(i => i.Operand is MethodReference call &&
            (call.DeclaringType.Name == "Customizing_Extension_Method" && call.Name == "CreatePlayerAvatarHeroInfo" ||
             call.DeclaringType.Name == "SprkAccounts" && call.Name == "CreateLobbyAvatarHeroInfo"));
        clone.OpCode = OpCodes.Call;
        clone.Operand = Bridge("CreateLobbyAvatarHeroInfo");

        var observer = map.Methods.Single(m => m.Parameters.Count == 1 &&
            m.Parameters[0].ParameterType.FullName == "NGame2.NAccount.UserHeroManager/ChangeHeroInfo");
        if (!observer.Body.Instructions.Any(i => i.Operand is MethodReference call && call.Name == "ShouldRefreshLobbyAccessories"))
        {
            var original = observer.Body.Instructions[0];
            var mediator = map.Fields.Single(f => f.Name == "_mediator");
            var avatarGetter = module.GetType("NGame2.NUI.NWorldMap.NMapLayout.Mediator").Methods.Single(m => m.Name == "get_Avatar");
            var il = observer.Body.GetILProcessor();
            var prefix = new[]
            {
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call, Bridge("ShouldRefreshLobbyAccessories")),
                Instruction.Create(OpCodes.Brfalse, original),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, map.Methods.Single(m => m.Name == "IsForcedHeroAvatar")),
                Instruction.Create(OpCodes.Brtrue, original),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, mediator),
                Instruction.Create(OpCodes.Brfalse, original),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, mediator),
                Instruction.Create(OpCodes.Callvirt, avatarGetter),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, map.Fields.Single(f => f.Name == "_avatarIndex")),
                Instruction.Create(OpCodes.Call, map.Methods.Single(m => m.Name == "ChangePlayerAvatarRenderFacadeOnly"))
            };
            foreach (var instruction in prefix) il.InsertBefore(original, instruction);
        }
        Console.WriteLine("Lobby avatar uses equipped accessories and refreshes on accessory changes.");
    }
}
