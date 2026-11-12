using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DllPatcher;
partial class Program {
    static void PatchInventoryExpansion(ModuleDefinition module,ModuleDefinition helper) {
        var type = module.GetType("NShared.InventoryExtendData");
        var getter = type.Methods.Single(m => m.Name == "get_EquipItemExtendCount");
        var inventory = type.Methods.Single(m => m.Name == "get_InventoryType");
        var field = type.Fields.Single(f => f.Name == "<EquipItemExtendCount>k__BackingField");
        getter.Body = new MethodBody(getter);
        var il = getter.Body.GetILProcessor();
        var original = il.Create(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, inventory);
        il.Emit(OpCodes.Brtrue, original);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, field);
        il.Emit(OpCodes.Ldc_I4, 90); il.Emit(OpCodes.Bne_Un, original);
        il.Emit(OpCodes.Ldc_I4, 140); il.Emit(OpCodes.Ret);
        il.Append(original); il.Emit(OpCodes.Ldfld, field); il.Emit(OpCodes.Ret);
        // Old clients may still have the portal-era ceiling hook installed.
        var legacy = module.GetType("RestoredPortal")?.Methods.SingleOrDefault(m => m.Name == "EquipmentBagMaxExtensions");
        if (legacy != null) foreach (var i in legacy.Body.Instructions)
            if (i.OpCode == OpCodes.Ldc_I4_S && Convert.ToInt32(i.Operand) == 90 || i.OpCode == OpCodes.Ldc_I4 && Equals(i.Operand, 90)) {
                i.OpCode = OpCodes.Ldc_I4; i.Operand = 140;
            }
        PatchInventoryExpansionCostText(module);
        PatchInventoryTicketCompensation(module,helper);
        VerifyInventoryExpansion(module);
    }
    static void PatchInventoryExpansionCostText(ModuleDefinition module) {
        var popup=module.GetType("NGame2.NUI.NWindow.ExtendInvenMessagePopup");
        var text=popup.Methods.SingleOrDefault(m=>m.Name=="SprkInventoryExpansionCostText");
        if(text==null) {
            text=new MethodDefinition("SprkInventoryExpansionCostText",MethodAttributes.Private|MethodAttributes.Static,module.TypeSystem.String);
            text.Parameters.Add(new ParameterDefinition("original",ParameterAttributes.None,module.TypeSystem.String));
            text.Parameters.Add(new ParameterDefinition("maximum",ParameterAttributes.None,module.TypeSystem.Int32));
            popup.Methods.Add(text);
        }
        text.Body=new MethodBody(text);var body=text.Body.GetILProcessor();
        var number=new MethodReference("ToString",module.TypeSystem.String,module.TypeSystem.Int32){HasThis=true};
        var replace=new MethodReference("Replace",module.TypeSystem.String,module.TypeSystem.String){HasThis=true};
        replace.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));replace.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        body.Emit(OpCodes.Ldarg_0);body.Emit(OpCodes.Ldstr,"1000");body.Emit(OpCodes.Ldarga_S,text.Parameters[1]);
        body.Emit(OpCodes.Call,number);body.Emit(OpCodes.Callvirt,replace);body.Emit(OpCodes.Ret);
        var label=popup.Methods.Single(m=>m.Name=="SetLabel");
        RepairInventoryPopupBranches(label);
        // Baked tables may contain one gear tier, so the old 1000-slot notice
        // can come from COST1 as well as COST2. Preserve wording and prices.
        foreach(var name in new[]{"EQUIP_INVENTORY_EXPAND_COST1","EQUIP_INVENTORY_EXPAND_COST2"}) {
            var key=label.Body.Instructions.Single(i=>Equals(i.Operand,name));var call=key.Next;
            if(call.Operand is not MethodReference {Name:"GetStringDirect"})throw new InvalidOperationException("Unexpected inventory cost localization call.");
            if(call.Next?.Next?.Next?.Operand is MethodReference m && m.Name==text.Name)continue;
            var il=label.Body.GetILProcessor();
            var load=il.Create(OpCodes.Ldarg_0);il.InsertAfter(call,load);
            var maximum=il.Create(OpCodes.Ldfld,popup.Fields.Single(f=>f.Name=="_maxEquipInvenSize"));il.InsertAfter(load,maximum);
            il.InsertAfter(maximum,il.Create(OpCodes.Call,text));
        }
    }
    static void RepairInventoryPopupBranches(MethodDefinition label) {
        var code=label.Body.Instructions;
        // The old cost-text insertion grew the gear block past the signed-byte
        // branch range. Recover its known native destination on affected clients.
        var dispatch=code.Single(i=>i.OpCode==OpCodes.Ldfld && i.Operand is FieldReference {Name:"_inventoryType"} &&
            (i.Next?.OpCode==OpCodes.Brtrue_S || i.Next?.OpCode==OpCodes.Brtrue));
        var chest=code.Single(i=>i.OpCode==OpCodes.Ldarg_0 &&
            i.Next?.Operand is FieldReference {Name:"_inventoryType"} && i.Next.Next?.OpCode==OpCodes.Ldc_I4_1);
        dispatch.Next.Operand=chest;
        foreach(var instruction in code) {
            if(instruction.OpCode.OperandType==OperandType.ShortInlineBrTarget)
                instruction.OpCode=typeof(OpCodes).GetFields().Select(f=>f.GetValue(null)).OfType<OpCode>()
                    .Single(c=>c.Name==instruction.OpCode.Name[..^2]);
        }
    }
    static void VerifyInventoryPopupBranches(MethodDefinition label) {
        var code=label.Body.Instructions;
        foreach(var instruction in code) {
            if(instruction.OpCode.OperandType is OperandType.InlineBrTarget or OperandType.ShortInlineBrTarget) {
                if(instruction.Operand is not Instruction target || !code.Contains(target))
                    throw new InvalidOperationException("Invalid inventory popup branch destination: "+instruction);
                if(instruction.OpCode.OperandType==OperandType.ShortInlineBrTarget)
                    throw new InvalidOperationException("Inventory popup branches must use full-width offsets.");
            }
        }
        var dispatch=code.Single(i=>i.OpCode==OpCodes.Ldfld && i.Operand is FieldReference {Name:"_inventoryType"} && i.Next?.OpCode==OpCodes.Brtrue);
        if(dispatch.Next.Operand is not Instruction chest || chest.OpCode!=OpCodes.Ldarg_0 ||
            chest.Next?.Operand is not FieldReference {Name:"_inventoryType"} || chest.Next.Next?.OpCode!=OpCodes.Ldc_I4_1 ||
            chest.Next.Next.Next?.OpCode!=OpCodes.Bne_Un)
            throw new InvalidOperationException("Storage Box must branch to the native chest expansion block.");
    }
    static void VerifyInventoryExpansion(ModuleDefinition module) {
        var getter = module.GetType("NShared.InventoryExtendData").Methods.Single(m => m.Name == "get_EquipItemExtendCount");
        if (!getter.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4 && Equals(i.Operand, 140)) ||
            !getter.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "get_InventoryType"))
            throw new InvalidOperationException("Inventory expansion row patch missing.");
        var popup=module.GetType("NGame2.NUI.NWindow.ExtendInvenMessagePopup");
        var label=popup.Methods.Single(m=>m.Name=="SetLabel");
        VerifyInventoryPopupBranches(label);
        foreach(var name in new[]{"EQUIP_INVENTORY_EXPAND_COST1","EQUIP_INVENTORY_EXPAND_COST2"}) {
        var key=label.Body.Instructions.Single(i=>Equals(i.Operand,name));var call=key.Next;
        if(call.Operand is not MethodReference {Name:"GetStringDirect"} || call.Next.OpCode!=OpCodes.Ldarg_0 ||
           call.Next.Next.Operand is not FieldReference {Name:"_maxEquipInvenSize"} ||
           call.Next.Next.Next.Operand is not MethodReference {Name:"SprkInventoryExpansionCostText"} ||
           label.Body.Instructions.Count(i=>i.Operand is MethodReference m&&m.Name=="SprkInventoryExpansionCostText")!=2)
            throw new InvalidOperationException("Inventory price notice must use the actual gear bag maximum.");
        }
        VerifyArenaResultStack(popup.Methods.Single(m=>m.Name=="SprkInventoryExpansionCostText"));
        VerifyInventoryTicketHooks(module);
    }
    static void VerifyInventoryExpansionFile(string root, bool staged = false) {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var path = Path.Combine(managed, "Assembly-CSharp.dll" + (staged ? ".inventory-expansion-staged" : ""));
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        var read=new ReaderParameters {AssemblyResolver=resolver};
        using (var assembly = AssemblyDefinition.ReadAssembly(path,read)) {
            VerifyInventoryExpansion(assembly.MainModule);
            // Reproduce the old writer overflow against the actual popup, then
            // require verification to reject it and the focused patch to repair it.
            var label=assembly.MainModule.GetType("NGame2.NUI.NWindow.ExtendInvenMessagePopup").Methods.Single(m=>m.Name=="SetLabel");
            var dispatch=label.Body.Instructions.Single(i=>i.OpCode==OpCodes.Ldfld && i.Operand is FieldReference {Name:"_inventoryType"} && i.Next?.OpCode==OpCodes.Brtrue);
            dispatch.Next.OpCode=OpCodes.Brtrue_S;
            using var brokenBytes=new MemoryStream();assembly.Write(brokenBytes);brokenBytes.Position=0;
            using var broken=AssemblyDefinition.ReadAssembly(brokenBytes,read);
            var brokenLabel=broken.MainModule.GetType(label.DeclaringType.FullName).Methods.Single(m=>m.Name=="SetLabel");
            bool rejected=false;
            try { VerifyInventoryPopupBranches(brokenLabel); } catch(InvalidOperationException) { rejected=true; }
            if(!rejected)throw new InvalidOperationException("Overflowed Storage Box branch was not rejected.");
            RepairInventoryPopupBranches(brokenLabel);
            using var repairedBytes=new MemoryStream();broken.Write(repairedBytes);repairedBytes.Position=0;
            using var repaired=AssemblyDefinition.ReadAssembly(repairedBytes,read);
            VerifyInventoryExpansion(repaired.MainModule);
            Console.WriteLine("PASS: serialized Storage Box branch destinations; old short-branch overflow rejected and repaired.");
        }
        using(var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkInventoryTickets.dll"+(staged?".staged":"")))) {
            var type=helper.MainModule.GetType("SprkInventoryTickets");
            if(type==null || !type.Methods.Any(m=>m.Name=="TryUse") || !type.Methods.Any(m=>m.Name=="AddParameters"))throw new InvalidOperationException("Missing inventory ticket compensation helper.");
        }
        var context = new System.Runtime.Loader.AssemblyLoadContext("InventoryExpansion-" + Guid.NewGuid(), true);
        context.Resolving += (load, name) => {
            var dep = Path.Combine(managed, name.Name + ".dll"+(staged && name.Name=="SprkInventoryTickets"?".staged":""));
            if (!File.Exists(dep)) return null;
            using var stream = new MemoryStream(File.ReadAllBytes(dep));
            return load.LoadFromStream(stream);
        };
        try {
            using var stream = new MemoryStream(File.ReadAllBytes(path));
            var game = context.LoadFromStream(stream);
            var type = game.GetType("NShared.InventoryExtendData")!;
            var inventory = type.GetProperty("InventoryType")!;
            foreach (int kind in new[] { 0, 1, 3 }) foreach (int count in new[] { 10, 50, 90, 140, 190 }) {
                var row = Activator.CreateInstance(type)!;
                inventory.SetValue(row, Enum.ToObject(inventory.PropertyType, kind));
                type.GetProperty("EquipItemExtendCount")!.SetValue(row, count);
                int actual = (int)type.GetProperty("EquipItemExtendCount")!.GetValue(row)!;
                if (actual != (kind == 0 && count == 90 ? 140 : count)) throw new InvalidOperationException("Inventory row regression.");
            }
            var formatter=game.GetType("NGame2.NUI.NWindow.ExtendInvenMessagePopup")!.GetMethod("SprkInventoryExpansionCostText",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;
            foreach(int maximum in new[]{1400,1600}) {
                var actual=(string)formatter.Invoke(null,new object[]{"Expansion costs 250 Rubies up to 1000 slots.",maximum})!;
                if(actual!=$"Expansion costs 250 Rubies up to {maximum} slots.")throw new InvalidOperationException("Inventory cost notice maximum regression.");
            }
            // Native ticket and gem expansion flows must read the patched getter.
            Console.WriteLine("PASS: 15 actual inventory row cases; gear ceiling 280 + 140*8 = 1400; chest/rune tiers preserved.");
            Console.WriteLine("PASS: localized upper-tier cost notice follows the bag maximum and preserves the 250 Ruby price.");
            using var ticketStream=new MemoryStream(File.ReadAllBytes(Path.Combine(managed,"SprkInventoryTickets.dll"+(staged?".staged":""))));
            var ticket=context.LoadFromStream(ticketStream).GetType("SprkInventoryTickets")!;
            foreach(var c in new[]{(25,50,250,6250),(0,50,250,12500),(50,50,250,0),(10,50,250,10000)}) {
                var gems=(int)ticket.GetMethod("Compensation")!.Invoke(null,new object[]{c.Item1,c.Item2,c.Item3})!;
                if(gems!=c.Item4)throw new InvalidOperationException("Ticket compensation quote regression.");
            }
            var prompt=(string)ticket.GetMethod("ConfirmationText")!.Invoke(null,new object[]{200,200,6250})!;
            if(!prompt.Contains("6,250 Rubies") || !prompt.Contains("expand your inventory by 200 slots"))throw new InvalidOperationException("Ticket confirmation does not disclose expansion and Rubies.");
            Console.WriteLine("PASS: partial/full ticket compensation, fitting ticket fallback and exact Ruby confirmation amounts.");
        } finally { context.Unload(); }
    }
    static void InstallInventoryExpansion(string root, bool stageOnly) {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var path = Path.Combine(managed, "Assembly-CSharp.dll");
        using var source=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.InventoryTickets.cs.txt")!;
        using var reader=new StreamReader(source);
        var references=Directory.GetFiles(managed,"*.dll").Where(p=>Path.GetFileName(p)!="SprkInventoryTickets.dll").Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkInventoryTickets",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},references,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        File.WriteAllBytes(Path.Combine(managed,"SprkInventoryTickets.dll.staged"),compiled.ToArray());
        using var helper=ModuleDefinition.ReadModule(new MemoryStream(compiled.ToArray()));
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using (var game = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver, InMemory = true })) {
            PatchInventoryExpansion(game.MainModule,helper);
            var getter = game.MainModule.GetType("NShared.InventoryExtendData").Methods.Single(m => m.Name == "get_EquipItemExtendCount");
            int count = getter.Body.Instructions.Count;
            var label=game.MainModule.GetType("NGame2.NUI.NWindow.ExtendInvenMessagePopup").Methods.Single(m=>m.Name=="SetLabel");
            var popupBefore=string.Join("\n",label.Body.Instructions);
            PatchInventoryExpansion(game.MainModule,helper);
            if (count != getter.Body.Instructions.Count || popupBefore!=string.Join("\n",label.Body.Instructions))
                throw new InvalidOperationException("Inventory expansion patch is not idempotent.");
            game.Write(path + ".inventory-expansion-staged");
        }
        VerifyInventoryExpansionFile(root, true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Inventory expansion staged. Close the game and local server before installation.");
        File.Copy(path, path + ".before-inventory-expansion-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(path + ".inventory-expansion-staged", path, true);
        var helperPath=Path.Combine(managed,"SprkInventoryTickets.dll");
        if(File.Exists(helperPath))File.Copy(helperPath,helperPath+".before-inventory-expansion-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(helperPath+".staged",helperPath,true);
        VerifyInventoryExpansionFile(root);
    }
}
