using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static void PatchShopFeaturedCatalog(ModuleDefinition module)
    {
        var type=module.GetType("NGame2.NUI.NComponent2.NPayShop.PayShopHotAndNewList");
        var method=type.Methods.Single(m=>m.Name=="GetDatas");
        if(method.Body.Instructions.Any(i=>Equals(i.Operand,"HotNew")))return;
        // The archived Hot & New page uses retired hard-coded product indices.
        // Read the server's featured category through the native product manager.
        var source=module.GetType("NGame2.NUI.NComponent2.NPayShop.PayShopProductList")
            .Methods.Single(m=>m.Name=="GetRealDataList");
        var calls=source.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        var global=calls.Single(m=>m.Name=="get_instance");
        var manager=calls.Single(m=>m.Name=="get_NewPayShopProductManager");
        var products=calls.Single(m=>m.Name=="GetProducts");
        var constructor=calls.Single(m=>m.Name==".ctor");
        var addRange=calls.Single(m=>m.Name=="AddRange");
        method.Body=new MethodBody(method){InitLocals=true};
        var list=new VariableDefinition(method.ReturnType);method.Body.Variables.Add(list);
        var il=method.Body.GetILProcessor();
        il.Emit(OpCodes.Newobj,constructor);il.Emit(OpCodes.Stloc,list);
        il.Emit(OpCodes.Ldloc,list);il.Emit(OpCodes.Call,global);il.Emit(OpCodes.Callvirt,manager);
        il.Emit(OpCodes.Ldstr,"HotNew");il.Emit(OpCodes.Callvirt,products);il.Emit(OpCodes.Callvirt,addRange);
        il.Emit(OpCodes.Ldloc,list);il.Emit(OpCodes.Ret);
        Console.WriteLine("Connected Hot & New to the server's featured shop catalog.");
    }

    static void InstallShopCatalog(string clientRoot, bool stageOnly)
    {
        var managed=Path.GetFullPath(Path.Combine(clientRoot,"King's Raid_Data","Managed"));
        var path=Path.Combine(managed,"Assembly-CSharp.dll");
        var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var assembly=AssemblyDefinition.ReadAssembly(path,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        PatchShopFeaturedCatalog(assembly.MainModule);
        var staged=path+".shop-catalog-staged";assembly.Write(staged);
        if(stageOnly){Console.WriteLine("Staged "+staged);return;}
        File.Copy(path,path+".before-shop-catalog-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        File.Copy(staged,path,true);
        Console.WriteLine("Installed focused shop catalog patch.");
    }
}
