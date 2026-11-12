using System.Diagnostics;
using System.Reflection;

namespace DllPatcher;
partial class Program {
    static void InstallLavrilCircle(string root,bool stageOnly,bool verifyOnly=false) {
        root=Path.GetFullPath(root);
        var asset=Path.Combine(root,"King's Raid_Data","Documents","Patch","StandaloneWindows","PlainAsset","Text","NodeTree","Hero","Lavril.bjs");
        var work=Path.GetFullPath(Path.Combine("target","lavril-circle",Guid.NewGuid().ToString("N")));Directory.CreateDirectory(work);
        var script=Path.Combine(work,"FixLavrilCircle.py");
        using(var source=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.FixLavrilCircle.py")!)
        using(var output=File.Create(script))source.CopyTo(output);
        var staged=asset+".lavril-circle-staged";
        void Run(string input,string? output) {
            var start=new ProcessStartInfo(Environment.GetEnvironmentVariable("SPRK_PYTHON")??(OperatingSystem.IsWindows()?"python":"python3")){UseShellExecute=false,CreateNoWindow=true};
            start.ArgumentList.Add(script);start.ArgumentList.Add(input);
            if(output!=null){start.ArgumentList.Add("--output");start.ArgumentList.Add(output);}
            using var p=Process.Start(start)!;p.WaitForExit();if(p.ExitCode!=0)throw new InvalidOperationException("Lavril circle asset verification failed.");
        }
        TestLavrilNativeArea(root);
        if(verifyOnly){Run(stageOnly?staged:asset,null);return;}
        Run(asset,staged);Run(staged,null);
        if(stageOnly)return;
        if(Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Lavril circle staged. Close client processes before installation.");
        File.Copy(asset,Path.Combine(work,"Lavril.bjs.before"));File.Move(staged,asset,true);Run(asset,null);
        Console.WriteLine("Installed Lavril S3 visual field alignment. Restart the game.");
    }

    static void TestLavrilNativeArea(string root) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        ResolveEventHandler resolve=(_,e)=>{var p=Path.Combine(managed,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
        AppDomain.CurrentDomain.AssemblyResolve+=resolve;
        try {
            var assembly=Assembly.LoadFrom(Path.Combine(managed,"Assembly-CSharp.dll"));
            object Call(object target,string name,params object[] args)=>target.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
                .Single(m=>m.Name==name && m.GetParameters().Length==args.Length && m.GetParameters().Select((p,i)=>p.ParameterType.IsInstanceOfType(args[i])).All(v=>v)).Invoke(target,args)!;
            object Table(string name) {
                var type=assembly.GetType("NShared."+name+"DataContainer")!;var container=Activator.CreateInstance(type)!;
                var parent=type;PropertyInfo? property=null;
                while(parent!=null && property==null){property=parent.GetProperty("Packer",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly);parent=parent.BaseType;}
                Call(property!.GetValue(container)!,"Deserialize",File.ReadAllBytes(Path.Combine(root,"King's Raid_Data","Documents","Patch","StandaloneWindows","TableJit",name+"Table.jit")));
                return container;
            }
            int Number(object o,string name)=>Convert.ToInt32(o.GetType().GetProperty(name)!.GetValue(o));
            var operation=Call(Table("Skill"),"GetOperationData",5430,3);
            var values=(string[])operation.GetType().GetProperty("Values")!.GetValue(operation)!;
            if(values[0]!="34" || values[1]!="6000" || Number(operation,"Type")!=166)
                throw new InvalidOperationException("Lavril native S3 projectile/offset changed; review visual alignment.");
            var projectile=Call(Table("SkillProjectile"),"GetData",34);
            if(Number(projectile,"SkillIndex")!=5434 || Number(projectile,"SpeedMms")!=0 || Number(projectile,"RadiusMm")!=5000)
                throw new InvalidOperationException("Lavril native field geometry changed.");
            var vector=assembly.GetType("NShared.NUtil.Vector3")!;
            object Vec(long x,long z)=>Activator.CreateInstance(vector,x,0L,z)!;
            var collision=assembly.GetType("NShared.BattleCreatureContainer")!.GetMethods().Single(m=>m.Name=="IsInRadius" && m.GetParameters().Length==4);
            int checks=0;
            foreach(long facing in new[]{-1L,1L}) {
                var center=Vec(facing*6000,0);
                // Multiple enemies straddling the real field. Both interior
                // enemies must qualify; enemies beyond either edge must not.
                foreach(var (x,z,expected) in new[]{(5500L,0L,true),(9000L,0L,true),(12000L,0L,false),(0L,0L,false),(6000L,5000L,true),(6000L,5001L,false)}) {
                    if((bool)collision.Invoke(null,new[]{Vec(facing*x,z),(object)0,center,5000})! != expected)
                        throw new InvalidOperationException("Native multi-enemy field boundary failed.");
                    checks++;
                }
            }
            Console.WriteLine($"PASS: native S3 operation/projectile data and {checks} multi-enemy radius checks across both teams; combat tables unchanged.");
        } finally {AppDomain.CurrentDomain.AssemblyResolve-=resolve;}
    }
}
