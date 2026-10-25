using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static MethodDefinition CircleShadowUpdate(ModuleDefinition module) => module.GetType("NGameAsset.NCreature.CircleShadowInstance").Methods.Single(m=>m.Name=="NVespa.NSingleton.ILateUpdater.ChainLateUpdate");
    static MethodDefinition PooledObjectDequeue(ModuleDefinition module) => module.GetType("NGame2.NNodeTree.NObject.Container").Methods.Single(m=>m.Name=="DequeueUnused" && m.GenericParameters.Count==1);
    static void InstallBattleGraphics(ModuleDefinition module,TypeDefinition bridge) {
        var shadow=CircleShadowUpdate(module);
        if(!shadow.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name==bridge.Name && r.Name=="UpdateCircleShadow")) {
            var type=shadow.DeclaringType;
            var pivot=type.Fields.Single(f=>f.Name=="_pivotTm" && f.FieldType.FullName=="UnityEngine.Transform");
            var cache=type.Fields.Single(f=>f.Name=="_renderCache" && f.FieldType.FullName=="NGameAsset.NCreature.RenderCache");
            foreach(var name in new[]{"set_position","set_localPosition","set_rotation"})
                if(shadow.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.FullName=="UnityEngine.Transform" && r.Name==name)!=1)throw new InvalidOperationException("Unsupported native circle-shadow transform update.");
            shadow.Body.Instructions.Clear();shadow.Body.Variables.Clear();shadow.Body.ExceptionHandlers.Clear();
            var il=shadow.Body.GetILProcessor();il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,pivot);il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,cache);
            il.Emit(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="UpdateCircleShadow")));il.Emit(OpCodes.Ret);
        }
        var dequeue=PooledObjectDequeue(module);
        var destroy=dequeue.Body.Instructions.Single(i=>i.Operand is MethodReference r && r.Name=="DestroyGo");
        if(destroy.Next.OpCode!=OpCodes.Br) {
            var first=destroy.Next;
            if(first.OpCode!=OpCodes.Ldloca_S || first.Next.OpCode!=OpCodes.Initobj || first.Next.Next.OpCode!=OpCodes.Ldloc_1 || first.Next.Next.Next.OpCode!=OpCodes.Ret)
                throw new InvalidOperationException("Unsupported native destroyed-pool-entry return.");
            var count=dequeue.Body.Instructions.Single(i=>i.Operand is MethodReference r && r.Name=="get_Count");
            if(count.Previous.OpCode!=OpCodes.Ldloc_0)throw new InvalidOperationException("Unsupported native pool loop.");
            first.OpCode=OpCodes.Br;first.Operand=count.Previous;
            for(int i=0;i<3;i++){first=first.Next;first.OpCode=OpCodes.Nop;first.Operand=null;}
        }
        VerifyBattleGraphics(module);
    }
    static void VerifyBattleGraphics(ModuleDefinition module) {
        var shadow=CircleShadowUpdate(module);
        if(shadow.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && r.Name=="UpdateCircleShadow")!=1)
            throw new InvalidOperationException("Missing native shadow visual update optimization.");
        VerifyArenaResultStack(shadow);
        var dequeue=PooledObjectDequeue(module);
        var destroy=dequeue.Body.Instructions.Single(i=>i.Operand is MethodReference r && r.Name=="DestroyGo");
        if(destroy.Next.OpCode!=OpCodes.Br || destroy.Next.Operand is not Instruction loop || loop.Next.Operand is not MethodReference count || count.Name!="get_Count")
            throw new InvalidOperationException("Destroyed pool entries must continue to the next reusable entry.");
        VerifyArenaResultStack(dequeue);
    }
    // Match complete raw payloads for the baseline and isolated company name.
    // Company conversion shortens the aligned string by eight bytes, shifting
    // the timing flag from 648 to 640. Never guess Unity's opaque schema.
    const int FrameTimingFlag=648;
    static byte[] IsolatedPlayerSettings(byte[] original) {
        if(original.Length!=808 || BitConverter.ToInt32(original,36)!=10 || System.Text.Encoding.UTF8.GetString(original,40,10)!="masangsoft")
            throw new InvalidOperationException("Unsupported native company fingerprint.");
        var result=new byte[800];Array.Copy(original,0,result,0,36);Array.Copy(BitConverter.GetBytes(4),0,result,36,4);
        Array.Copy(System.Text.Encoding.UTF8.GetBytes("sprk"),0,result,40,4);Array.Copy(original,52,result,44,756);return result;
    }
    static byte[] OriginalPlayerSettings() {
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.FrameTimingPlayerSettings.bin")!;
        using var buffer=new MemoryStream();resource.CopyTo(buffer);return buffer.ToArray();
    }
    static int FindFrameTimingSettings(byte[] bytes,out bool enabled) {
        var original=OriginalPlayerSettings();int match=-1;enabled=false;
        foreach(var (pattern,flag) in new[]{(original,648),(IsolatedPlayerSettings(original),640)})
        foreach(var value in new[]{0,1}) {
            var candidate=(byte[])pattern.Clone();candidate[flag]=(byte)value;
            int offset=bytes.AsSpan().IndexOf(candidate);if(offset<0)continue;
            if(match>=0 || bytes.AsSpan(offset+1).IndexOf(candidate)>=0)throw new InvalidOperationException("Ambiguous PlayerSettings fingerprint.");
            match=offset+flag;enabled=value==1;
        }
        if(match<0)throw new InvalidOperationException("Unsupported native PlayerSettings layout.");return match;
    }
    static void StageFrameTimingSettings(string root) {
        var path=Path.Combine(root,"King's Raid_Data","globalgamemanagers");var bytes=File.ReadAllBytes(path);
        int offset=FindFrameTimingSettings(bytes,out _);bytes[offset]=1;File.WriteAllBytes(path+".graphics-options-staged",bytes);
    }
    static void VerifyFrameTimingSettings(string root,bool staged) {
        var path=Path.Combine(root,"King's Raid_Data","globalgamemanagers")+(staged?".graphics-options-staged":"");
        FindFrameTimingSettings(File.ReadAllBytes(path),out bool enabled);
        if(!enabled)throw new InvalidOperationException("Native GPU frame timing statistics are disabled.");
    }
}
