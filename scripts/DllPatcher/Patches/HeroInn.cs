using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static bool PatchHeroInn(ModuleDefinition module, string unityCorePath, string unityEnginePath, IAssemblyResolver resolver)
    {
        // ================================================================
        // Load Unity assemblies
        // ================================================================
        var unityAssembly = AssemblyDefinition.ReadAssembly(unityCorePath, new ReaderParameters { AssemblyResolver = resolver });
        var componentType = unityAssembly.MainModule.Types.FirstOrDefault(t => t.FullName == "UnityEngine.Component");
        var gameObjectType = unityAssembly.MainModule.Types.FirstOrDefault(t => t.FullName == "UnityEngine.GameObject");

        // Load UnityEngine.dll for Debug.Log
        TypeDefinition debugType = null;
        try
        {
            var unityEngineAssembly = AssemblyDefinition.ReadAssembly(unityEnginePath, new ReaderParameters { AssemblyResolver = resolver });
            debugType = unityEngineAssembly.MainModule.Types.FirstOrDefault(t => t.FullName == "UnityEngine.Debug");
            if (debugType == null)
            {
                // Try CoreModule
                debugType = unityAssembly.MainModule.Types.FirstOrDefault(t => t.FullName == "UnityEngine.Debug");
            }
        }
        catch
        {
            debugType = unityAssembly.MainModule.Types.FirstOrDefault(t => t.FullName == "UnityEngine.Debug");
        }

        MethodReference debugLogRef = null;
        if (debugType != null)
        {
            var debugLog = debugType.Methods.FirstOrDefault(m => m.Name == "Log" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.Object");
            if (debugLog != null)
            {
                debugLogRef = module.ImportReference(debugLog);
                Console.WriteLine("Found Debug.Log method");
            }
        }
        else
        {
            Console.WriteLine("WARNING: Could not find Debug type");
        }

        var heroInnViewType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroInnView");
        if (heroInnViewType == null)
        {
            Console.WriteLine("ERROR: HeroInnView type not found!");
            return false;
        }
        Console.WriteLine($"Found type: {heroInnViewType.FullName}");
        if (!PatchHeroInnRecruiting(heroInnViewType)) return false;
        PatchHeroInnPortraits(heroInnViewType);
        return PatchHeroInnButtons(module, componentType, gameObjectType);
    }
}
