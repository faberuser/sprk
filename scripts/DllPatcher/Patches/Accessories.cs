using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static void PatchAccessories(ModuleDefinition game, ModuleDefinition helper)
    {
        MethodReference Hook(string name) => game.ImportReference(helper.GetType("SprkAccessories").Methods.Single(m=>m.Name==name));
        void Replace(MethodDefinition method,string hook,params int[] args) {
            method.Body=new MethodBody(method);var il=method.Body.GetILProcessor();
            foreach(var arg in args)il.Emit(OpCodes.Ldarg,arg);
            il.Emit(OpCodes.Call,Hook(hook));il.Emit(OpCodes.Ret);
        }
        void Tail(MethodDefinition method,string hook,Action<ILProcessor,Instruction> arguments) {
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkAccessories" && r.Name==hook))return;
            var il=method.Body.GetILProcessor();foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                // Branches to ret must execute the hook as well. Preserve the
                // original instruction as the first argument producer.
                ret.OpCode=OpCodes.Nop;ret.Operand=null;var end=il.Create(OpCodes.Ret);il.InsertAfter(ret,end);arguments(il,end);il.InsertBefore(end,il.Create(OpCodes.Call,Hook(hook)));
            }
        }
        foreach(var type in game.Types.Where(t=>t.Name.StartsWith("JM_") && t.Methods.Any(m=>m.Name=="EncodeJsonObject" && m.HasBody && m.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="get_AccessoryCostumeIndex1")))) {
            var parse=type.Methods.Single(m=>m.Name=="Parse" && m.Parameters.Count==2 && m.Parameters[0].ParameterType.FullName=="System.Collections.IDictionary");
            Tail(parse,"Read",(il,end)=>{il.InsertBefore(end,il.Create(OpCodes.Dup));il.InsertBefore(end,il.Create(OpCodes.Ldarg_0));});
            var encode=type.Methods.Single(m=>m.Name=="EncodeJsonObject");
            Tail(encode,"Write",(il,end)=>{il.InsertBefore(end,il.Create(OpCodes.Dup));il.InsertBefore(end,il.Create(OpCodes.Ldarg_0));});
        }
        var creature=game.GetType("NGameAsset.CreatureStock");var set=creature.Methods.Single(m=>m.Name=="SetCustomizeStock");
        set.Body=new MethodBody(set);var stockIl=set.Body.GetILProcessor();stockIl.Emit(OpCodes.Ldarg_0);
        stockIl.Emit(OpCodes.Ldflda,creature.Fields.Single(f=>f.Name=="CustomizeStockDictionary"));stockIl.Emit(OpCodes.Ldarg_1);stockIl.Emit(OpCodes.Call,Hook("SetStock"));stockIl.Emit(OpCodes.Ret);
        Replace(game.GetType("NGame2.N3D.RenderFacadeBuilder").Methods.Single(m=>m.Name=="ChangeAssetStockByAccessoryIndex" && m.Parameters.Count==2),"Build",0,1);
        Replace(game.GetType("NGame2.NUtil.NCostume.AccessoryCostume").Methods.Single(m=>m.Name=="GetCostumeState" && m.Parameters[1].ParameterType.Name=="AccessoryCostumeData"),"State",0,1,2);
        var customizing=game.GetType("NGame2.NUtil.NCostume.CustomizingHelper");
        Replace(customizing.Methods.Single(m=>m.Name=="CollectAccessoryCostume"),"Collect",0,1);
        var apply=customizing.Methods.Single(m=>m.Name=="SetHeroCostume");
        if(!apply.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="ApplyResult")) {
            var il=apply.Body.GetILProcessor();var first=apply.Body.Instructions[0];il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));il.InsertBefore(first,il.Create(OpCodes.Call,Hook("ApplyResult")));
        }
        Tail(customizing.Methods.Single(m=>m.Name=="OnResponse"),"AfterResponse",(_,_)=>{});
        var widget=game.GetType("NGame2.NUI.NWindow.NCustomizing.AccessoryCostumeViewWidget");
        Tail(widget.Methods.Single(m=>m.Name=="SelectCostume"),"Preview",(il,end)=>il.InsertBefore(end,il.Create(OpCodes.Ldarg_0)));
        Tail(widget.Methods.Single(m=>m.Name=="UpdateButtonState"),"UpdateButtons",(il,end)=>il.InsertBefore(end,il.Create(OpCodes.Ldarg_0)));
        Tail(widget.Methods.Single(m=>m.Name=="CreateCurrentAccessoryCostumeGetter"),"Getter",(il,end)=>il.InsertBefore(end,il.Create(OpCodes.Ldarg_0)));
        Tail(widget.Methods.Single(m=>m.Name.EndsWith(".SetCostumeTransform")),"EditPosition",(il,end)=>{il.InsertBefore(end,il.Create(OpCodes.Ldarg_0));il.InsertBefore(end,il.Create(OpCodes.Ldarg_1));});
        var unequip=widget.Methods.Single(m=>m.Name=="OnUnEquip");unequip.Body=new MethodBody(unequip);var ui=unequip.Body.GetILProcessor();
        ui.Emit(OpCodes.Ldarg_0);ui.Emit(OpCodes.Call,widget.Methods.Single(m=>m.Name=="OnEquip"));ui.Emit(OpCodes.Ret);
        var view=game.GetType("NGame2.NUI.NWindow.HeroCostumeView");
        var render=view.Methods.Single(m=>m.Name.EndsWith("IRenderFacadeController.ChangeRenderFacadeOnly"));
        var renderBase=render.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Single(m=>m.Name=="ChangeRenderFacadeOnly");
        render.Body=new MethodBody(render);var ri=render.Body.GetILProcessor();ri.Emit(OpCodes.Ldarg_0);ri.Emit(OpCodes.Ldarg_1);ri.Emit(OpCodes.Call,renderBase);ri.Emit(OpCodes.Ret);
        var extensions=game.GetType("Customizing_Extension_Method");
        // CostumeStock is a value type. Earlier versions passed its unboxed
        // return/argument to object hooks, invalidating native reset checks.
        // These scalar snapshots remain native; full render lists live on heroes.
        foreach(var name in new[]{"GetCostumeStock","IsEqualCostumeStock"}) {
            var method=extensions.Methods.Single(m=>m.Name==name);
            foreach(var call in method.Body.Instructions.Where(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkAccessories").ToArray()) {
                var first=call.Previous.Previous;var second=call.Previous;
                if(name=="GetCostumeStock" && first.OpCode!=OpCodes.Dup || name=="IsEqualCostumeStock" && first.OpCode!=OpCodes.Ldarg_0)
                    throw new Exception("Unexpected legacy costume stock hook shape");
                foreach(var instruction in new[]{first,second,call}) {instruction.OpCode=OpCodes.Nop;instruction.Operand=null;}
            }
        }
        foreach(var method in extensions.Methods.Where(m=>m.HasBody && (m.Name=="CreateCachedHeroInfo" || m.Name=="SetCostumeStorageSlotInfo" || m.Name=="CreatePlayerAvatarHeroInfo" && m.Parameters[0].ParameterType.Name=="CachedHeroInfo"))) {
            if(method.ReturnType.IsValueType)throw new Exception("Object appearance copy requires a reference-type return: "+method.FullName);
            int source=method.Name=="SetCostumeStorageSlotInfo" || method.Name=="CreatePlayerAvatarHeroInfo"?1:0;
            Tail(method,"Copy",(il,end)=>{il.InsertBefore(end,il.Create(OpCodes.Dup));il.InsertBefore(end,il.Create(OpCodes.Ldarg,source));});
        }
        foreach(var method in extensions.Methods.Where(m=>m.HasBody && (m.Name=="IsEqualCostumeInfo" || m.Name=="IsEqual" && m.Parameters[0].ParameterType.Name=="PlayerAvatarHeroInfo"))) {
            if(method.Parameters.Any(p=>p.ParameterType.IsValueType))throw new Exception("Object appearance equality requires reference-type parameters: "+method.FullName);
            Tail(method,"Equal",(il,end)=>{il.InsertBefore(end,il.Create(OpCodes.Ldarg_0));il.InsertBefore(end,il.Create(OpCodes.Ldarg_1));});
        }
        VerifyAccessories(game);
    }
    static void VerifyAccessories(ModuleDefinition game) {
        foreach(var name in new[]{"GetCostumeStock","IsEqualCostumeStock"}) {
            var method=game.GetType("Customizing_Extension_Method").Methods.Single(m=>m.Name==name);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkAccessories"))throw new Exception("Native value-type costume stock must not use object hooks");
            VerifyArenaResultStack(method);
        }
        foreach(var entry in new[]{("NGameAsset.CreatureStock","SetCustomizeStock","SetStock"),
            ("NGame2.NUtil.NCostume.CustomizingHelper","CollectAccessoryCostume","Collect"),
            ("NGame2.NUI.NWindow.NCustomizing.AccessoryCostumeViewWidget","SelectCostume","Preview"),
            ("NGame2.NUI.NWindow.NCustomizing.AccessoryCostumeViewWidget","UpdateButtonState","UpdateButtons")}) {
            var method=game.GetType(entry.Item1).Methods.Single(m=>m.Name==entry.Item2);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkAccessories" && r.Name==entry.Item3))throw new Exception("Missing multi-accessory hook "+entry);
        }
        foreach(var type in game.Types)foreach(var method in type.Methods.Where(m=>m.HasBody && m.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkAccessories")))VerifyArenaResultStack(method);
        Console.WriteLine("PASS: accessory JSON, individual render stocks, additive apply, preview and Remove button hooks.");
    }
    static void InstallAccessories(string root,bool stageOnly,bool verify=false) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data/Managed"));var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkAccessories.dll");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        if(verify) {VerifyAccessories(game.MainModule);TestAccessories(root);return;}
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Accessories.cs.txt")!;using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkAccessories",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new Exception(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        output.Position=0;using var helper=AssemblyDefinition.ReadAssembly(output);PatchAccessories(game.MainModule,helper.MainModule);
        var staged=dll+".accessories-staged";game.Write(staged);File.WriteAllBytes(helperPath+".staged",output.ToArray());
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new Exception("Accessories staged. Close game clients and workers before installation.");
        File.Copy(dll,dll+".before-accessories-"+DateTime.Now.ToString("yyyyMMdd-HHmmssfff"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);VerifyAccessories(game.MainModule);TestAccessories(root);
    }
    static void TestAccessories(string root) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data/Managed"));
        var identity=Guid.NewGuid().ToString("N");var nativeName="SPRK.Accessories.Native."+identity;var helperName="SPRK.Accessories.Helper."+identity;
        System.Reflection.Assembly? native=null,helper=null;
        System.ResolveEventHandler resolve=(_,e)=> {
            var name=new System.Reflection.AssemblyName(e.Name).Name;
            if(name==nativeName)return native;if(name==helperName)return helper;
            var path=Path.Combine(managed,name+".dll");return File.Exists(path)?System.Reflection.Assembly.LoadFrom(path):null;
        };
        AppDomain.CurrentDomain.AssemblyResolve+=resolve;
        try {
            using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
            using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"),new ReaderParameters{AssemblyResolver=resolver});
            using var bridge=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkAccessories.dll"),new ReaderParameters{AssemblyResolver=resolver});
            game.Name.Name=nativeName;bridge.Name.Name=helperName;
            foreach(var module in new[]{game.MainModule,bridge.MainModule})foreach(var reference in module.AssemblyReferences) {
                if(reference.Name=="Assembly-CSharp")reference.Name=nativeName;
                if(reference.Name=="SprkAccessories")reference.Name=helperName;
            }
            using var gb=new MemoryStream();game.Write(gb);native=System.Reflection.Assembly.Load(gb.ToArray());
            using var hb=new MemoryStream();bridge.Write(hb);helper=System.Reflection.Assembly.Load(hb.ToArray());
            var hooks=helper.GetType("SprkAccessories")!;
            object? Call(string name,params object?[] args)=>hooks.GetMethod(name)!.Invoke(null,args);
            var heroType=native.GetType("NShared.HeroInfo")!;var hero=Activator.CreateInstance(heroType)!;
            var entries=new System.Collections.ArrayList();
            for(int part=1;part<=4;part++)for(int n=1;n<=2;n++)entries.Add(new System.Collections.Hashtable {
                ["Index"]=part*100+n,["Part"]=part,["PositionInfo"]="{\"PositionX\":"+n+",\"Scale\":1}"});
            var json=new System.Collections.Hashtable {["HeroIndex"]=1,["SprkAccessories"]=entries};
            Call("Read",hero,json);
            if(((System.Collections.IList)Call("Entries",hero)!).Count!=8)throw new Exception("Same-part accessories lost on JSON read");
            var encoded=new System.Collections.Hashtable();Call("Write",encoded,hero);
            var copy=Activator.CreateInstance(heroType)!;Call("Read",copy,encoded);
            if((string)Call("Fingerprint",hero)! != (string)Call("Fingerprint",copy)!)throw new Exception("Accessory transforms lost on clone roundtrip");
            var cloneList=(System.Collections.IList)Call("Entries",copy)!;cloneList.RemoveAt(0);Call("Store",copy,cloneList);
            if(((System.Collections.IList)Call("Entries",hero)!).Count!=8 || ((System.Collections.IList)Call("Entries",copy)!).Count!=7)throw new Exception("Accessory clone aliases live list");
            Call("Read",copy,new System.Collections.Hashtable {["SprkAccessories"]=new System.Collections.ArrayList()});
            if(((System.Collections.IList)Call("Entries",copy)!).Count!=0)throw new Exception("Explicit empty list failed");
            // Exercise the patched native stock method, including per-ID removal
            // and part removal, rather than mirroring its implementation.
            var stockType=native.GetType("NGameAsset.CreatureStock")!;var stock=Activator.CreateInstance(stockType)!;
            var itemType=native.GetType("NGameAsset.CustomizeStock")!;var partType=native.GetType("NShared.AccessoryPartType")!;
            void Set(int id,int part) {var item=Activator.CreateInstance(itemType)!;itemType.GetField("AccessoryIndex")!.SetValue(item,id);itemType.GetField("Part")!.SetValue(item,Enum.ToObject(partType,part));stockType.GetMethod("SetCustomizeStock")!.Invoke(stock,new[]{item});}
            int Count()=>((System.Collections.ICollection)stockType.GetMethod("CustomizeStockValues")!.Invoke(stock,null)!).Count;
            Set(101,1);Set(102,1);Set(201,2);if(Count()!=3)throw new Exception("Native render stock still replaces same-part accessories");
            Set(102,1);if(Count()!=3)throw new Exception("Reapply duplicates accessory");
            Set(0,-101);if(Count()!=2)throw new Exception("Single removal removed another accessory");
            Set(0,1);if(Count()!=1)throw new Exception("Part removal failed");
            Set(501,5);Set(502,5);if(Count()!=2)throw new Exception("Unrequested parts became stackable");
            object Proxy(string name,Func<System.Reflection.MethodInfo,object?[]?,object?> handler) {
                var create=typeof(System.Reflection.DispatchProxy).GetMethods().Single(m=>m.Name=="Create" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length==2);
                var proxy=create.MakeGenericMethod(native.GetType(name)!,typeof(LobbyAccessoryProbe)).Invoke(null,null)!;
                ((LobbyAccessoryProbe)proxy).Handler=handler;return proxy;
            }
            var cachedType=native.GetType("NShared.CachedHeroInfo")!;
            var extensions=native.GetType("Customizing_Extension_Method")!;
            var getStock=extensions.GetMethod("GetCostumeStock")!;
            var equalStock=extensions.GetMethod("IsEqualCostumeStock")!;
            var resetHero=Activator.CreateInstance(cachedType)!;
            bool IsDefault(object value)=>(bool)value.GetType().GetMethod("IsDefaultCostume")!.Invoke(value,null)!;
            if(!IsDefault(getStock.Invoke(null,new[]{resetHero})!))throw new Exception("Empty hero must not require a cosmetic reset");
            for(int part=1;part<=6;part++) {
                var property=cachedType.GetProperty("AccessoryCostumeIndex"+part)!;
                property.SetValue(resetHero,part*100+2);
                var appearance=getStock.Invoke(null,new[]{resetHero})!;
                if(IsDefault(appearance) || !(bool)equalStock.Invoke(null,new[]{resetHero,appearance})!)throw new Exception("Remove All native eligibility/equality failed for accessory part "+part);
                property.SetValue(resetHero,0);
                if((bool)equalStock.Invoke(null,new[]{resetHero,appearance})!)throw new Exception("Native appearance snapshot aliases changed hero");
            }
            if(!IsDefault(getStock.Invoke(null,new[]{resetHero})!))throw new Exception("Cleared hero remains eligible for Remove All");
            Console.WriteLine("PASS: native Remove All stock return, reset eligibility for every accessory slot, snapshot equality/change detection and cleared state.");
            var equipped=Activator.CreateInstance(cachedType)!;var preview=Activator.CreateInstance(cachedType)!;
            Call("Copy",equipped,hero);Call("Copy",preview,hero);
            var trialType=helper.GetType("SprkAccessoryEntry")!;var trial=Activator.CreateInstance(trialType)!;
            trialType.GetField("Index")!.SetValue(trial,999);trialType.GetField("Part")!.SetValue(trial,Enum.ToObject(partType,2));
            var previewEntries=(System.Collections.IList)Call("Entries",preview)!;previewEntries.Add(trial);Call("Store",preview,previewEntries);
            var remaining=(System.Collections.IList)Call("Entries",equipped)!;remaining.RemoveAt(0);Call("Store",equipped,remaining);
            int refreshes=0;
            var controller=Proxy("NGame2.NUI.NWindow.NCustomizing.IRenderFacadeController",(m,_)=>m.Name switch {
                "GetHeroInfo"=>equipped,"GetLocalHeroInfo"=>preview,"ChangeRenderFacadeOnly"=>Refresh(),_=>throw new Exception(m.Name)});
            object? Refresh() {refreshes++;return null;}
            var widgetType=native.GetType("NGame2.NUI.NWindow.NCustomizing.AccessoryCostumeViewWidget")!;
            var widget=System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(widgetType);
            widgetType.GetField("_partType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.SetValue(widget,Enum.ToObject(partType,1));
            widgetType.BaseType!.GetField("_renderFacadeController",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.SetValue(widget,controller);
            Call("ShowEquipped",widget);
            if(refreshes!=1 || ((System.Collections.IList)Call("Entries",preview)!).Count!=8 ||
                !(bool)Call("Contains",preview,999)! || (bool)Call("Contains",preview,101)!)
                throw new Exception("Explicit removal must remove the viewed model and preserve other parts' previews");
            stock=Activator.CreateInstance(stockType)!;int changes=0;bool ready=false;
            var instances=new HashSet<int>();
            var instance=Proxy("NGameAsset.NCreature.ITransformInstnace",(_,_)=>throw new Exception("Unexpected transform mutation"));
            var open=Proxy("NGameAsset.ICreatureOpenData",(m,_)=>m.Name=="GetStock"?stock:throw new Exception(m.Name));
            int Id(object item)=>(int)itemType.GetField("AccessoryIndex")!.GetValue(item)!;
            var creature=Proxy("NGameAsset.ICreature",(m,args)=>m.Name switch {
                "GetCreatureOpenData"=>open,"GetCutomizeItemInstance"=>instances.Contains(Id(args![0]!))?instance:null,_=>throw new Exception(m.Name)});
            object? Change(object item) {
                changes++;stockType.GetMethod("SetCustomizeStock")!.Invoke(stock,new[]{item});
                int id=Id(item);if(id>0)instances.Add(id);else instances.Remove(-(int)itemType.GetField("Part")!.GetValue(item)!);return null;
            }
            var load=Proxy("NGameAsset.ICreatureLoad",(m,args)=>m.Name switch {
                "get_IsDone"=>ready,"get_IsValid"=>true,"get_Value"=>creature,
                "ChangeCustomizeStock"=>Change(args![0]!),_=>throw new Exception(m.Name)});
            object Item(int id) {var item=Activator.CreateInstance(itemType)!;itemType.GetField("Part")!.SetValue(item,Enum.ToObject(partType,1));itemType.GetField("AccessoryIndex")!.SetValue(item,id);itemType.GetField("Code")!.SetValue(item,"Accessory_"+id);return item;}
            bool Reconcile(params int[] ids) {var desired=Array.CreateInstance(itemType,ids.Length);for(int i=0;i<ids.Length;i++)desired.SetValue(Item(ids[i]),i);return (bool)Call("Reconcile",load,desired)!;}
            if(Reconcile(101,102)||changes!=0)throw new Exception("Accessory renderer applied before readiness");
            ready=true;
            if(!Reconcile(101,102)||Count()!=2||changes!=2||!Reconcile(101,102)||changes!=2)throw new Exception("Same-part live layers failed to converge");
            if(!Reconcile(102)||Count()!=1||changes!=3||!instances.Contains(102))throw new Exception("Live single removal disturbed retained accessory");
            instances.Clear();if(!Reconcile(102)||changes!=4)throw new Exception("Missing retained instance was not repaired");
            if(!Reconcile()||Count()!=0||changes!=5||!Reconcile()||changes!=5)throw new Exception("Live Remove All failed to converge");
            var before=game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
            PatchAccessories(game.MainModule,bridge.MainModule);
            var after=game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
            if(before!=after)throw new Exception("Multi-accessory patch is not idempotent");
            Console.WriteLine("PASS: eight accessories across four parts, transform roundtrip, independent clones, native stock stacking, deduplication, individual/part removal, live reconciliation/readiness/missing instances, other-slot limits and patch idempotence.");
        } finally {AppDomain.CurrentDomain.AssemblyResolve-=resolve;}
    }
}
