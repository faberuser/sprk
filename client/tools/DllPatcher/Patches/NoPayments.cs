using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;
partial class Program {
    internal static bool IsPaymentAssembly(string name) =>
        name.StartsWith("UnityEngine.Purchasing",StringComparison.Ordinal) || name is "Purchasing.Common" or "UDP";

    static bool IsPaymentImplementation(TypeReference type) =>
        type.FullName.StartsWith("UnityEngine.Purchasing.",StringComparison.Ordinal) ||
        new[]{"NGame2.IAP.Unity.","NGame2.IAP.Steam.","NGame2.IAP.Apple2.","NGame2.IAP.OneStore.","NGame2.IAP.Samsung.",
            "NGame2.NUI.NWindow.NInAppPurchase."}.Any(prefix=>type.FullName.StartsWith(prefix,StringComparison.Ordinal)) ||
        type.FullName=="NGame2.IAP.IAPTestManager";

    static bool PaymentReference(TypeReference type) =>
        IsPaymentImplementation(type) || type.Scope is AssemblyNameReference scope && IsPaymentAssembly(scope.Name) ||
        type is GenericInstanceType generic && generic.GenericArguments.Any(PaymentReference) ||
        type is TypeSpecification spec && PaymentReference(spec.ElementType);

    static bool PaymentCall(MethodReference call) => PaymentReference(call.DeclaringType) ||
        PaymentReference(call.ReturnType) || call.Parameters.Any(p=>PaymentReference(p.ParameterType)) ||
        call is GenericInstanceMethod generic && generic.GenericArguments.Any(PaymentReference);

