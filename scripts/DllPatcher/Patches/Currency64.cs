using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void InstallCurrency64(string root, bool stageOnly, string? report = null)
    {
        var managed = Path.Combine(root, "King's Raid_Data", "Managed");
        using var migration = new Currency64Migration(managed);
        migration.Apply();
        if(report!=null)File.WriteAllLines(report,migration.VerificationMethods());
        migration.Write(stageOnly);
        if(!stageOnly) { VerifyCurrency64File(root); TestCurrency64(root); }
        Console.WriteLine("Currency64: migrated currency storage, serializers and consumers.");
    }

    static HashSet<string> Currency64Names(ModuleDefinition module)
    {
        var names=module.GetType("NShared.CurrencyType").Fields.Where(f=>f.IsStatic).Select(f=>f.Name).ToHashSet();
        names.UnionWith(new[]{"PayGem","RoyalPoint","EventDungeonDailyPoint","DailyAccFriendshipPoint","DailyAccGuildPoint","SoulStoneMileage","GlitterPoint","KarmaPoint","OrdealPoint"});
        names.Remove("TeamExp");names.Remove("None");return names;
    }

    static void VerifyCurrency64File(string root)
    {
        using var a=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"));
        var module=a.MainModule;
        var names=Currency64Names(module);
        foreach(var tn in new[]{"NShared.UserInfo","NGame2.NAccount.UserManager"})
            foreach(var p in module.GetType(tn).Properties.Where(p=>names.Contains(p.Name)))
                if(p.PropertyType.MetadataType!=MetadataType.Int64 || p.GetMethod?.ReturnType.MetadataType!=MetadataType.Int64)
                    throw new InvalidOperationException("Currency64 property mismatch: "+tn+"."+p.Name);
        foreach(var name in new[]{"AvatarHeroIndex","LastChapterIndex","LastDungeonIndex","TeamLevel","TeamExp"})
            if(module.GetType("NShared.UserInfo").Properties.Single(p=>p.Name==name).PropertyType.MetadataType!=MetadataType.Int32)
                throw new InvalidOperationException("Currency64 changed a non-currency field: "+name);
        foreach(var tn in new[]{"NShared.CurrencyResultInfo2","NShared.CurrencyResultInfo3","NShared.StaminaResultInfo"})
            foreach(var p in module.GetType(tn).Properties.Where(p=>p.Name is "AddValue" or "NewValue"))
                if(p.PropertyType.MetadataType!=MetadataType.Int64)throw new InvalidOperationException("Currency64 result mismatch: "+tn+"."+p.Name);
        Console.WriteLine("Currency64 storage and result signatures verified.");
    }

    static void TestCurrency64(string root)
    {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var context=new System.Runtime.Loader.AssemblyLoadContext("Currency64-"+Guid.NewGuid(),true);
        context.Resolving+=(load,name)=> {
            var p=Path.Combine(managed,name.Name+".dll");
            return File.Exists(p)?load.LoadFromStream(new MemoryStream(File.ReadAllBytes(p))):null;
        };
        try {
            var a=context.LoadFromStream(new MemoryStream(File.ReadAllBytes(Path.Combine(managed,"Assembly-CSharp.dll"))));
            var info=a.GetType("NShared.UserInfo")!;var jm=a.GetType("JM_NShared_UserInfo")!;
            var parse=jm.GetMethod("Parse",new[]{typeof(string)})!;
            var userType=a.GetType("NGame2.NAccount.UserManager")!;
            var user=System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(userType);
            var field=userType.GetField("_userInfo",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
            var names=Enum.GetNames(a.GetType("NShared.CurrencyType")!).Concat(new[]{"PayGem","RoyalPoint","EventDungeonDailyPoint"}).Where(n=>n!="TeamExp"&&n!="None").ToHashSet();
            foreach(long amount in new[]{2_147_483_648L,9_007_199_254_740_993L,long.MaxValue}) {
                var payload="{"+string.Join(",",info.GetProperties().Where(p=>names.Contains(p.Name)).Select(p=>"\""+p.Name+"\":"+amount))+"}";
                var value=parse.Invoke(null,new object[]{payload})!;
                foreach(var p in info.GetProperties().Where(p=>names.Contains(p.Name)))
                if(p.PropertyType!=typeof(long)||(long)p.GetValue(value)! !=amount)throw new InvalidOperationException("Currency64 JSON lost "+p.Name);
                info.GetProperty("PayGem")!.SetValue(value,0L);field.SetValue(user,value);
                if((long)userType.GetProperty("TotalGem")!.GetValue(user)! !=amount)throw new InvalidOperationException("Currency64 ruby total lost precision");
                var setter=jm.GetMethods().Single(m=>m.Name=="SetField").MakeGenericMethod(typeof(long));
                if(!(bool)setter.Invoke(null,new object[]{value,"Gem",amount})! || (long)info.GetProperty("Gem")!.GetValue(value)! !=amount)
                    throw new InvalidOperationException("Currency64 generated setter failed");
                var stamina=a.GetType("NShared.StaminaData")!;var data=Activator.CreateInstance(stamina)!;
                stamina.GetProperty("AttributeName")!.SetValue(data,"Stamina");
                if((long)userType.GetMethod("GetStamina",new[]{stamina})!.Invoke(user,new[]{data})! !=amount)
                    throw new InvalidOperationException("Currency64 stamina accessor narrowed balance");
            }
            var helperPath=Path.Combine(managed,"SprkSpecialShop.dll");
            if(File.Exists(helperPath))TestSpecialShopBalances(managed,File.ReadAllBytes(helperPath));
            Console.WriteLine("PASS: native JSON, generated setters, ruby totals, stamina and shop getters at 2^31, above 2^53 and Int64.MaxValue.");
        } finally {context.Unload();}
    }

    // Widen the compiled currency dependency graph, rather than changing every
    // Int32 in the game (indices, enums, array lengths and loop counters remain Int32).
    sealed class Currency64Migration : IDisposable
    {
        sealed class Resolver : DefaultAssemblyResolver { public void Register(AssemblyDefinition a)=>RegisterAssembly(a); }
        readonly Resolver resolver=new();
        readonly string managed;
        readonly List<AssemblyDefinition> assemblies = new();
        readonly Dictionary<MethodReference, MethodDefinition> methods = new();
        readonly Dictionary<FieldReference, FieldDefinition> fields = new();
        readonly HashSet<object> wide = new();
        readonly HashSet<MethodDefinition> touched = new();
        readonly Dictionary<MethodDefinition,Dictionary<Instruction,Val[]>> outputs=new();
        HashSet<string>? currencyNames;
        bool changed;
        ModuleDefinition Main => assemblies[0].MainModule;
        TypeReference Long => Main.TypeSystem.Int64;
        static bool Int(TypeReference? t) => t?.MetadataType == MetadataType.Int32;
        static bool Stack32(TypeReference? t) {
            if(Int(t))return true;
            if(t?.MetadataType!=MetadataType.ValueType)return false;
            try{return t.Resolve()?.IsEnum==true;}catch(AssemblyResolutionException){return false;}
        }
        static bool I64(TypeReference? t) => t?.MetadataType == MetadataType.Int64;
        static bool Same(TypeReference? a, TypeReference? b) => a?.FullName == b?.FullName;
        static bool Numeric(TypeReference? t) => Int(t) || I64(t);
        static string Key(MethodReference m) => m.FullName;

        bool IsCountOrIdentity(object source)
        {
            var ownerType=source switch {MethodDefinition m=>m.DeclaringType,FieldDefinition f=>f.DeclaringType,ParameterDefinition p when p.Method is MethodDefinition m=>m.DeclaringType,_=>null};
            if(ownerType?.Name=="SprkTestPrefs" || ownerType?.Name.StartsWith("LocalStorageInt")==true)return true;
            if(source is ParameterDefinition pp && pp.Method is MethodReference pm && pm.Name=="GetGloryPointTitle")return true;
            var name=source switch {FieldDefinition f=>f.Name.Trim('<','>').Split('>')[0],MethodDefinition m=>m.Name.Replace("get_","").Replace("set_",""),ParameterDefinition p=>p.Name,_=>""};
            if((currencyNames??=Currency64Names(Main)).Contains(name) || name=="StaminaCount")return false;
            if(source is ParameterDefinition parameter && parameter.Method is MethodDefinition owner && (owner.Name.StartsWith("set_")||owner.Name.StartsWith("Set")) && IsCountOrIdentity(owner))return true;
            return new[]{"Index","Indices","Level","Type","Exp","Slot","Star","Rank","Score","Seed","Length","Size","Width","Height","Depth","RechargeCount"}.Any(s=>name.Contains(s,StringComparison.OrdinalIgnoreCase))
                || name.EndsWith("Id",StringComparison.Ordinal) || name.Equals("id",StringComparison.OrdinalIgnoreCase) || name.EndsWith("Uid",StringComparison.Ordinal);
        }

        public Currency64Migration(string managed)
        {
            this.managed = managed;
            resolver.AddSearchDirectory(managed);
            foreach (var path in new[] { Path.Combine(managed,"Assembly-CSharp.dll") }.Concat(Directory.GetFiles(managed,"Sprk*.dll")))
            {
                var a=AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver=resolver, InMemory=true });
                assemblies.Add(a);resolver.Register(a);
            }
            var definitions = assemblies.SelectMany(a=>a.MainModule.GetTypes()).ToArray();
            var md = definitions.SelectMany(t=>t.Methods).GroupBy(Key).ToDictionary(g=>g.Key,g=>g.First());
            var fd = definitions.SelectMany(t=>t.Fields).GroupBy(f=>f.FullName).ToDictionary(g=>g.Key,g=>g.First());
            foreach (var m in definitions.SelectMany(t=>t.Methods).Where(m=>m.HasBody))
                foreach (var i in m.Body.Instructions) {
                    if (i.Operand is MethodReference r) {
                        var e = r is GenericInstanceMethod g ? g.ElementMethod : r;
                        if (md.TryGetValue(Key(e),out var d)) { methods[r]=d; methods[e]=d; }
                    }
                    if (i.Operand is FieldReference f && fd.TryGetValue(f.FullName,out var d2)) fields[f]=d2;
                }
            foreach (var d in md.Values) methods[d]=d;
            foreach (var d in fd.Values) fields[d]=d;
        }

        void Promote(object source)
        {
            if(IsCountOrIdentity(source))return;
            var existing=source switch {
                VariableDefinition v=>v.VariableType,ParameterDefinition p=>p.ParameterType,
                FieldDefinition f=>f.FieldType,MethodDefinition m=>m.ReturnType,_=>null};
            if(I64(existing) || existing is ByReferenceType b64 && I64(b64.ElementType)) {
                if(wide.Add(source))changed=true;
                return;
            }
            switch (source) {
                case VariableDefinition v when Int(v.VariableType): v.VariableType=Long; break;
                case ParameterDefinition p when Int(p.ParameterType): p.ParameterType=Long; break;
                case ParameterDefinition p when p.ParameterType is ByReferenceType b && Int(b.ElementType): p.ParameterType=new ByReferenceType(Long); break;
                case FieldDefinition f when Int(f.FieldType): f.FieldType=Long; break;
                case MethodDefinition m when Int(m.ReturnType): m.ReturnType=Long; break;
                default: return;
            }
            wide.Add(source); changed=true;
        }

        void Property(TypeDefinition t, string name)
        {
            var p=t.Properties.SingleOrDefault(p=>p.Name==name);
            if(p==null) return;
            if (Int(p.PropertyType)) p.PropertyType=Long;
            if (p.GetMethod!=null) { Promote(p.GetMethod); wide.Add(p.GetMethod); }
            if (p.SetMethod!=null) { Promote(p.SetMethod.Parameters[0]); wide.Add(p.SetMethod.Parameters[0]); }
            var f=t.Fields.SingleOrDefault(f=>f.Name=="<"+name+">k__BackingField");
            if(f!=null) { Promote(f); wide.Add(f); }
        }

        GenericInstanceMethod LongGeneric(GenericInstanceMethod source)
        {
            // Cecil interns MemberRefs; mutate a new MethodSpec so To<int> and
            // fire<int> used by unrelated fields/counters retain their types.
            var copy=new GenericInstanceMethod(source.ElementMethod);
            foreach(var a in source.GenericArguments)copy.GenericArguments.Add(Int(a)?Long:a);
            if(methods.TryGetValue(source,out var d))methods[copy]=d;
            return copy;
        }

        void Sync()
        {
            foreach(var (r,d) in methods) {
                if (r is GenericInstanceMethod) continue;
                r.ReturnType=d.ReturnType;
                for(int n=0;n<r.Parameters.Count;n++) r.Parameters[n].ParameterType=d.Parameters[n].ParameterType;
            }
            foreach(var (r,d) in fields) r.FieldType=d.FieldType;
            foreach(var t in assemblies.SelectMany(a=>a.MainModule.GetTypes()))
                foreach(var p in t.Properties)
                    if(p.GetMethod!=null && I64(p.GetMethod.ReturnType) && Int(p.PropertyType)) p.PropertyType=Long;
        }

        void Seed()
        {
            var names=Currency64Names(Main);
            foreach(var typeName in new[]{"NShared.UserInfo","NGame2.NAccount.UserManager","NShared.PlayerMiscInfo","NShared.PlayerGuildPointInfo"}) {
                var t=Main.GetType(typeName); if(t==null) continue;
                foreach(var name in names) Property(t,name);
                foreach(var m in t.Methods.Where(m=>m.Name.StartsWith("Set") && names.Contains(m.Name[3..])))
                    foreach(var p in m.Parameters.Where(p=>Int(p.ParameterType))) Promote(p);
            }
            foreach(var (type,props) in new Dictionary<string,string[]> {
                ["NShared.PlayerCurrencyInfo"]=new[]{"Amount","DailyAmount"},
                ["NShared.CurrencyResultInfo2"]=new[]{"AddValue","NewValue"},
                ["NShared.ICurrencyResultInfo"]=new[]{"AddValue","NewValue"},
                ["NShared.CurrencyResultInfo3"]=new[]{"AddValue","NewValue","Value1","Value2","Value3","Value4"},
                ["NShared.StaminaResultInfo"]=new[]{"AddValue","NewValue"},
                ["NShared.GemResultInfo"]=new[]{"AddValue","NewValue","NewSysGem","NewPayGem"},
                ["NShared.GoldResultInfo"]=new[]{"AddValue","NewValue"},
                ["NShared.NMessage.NProtocol.CurrencyInfoChanged"]=new[]{"Amount"},
                ["NShared.NMessage.NProtocol.StaminaInfoChanged"]=new[]{"StaminaCount"},
                ["NGame2.NAccount.StaminaInfo"]=new[]{"CurValue"},
            }) {var t=Main.GetType(type);if(t!=null)foreach(var name in props)Property(t,name);}
            var um=Main.GetType("NGame2.NAccount.UserManager");
            foreach(var m in um.Methods.Where(m=>m.Name=="GetStamina")) {
                Promote(m);
                foreach(var v in m.Body.Variables.Where(v=>Int(v.VariableType))) Promote(v);
                foreach(var i in m.Body.Instructions)
                    if(i.Operand is GenericInstanceMethod g && g.Name=="CustomUserInfoParseField") i.Operand=LongGeneric(g);
            }
            var setter=um.Methods.Single(m=>m.Name=="SetStamina"); Promote(setter.Parameters[1]);
            foreach(var i in setter.Body.Instructions)
                if(i.Operand is GenericInstanceMethod g && g.Name=="CustomUserInfoSetField") i.Operand=LongGeneric(g);
            foreach(var m in um.Methods.Where(m=>m.Name.Contains("ApplyCurrency")))
                foreach(var i in m.Body.Instructions.Where(i=>i.OpCode==OpCodes.Conv_I4)) i.OpCode=OpCodes.Conv_I8;
            var si=Main.GetType("NGame2.NAccount.StaminaInfo");
            var func=(GenericInstanceType)si.Fields.Single(f=>f.Name=="_curValueFunc").FieldType;
            var lf=new GenericInstanceType(func.ElementType);lf.GenericArguments.Add(Long);
            si.Fields.Single(f=>f.Name=="_curValueFunc").FieldType=lf;
            si.Methods.Single(m=>m.IsConstructor).Parameters[1].ParameterType=lf;
            foreach(var i in si.Methods.Single(m=>m.Name=="get_CurValue").Body.Instructions)
                if(i.Operand is GenericInstanceMethod g)i.Operand=LongGeneric(g);
        }

        // Generated JSON readers must parse Int64 before invoking the new setters;
        // merely adding conv.i8 after ParseValueInt would already have lost data.
        void Serializers()
        {
            foreach(var m in Main.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody && m.DeclaringType.Name.StartsWith("JM_"))) {
                foreach(var i in m.Body.Instructions) {
                    if(i.Operand is not MethodReference r || !methods.TryGetValue(r,out var d) || !d.Name.StartsWith("set_") || d.Parameters.Count!=1 || !wide.Contains(d.Parameters[0]))continue;
                    var before=i.Previous;
                    if(before?.Operand is GenericInstanceMethod g && g.Name=="To")before.Operand=LongGeneric(g);
                    else if(before?.Operand is MethodReference parse && parse.Name=="ParseValueInt") {
                        var p=Main.GetType("NShared.BaseJsonMarshaler").Methods.Single(x=>x.Name=="ParseValueLong" && x.Parameters.Count==parse.Parameters.Count);
                        before.Operand=p;
                    }
                }
            }
        }

        record Val(TypeReference? Type, bool Wide=false, object? Origin=null, MethodReference? Function=null);
        void RequireWide(Val v)
        {
            if(!Int(v.Type))return;
            if(v.Origin is VariableDefinition variable) {Promote(variable);return;}
            if(v.Origin is Instruction i && i.OpCode.Code is Code.Add or Code.Sub or Code.Mul or Code.Div or Code.Rem or Code.Conv_I4 or Code.Conv_Ovf_I4 or Code.Conv_Ovf_I4_Un)
                if(wide.Add(i))changed=true;
        }
        TypeReference? Actual(TypeReference? t, MethodReference? call=null)
        {
            if(t is GenericParameter g && call!=null) {
                if(g.Type==GenericParameterType.Method && call is GenericInstanceMethod gm)return gm.GenericArguments[g.Position];
                if(g.Type==GenericParameterType.Type && call.DeclaringType is GenericInstanceType gt)return gt.GenericArguments[g.Position];
            }
            if(t is ByReferenceType b)return new ByReferenceType(Actual(b.ElementType,call)!);
            if(t is GenericInstanceType gt2) {
                var result=new GenericInstanceType(gt2.ElementType);
                foreach(var a in gt2.GenericArguments)result.GenericArguments.Add(Actual(a,call)!);
                return result;
            }
            return t;
        }
        static int Local(Instruction i) => i.OpCode.Code switch {
            Code.Ldloc_0 or Code.Stloc_0=>0,Code.Ldloc_1 or Code.Stloc_1=>1,Code.Ldloc_2 or Code.Stloc_2=>2,Code.Ldloc_3 or Code.Stloc_3=>3,
            _=>((VariableDefinition)i.Operand).Index };
        ParameterDefinition? Arg(MethodDefinition m,Instruction i) {
            var n=i.OpCode.Code switch {Code.Ldarg_0=>0,Code.Ldarg_1=>1,Code.Ldarg_2=>2,Code.Ldarg_3=>3,_=>-1};
            if(n<0)return (ParameterDefinition)i.Operand;
            return m.HasThis ? n==0 ? null : m.Parameters[n-1] : m.Parameters[n];
        }

        Dictionary<Instruction,Val[]> Analyze(MethodDefinition m, bool mutate)
        {
            var states=new Dictionary<Instruction,Val[]>();var pending=new Queue<Instruction>();
            var outgoing=new Dictionary<Instruction,Val[]>();outputs[m]=outgoing;
            void Join(Instruction? i,Val[] s) {
                if(i==null)return;
                if(!states.TryGetValue(i,out var old)){states[i]=s;pending.Enqueue(i);return;}
                if(old.Length!=s.Length)throw new InvalidOperationException("Currency64 stack height: "+m.FullName+" "+i);
                bool dirty=false;var merged=old.ToArray();
                for(int n=0;n<s.Length;n++) {
                    var a=old[n];var b=s[n];
                    var type=I64(a.Type)||I64(b.Type) ? (Numeric(a.Type)&&Numeric(b.Type)?Long:a.Type??b.Type) : a.Type??b.Type;
                    var v=new Val(type,a.Wide||b.Wide,Equals(a.Origin,b.Origin)?a.Origin:null,Equals(a.Function,b.Function)?a.Function:null);
                    if(!Equals(a,v)){merged[n]=v;dirty=true;}
                }
                if(dirty){states[i]=merged;pending.Enqueue(i);}
            }
            Join(m.Body.Instructions.FirstOrDefault(),Array.Empty<Val>());
            foreach(var h in m.Body.ExceptionHandlers) {
                Join(h.HandlerStart,h.HandlerType==ExceptionHandlerType.Catch||h.HandlerType==ExceptionHandlerType.Filter ? new[]{new Val(h.CatchType??Main.TypeSystem.Object)}:Array.Empty<Val>());
                if(h.FilterStart!=null)Join(h.FilterStart,new[]{new Val(Main.TypeSystem.Object)});
            }
            int visits=0;
            while(pending.Count>0) {
                if(++visits>m.Body.Instructions.Count*100)throw new InvalidOperationException("Currency64 flow did not converge: "+m);
                var i=pending.Dequeue();var stack=states[i].ToList();
                Val Pop(){if(stack.Count==0)throw new InvalidOperationException("Currency64 underflow: "+m+" "+i);var v=stack[^1];stack.RemoveAt(stack.Count-1);return v;}
                void Push(TypeReference? t,bool w=false,object? o=null,MethodReference? f=null)=>stack.Add(new Val(t,w,o,f));
                var code=i.OpCode.Code;
                if(code is Code.Ldarg or Code.Ldarg_S or Code.Ldarg_0 or Code.Ldarg_1 or Code.Ldarg_2 or Code.Ldarg_3 or Code.Ldarga or Code.Ldarga_S) {
                    var p=Arg(m,i);var t=p?.ParameterType??m.DeclaringType;
                    Push(code is Code.Ldarga or Code.Ldarga_S ? new ByReferenceType(t):t,p!=null&&wide.Contains(p),p);
                } else if(code is Code.Starg or Code.Starg_S) {var v=Pop();if(mutate&&v.Wide)Promote(i.Operand);}
                else if(code is Code.Ldloc or Code.Ldloc_S or Code.Ldloc_0 or Code.Ldloc_1 or Code.Ldloc_2 or Code.Ldloc_3 or Code.Ldloca or Code.Ldloca_S) {
                    var v=m.Body.Variables[Local(i)];Push(code is Code.Ldloca or Code.Ldloca_S?new ByReferenceType(v.VariableType):v.VariableType,wide.Contains(v),v);
                } else if(code is Code.Stloc or Code.Stloc_S or Code.Stloc_0 or Code.Stloc_1 or Code.Stloc_2 or Code.Stloc_3) {
                    var v=Pop();var target=m.Body.Variables[Local(i)];if(mutate&&v.Wide) {
                        Promote(target);
                        if(v.Type is GenericInstanceType && target.VariableType is GenericInstanceType && !Same(v.Type,target.VariableType)) {target.VariableType=v.Type;wide.Add(target);changed=true;}
                    }
                    if(mutate&&wide.Contains(target)&&I64(target.VariableType))RequireWide(v);
                } else if(code is Code.Ldfld or Code.Ldsfld or Code.Ldflda or Code.Ldsflda) {
                    if(code is Code.Ldfld or Code.Ldflda)Pop();var r=(FieldReference)i.Operand;
                    var f=fields.GetValueOrDefault(r);var t=f?.FieldType??r.FieldType;
                    if(t is GenericParameter gp && r.DeclaringType is GenericInstanceType gt)t=gt.GenericArguments[gp.Position];
                    Push(code is Code.Ldflda or Code.Ldsflda?new ByReferenceType(t):t,f!=null&&wide.Contains(f),f);
                } else if(code is Code.Stfld or Code.Stsfld) {
                    var v=Pop();if(code==Code.Stfld)Pop();var r=(FieldReference)i.Operand;
                    if(mutate&&v.Wide&&fields.TryGetValue(r,out var f)) {
                        Promote(f);
                        if(v.Type is GenericInstanceType && f.FieldType is GenericInstanceType && !Same(v.Type,f.FieldType)){f.FieldType=v.Type;wide.Add(f);changed=true;}
                    }
                } else if(code is Code.Call or Code.Callvirt or Code.Newobj) {
                    var r=(MethodReference)i.Operand;var args=new Val[r.Parameters.Count];for(int n=args.Length-1;n>=0;n--)args[n]=Pop();
                    Val? self=r.HasThis&&code!=Code.Newobj?Pop():null;
                    methods.TryGetValue(r,out var target);
                    if(mutate) {
                        for(int n=0;n<args.Length;n++) {
                            var expect=Actual(r.Parameters[n].ParameterType,r);var v=args[n];
                            if(I64(expect) && (v.Wide || target!=null&&wide.Contains(target.Parameters[n])))RequireWide(v);
                            if(expect is ByReferenceType eb && I64(eb.ElementType) && v.Type is ByReferenceType vb && Int(vb.ElementType) && v.Origin!=null)Promote(v.Origin);
                            if(v.Wide && target!=null && !target.Parameters[n].ParameterType.ContainsGenericParameter && Numeric(expect))Promote(target.Parameters[n]);
                            if(v.Wide && target!=null && !target.Parameters[n].ParameterType.ContainsGenericParameter && expect is ByReferenceType b && Int(b.ElementType))Promote(target.Parameters[n]);
                            if(v.Wide && target!=null && !target.Parameters[n].ParameterType.ContainsGenericParameter && v.Type is GenericInstanceType && expect is GenericInstanceType && !Same(v.Type,expect)) {
                                target.Parameters[n].ParameterType=v.Type;wide.Add(target.Parameters[n]);changed=true;
                            }
                        }
                        if(self?.Type is ByReferenceType sb && I64(sb.ElementType) && r.DeclaringType.MetadataType==MetadataType.Int32) {
                            var replacement=new MethodReference(r.Name,r.ReturnType,Long){HasThis=r.HasThis};foreach(var p in r.Parameters)replacement.Parameters.Add(new ParameterDefinition(p.ParameterType));i.Operand=r=replacement;changed=true;
                        }
                        if(target==null && args.Any(v=>v.Wide) && r.DeclaringType.FullName is "System.Math" or "UnityEngine.Mathf" && r.Name is "Max" or "Min" or "Abs" && r.Parameters.Any(p=>Int(p.ParameterType))) {
                            var mathType=r.DeclaringType.FullName=="System.Math"?r.DeclaringType:Main.GetTypeReferences().First(t=>t.FullName=="System.Math");
                            var replacement=new MethodReference(r.Name,Int(r.ReturnType)?Long:r.ReturnType,mathType){HasThis=r.HasThis};
                            foreach(var p in r.Parameters)replacement.Parameters.Add(new ParameterDefinition(Int(p.ParameterType)?Long:p.ParameterType));i.Operand=r=replacement;changed=true;
                        }
                        if(code==Code.Newobj && r.DeclaringType is GenericInstanceType gt && gt.ElementType.FullName=="System.Func`1" && args.LastOrDefault()?.Function is MethodReference fn && methods.TryGetValue(fn,out var fm) && I64(fm.ReturnType) && Int(gt.GenericArguments[0])) {
                            var gt2=new GenericInstanceType(gt.ElementType);gt2.GenericArguments.Add(Long);
                            var replacement=new MethodReference(r.Name,r.ReturnType,gt2){HasThis=true};foreach(var p in r.Parameters)replacement.Parameters.Add(new ParameterDefinition(p.ParameterType));i.Operand=r=replacement;changed=true;
                        }
                        if(self?.Wide==true && self.Type is GenericInstanceType sg && r.DeclaringType is GenericInstanceType rg && sg.ElementType.FullName=="System.Func`1" && sg.ElementType.FullName==rg.ElementType.FullName && !Same(sg,rg)) {
                            var replacement=new MethodReference(r.Name,r.ReturnType,sg){HasThis=r.HasThis};foreach(var p in r.Parameters)replacement.Parameters.Add(new ParameterDefinition(p.ParameterType));i.Operand=r=replacement;changed=true;
                        }
                    }
                    var result=code==Code.Newobj?r.DeclaringType:Actual(target?.ReturnType??r.ReturnType,r);
                    var numericResult=target==null&&Numeric(result)&&args.Any(v=>v.Wide)&&r.Name is not ("CompareTo" or "Compare" or "Sign" or "GetHashCode");
                    if(code==Code.Newobj || result?.MetadataType!=MetadataType.Void)Push(result,(target!=null&&wide.Contains(target)) || numericResult || (code==Code.Newobj&&r.DeclaringType is GenericInstanceType gt3&&gt3.GenericArguments.Any(I64)),null);
                } else if(code is Code.Ldftn or Code.Ldvirtftn) {if(code==Code.Ldvirtftn)Pop();Push(Main.TypeSystem.IntPtr,false,null,(MethodReference)i.Operand);}
                else if(code==Code.Ret) {if(m.ReturnType.MetadataType!=MetadataType.Void){var v=Pop();if(mutate&&v.Wide)Promote(m);if(mutate&&wide.Contains(m)&&I64(m.ReturnType))RequireWide(v);}}
                else if(code==Code.Dup){var v=Pop();stack.Add(v);stack.Add(v);}
                else if(code==Code.Pop)Pop();
                else if(code is Code.Box) {var v=Pop();if(mutate&&v.Wide&&Int((TypeReference)i.Operand)){i.Operand=Long;changed=true;}Push(Main.TypeSystem.Object);}
                else if(code is Code.Conv_I4 or Code.Conv_Ovf_I4 or Code.Conv_Ovf_I4_Un) {
                    // This helper explicitly bounds the selectable item count to
                    // 999. Its final cast is a count, not a currency narrowing.
                    var countCast=m.DeclaringType.Name=="SprkSpecialShop"&&m.Name=="Maximum";
                    var v=Pop();if(mutate&&(v.Wide||wide.Contains(i))&&!countCast){i.OpCode=OpCodes.Conv_I8;changed=true;Push(Long,true,i);}else Push(Main.TypeSystem.Int32,false,i);
                } else if(code is Code.Conv_I8 or Code.Conv_U8 or Code.Conv_Ovf_I8 or Code.Conv_Ovf_I8_Un){var v=Pop();Push(Long,v.Wide);}
                else if(code is Code.Add or Code.Sub or Code.Mul or Code.Div or Code.Rem or Code.Add_Ovf or Code.Sub_Ovf or Code.Mul_Ovf or Code.Add_Ovf_Un or Code.Sub_Ovf_Un or Code.Mul_Ovf_Un or Code.Div_Un or Code.Rem_Un or Code.And or Code.Or or Code.Xor) {
                    var b=Pop();var a=Pop();var forced=wide.Contains(i);
                    if(mutate&&forced){RequireWide(a);RequireWide(b);}
                    if(mutate&&(forced||a.Wide||b.Wide)) {
                        var check=code switch {Code.Add=>OpCodes.Add_Ovf,Code.Sub=>OpCodes.Sub_Ovf,Code.Mul=>OpCodes.Mul_Ovf,_=>i.OpCode};
                        if(check!=i.OpCode){i.OpCode=check;changed=true;}
                    }
                    Push(forced||I64(a.Type)||I64(b.Type)?Long:a.Type,forced||a.Wide||b.Wide,i);
                } else if(code is Code.Shl or Code.Shr or Code.Shr_Un){Pop();var a=Pop();stack.Add(a);}
                else if(code is Code.Neg or Code.Not){var a=Pop();stack.Add(a);}
                else if(code is Code.Ceq or Code.Cgt or Code.Cgt_Un or Code.Clt or Code.Clt_Un){Pop();Pop();Push(Main.TypeSystem.Int32);}
                else if(code is Code.Stind_I4 or Code.Stind_I8 or Code.Stobj) {
                    var v=Pop();var dest=Pop();if(mutate&&v.Wide&&dest.Origin!=null)Promote(dest.Origin);
                    var destType=dest.Origin switch {ParameterDefinition p=>p.ParameterType,VariableDefinition d=>new ByReferenceType(d.VariableType),_=>dest.Type};
                    if(mutate && destType is ByReferenceType b&&I64(b.ElementType) && code==Code.Stind_I4){i.OpCode=OpCodes.Stind_I8;changed=true;}
                } else if(code is Code.Ldind_I4 or Code.Ldind_I8 or Code.Ldobj) {
                    var v=Pop();var t=(v.Type as ByReferenceType)?.ElementType??(i.Operand as TypeReference)??Main.TypeSystem.Int32;
                    if(mutate&&code==Code.Ldind_I4&&I64(t)){i.OpCode=OpCodes.Ldind_I8;changed=true;}Push(t,v.Wide);
                } else if(code is Code.Ldelem_Ref or Code.Ldelem_Any or Code.Ldelem_I4 or Code.Ldelem_I8 or Code.Ldelem_R4 or Code.Ldelem_R8 or Code.Ldelema) {
                    Pop();var array=Pop();var element=(array.Type as ArrayType)?.ElementType??i.Operand as TypeReference??Main.TypeSystem.Object;
                    Push(code==Code.Ldelema?new ByReferenceType(element):element);
                } else {
                    var pops=i.OpCode.StackBehaviourPop;
                    var count=pops is StackBehaviour.Pop0 or StackBehaviour.PopAll?0:pops==StackBehaviour.Varpop?0:pops.ToString().Split('_').Length;
                    for(int n=0;n<count;n++)Pop();
                    if(i.OpCode.StackBehaviourPush!=StackBehaviour.Push0) {
                        TypeReference? t=code switch {
                            Code.Ldc_I8=>Long,Code.Ldc_R4=>Main.TypeSystem.Single,Code.Ldc_R8=>Main.TypeSystem.Double,Code.Ldstr=>Main.TypeSystem.String,
                            Code.Ldnull=>null,Code.Castclass or Code.Isinst or Code.Unbox_Any=>(TypeReference)i.Operand,
                            Code.Newarr=>new ArrayType((TypeReference)i.Operand),Code.Conv_R4=>Main.TypeSystem.Single,Code.Conv_R8 or Code.Conv_R_Un=>Main.TypeSystem.Double,
                            _=>Main.TypeSystem.Int32};
                        Push(t);
                    }
                }
                if(code is Code.Leave or Code.Leave_S)stack.Clear();
                var output=stack.ToArray();
                outgoing[i]=output;
                if(i.Operand is Instruction branch)Join(branch,output);
                if(i.Operand is Instruction[] branches)foreach(var branch2 in branches)Join(branch2,output);
                if(i.OpCode.FlowControl is not (FlowControl.Branch or FlowControl.Return or FlowControl.Throw))Join(i.Next,output);
            }
            return states;
        }

        public void Apply()
        {
            Seed();Sync();Serializers();
            var all=assemblies.SelectMany(a=>a.MainModule.GetTypes()).SelectMany(t=>t.Methods).Where(m=>m.HasBody).ToArray();
            for(int pass=0;pass<80;pass++) {
                changed=false;Sync();
                foreach(var m in all) {
                    bool relevant=touched.Contains(m)||wide.Contains(m)||m.Parameters.Any(wide.Contains)||m.Body.Instructions.Any(i=>i.Operand is MethodReference r&&methods.TryGetValue(r,out var d)&&(wide.Contains(d)||d.Parameters.Any(wide.Contains)) || i.Operand is FieldReference f&&fields.TryGetValue(f,out var fd)&&wide.Contains(fd));
                    if(!relevant)continue;
                    touched.Add(m);Analyze(m,true);
                }
                Console.WriteLine($"Currency64 pass {pass}: {touched.Count} methods, changed={changed}");
                if(!changed)break;
                if(pass==79)throw new InvalidOperationException("Currency64 signatures failed to converge");
            }
            Sync();Serializers();
            foreach(var m in touched) {
                for(int pass=0;pass<6;pass++) {
                    var states=Analyze(m,false);
                    if(RepairEdges(m,states,outputs[m]))continue;
                    Repair(m,states);break;
                }
            }
            foreach(var m in touched) CheckWidths(m,Analyze(m,false));
            Console.WriteLine($"Currency64 widened {wide.Count} definitions; repaired {touched.Count} methods.");
        }

        public IEnumerable<string> VerificationMethods() => touched.Select(m=>
            System.Text.RegularExpressions.Regex.Escape(m.DeclaringType.FullName.Split('/')[0])).Distinct().OrderBy(s=>s);

        void CheckWidths(MethodDefinition m,Dictionary<Instruction,Val[]> states)
        {
            void Check(Instruction i,TypeReference? actual,TypeReference? expected) {
                if(Numeric(actual)&&Numeric(expected)&&!Same(actual,expected))
                    throw new InvalidOperationException($"Currency64 width mismatch in {m}: {i}, {actual} -> {expected}");
                if(actual is ByReferenceType ab&&expected is ByReferenceType eb)Check(i,ab.ElementType,eb.ElementType);
            }
            foreach(var (i,stack) in states) {
                if(i.Operand is MethodReference r && i.OpCode.Code is Code.Call or Code.Callvirt or Code.Newobj)
                    for(int n=0;n<r.Parameters.Count;n++)Check(i,stack[stack.Length-r.Parameters.Count+n].Type,Actual(r.Parameters[n].ParameterType,r));
                else if(i.OpCode==OpCodes.Ret&&m.ReturnType.MetadataType!=MetadataType.Void)Check(i,stack[^1].Type,m.ReturnType);
                else if(i.OpCode.Code is Code.Stloc or Code.Stloc_S or Code.Stloc_0 or Code.Stloc_1 or Code.Stloc_2 or Code.Stloc_3)Check(i,stack[^1].Type,m.Body.Variables[Local(i)].VariableType);
                else if(i.OpCode.Code is Code.Stfld or Code.Stsfld)Check(i,stack[^1].Type,((FieldReference)i.Operand).FieldType);
            }
        }

        // Normalize operand widths by spilling the consumer's operands. Keeping
        // the old instruction as a nop anchor preserves branch/exception targets.
        void ConvertOperands(MethodDefinition m,Instruction i,Val[] values,TypeReference?[] expected)
        {
            bool RefChange(int n)=>values[n].Type is ByReferenceType a&&expected[n] is ByReferenceType b&&Numeric(a.ElementType)&&Numeric(b.ElementType)&&!Same(a.ElementType,b.ElementType);
            if(!values.Select((v,n)=>(v,n)).Any(x=>Stack32(x.v.Type)&&I64(expected[x.n])||I64(x.v.Type)&&Stack32(expected[x.n])||RefChange(x.n)))return;
            var fullCount=values.Length;
            var skip=Enumerable.Range(0,values.Length).First(n=>Stack32(values[n].Type)&&I64(expected[n])||I64(values[n].Type)&&Stack32(expected[n])||RefChange(n));
            values=values.Skip(skip).ToArray();expected=expected.Skip(skip).ToArray();
            var il=m.Body.GetILProcessor();var op=i.OpCode;var operand=i.Operand;
            i.OpCode=OpCodes.Nop;i.Operand=null;var cursor=i;
            void Emit(Instruction n){il.InsertAfter(cursor,n);cursor=n;}
            var vars=values.Select((v,n)=>new VariableDefinition(v.Type??expected[n]??Main.TypeSystem.Object)).ToArray();
            var refs=new Dictionary<int,VariableDefinition>();
            foreach(var v in vars)m.Body.Variables.Add(v);
            for(int n=vars.Length-1;n>=0;n--)Emit(Instruction.Create(OpCodes.Stloc,vars[n]));
            for(int n=0;n<vars.Length;n++)if(RefChange(n)) {
                var element=((ByReferenceType)expected[n]!).ElementType;
                var temp=new VariableDefinition(element);m.Body.Variables.Add(temp);refs[n]=temp;
                bool isOut=false;
                if(operand is MethodReference method) {
                    var offset=fullCount-method.Parameters.Count-skip;
                    try {isOut=n>=offset&&method.Resolve().Parameters[n-offset].IsOut;} catch(AssemblyResolutionException) { }
                }
                if(!isOut) {
                    Emit(Instruction.Create(OpCodes.Ldloc,vars[n]));
                    Emit(Instruction.Create(I64(((ByReferenceType)values[n].Type!).ElementType)?OpCodes.Ldind_I8:OpCodes.Ldind_I4));
                    Emit(Instruction.Create(I64(element)?OpCodes.Conv_I8:OpCodes.Conv_Ovf_I4));
                    Emit(Instruction.Create(OpCodes.Stloc,temp));
                }
            }
            for(int n=0;n<vars.Length;n++){
                if(refs.TryGetValue(n,out var temp)){Emit(Instruction.Create(OpCodes.Ldloca,temp));continue;}
                Emit(Instruction.Create(OpCodes.Ldloc,vars[n]));
                if(Stack32(values[n].Type)&&I64(expected[n]))Emit(Instruction.Create(OpCodes.Conv_I8));
                if(I64(values[n].Type)&&Stack32(expected[n]))Emit(Instruction.Create(OpCodes.Conv_Ovf_I4));
                if(values[n].Type?.MetadataType==MetadataType.Object && expected[n]!=null && expected[n]!.MetadataType!=MetadataType.Object && !expected[n]!.IsValueType && expected[n] is not ByReferenceType)
                    Emit(Instruction.Create(OpCodes.Castclass,expected[n]!));
            }
            var original=Instruction.Create(OpCodes.Nop);original.OpCode=op;original.Operand=operand;Emit(original);m.Body.InitLocals=true;
            foreach(var (n,temp) in refs) {
                var element=((ByReferenceType)values[n].Type!).ElementType;
                Emit(Instruction.Create(OpCodes.Ldloc,vars[n]));Emit(Instruction.Create(OpCodes.Ldloc,temp));
                Emit(Instruction.Create(I64(element)?OpCodes.Conv_I8:OpCodes.Conv_Ovf_I4));
                Emit(Instruction.Create(I64(element)?OpCodes.Stind_I8:OpCodes.Stind_I4));
            }
        }

        bool RepairEdges(MethodDefinition m,Dictionary<Instruction,Val[]> states,Dictionary<Instruction,Val[]> outgoing)
        {
            bool changedEdges=false;
            foreach(var i in m.Body.Instructions.ToArray()) {
                if(!outgoing.TryGetValue(i,out var output)||output.Length==0)continue;
                if(i.Operand is Instruction target && states.TryGetValue(target,out var want) && output.Length==want.Length && output.Select((v,n)=>Int(v.Type)&&I64(want[n].Type)).Any(v=>v)) {
                    // Preserve condition operands and normalize the carried stack
                    // before the branch. The next pass recomputes both edges.
                    var input=states[i];var expected=input.Select(v=>v.Type).ToArray();
                    for(int n=0;n<output.Length;n++)if(Int(output[n].Type)&&I64(want[n].Type))expected[n]=Long;
                    ConvertOperands(m,i,input,expected);changedEdges=true;continue;
                }
                if(i.OpCode.FlowControl is FlowControl.Branch or FlowControl.Return or FlowControl.Throw || i.Next==null || !states.TryGetValue(i.Next,out var next) || next.Length!=output.Length)continue;
                if(!output.Select((v,n)=>Int(v.Type)&&I64(next[n].Type)).Any(v=>v))continue;
                var anchor=Instruction.Create(OpCodes.Nop);m.Body.GetILProcessor().InsertAfter(i,anchor);
                ConvertOperands(m,anchor,output,next.Select(v=>v.Type).ToArray());changedEdges=true;
            }
            return changedEdges;
        }

        void Repair(MethodDefinition m,Dictionary<Instruction,Val[]> states)
        {
            foreach(var i in m.Body.Instructions.ToArray()) {
                if(!states.TryGetValue(i,out var stack))continue;
                TypeReference?[]? want=null;int count=0;
                if(i.OpCode.Code is Code.Call or Code.Callvirt or Code.Newobj) {
                    var r=(MethodReference)i.Operand;count=r.Parameters.Count+(r.HasThis&&i.OpCode!=OpCodes.Newobj?1:0);
                    want=new TypeReference?[count];var offset=count-r.Parameters.Count;
                    if(offset==1)want[0]=r.DeclaringType;
                    for(int n=0;n<r.Parameters.Count;n++)want[n+offset]=Actual(r.Parameters[n].ParameterType,r);
                } else if(i.OpCode.Code is Code.Add or Code.Sub or Code.Mul or Code.Div or Code.Rem or Code.Add_Ovf or Code.Sub_Ovf or Code.Mul_Ovf or Code.Add_Ovf_Un or Code.Sub_Ovf_Un or Code.Mul_Ovf_Un or Code.Div_Un or Code.Rem_Un or Code.And or Code.Or or Code.Xor or Code.Ceq or Code.Cgt or Code.Cgt_Un or Code.Clt or Code.Clt_Un or Code.Beq or Code.Beq_S or Code.Bne_Un or Code.Bne_Un_S or Code.Bge or Code.Bge_S or Code.Bge_Un or Code.Bge_Un_S or Code.Bgt or Code.Bgt_S or Code.Bgt_Un or Code.Bgt_Un_S or Code.Ble or Code.Ble_S or Code.Ble_Un or Code.Ble_Un_S or Code.Blt or Code.Blt_S or Code.Blt_Un or Code.Blt_Un_S) {
                    count=2;if(wide.Contains(i)||stack.TakeLast(2).Any(v=>I64(v.Type)))want=new TypeReference?[]{Long,Long};
                } else if(i.OpCode==OpCodes.Ret&&m.ReturnType.MetadataType!=MetadataType.Void){count=1;want=new[]{m.ReturnType};}
                else if(i.OpCode.Code is Code.Stloc or Code.Stloc_S or Code.Stloc_0 or Code.Stloc_1 or Code.Stloc_2 or Code.Stloc_3){count=1;want=new[]{m.Body.Variables[Local(i)].VariableType};}
                else if(i.OpCode.Code is Code.Starg or Code.Starg_S){count=1;want=new[]{((ParameterDefinition)i.Operand).ParameterType};}
                else if(i.OpCode.Code is Code.Stfld or Code.Stsfld){count=i.OpCode==OpCodes.Stfld?2:1;want=new TypeReference?[count];want[^1]=((FieldReference)i.Operand).FieldType;}
                else if(i.OpCode==OpCodes.Stind_I8){count=2;want=new TypeReference?[]{null,Long};}
                else if(i.OpCode==OpCodes.Stind_I4){count=2;want=new TypeReference?[]{null,Main.TypeSystem.Int32};}
                else if(i.OpCode.Code is Code.Newarr or Code.Switch){count=1;want=new TypeReference?[]{Main.TypeSystem.Int32};}
                else if(i.OpCode.Name.StartsWith("ldelem")){count=2;want=new TypeReference?[]{null,Main.TypeSystem.Int32};}
                else if(i.OpCode.Name.StartsWith("stelem")){count=3;want=new TypeReference?[]{null,Main.TypeSystem.Int32,(stack[^3].Type as ArrayType)?.ElementType};}
                else if(i.OpCode.Code is Code.Shl or Code.Shr or Code.Shr_Un){count=2;want=new TypeReference?[]{null,Main.TypeSystem.Int32};}
                if(want!=null)ConvertOperands(m,i,stack.TakeLast(count).ToArray(),want);
            }
            // Insertions can exceed the signed-byte displacement of short branches.
            foreach(var i in m.Body.Instructions)if(i.OpCode.OperandType==OperandType.ShortInlineBrTarget) i.OpCode=i.OpCode.Code switch {
                Code.Br_S=>OpCodes.Br,Code.Brfalse_S=>OpCodes.Brfalse,Code.Brtrue_S=>OpCodes.Brtrue,Code.Beq_S=>OpCodes.Beq,Code.Bge_S=>OpCodes.Bge,Code.Bge_Un_S=>OpCodes.Bge_Un,Code.Bgt_S=>OpCodes.Bgt,Code.Bgt_Un_S=>OpCodes.Bgt_Un,Code.Ble_S=>OpCodes.Ble,Code.Ble_Un_S=>OpCodes.Ble_Un,Code.Blt_S=>OpCodes.Blt,Code.Blt_Un_S=>OpCodes.Blt_Un,Code.Bne_Un_S=>OpCodes.Bne_Un,Code.Leave_S=>OpCodes.Leave,_=>i.OpCode};
        }

        public void Write(bool stageOnly)
        {
            foreach(var a in assemblies) {
                // Import cross-module type references before serializing.
                foreach(var t in a.MainModule.GetTypes()) {
                    foreach(var f in t.Fields)f.FieldType=a.MainModule.ImportReference(f.FieldType);
                    foreach(var p in t.Properties)p.PropertyType=a.MainModule.ImportReference(p.PropertyType);
                    foreach(var m in t.Methods) {
                        m.ReturnType=a.MainModule.ImportReference(m.ReturnType);foreach(var p in m.Parameters)p.ParameterType=a.MainModule.ImportReference(p.ParameterType);
                        if(!m.HasBody)continue;
                        foreach(var v in m.Body.Variables)v.VariableType=a.MainModule.ImportReference(v.VariableType);
                        foreach(var i in m.Body.Instructions) {
                            if(i.Operand is TypeReference tr)i.Operand=a.MainModule.ImportReference(tr);
                            else if(i.Operand is MethodReference mr && mr is not MethodDefinition) {
                                mr.ReturnType=a.MainModule.ImportReference(mr.ReturnType);
                                foreach(var p in mr.Parameters)p.ParameterType=a.MainModule.ImportReference(p.ParameterType);
                                i.Operand=a.MainModule.ImportReference(mr);
                            }
                            else if(i.Operand is FieldReference fr && fr is not FieldDefinition) {
                                fr.FieldType=a.MainModule.ImportReference(fr.FieldType);
                                i.Operand=a.MainModule.ImportReference(fr);
                            }
                        }
                    }
                }
                var path=Path.Combine(managed,a.Name.Name+".dll");var staged=path+".currency64-staged";
                a.Write(staged);
                if(!stageOnly){if(!File.Exists(path+".before-currency64"))File.Copy(path,path+".before-currency64");File.Move(staged,path,true);}
            }
        }
        public void Dispose(){foreach(var a in assemblies)a.Dispose();resolver.Dispose();}
    }
}
