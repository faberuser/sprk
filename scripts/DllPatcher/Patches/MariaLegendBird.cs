using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static readonly string[] MariaBirdEntries={"CreateOpenData","CreateAnimationLoadList","CreateAnimationAsyncList"};

    static void PatchMariaLegendBird(ModuleDefinition module)
    {
        var weapon=module.GetType("NGameAsset.NCreature.WeaponData");
        var stock=module.GetType("NGameAsset.CreatureStock");
        var weaponStock=module.GetType("NGameAsset.WeaponStock");
        var normalize=weapon.Methods.SingleOrDefault(m=>m.Name=="SprkMariaLegendStock");
        if(normalize==null) {
            normalize=new MethodDefinition("SprkMariaLegendStock",MethodAttributes.Public|MethodAttributes.HideBySig,stock);
            normalize.Parameters.Add(new ParameterDefinition("stock",ParameterAttributes.None,stock));weapon.Methods.Add(normalize);
            var il=normalize.Body.GetILProcessor();var done=il.Create(OpCodes.Ldarg_1);var bodyOk=il.Create(OpCodes.Nop);var apply=il.Create(OpCodes.Nop);
            // Use the game's mscorlib references, never the patcher's .NET runtime.
            var nativeCalls=module.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody)
                .SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>()
                .Where(m=>m.DeclaringType.FullName=="System.String").ToArray();
            var equal=nativeCalls.First(m=>m.Name=="op_Equality");
            var empty=nativeCalls.First(m=>m.Name=="IsNullOrEmpty");
            var code=stock.Fields.Single(f=>f.Name=="Code");var body=stock.Fields.Single(f=>f.Name=="BodyPrefix");
            var ws=stock.Fields.Single(f=>f.Name=="WeaponStock");var prefix=weaponStock.Fields.Single(f=>f.Name=="WeaponPrefix");
            void Body() {il.Emit(OpCodes.Ldarga,normalize.Parameters[0]);il.Emit(OpCodes.Ldfld,body);}
            void Prefix() {il.Emit(OpCodes.Ldarga,normalize.Parameters[0]);il.Emit(OpCodes.Ldflda,ws);il.Emit(OpCodes.Ldfld,prefix);}
            il.Emit(OpCodes.Ldarga,normalize.Parameters[0]);il.Emit(OpCodes.Ldfld,code);il.Emit(OpCodes.Ldstr,"Hero.Maria");il.Emit(OpCodes.Call,equal);il.Emit(OpCodes.Brfalse,done);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Call,weapon.Methods.Single(m=>m.Name=="get_Name"));il.Emit(OpCodes.Ldstr,"Weapon1");il.Emit(OpCodes.Call,equal);il.Emit(OpCodes.Brfalse,done);
            Body();il.Emit(OpCodes.Ldstr,"Cos21Legend");il.Emit(OpCodes.Call,equal);il.Emit(OpCodes.Brtrue,bodyOk);
            Body();il.Emit(OpCodes.Ldstr,"Cos21Legend1");il.Emit(OpCodes.Call,equal);il.Emit(OpCodes.Brfalse,done);
            il.Append(bodyOk);
            // Equipped UW/base appearance may refresh independently of the body.
            // Preserve explicitly selected weapon costumes; only repair the base
            // and UW choices that otherwise replace the Legend companion.
            Prefix();il.Emit(OpCodes.Call,empty);il.Emit(OpCodes.Brtrue,apply);
            foreach(var value in new[]{"Unique01","Trans","Trial","Cos21Legend","Cos21Legend1"}) {
                Prefix();il.Emit(OpCodes.Ldstr,value);il.Emit(OpCodes.Call,equal);il.Emit(OpCodes.Brtrue,apply);
            }
            il.Emit(OpCodes.Br,done);il.Append(apply);
            il.Emit(OpCodes.Ldarga,normalize.Parameters[0]);il.Emit(OpCodes.Ldflda,ws);Body();il.Emit(OpCodes.Stfld,prefix);
            il.Append(done);il.Emit(OpCodes.Ret);
        }
        foreach(var name in MariaBirdEntries) {
            var method=weapon.Methods.Single(m=>m.Name==name);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==normalize.Name))continue;
            var il=method.Body.GetILProcessor();var first=method.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));il.InsertBefore(first,il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first,il.Create(OpCodes.Call,normalize));il.InsertBefore(first,il.Create(OpCodes.Starg,method.Parameters[0]));
        }
        VerifyMariaLegendBird(module);
    }

    static void VerifyMariaLegendBird(ModuleDefinition module)
    {
        if(module.AssemblyReferences.Any(r=>r.Name=="System.Private.CoreLib"))
            throw new InvalidOperationException("Maria bird patch must use the game's runtime references.");
        var weapon=module.GetType("NGameAsset.NCreature.WeaponData");
        var normalize=weapon.Methods.Single(m=>m.Name=="SprkMariaLegendStock");
        foreach(var literal in new[]{"Hero.Maria","Weapon1","Cos21Legend","Cos21Legend1","Unique01"})
            if(!normalize.Body.Instructions.Any(i=>Equals(i.Operand,literal)))throw new InvalidOperationException("Missing Maria bird scope: "+literal);
        if(normalize.Body.Instructions.Count(i=>i.OpCode==OpCodes.Stfld)!=1 ||
            !normalize.Body.Instructions.Any(i=>i.OpCode==OpCodes.Stfld && i.Operand is FieldReference f && f.Name=="WeaponPrefix"))
            throw new InvalidOperationException("Maria bird must only change its appearance prefix, preserving soul weapon data.");
        VerifyArenaResultStack(normalize);
        foreach(var name in MariaBirdEntries) {
            var method=weapon.Methods.Single(m=>m.Name==name);var code=method.Body.Instructions;
            if(code.Count(i=>i.Operand is MethodReference m && m.Name==normalize.Name)!=1 ||
                code[2].Operand is not MethodReference call || call.Name!=normalize.Name || code[3].OpCode!=OpCodes.Starg)
                throw new InvalidOperationException("Maria bird selection must precede model/animation/preload selection: "+name);
            VerifyArenaResultStack(method);
        }
    }

    static void InstallMariaLegendBird(string root,bool verifyOnly=false)
    {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using(var assembly=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true})) {
            if(verifyOnly) {VerifyMariaLegendBird(assembly.MainModule);return;}
            PatchMariaLegendBird(assembly.MainModule);PatchMariaLegendBird(assembly.MainModule);
            assembly.Write(dll+".maria-bird-staged");
        }
        resolver.Dispose();
        using(var staged=AssemblyDefinition.ReadAssembly(dll+".maria-bird-staged"))VerifyMariaLegendBird(staged.MainModule);
        var backup=Path.Combine("target","maria-bird-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));Directory.CreateDirectory(backup);
        File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));File.Move(dll+".maria-bird-staged",dll,true);
        Console.WriteLine("Installed and verified Maria Legend bird model, animation and preload selection. Restart the client.");
    }
}
