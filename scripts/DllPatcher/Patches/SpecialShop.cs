using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static (MethodDefinition Method, string Helper)[] SpecialShopHooks(ModuleDefinition module) => new[] {
        (module.GetType("NGame2.NUI.NComponent2.NPayShop.PayShopProductList").Methods.Single(m => m.Name == "ProductSort"), "Sort"),
        (module.GetType("NGame2.NUI.NWindow.NPayShop.PayShopProductInfoPopup").Methods.Single(m => m.Name == "Init"), "Init"),
        (module.GetType("NGame2.NUI.NWindow.NPayShop.PayShopBaseInfoPopup").Methods.Single(m => m.Name == "OnClickBuy"), "BeforeBuy"),
        (module.GetType("NGame2.NUI.NWindow.NPayShop.PayShop").Methods.Single(m => m.Name == "UpdateProduct"), "AfterPurchase"),
        (module.GetType("NGame2.NUI.NWindow.NPayShop.PayShop").Methods.Single(m => m.Name == "UpdateDerived"), "Tick"),
        (module.GetType("NShared.BuyPayShopProduct.Request").Methods.Single(m => m.Name == "set_Index"), "TakeQuantity"),
        (module.GetType("JM_NShared_BuyPayShopProduct_Request").Methods.Single(m => m.Name == "EncodeJsonObject"), "Encode")
    };
    static void PatchSpecialShop(ModuleDefinition module, ModuleDefinition helper) {
        var request = module.GetType("NShared.BuyPayShopProduct.Request");
        var quantity = request.Fields.SingleOrDefault(f => f.Name == "SprkQuantity");
        if (quantity == null) {
            quantity = new FieldDefinition("SprkQuantity", FieldAttributes.Public, module.TypeSystem.Int32);
            request.Fields.Add(quantity);
        }
        foreach (var (method, name) in SpecialShopHooks(module)) {
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkSpecialShop" && r.Name == name)) continue;
            var call = module.ImportReference(helper.GetType("SprkSpecialShop").Methods.Single(m => m.Name == name));
            var il = method.Body.GetILProcessor();
            if (name == "BeforeBuy") {
                var first = method.Body.Instructions[0];
                il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
                il.InsertBefore(first, il.Create(OpCodes.Call, call));
                il.InsertBefore(first, il.Create(OpCodes.Brtrue, first));
                il.InsertBefore(first, il.Create(OpCodes.Ret));
                continue;
            }
            foreach (var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray()) {
                var instructions = new List<Instruction>();
                if (name == "TakeQuantity") {
                    instructions.Add(il.Create(OpCodes.Ldarg_0));
                    instructions.Add(il.Create(OpCodes.Ldarg_1));
                    instructions.Add(il.Create(OpCodes.Call, call));
                    instructions.Add(il.Create(OpCodes.Stfld, quantity));
                } else {
                    instructions.Add(il.Create(OpCodes.Ldarg_0));
                    if (name is "Init" or "AfterPurchase") instructions.Add(il.Create(OpCodes.Ldarg_1));
                    if (name == "Encode") instructions.Add(il.Create(OpCodes.Ldfld, quantity));
                    instructions.Add(il.Create(OpCodes.Call, call));
                }
                // Keep branches to the original ret pointing at the hook.
                ret.OpCode = instructions[0].OpCode; ret.Operand = instructions[0].Operand;
                var cursor = ret;
                foreach (var instruction in instructions.Skip(1).Append(il.Create(OpCodes.Ret))) {
                    il.InsertAfter(cursor, instruction); cursor = instruction;
                }
            }
        }
        VerifySpecialShop(module);
        Console.WriteLine("Patched free offer ordering, daily reset refresh, quantity UI and per-request batch count.");
    }
    static void VerifySpecialShop(ModuleDefinition module) {
        foreach (var (method, name) in SpecialShopHooks(module))
            if (!method.Body.Instructions.Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkSpecialShop" && r.Name == name))
                throw new InvalidOperationException("Missing special shop hook: " + name);
        var encoder = module.GetType("JM_NShared_BuyPayShopProduct_Request").Methods.Single(m => m.Name == "EncodeJsonObject");
        if (!encoder.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference f && f.Name == "SprkQuantity"))
            throw new InvalidOperationException("Batch encoder does not read the bound request quantity.");
        var setter = module.GetType("NShared.BuyPayShopProduct.Request").Methods.Single(m => m.Name == "set_Index");
        if (!setter.Body.Instructions.Any(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "SprkQuantity"))
            throw new InvalidOperationException("Batch quantity is not bound to the request.");
    }
    static void TestSpecialShop(byte[] bytes) {
        var type = System.Reflection.Assembly.Load(bytes).GetType("SprkSpecialShop")!;
        int Rank(bool free, int purchased, int limit) => (int)type.GetMethod("Rank")!.Invoke(null, new object[] { free, purchased, limit })!;
        if (Rank(true, 0, 1) != 0 || Rank(false, 0, 0) != 1 || Rank(true, 1, 1) != 2 || Rank(true, 100, 1) != 2)
            throw new InvalidOperationException("Free shop sorting regression.");
        int Max(long balance, long price, int purchased, int limit, int grant) => (int)type.GetMethod("Maximum")!.Invoke(null, new object[] { balance, price, purchased, limit, grant })!;
        if (Max(10000, 100, 0, 0, 999) != 100 || Max(0, 0, 0, 1, 999) != 1 || Max(0, 0, 1, 1, 999) != 0
            || Max(99, 100, 0, 0, 999) != 0 || Max(long.MaxValue, 1, 0, 0, 999) != 999
            || Max(long.MaxValue, int.MaxValue, 0, 0, 999) != 1 || Max(10000, 1, 2, 5, 999) != 3 || Max(10000, 1, 0, 0, 2) != 2)
            throw new InvalidOperationException("Batch shop maximum regression.");
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        type.GetField("pendingIndex", flags)!.SetValue(null, 7);
        type.GetField("pendingQuantity", flags)!.SetValue(null, 17);
        var take = type.GetMethod("TakeQuantity")!;
        if ((int)take.Invoke(null, new object[] { 7L })! != 17 || (int)take.Invoke(null, new object[] { 7L })! != 1)
            throw new InvalidOperationException("Batch request count is not consumed exactly once.");
        var encode = type.GetMethod("Encode")!;
        foreach (int value in new[] { 0, 1, 17, 999 }) {
            var data = new Dictionary<string, object>();
            encode.Invoke(null, new object[] { data, value });
            if (!Equals(data["Quantity"], Math.Max(1, value).ToString())) throw new InvalidOperationException("Batch encoding regression.");
        }
        Console.WriteLine("PASS: special shop rank, stock, affordability, overflow and quantity encoding checks.");
    }
    static void TestSpecialShopBalances(string managed, byte[] bytes) {
        // Execute the shipped helper against the real UserManager/UserInfo types.
        // A separate context avoids loading Unity into the patcher's own context.
        var context = new System.Runtime.Loader.AssemblyLoadContext("ShopBalances-" + Guid.NewGuid(), isCollectible: true);
        context.Resolving += (load, name) => {
            var path = Path.Combine(managed, name.Name + ".dll");
            if (!File.Exists(path)) return null;
            using var assemblyBytes = new MemoryStream(File.ReadAllBytes(path));
            return load.LoadFromStream(assemblyBytes);
        };
        try {
            using var stream = new MemoryStream(bytes);
            var helper = context.LoadFromStream(stream).GetType("SprkSpecialShop")!;
            var balance = helper.GetMethod("Balance")!;
            var userType = balance.GetParameters()[0].ParameterType;
            var enumType = balance.GetParameters()[1].ParameterType;
            var user = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(userType);
            var infoField = userType.GetField("_userInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var info = Activator.CreateInstance(infoField.FieldType)!;
            infoField.SetValue(user, info);
            foreach (var (name, amount) in new[] { ("Gold", 853410822L), ("Gem", 549990L), ("PayGem", 2L), ("Mileage", 35200L), ("LuaPoint", 71L) }) {
                var property = info.GetType().GetProperty(name)!;
                property.SetValue(info, Convert.ChangeType(amount, property.PropertyType));
            }
            foreach (var (currency, expected) in new[] { ("Gold", 853410822L), ("Gem", 549992L), ("Mileage", 35200L), ("LuaPoint", 71L) }) {
                long actual = (long)balance.Invoke(null, new[] { user, Enum.Parse(enumType, currency) })!;
                if (actual != expected) throw new InvalidOperationException("Incorrect native shop balance for " + currency);
            }
            var maximum = helper.GetMethod("Maximum")!;
            var gems = balance.Invoke(null, new[] { user, Enum.Parse(enumType, "Gem") })!;
            if ((int)maximum.Invoke(null, new object[] { gems, 5000L, 0, 0, 999 })! != 109)
                throw new InvalidOperationException("Screenshot regression: 549,992 rubies must allow 109 packages at 5,000 each.");
            if ((int)maximum.Invoke(null,new object[]{918282532L,10000000L,0,0,999})! != 91 ||
                (int)maximum.Invoke(null,new object[]{395852L,1000L,0,0,999})! != 395)
                throw new InvalidOperationException("Ruby/Gold exchange affordability regression.");
            var eligibility=helper.GetMethod("CanSelectQuantity")!;
            var infoType=eligibility.GetParameters()[0].ParameterType;
            var productType=infoType.GetProperty("ProductType")!;
            foreach(var name in new[]{"Item","Gem","Gold","DailyGem","Booster","Attendance","Package"}) {
                var product=Activator.CreateInstance(infoType)!;
                productType.SetValue(product,Enum.Parse(productType.PropertyType,name));
                bool Eligible()=>(bool)eligibility.Invoke(null,new[]{product})!;
                bool expected=name is "Item" or "Gem" or "Gold";
                if(Eligible()!=expected)throw new InvalidOperationException("Shop quantity eligibility regression: "+name);
                if(!expected)continue;
                foreach(var store in new[]{"AppleId","GoogleId","OneStoreId","UDPStoreId"})
                    infoType.GetProperty(store)!.SetValue(product,"cash-offer");
                if(Eligible())throw new InvalidOperationException("Cash offer must remain single-purchase.");
                foreach(var store in new[]{"AppleId","GoogleId","OneStoreId","UDPStoreId"})
                    infoType.GetProperty(store)!.SetValue(product,"");
                infoType.GetProperty("DurationDay")!.SetValue(product,30);
                if(Eligible())throw new InvalidOperationException("Subscription must remain single-purchase.");
                infoType.GetProperty("DurationDay")!.SetValue(product,0);
                infoType.GetProperty("StepProductGroup")!.SetValue(product,new[]{1,2});
                if(Eligible())throw new InvalidOperationException("Step offer must remain single-purchase.");
            }
            Console.WriteLine("PASS: Item/Gem/Gold selector eligibility, cash/subscription/step exclusions and both exchange affordability cases.");
            Console.WriteLine("PASS: real UserInfo balances for gold, free+paid rubies, mileage and Lua; screenshot affordability regression.");
        } finally { context.Unload(); }
    }
    static void VerifySpecialShopFile(string root, bool staged = false) {
        var managed = Path.Combine(root, "King's Raid_Data", "Managed");
        using var game = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll" + (staged ? ".special-shop-staged" : "")));
        VerifySpecialShop(game.MainModule);
        using var helper = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "SprkSpecialShop.dll" + (staged ? ".staged" : "")));
        var calls = helper.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).SelectMany(m => m.Body.Instructions).Select(i => i.Operand).OfType<MethodReference>().Select(m => m.Name).ToArray();
        foreach (var name in new[] { "OrderBy", "CreateList", "ReCreateList", "get_PurchaseCountResetTime", "SetGamePrice", "Maximum", "Balance", "CanSelectQuantity", "get_TotalGem", "get_Gold", "get_Mileage", "get_LuaPoint" }) {
            if (!calls.Contains(name)) throw new InvalidOperationException("Incomplete special shop helper: " + name);
        }
        var fields = helper.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody)
            .SelectMany(m => m.Body.Instructions).Where(i => i.OpCode == OpCodes.Stfld)
            .Select(i => i.Operand).OfType<FieldReference>().Select(f => f.Name).ToArray();
        foreach (var field in new[] { "label", "validation", "minimum", "maximum" })
            if (!fields.Contains(field)) throw new InvalidOperationException("Missing quantity input field: " + field);
        TestSpecialShop(File.ReadAllBytes(Path.Combine(managed, "SprkSpecialShop.dll" + (staged ? ".staged" : ""))));
        TestSpecialShopBalances(managed, File.ReadAllBytes(Path.Combine(managed, "SprkSpecialShop.dll" + (staged ? ".staged" : ""))));
        Console.WriteLine("Verified special shop installed/staged hooks and helper.");
    }
    static void InstallSpecialShop(string root, bool stageOnly) {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, "SprkSpecialShop.dll");
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.SpecialShop.cs.txt")!;
        using var reader = new StreamReader(resource);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => p != helperPath).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkSpecialShop", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var compiled = new MemoryStream(); var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        compiled.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(compiled);
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        PatchSpecialShop(game.MainModule, helper.MainModule);
        int count = game.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count);
        PatchSpecialShop(game.MainModule, helper.MainModule);
        if (count != game.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count))
            throw new InvalidOperationException("Special shop patch is not idempotent.");
        var staged = dll + ".special-shop-staged";
        game.Write(staged); File.WriteAllBytes(helperPath + ".staged", compiled.ToArray());
        VerifySpecialShopFile(root, true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Special shop patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll, dll + ".before-special-shop-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(staged, dll, true); File.Move(helperPath + ".staged", helperPath, true);
        VerifySpecialShopFile(root);
    }
}