    static void EmptyPaymentResult(MethodDefinition method) {
        MethodReference? baseCtor=null;
        if(method.IsConstructor && !method.IsStatic && method.HasBody)
            baseCtor=method.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>()
                .FirstOrDefault(m=>m.Name==".ctor" && m.Parameters.Count==0 && m.DeclaringType.FullName!=method.DeclaringType.FullName);
        method.Body=new MethodBody(method){InitLocals=true};var il=method.Body.GetILProcessor();
        if(baseCtor!=null){il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Call,baseCtor);}
        if(method.ReturnType.FullName=="System.Collections.IEnumerator") {
            il.Emit(OpCodes.Ldc_I4_0);il.Emit(OpCodes.Newarr,method.Module.TypeSystem.Object);
            var enumerable=new TypeReference("System.Collections","IEnumerable",method.Module,method.Module.TypeSystem.CoreLibrary);
            il.Emit(OpCodes.Callvirt,new MethodReference("GetEnumerator",method.ReturnType,enumerable){HasThis=true});
        } else if(method.ReturnType.IsValueType && method.ReturnType.MetadataType!=MetadataType.Void) {
            var value=new VariableDefinition(method.ReturnType);method.Body.Variables.Add(value);
            il.Emit(OpCodes.Ldloca,value);il.Emit(OpCodes.Initobj,method.ReturnType);il.Emit(OpCodes.Ldloc,value);
        } else if(method.ReturnType.MetadataType!=MetadataType.Void)il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Ret);
    }

    static void RemovePaymentReferences(ModuleDefinition module) {
        var wrapper=module.GetType("NGame2.IAP.IAPWrapper");
        if(wrapper!=null) {
            var setter=wrapper.Methods.Single(m=>m.Name=="set_IAPManager");
            foreach(var method in wrapper.Methods.Where(m=>m.Name.StartsWith("Set",StringComparison.Ordinal) || m.Name=="ForceSetIAPManager")) {
                method.Body=new MethodBody(method);var il=method.Body.GetILProcessor();
                il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldnull);il.Emit(OpCodes.Call,setter);il.Emit(OpCodes.Ret);
            }
            EmptyPaymentResult(wrapper.Methods.Single(m=>m.Name=="GetSkuInfo"));
            EmptyPaymentResult(wrapper.Methods.Single(m=>m.Name=="get_IsReady"));
        }
        // Billing is optional: finish its login state immediately without initializing a store.
        var state=module.GetType("NGame2.NLogin.NState.StateBase_InAppBilling");
        if(state!=null) {
            var run=state.Methods.Single(m=>m.Name=="coRun");EmptyPaymentResult(run);
            var il=run.Body.GetILProcessor();var first=run.Body.Instructions[0];
            int success=Convert.ToInt32(state.NestedTypes.Single(t=>t.Name=="InAppBillingState").Fields.Single(f=>f.Name=="Success").Constant);
            foreach(var instruction in new[]{Instruction.Create(OpCodes.Ldarg_0),Instruction.Create(OpCodes.Ldc_I4,success),
                Instruction.Create(OpCodes.Stfld,state.Fields.Single(f=>f.Name=="_checkState")),Instruction.Create(OpCodes.Ldc_I4_1),
                Instruction.Create(OpCodes.Call,state.Methods.Single(m=>m.Name=="set_IsReady"))})il.InsertBefore(first,instruction);
            foreach(var nested in state.NestedTypes.Where(t=>t.Name.StartsWith("<coRun>",StringComparison.Ordinal)).ToArray())state.NestedTypes.Remove(nested);
        }
        var manager=module.GetType("NGame2.NUI.NManager.PayShopManagement");
        if(manager!=null) {
            foreach(var method in manager.Methods.Where(m=>m.Name is "coRequestInAppPurchase" or "RequestBuyPayShopProduct_InAppPurchase"))EmptyPaymentResult(method);
            foreach(var nested in manager.NestedTypes.Where(t=>t.Name.StartsWith("<coRequestInAppPurchase>",StringComparison.Ordinal)).ToArray())manager.NestedTypes.Remove(nested);
        }
        // Preserve native pricing/stock checks for in-game currencies, reject cash products first.
        var helper=module.GetType("NGame2.NUI.NHelper.NPayShop.PayShopHelper");
        if(helper!=null) {
            var product=module.GetType("NShared.NewPayShopProductInfo");
            var cashCheck=helper.Methods.FirstOrDefault(m=>m.Name=="SprkIsCashProduct");
            if(cashCheck==null) {
                cashCheck=new MethodDefinition("SprkIsCashProduct",MethodAttributes.Public|MethodAttributes.Static,module.TypeSystem.Boolean);
                cashCheck.Parameters.Add(new ParameterDefinition("product",ParameterAttributes.None,product));helper.Methods.Add(cashCheck);
                var il=cashCheck.Body.GetILProcessor();var yes=Instruction.Create(OpCodes.Ldc_I4_1);
                var isEmpty=new MethodReference("IsNullOrEmpty",module.TypeSystem.Boolean,module.TypeSystem.String);
                isEmpty.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
                foreach(var name in new[]{"GoogleId","AppleId","OneStoreId","UDPStoreId"}) {
                    il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Callvirt,product.Methods.Single(m=>m.Name=="get_"+name));
                    il.Emit(OpCodes.Call,isEmpty);il.Emit(OpCodes.Brfalse,yes);
                }
                il.Emit(OpCodes.Ldc_I4_0);il.Emit(OpCodes.Ret);il.Append(yes);il.Emit(OpCodes.Ret);
            }
            var buyable=helper.Methods.Single(m=>m.Name=="IsBuyable");
            if(!buyable.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.FullName==cashCheck.FullName)) {
                var il=buyable.Body.GetILProcessor();var first=buyable.Body.Instructions[0];
                foreach(var instruction in new[]{Instruction.Create(OpCodes.Ldarg_0),Instruction.Create(OpCodes.Brfalse,first),
                    Instruction.Create(OpCodes.Ldarg_0),Instruction.Create(OpCodes.Call,cashCheck),
                    Instruction.Create(OpCodes.Brfalse,first),Instruction.Create(OpCodes.Ldc_I4_0),Instruction.Create(OpCodes.Ret)})il.InsertBefore(first,instruction);
            }
            if(manager!=null)foreach(var request in manager.Methods.Where(m=>m.Name=="RequestBuyPayShopProduct" && m.HasBody)) {
                if(request.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.FullName==cashCheck.FullName))continue;
                var il=request.Body.GetILProcessor();var first=request.Body.Instructions[0];
                foreach(var instruction in new[]{Instruction.Create(OpCodes.Ldarg_1),Instruction.Create(OpCodes.Call,cashCheck),
                    Instruction.Create(OpCodes.Brfalse,first),Instruction.Create(OpCodes.Ret)})il.InsertBefore(first,instruction);
            }
        }
        var price=module.GetType("NGame2.NUI.NComponent2.NPayShop.PayShopPriceComponent");
        if(price!=null) {
            var cash=price.Methods.Single(m=>m.Name=="SetCashPrice");cash.Body=new MethodBody(cash);
            var il=cash.Body.GetILProcessor();il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Call,price.Methods.Single(m=>m.Name=="SetBuyable"));il.Emit(OpCodes.Ret);
        }
        var removedMethods=module.GetTypes().Where(t=>!IsPaymentImplementation(t)).SelectMany(t=>t.Methods)
            .Where(m=>PaymentReference(m.ReturnType) || m.Parameters.Any(p=>PaymentReference(p.ParameterType))).ToHashSet();
        foreach(var type in module.GetTypes().Where(t=>!IsPaymentImplementation(t)).ToArray()) {
            var removedFields=type.Fields.Where(f=>PaymentReference(f.FieldType)).ToHashSet();
            foreach(var method in type.Methods.Where(m=>!removedMethods.Contains(m) && m.HasBody)) {
                bool touches=method.Body.Variables.Any(v=>PaymentReference(v.VariableType)) || method.Body.Instructions.Any(i=>i.Operand switch {
                    MethodReference call=>PaymentCall(call),
                    FieldReference field=>PaymentReference(field.DeclaringType) || PaymentReference(field.FieldType),
                    TypeReference operand=>PaymentReference(operand),_=>false});
                if(touches)EmptyPaymentResult(method);
            }
            foreach(var field in removedFields)type.Fields.Remove(field);
            foreach(var property in type.Properties.Where(p=>PaymentReference(p.PropertyType) ||
                p.GetMethod!=null && removedMethods.Contains(p.GetMethod) || p.SetMethod!=null && removedMethods.Contains(p.SetMethod)).ToArray())type.Properties.Remove(property);
            foreach(var method in type.Methods.Where(removedMethods.Contains).ToArray())type.Methods.Remove(method);
        }
        foreach(var type in module.Types.Where(IsPaymentImplementation).ToArray())module.Types.Remove(type);
        foreach(var reference in module.AssemblyReferences.Where(r=>IsPaymentAssembly(r.Name)).ToArray())module.AssemblyReferences.Remove(reference);
        using var bytes=new MemoryStream();module.Write(bytes);bytes.Position=0;using var check=ModuleDefinition.ReadModule(bytes);
        var remaining=check.GetTypeReferences().Where(PaymentReference).Select(t=>t.FullName).ToArray();
        if(remaining.Length>0)throw new InvalidOperationException("Payment references remain: "+string.Join(", ",remaining));
    }

    static void StagePaymentRemoval(string clientRoot,string output) {
        var data=Path.GetFullPath(Path.Combine(clientRoot,"King's Raid_Data"));var stage=Path.GetFullPath(output);
        if(stage.StartsWith(data+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Stage outside the client directory.");
        Directory.CreateDirectory(Path.Combine(stage,"Managed"));
        var removedFiles=new List<string>();var modifiedFiles=new List<string>();var removedTypes=new List<string>();var removedAssemblies=new List<string>();
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(Path.Combine(data,"Managed"));
        foreach(var path in Directory.GetFiles(Path.Combine(data,"Managed"),"*.dll")) {
            using var assembly=AssemblyDefinition.ReadAssembly(path,new ReaderParameters{AssemblyResolver=resolver});
            if(IsPaymentAssembly(assembly.Name.Name)) {
                removedAssemblies.Add(assembly.Name.Name);removedFiles.Add(Path.GetRelativePath(data,path));
                foreach(var symbol in new[]{Path.ChangeExtension(path,".pdb"),path+".mdb"})if(File.Exists(symbol))removedFiles.Add(Path.GetRelativePath(data,symbol));
            } else if(assembly.Name.Name=="Assembly-CSharp") {
                removedTypes.AddRange(assembly.MainModule.GetTypes().Where(IsPaymentImplementation).Select(t=>"Assembly-CSharp:"+t.FullName));
                RemovePaymentReferences(assembly.MainModule);assembly.Write(Path.Combine(stage,"Managed","Assembly-CSharp.dll"));modifiedFiles.Add("Managed/Assembly-CSharp.dll");
            } else if(assembly.MainModule.AssemblyReferences.Any(r=>IsPaymentAssembly(r.Name)))throw new InvalidOperationException("Payment dependency in retained assembly: "+assembly.Name.Name);
        }
        foreach(var name in new[]{"RuntimeInitializeOnLoads.json","ScriptingAssemblies.json"}) {
            var json=JsonNode.Parse(File.ReadAllText(Path.Combine(data,name)))!;
            if(name=="RuntimeInitializeOnLoads.json") {
                var hooks=json["root"]!.AsArray();foreach(var hook in hooks.ToArray())
                    if(IsPaymentAssembly(hook!["assemblyName"]!.GetValue<string>()))hooks.Remove(hook);
            } else {
                var names=json["names"]!.AsArray();var types=json["types"]!.AsArray();
                for(int i=names.Count-1;i>=0;i--)if(IsPaymentAssembly(Path.GetFileNameWithoutExtension(names[i]!.GetValue<string>()))) {names.RemoveAt(i);types.RemoveAt(i);}
            }
            File.WriteAllText(Path.Combine(stage,name),json.ToJsonString());modifiedFiles.Add(name);
        }
        var removedComponentTypes=removedTypes.Where(t=>t.StartsWith("Assembly-CSharp:NGame2.NUI.NWindow.NInAppPurchase.",StringComparison.Ordinal)).ToArray();
        File.WriteAllText(Path.Combine(stage,"manifest.json"),JsonSerializer.Serialize(new {removedFiles,modifiedFiles,removedTypes,removedAssemblies,removedComponentTypes},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Staged {removedFiles.Count} payment SDK file removals at {stage}");
    }
}
