using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            // Determine client path: first CLI arg, or prompt, or default
            string clientRoot = args.Length > 0
                ? args[0].TrimEnd('\\', '/')
                : PromptForClientPath();

            string managedDir = Path.Combine(clientRoot, "King's Raid_Data", "Managed");
            string dllPath = Path.Combine(managedDir, "Assembly-CSharp.dll");
            string backupPath = dllPath + ".backup_before_patch";
            string unityCorePath = Path.Combine(managedDir, "UnityEngine.CoreModule.dll");
            string unityEnginePath = Path.Combine(managedDir, "UnityEngine.dll");
            string patchedPath = dllPath + ".patched";
            
            // Use the backup as source if it exists
            string sourcePath = File.Exists(backupPath) ? backupPath : dllPath;
            
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"DLL not found: {sourcePath}");
                return;
            }
            
            // Create backup if not exists
            if (!File.Exists(backupPath))
            {
                Console.WriteLine($"Creating backup: {backupPath}");
                File.Copy(dllPath, backupPath);
            }
            
            Console.WriteLine($"Loading assembly from: {sourcePath}");
            
            // Set up resolver to find Unity DLLs
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(dllPath)!);
            
            var readerParams = new ReaderParameters { 
                ReadWrite = false, // Don't lock the file
                AssemblyResolver = resolver
            };
            
            using (var assembly = AssemblyDefinition.ReadAssembly(sourcePath, readerParams))
            {
                var module = assembly.MainModule;
                
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
                
                // ================================================================
                // PATCH HeroInnView.InitUIBtnGroup to force Friendly group when
                // the player doesn't own the hero
                // ================================================================
                Console.WriteLine("\n=== Patching HeroInnView.InitUIBtnGroup ===");
                var heroInnViewType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroInnView");
                if (heroInnViewType == null)
                {
                    Console.WriteLine("ERROR: HeroInnView type not found!");
                    return;
                }
                Console.WriteLine($"Found type: {heroInnViewType.FullName}");
                
                var initUIBtnGroup = heroInnViewType.Methods.FirstOrDefault(m => m.Name == "InitUIBtnGroup");
                if (initUIBtnGroup == null)
                {
                    Console.WriteLine("ERROR: InitUIBtnGroup method not found!");
                    return;
                }
                
                Console.WriteLine($"Found InitUIBtnGroup with {initUIBtnGroup.Body.Instructions.Count} instructions");
                
                // Find the EGroupButtonType enum (nested in HeroInnView)
                var groupButtonTypeEnum = heroInnViewType.NestedTypes.FirstOrDefault(t => t.Name == "EGroupButtonType");
                if (groupButtonTypeEnum != null)
                {
                    Console.WriteLine($"Found EGroupButtonType: {groupButtonTypeEnum.FullName}");
                    foreach (var field in groupButtonTypeEnum.Fields)
                    {
                        if (field.HasConstant)
                            Console.WriteLine($"  {field.Name} = {field.Constant}");
                    }
                }
                
                // The IL code pattern is:
                //   brtrue.s IL_0015   <- if flag2 (IsSelectedHero) is true, jump to load Friendly
                //   ldc.i4.0           <- else, load 0 (None)  <- WE WANT TO CHANGE THIS TO 1
                //   br.s IL_0019       <- skip to store
                //   ldc.i4.1           <- load 1 (Friendly)
                //   ...
                //   stloc.1            <- store gType
                
                var instructions = initUIBtnGroup.Body.Instructions;
                var il = initUIBtnGroup.Body.GetILProcessor();
                bool patched = false;
                
                Console.WriteLine("Looking for None -> Friendly patch opportunity...");
                
                // The pattern is: brtrue.s followed by ldc.i4.0 followed by br.s
                for (int i = 0; i < instructions.Count - 2; i++)
                {
                    var instr = instructions[i];
                    if (instr.OpCode == OpCodes.Brtrue || instr.OpCode == OpCodes.Brtrue_S)
                    {
                        var next = instructions[i + 1];
                        var afterNext = instructions[i + 2];
                        
                        // Check if next is ldc.i4.0 and afterNext is br
                        if ((next.OpCode == OpCodes.Ldc_I4_0 || 
                             (next.OpCode == OpCodes.Ldc_I4 && (int)next.Operand == 0) ||
                             (next.OpCode == OpCodes.Ldc_I4_S && (sbyte)next.Operand == 0)) &&
                            (afterNext.OpCode == OpCodes.Br || afterNext.OpCode == OpCodes.Br_S))
                        {
                            Console.WriteLine($"Found pattern at IL_{i:X4}:");
                            Console.WriteLine($"  {instr}");
                            Console.WriteLine($"  {next} <- Changing this from 0 (None) to 1 (Friendly)");
                            Console.WriteLine($"  {afterNext}");
                            
                            // Replace ldc.i4.0 with ldc.i4.1
                            var newInstr = il.Create(OpCodes.Ldc_I4_1);
                            il.Replace(next, newInstr);
                            patched = true;
                            Console.WriteLine("  Patched successfully!");
                            break;
                        }
                    }
                }
                
                if (!patched)
                {
                    Console.WriteLine("WARNING: Could not find None -> Friendly patch point. Checking all ldc.i4.0...");
                    // Print all instructions for debugging
                    for (int i = 0; i < instructions.Count && i < 30; i++)
                    {
                        Console.WriteLine($"  IL_{i:X4}: {instructions[i]}");
                    }
                }
                
                // ================================================================
                // PATCH InitHeroPortraits to fix array bounds issue
                // The original code crashes when heroSelectedIndices.Length < HeroInnPortraits.Length
                // Strategy: Find the loop's back-edge (where it checks i < HeroInnPortraits.Length)
                // and add an additional check: i < heroSelectedIndices.Length
                // ================================================================
                Console.WriteLine("\n=== Patching HeroInnView.InitHeroPortraits ===");
                var initHeroPortraits = heroInnViewType.Methods.FirstOrDefault(m => m.Name == "InitHeroPortraits");
                if (initHeroPortraits != null)
                {
                    Console.WriteLine($"Found InitHeroPortraits with {initHeroPortraits.Body.Instructions.Count} instructions");
                    var initIL = initHeroPortraits.Body.GetILProcessor();
                    var initInstrs = initHeroPortraits.Body.Instructions;
                    
                    // Print original IL for debugging
                    Console.WriteLine("Original IL (first 60 instructions):");
                    for (int i = 0; i < Math.Min(initInstrs.Count, 60); i++)
                    {
                        Console.WriteLine($"  IL_{i:D4}: {initInstrs[i]}");
                    }
                    
                    // Find the heroSelectedIndices variable (int[] type, should be V_0)
                    VariableDefinition heroIndicesVar = null;
                    foreach (var v in initHeroPortraits.Body.Variables)
                    {
                        if (v.VariableType.IsArray && v.VariableType.GetElementType().FullName == "System.Int32")
                        {
                            heroIndicesVar = v;
                            Console.WriteLine($"  Found heroSelectedIndices: {v}");
                            break;
                        }
                    }
                    
                    // The loop condition usually looks like:
                    // ldloc.X (loop counter)
                    // ldarg.0
                    // ldfld NGame2.NUI.NWindow.HeroInnView::HeroInnPortraits
                    // ldlen
                    // conv.i4
                    // blt.s IL_xxxx (jump to loop start)
                    // We want to insert BEFORE this sequence: 
                    //   ldloc.s V_3 (i)
                    //   ldloc.0 (heroSelectedIndices)  
                    //   ldlen
                    //   conv.i4
                    //   bge <after loop>  // if i >= heroSelectedIndices.Length, exit loop
                    
                    // Actually, simpler: find where the loop ENDS and add our own check there
                    // Look for pattern: ldloc (i), ldarg.0, ldfld HeroInnPortraits, ldlen, conv.i4, blt/clt
                    
                    bool patchedInitHP = false;
                    for (int i = 0; i < initInstrs.Count - 5; i++)
                    {
                        var instr = initInstrs[i];
                        
                        // Look for: ldarg.0, ldfld HeroInnPortraits, ldlen, conv.i4, blt.s <target>
                        if (instr.OpCode == OpCodes.Ldarg_0 && 
                            i + 4 < initInstrs.Count)
                        {
                            var next1 = initInstrs[i + 1]; // ldfld HeroInnPortraits
                            var next2 = initInstrs[i + 2]; // ldlen
                            var next3 = initInstrs[i + 3]; // conv.i4
                            var next4 = initInstrs[i + 4]; // blt.s or clt
                            
                            if (next1.OpCode == OpCodes.Ldfld &&
                                next1.Operand is FieldReference fr && fr.Name == "HeroInnPortraits" &&
                                next2.OpCode == OpCodes.Ldlen &&
                                next3.OpCode == OpCodes.Conv_I4)
                            {
                                Console.WriteLine($"  Found loop condition at IL_{i:D4}");
                                Console.WriteLine($"    {next1}");
                                Console.WriteLine($"    {next2}");
                                Console.WriteLine($"    {next3}");
                                Console.WriteLine($"    {next4}");
                                
                                // The branch target of next4 (blt.s) is the loop body
                                // We need to add our check right before next4
                                
                                if ((next4.OpCode == OpCodes.Blt_S || next4.OpCode == OpCodes.Blt) && 
                                    heroIndicesVar != null)
                                {
                                    var loopBodyTarget = (Instruction)next4.Operand;
                                    
                                    // After the original blt.s, we need to also check heroSelectedIndices.Length
                                    // Actually, easier approach: change the loop to use the minimum length
                                    
                                    // Since we can't easily call Math.Min, let's insert a second check
                                    // AFTER the blt.s branches to loop body:
                                    // At the loop body start, add:
                                    //   ldloc.s V_3 (i)
                                    //   ldloc.0 (heroSelectedIndices)
                                    //   ldlen
                                    //   conv.i4
                                    //   bge <after loop>
                                    
                                    // Find the instruction AFTER the loop (where blt.s falls through)
                                    // This should be right after next4
                                    var afterLoopInstr = initInstrs[i + 5];
                                    Console.WriteLine($"  After loop: {afterLoopInstr}");
                                    
                                    // Find the loop variable - look for what's loaded before ldarg.0
                                    // It should be: ldloc.s V_3, ldarg.0, ldfld...
                                    VariableDefinition loopVarDef = null;
                                    if (i > 0)
                                    {
                                        var prevInstr = initInstrs[i - 1];
                                        if (prevInstr.OpCode == OpCodes.Ldloc_S || prevInstr.OpCode == OpCodes.Ldloc)
                                        {
                                            loopVarDef = (VariableDefinition)prevInstr.Operand;
                                            Console.WriteLine($"  Loop variable: V_{loopVarDef.Index}");
                                        }
                                        else if (prevInstr.OpCode == OpCodes.Ldloc_0)
                                        {
                                            loopVarDef = initHeroPortraits.Body.Variables[0];
                                            Console.WriteLine($"  Loop variable: V_0");
                                        }
                                        else if (prevInstr.OpCode == OpCodes.Ldloc_1)
                                        {
                                            loopVarDef = initHeroPortraits.Body.Variables[1];
                                            Console.WriteLine($"  Loop variable: V_1");
                                        }
                                        else if (prevInstr.OpCode == OpCodes.Ldloc_2)
                                        {
                                            loopVarDef = initHeroPortraits.Body.Variables[2];
                                            Console.WriteLine($"  Loop variable: V_2");
                                        }
                                        else if (prevInstr.OpCode == OpCodes.Ldloc_3)
                                        {
                                            loopVarDef = initHeroPortraits.Body.Variables[3];
                                            Console.WriteLine($"  Loop variable: V_3");
                                        }
                                    }
                                    
                                    if (loopVarDef != null)
                                    {
                                        // Insert bounds check at the START of the loop body
                                        // (right at loopBodyTarget)
                                        
                                        // Create new instructions for the bounds check
                                        // ldloc.s loopVar
                                        // ldloc heroIndicesVar  
                                        // ldlen
                                        // conv.i4
                                        // bge afterLoopInstr
                                        
                                        var loadLoopVar = loopVarDef.Index switch {
                                            0 => initIL.Create(OpCodes.Ldloc_0),
                                            1 => initIL.Create(OpCodes.Ldloc_1),
                                            2 => initIL.Create(OpCodes.Ldloc_2),
                                            3 => initIL.Create(OpCodes.Ldloc_3),
                                            _ => initIL.Create(OpCodes.Ldloc_S, loopVarDef)
                                        };
                                        
                                        var loadHeroIndices = heroIndicesVar.Index switch {
                                            0 => initIL.Create(OpCodes.Ldloc_0),
                                            1 => initIL.Create(OpCodes.Ldloc_1),
                                            2 => initIL.Create(OpCodes.Ldloc_2),
                                            3 => initIL.Create(OpCodes.Ldloc_3),
                                            _ => initIL.Create(OpCodes.Ldloc_S, heroIndicesVar)
                                        };
                                        
                                        var ldlenInstr = initIL.Create(OpCodes.Ldlen);
                                        var convI4Instr = initIL.Create(OpCodes.Conv_I4);
                                        var bgeInstr = initIL.Create(OpCodes.Bge, afterLoopInstr);
                                        
                                        // Insert BEFORE loopBodyTarget
                                        initIL.InsertBefore(loopBodyTarget, loadLoopVar);
                                        initIL.InsertBefore(loopBodyTarget, loadHeroIndices);
                                        initIL.InsertBefore(loopBodyTarget, ldlenInstr);
                                        initIL.InsertBefore(loopBodyTarget, convI4Instr);
                                        initIL.InsertBefore(loopBodyTarget, bgeInstr);
                                        
                                        // Update the blt.s to point to our new check
                                        next4.Operand = loadLoopVar;
                                        
                                        patchedInitHP = true;
                                        Console.WriteLine("  Successfully patched InitHeroPortraits with bounds check!");
                                    }
                                }
                                
                                break;
                            }
                        }
                    }
                    
                    if (!patchedInitHP)
                    {
                        Console.WriteLine("  WARNING: Could not patch InitHeroPortraits bounds check");
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: InitHeroPortraits method not found");
                }
                
                // ================================================================
                // PATCH HeroInnViewButtonGroup (same as before)
                // ================================================================
                Console.WriteLine("\n=== Patching HeroInnViewButtonGroup ===");
                
                // Find the HeroInnViewButtonGroup type
                var buttonGroupType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroInnViewButtonGroup");
                if (buttonGroupType == null)
                {
                    Console.WriteLine("ERROR: HeroInnViewButtonGroup type not found!");
                    return;
                }
                Console.WriteLine($"Found type: {buttonGroupType.FullName}");
                
                // Find the HeroInnViewButton type
                var buttonType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroInnViewButton");
                if (buttonType == null)
                {
                    Console.WriteLine("ERROR: HeroInnViewButton type not found!");
                    return;
                }
                
                // Find the SetListener method
                var setListenerMethod = buttonGroupType.Methods.FirstOrDefault(m => m.Name == "SetListener");
                if (setListenerMethod == null)
                {
                    Console.WriteLine("ERROR: SetListener method not found!");
                    return;
                }
                Console.WriteLine($"Found SetListener method");
                
                // Check if already patched
                if (buttonGroupType.Methods.Any(m => m.Name == "HideUnsupportedButtons"))
                {
                    Console.WriteLine("HideUnsupportedButtons already exists - skipping button group patch");
                }
                else
                {
                    // Find the Buttons field
                    var buttonsField = buttonGroupType.Fields.FirstOrDefault(f => f.Name == "Buttons");
                    if (buttonsField == null)
                    {
                        Console.WriteLine("ERROR: Buttons field not found!");
                        return;
                    }
                    
                    // Find the ActionType field in HeroInnViewButton
                    var actionTypeField = buttonType.Fields.FirstOrDefault(f => f.Name == "ActionType");
                    if (actionTypeField == null)
                    {
                        Console.WriteLine("ERROR: ActionType field not found!");
                        return;
                    }
                    
                    if (componentType == null || gameObjectType == null)
                    {
                        Console.WriteLine("ERROR: Unity types not found!");
                        return;
                    }
                    
                    var gameObjectGetter = componentType.Methods.FirstOrDefault(m => m.Name == "get_gameObject");
                    var setActiveMethod = gameObjectType.Methods.FirstOrDefault(m => m.Name == "SetActive");
                    
                    var gameObjectGetterRef = module.ImportReference(gameObjectGetter);
                    var setActiveRef = module.ImportReference(setActiveMethod);
                    
                    // Create the HideUnsupportedButtons method
                    var hideMethod = new MethodDefinition("HideUnsupportedButtons", 
                        MethodAttributes.Private | MethodAttributes.HideBySig,
                        module.TypeSystem.Void);
                    
                    hideMethod.Body.InitLocals = true;
                    
                    var hideIL = hideMethod.Body.GetILProcessor();
                    var endMethod = hideIL.Create(OpCodes.Ret);
                    
                    // if (this.Buttons == null || this.Buttons.Length == 0) return;
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Brfalse, endMethod);
                    
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldlen);
                    hideIL.Emit(OpCodes.Conv_I4);
                    hideIL.Emit(OpCodes.Brfalse, endMethod);
                    
                    // Fix button 0: ActionType = Greeting (1)
                    var skipBtn0 = hideIL.Create(OpCodes.Nop);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldlen);
                    hideIL.Emit(OpCodes.Conv_I4);
                    hideIL.Emit(OpCodes.Ldc_I4_0);
                    hideIL.Emit(OpCodes.Ble, skipBtn0);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldc_I4_0);
                    hideIL.Emit(OpCodes.Ldelem_Ref);
                    hideIL.Emit(OpCodes.Brfalse, skipBtn0);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldc_I4_0);
                    hideIL.Emit(OpCodes.Ldelem_Ref);
                    hideIL.Emit(OpCodes.Ldc_I4_1); // Greeting = 1
                    hideIL.Emit(OpCodes.Stfld, actionTypeField);
                    hideIL.Append(skipBtn0);
                    
                    // Fix button 1: ActionType = Conversation (2)
                    var skipBtn1 = hideIL.Create(OpCodes.Nop);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldlen);
                    hideIL.Emit(OpCodes.Conv_I4);
                    hideIL.Emit(OpCodes.Ldc_I4_1);
                    hideIL.Emit(OpCodes.Ble, skipBtn1);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldc_I4_1);
                    hideIL.Emit(OpCodes.Ldelem_Ref);
                    hideIL.Emit(OpCodes.Brfalse, skipBtn1);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldc_I4_1);
                    hideIL.Emit(OpCodes.Ldelem_Ref);
                    hideIL.Emit(OpCodes.Ldc_I4_2); // Conversation = 2
                    hideIL.Emit(OpCodes.Stfld, actionTypeField);
                    hideIL.Append(skipBtn1);
                    
                    // Fix button 2: ActionType = Gift (3)
                    var skipBtn2 = hideIL.Create(OpCodes.Nop);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldlen);
                    hideIL.Emit(OpCodes.Conv_I4);
                    hideIL.Emit(OpCodes.Ldc_I4_2);
                    hideIL.Emit(OpCodes.Ble, skipBtn2);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldc_I4_2);
                    hideIL.Emit(OpCodes.Ldelem_Ref);
                    hideIL.Emit(OpCodes.Brfalse, skipBtn2);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldc_I4_2);
                    hideIL.Emit(OpCodes.Ldelem_Ref);
                    hideIL.Emit(OpCodes.Ldc_I4_3); // Gift = 3
                    hideIL.Emit(OpCodes.Stfld, actionTypeField);
                    hideIL.Append(skipBtn2);
                    
                    // Hide buttons 3 and beyond
                    hideMethod.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Int32)); // i
                    hideMethod.Body.Variables.Add(new VariableDefinition(buttonType)); // button
                    
                    var startLoop = hideIL.Create(OpCodes.Nop);
                    var checkLoop = hideIL.Create(OpCodes.Nop);
                    
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldlen);
                    hideIL.Emit(OpCodes.Conv_I4);
                    hideIL.Emit(OpCodes.Ldc_I4_3);
                    hideIL.Emit(OpCodes.Ble, endMethod);
                    
                    hideIL.Emit(OpCodes.Ldc_I4_3);
                    hideIL.Emit(OpCodes.Stloc_0);
                    hideIL.Emit(OpCodes.Br, checkLoop);
                    
                    hideIL.Append(startLoop);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldloc_0);
                    hideIL.Emit(OpCodes.Ldelem_Ref);
                    hideIL.Emit(OpCodes.Stloc_1);
                    
                    var skipHide = hideIL.Create(OpCodes.Nop);
                    hideIL.Emit(OpCodes.Ldloc_1);
                    hideIL.Emit(OpCodes.Brfalse, skipHide);
                    
                    hideIL.Emit(OpCodes.Ldloc_1);
                    hideIL.Emit(OpCodes.Callvirt, gameObjectGetterRef);
                    hideIL.Emit(OpCodes.Ldc_I4_0);
                    hideIL.Emit(OpCodes.Callvirt, setActiveRef);
                    
                    hideIL.Append(skipHide);
                    hideIL.Emit(OpCodes.Ldloc_0);
                    hideIL.Emit(OpCodes.Ldc_I4_1);
                    hideIL.Emit(OpCodes.Add);
                    hideIL.Emit(OpCodes.Stloc_0);
                    
                    hideIL.Append(checkLoop);
                    hideIL.Emit(OpCodes.Ldloc_0);
                    hideIL.Emit(OpCodes.Ldarg_0);
                    hideIL.Emit(OpCodes.Ldfld, buttonsField);
                    hideIL.Emit(OpCodes.Ldlen);
                    hideIL.Emit(OpCodes.Conv_I4);
                    hideIL.Emit(OpCodes.Blt, startLoop);
                    
                    hideIL.Append(endMethod);
                    
                    buttonGroupType.Methods.Add(hideMethod);
                    Console.WriteLine("Added HideUnsupportedButtons method");
                    
                    // Modify SetListener to call HideUnsupportedButtons
                    var setListenerIL = setListenerMethod.Body.GetILProcessor();
                    var lastInstruction = setListenerMethod.Body.Instructions.Last();
                    
                    var callHide = setListenerIL.Create(OpCodes.Ldarg_0);
                    var callMethod = setListenerIL.Create(OpCodes.Call, hideMethod);
                    
                    setListenerIL.InsertBefore(lastInstruction, callHide);
                    setListenerIL.InsertBefore(lastInstruction, callMethod);
                    
                    Console.WriteLine("Modified SetListener to call HideUnsupportedButtons");
                }
                
                // ================================================================
                // PATCH StateBase_InAppBilling to skip payment error popup
                // When Steam/payment initialization fails, continue login instead
                // of showing an error popup
                // ================================================================
                Console.WriteLine("\n=== Patching StateBase_InAppBilling (Skip Payment Error Popup) ===");
                PatchPaymentErrorPopup(module);

                // ================================================================
                // PATCH Hero/Costume visibility gates for private-server browsing
                // - Hero list: treat CreatureOpenType.None as Opened
                // - Costumes: force IsOpen=true and preview-ownership gates off
                // ================================================================
                Console.WriteLine("\n=== Patching Hero/Costume Visibility Unlocks ===");
                PatchHeroAndCostumeVisibility(module);
                
                // Save the modified assembly
                Console.WriteLine($"\nSaving modified assembly to: {patchedPath}");
                assembly.Write(patchedPath);
                
                Console.WriteLine($"Copying to: {dllPath}");
                File.Copy(patchedPath, dllPath, true);
                Console.WriteLine("Done! The DLL has been patched successfully.");
                
                // ================================================================
                // PATCH SoulWeaponLimitBreak.TryGetMaxStar & GetAfterDataByCurrent
                // Disables Limit Break (stars 6-15) completely
                // ================================================================
                Console.WriteLine("\n=== Patching SoulWeaponLimitBreak (Disable Stars 6-15) ===");
                var swlbType = module.Types.FirstOrDefault(t => t.FullName == "NShared.NContentsDefine.SoulWeaponLimitBreak");
                if (swlbType != null)
                {
                    // 1. Patch TryGetMaxStar
                    var tryGetMaxStar = swlbType.Methods.FirstOrDefault(m => m.Name == "TryGetMaxStar");
                    if (tryGetMaxStar != null)
                    {
                        tryGetMaxStar.Body.Instructions.Clear();
                        tryGetMaxStar.Body.ExceptionHandlers.Clear();
                        tryGetMaxStar.Body.Variables.Clear();
                        
                        var ilProc = tryGetMaxStar.Body.GetILProcessor();
                        ilProc.Append(ilProc.Create(OpCodes.Ldarg_2));
                        ilProc.Append(ilProc.Create(OpCodes.Ldc_I4_0));
                        ilProc.Append(ilProc.Create(OpCodes.Stind_I4));
                        ilProc.Append(ilProc.Create(OpCodes.Ldc_I4_0));
                        ilProc.Append(ilProc.Create(OpCodes.Ret));
                        Console.WriteLine("Successfully patched SoulWeaponLimitBreak.TryGetMaxStar");
                    }
                    
                    // 2. Patch GetAfterDataByCurrent
                    var getAfterData = swlbType.Methods.FirstOrDefault(m => m.Name == "GetAfterDataByCurrent");
                    if (getAfterData != null)
                    {
                        getAfterData.Body.Instructions.Clear();
                        getAfterData.Body.ExceptionHandlers.Clear();
                        getAfterData.Body.Variables.Clear();
                        
                        var ilProc = getAfterData.Body.GetILProcessor();
                        ilProc.Append(ilProc.Create(OpCodes.Ldnull));
                        ilProc.Append(ilProc.Create(OpCodes.Ret));
                        Console.WriteLine("Successfully patched SoulWeaponLimitBreak.GetAfterDataByCurrent");
                    }
                    
                    // 3. Patch GetDataByResultStar
                    var getDataByResult = swlbType.Methods.FirstOrDefault(m => m.Name == "GetDataByResultStar");
                    if (getDataByResult != null)
                    {
                        getDataByResult.Body.Instructions.Clear();
                        getDataByResult.Body.ExceptionHandlers.Clear();
                        getDataByResult.Body.Variables.Clear();
                        
                        var ilProc = getDataByResult.Body.GetILProcessor();
                        ilProc.Append(ilProc.Create(OpCodes.Ldnull));
                        ilProc.Append(ilProc.Create(OpCodes.Ret));
                        Console.WriteLine("Successfully patched SoulWeaponLimitBreak.GetDataByResultStar");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: SoulWeaponLimitBreak type not found!");
                }
                
                // Write patched DLL
                Console.WriteLine($"\nSaving modified assembly to: {patchedPath}");
                assembly.Write(patchedPath);
            }
            
            Console.WriteLine($"Copying to: {dllPath}");
            File.Copy(patchedPath, dllPath, true);
            Console.WriteLine("Done! The DLL has been patched successfully.");
            
            // Print summary
            Console.WriteLine("\n=== PATCH SUMMARY ===");
            Console.WriteLine("1. HeroInnView.InitUIBtnGroup: Changed None -> Friendly (always show recruiting buttons)");
            Console.WriteLine("2. HeroInnView.InitHeroPortraits: Added bounds check to prevent IndexOutOfRangeException");
            Console.WriteLine("3. HeroInnViewButtonGroup: Added HideUnsupportedButtons (fix ActionTypes, hide extra buttons)");
            Console.WriteLine("4. StateBase_InAppBilling: Skip payment error popup (Steam initialization)");
            Console.WriteLine("5. SoulWeaponLimitBreak: Disabled Limit Break stars 6-15 (TryGetMaxStar, GetAfterDataByCurrent, GetDataByResultStar patched)");
            Console.WriteLine("6. Hero/Costume visibility: Hero OpenType None->Opened only for index 1..102 and 111 (Valance), costume visibility and priced purchases enabled, native ownership checks preserved");
        }

        static void PatchHeroAndCostumeVisibility(ModuleDefinition module)
        {
            PatchCreatureOpenTypeGetter(module);
            // Preserve the original ownership checks so the shop can offer unowned costumes.
            PatchCostumeBuying(module);
            PatchHeroCostumeViewMotionButton(module);

            // Costume IsOpen flags
            PatchBooleanGetter(module, "NShared.CostumeData", "get_IsOpen", true);
            PatchBooleanGetter(module, "NShared.HairCostumeData", "get_IsOpen", true);
            PatchBooleanGetter(module, "NShared.WeaponCostumeData", "get_IsOpen", true);
            PatchBooleanGetter(module, "NShared.AccessoryCostumeData", "get_IsOpen", true);

            // Costume IsShow flags (used by HeroDetailView costume list / motion-entry visibility)
            PatchBooleanGetter(module, "NShared.CostumeData", "get_IsShow", true);
            PatchBooleanGetter(module, "NShared.HairCostumeData", "get_IsShow", true);
            PatchBooleanGetter(module, "NShared.WeaponCostumeData", "get_IsShow", true);
            PatchBooleanGetter(module, "NShared.AccessoryCostumeData", "get_IsShow", true);

            // Preview ownership gates (force false so UI won't require ownership to preview/list)
            PatchBooleanGetter(module, "NShared.CostumeData", "get_PreviewableWhenOwned", false);
            PatchBooleanGetter(module, "NShared.HairCostumeData", "get_PreviewableWhenNeedCostumeOwned", false);
            PatchBooleanGetter(module, "NShared.HairCostumeData", "get_PreviewableWhenNeedHairOwned", false);
            PatchBooleanGetter(module, "NShared.WeaponCostumeData", "get_PreviewableWhenNeedCostumeOwned", false);
            PatchBooleanGetter(module, "NShared.WeaponCostumeData", "get_PreviewableWhenNeedWeaponOwned", false);
            PatchBooleanGetter(module, "NShared.AccessoryCostumeData", "get_PreviewableWhenOwned", false);
        }

        // Mirrors HeroShopSupport's Buyable policy: normal, priced, non-default costumes.
        static void PatchCostumeBuying(ModuleDefinition module)
        {
            var type = module.Types.FirstOrDefault(t => t.FullName == "NShared.CostumeData");
            var getter = type?.Methods.FirstOrDefault(m => m.Name == "get_IsBuy");
            if (getter == null || type == null) throw new InvalidOperationException("CostumeData.IsBuy not found");
            MethodDefinition Get(string name) => type.Methods.First(m => m.Name == "get_" + name);
            getter.Body = new MethodBody(getter);
            var il = getter.Body.GetILProcessor();
            var no = il.Create(OpCodes.Ldc_I4_0);
            var yes = il.Create(OpCodes.Ldc_I4_1);
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, Get("IsDefault")));
            il.Append(il.Create(OpCodes.Brtrue, no));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, Get("CostumeType")));
            il.Append(il.Create(OpCodes.Ldc_I4_1));
            il.Append(il.Create(OpCodes.Bne_Un, no));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, Get("ProductIndex")));
            il.Append(il.Create(OpCodes.Brtrue, no));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, type.Methods.First(m => m.Name == "IsBonusCostume")));
            il.Append(il.Create(OpCodes.Brtrue, no));
            foreach (var price in new[] { "ReqBuyGem", "ReqBuyGold", "ReqBuyMileage" })
            {
                il.Append(il.Create(OpCodes.Ldarg_0));
                il.Append(il.Create(OpCodes.Call, Get(price)));
                il.Append(il.Create(OpCodes.Ldc_I4_0));
                il.Append(il.Create(OpCodes.Bgt, yes));
            }
            il.Append(no);
            il.Append(il.Create(OpCodes.Ret));
            il.Append(yes);
            il.Append(il.Create(OpCodes.Ret));
            Console.WriteLine("Enabled priced costume purchases with native ownership checks");
        }

        static void PatchHeroCostumeViewMotionButton(ModuleDefinition module)
        {
            var type = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroCostumeView");
            if (type == null)
            {
                Console.WriteLine("WARNING: HeroCostumeView type not found (View Motion button patch skipped)");
                return;
            }

            var method = type.Methods.FirstOrDefault(m => m.Name == "OnClickClosePhoto" && !m.HasParameters);
            if (method == null)
            {
                Console.WriteLine("WARNING: HeroCostumeView.OnClickClosePhoto not found (View Motion button patch skipped)");
                return;
            }

            var il = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;
            bool patched = false;

            for (int i = 0; i < instructions.Count - 1; i++)
            {
                if (instructions[i].OpCode == OpCodes.Ldfld &&
                    instructions[i].Operand is FieldReference fieldRef &&
                    fieldRef.Name == "WidgetViewMotion")
                {
                    for (int j = i + 1; j < Math.Min(i + 6, instructions.Count); j++)
                    {
                        if (instructions[j].OpCode == OpCodes.Ldc_I4_0 ||
                            (instructions[j].OpCode == OpCodes.Ldc_I4_S && (sbyte)instructions[j].Operand == 0) ||
                            (instructions[j].OpCode == OpCodes.Ldc_I4 && (int)instructions[j].Operand == 0))
                        {
                            il.Replace(instructions[j], il.Create(OpCodes.Ldc_I4_1));
                            patched = true;
                            break;
                        }
                    }

                    if (patched)
                    {
                        break;
                    }
                }
            }

            if (patched)
            {
                Console.WriteLine("Patched HeroCostumeView.OnClickClosePhoto to keep WidgetViewMotion visible");
            }
            else
            {
                Console.WriteLine("WARNING: Could not locate WidgetViewMotion disable flag in OnClickClosePhoto");
            }
        }

        static void PatchCreatureOpenTypeGetter(ModuleDefinition module)
        {
            var creatureDataType = module.Types.FirstOrDefault(t => t.FullName == "NShared.CreatureData");
            if (creatureDataType == null)
            {
                Console.WriteLine("WARNING: NShared.CreatureData type not found (OpenType patch skipped)");
                return;
            }

            var getter = creatureDataType.Methods.FirstOrDefault(m => m.Name == "get_OpenType" && !m.HasParameters);
            var backingField = creatureDataType.Fields.FirstOrDefault(f => f.Name == "<OpenType>k__BackingField");
            var indexField = creatureDataType.Fields.FirstOrDefault(f => f.Name == "<Index>k__BackingField");
            if (getter == null || backingField == null || indexField == null)
            {
                Console.WriteLine("WARNING: CreatureData OpenType/Index backing fields not found (OpenType patch skipped)");
                return;
            }

            getter.Body.Instructions.Clear();
            getter.Body.ExceptionHandlers.Clear();
            getter.Body.Variables.Clear();
            getter.Body.InitLocals = false;

            var il = getter.Body.GetILProcessor();
            var returnNone = il.Create(OpCodes.Ldc_I4_0); // CreatureOpenType.None
            var returnOpened = il.Create(OpCodes.Ldc_I4_3); // CreatureOpenType.Opened
            var returnExisting = il.Create(OpCodes.Ret);

            // value = this.<OpenType>k__BackingField;
            // if (value != CreatureOpenType.None) return value;
            // Only open known hero indices when source OpenType is None.
            // if (index < 1) return None;
            // if (index <= 102 || index == 111) return Opened;
            // return None;
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, backingField));
            il.Append(il.Create(OpCodes.Dup));
            il.Append(il.Create(OpCodes.Ldc_I4_0)); // CreatureOpenType.None
            il.Append(il.Create(OpCodes.Bne_Un_S, returnExisting));
            il.Append(il.Create(OpCodes.Pop));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, indexField));
            il.Append(il.Create(OpCodes.Ldc_I4_1));
            il.Append(il.Create(OpCodes.Blt_S, returnNone));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, indexField));
            il.Append(il.Create(OpCodes.Ldc_I4, 102));
            il.Append(il.Create(OpCodes.Ble_S, returnOpened));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, indexField));
            il.Append(il.Create(OpCodes.Ldc_I4, 111));
            il.Append(il.Create(OpCodes.Beq_S, returnOpened));
            il.Append(il.Create(OpCodes.Ldc_I4_0)); // CreatureOpenType.None
            il.Append(returnExisting);
            il.Append(returnOpened);
            il.Append(il.Create(OpCodes.Ret));
            il.Append(returnNone);
            il.Append(il.Create(OpCodes.Ret));

            Console.WriteLine("Patched NShared.CreatureData.get_OpenType (None -> Opened only for index 1..102 and 111)");
        }

        static void PatchCostumeStateForPreview(ModuleDefinition module)
        {
            PatchCostumeStateForOpenData(module);

            // Force model-side costume state checks to treat valid costume entries as Owned.
            PatchCostumeStateMethodBySignature(module,
                "NGame2.NUtil.NCostume.HeroCostume",
                "GetCostumeState",
                new[] { "NShared.CostumeData", "NShared.CostumeData", "System.Boolean" },
                1);

            PatchCostumeStateMethodBySignature(module,
                "NGame2.NUtil.NCostume.HairCostume",
                "GetCostumeState",
                new[] { "NShared.HairCostumeData", "NShared.HairCostumeData", "System.Boolean" },
                1);

            PatchCostumeStateMethodBySignature(module,
                "NGame2.NUtil.NCostume.WeaponCostume",
                "GetCostumeState",
                new[] { "NShared.WeaponCostumeData", "NShared.WeaponCostumeData", "System.Boolean" },
                1);

            PatchCostumeStateMethodBySignature(module,
                "NGame2.NUtil.NCostume.AccessoryCostume",
                "GetCostumeState",
                new[] { "NShared.HeroInfo", "NShared.AccessoryCostumeData", "System.Boolean" },
                1);
        }

        static void PatchCostumeStateForOpenData(ModuleDefinition module)
        {
            // These overloads can still return NeedHero for unowned heroes, which blocks 3D preview/apply.
            PatchCostumeStateMethodBySignature(module,
                "NGame2.NUtil.NCostume.HairCostume",
                "GetCostumeState",
                new[] { "NShared.HeroInfo", "NGame2.NUI.NComponent2.NCostume.HairCostumeComponent/OpenData", "System.Boolean" },
                1);

            PatchCostumeStateMethodBySignature(module,
                "NGame2.NUtil.NCostume.WeaponCostume",
                "GetCostumeState",
                new[] { "NShared.HeroInfo", "NGame2.NUI.NComponent2.NCostume.WeaponCostumeComponent/OpenData", "System.Boolean" },
                1);

            PatchCostumeStateMethodBySignature(module,
                "NGame2.NUtil.NCostume.AccessoryCostume",
                "GetCostumeState",
                new[] { "NShared.HeroInfo", "NGame2.NUI.NComponent2.NCostume.AccessoryCostumeComponent/OpenData", "System.Boolean" },
                1);
        }

        static void PatchCostumeStateMethodBySignature(ModuleDefinition module, string typeName, string methodName, string[] parameterTypeNames, int costumeDataArgIndex)
        {
            var type = module.Types.FirstOrDefault(t => t.FullName == typeName);
            if (type == null)
            {
                Console.WriteLine($"WARNING: {typeName} not found ({methodName} state patch skipped)");
                return;
            }

            var method = type.Methods.FirstOrDefault(m =>
                m.Name == methodName &&
                m.Parameters.Count == parameterTypeNames.Length &&
                m.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(parameterTypeNames));

            if (method == null)
            {
                Console.WriteLine($"WARNING: {typeName}.{methodName}({string.Join(",", parameterTypeNames)}) not found");
                return;
            }

            method.Body.Instructions.Clear();
            method.Body.ExceptionHandlers.Clear();
            method.Body.Variables.Clear();
            method.Body.InitLocals = false;

            var il = method.Body.GetILProcessor();
            var hasCostumeData = il.Create(OpCodes.Ldc_I4_2); // CostumeState.Owned

            // if (costumeDataArg == null) return CostumeState.None; else return CostumeState.Owned;
            switch (costumeDataArgIndex)
            {
                case 0:
                    il.Append(il.Create(OpCodes.Ldarg_1));
                    break;
                case 1:
                    il.Append(il.Create(OpCodes.Ldarg_2));
                    break;
                case 2:
                    il.Append(il.Create(OpCodes.Ldarg_3));
                    break;
                default:
                    il.Append(il.Create(OpCodes.Ldarg, method.Parameters[costumeDataArgIndex]));
                    break;
            }
            il.Append(il.Create(OpCodes.Brtrue_S, hasCostumeData));
            il.Append(il.Create(OpCodes.Ldc_I4_0)); // CostumeState.None
            il.Append(il.Create(OpCodes.Ret));
            il.Append(hasCostumeData);
            il.Append(il.Create(OpCodes.Ret));

            Console.WriteLine($"Patched {typeName}.{methodName} costume state => Owned for valid costume data");
        }

        static void PatchBooleanGetter(ModuleDefinition module, string typeName, string getterName, bool value)
        {
            var type = module.Types.FirstOrDefault(t => t.FullName == typeName);
            if (type == null)
            {
                Console.WriteLine($"WARNING: {typeName} not found ({getterName} patch skipped)");
                return;
            }

            var getter = type.Methods.FirstOrDefault(m => m.Name == getterName && !m.HasParameters);
            if (getter == null)
            {
                Console.WriteLine($"WARNING: {typeName}.{getterName} not found");
                return;
            }

            getter.Body.Instructions.Clear();
            getter.Body.ExceptionHandlers.Clear();
            getter.Body.Variables.Clear();
            getter.Body.InitLocals = false;

            var il = getter.Body.GetILProcessor();
            il.Append(il.Create(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
            il.Append(il.Create(OpCodes.Ret));

            Console.WriteLine($"Patched {typeName}.{getterName} => {value}");
        }
        
        /// <summary>
        /// Patches StateBase_InAppBilling to skip payment error popup.
        /// When payment initialization fails (e.g., Steam not running), instead of showing
        /// an error popup and blocking login, we just continue as if it succeeded.
        /// 
        /// The method coRun() is an IEnumerator (coroutine) which makes IL patching complex.
        /// We'll patch the compiler-generated state machine class instead.
        /// </summary>
        static void PatchPaymentErrorPopup(ModuleDefinition module)
        {
            // The coRun method is compiled into a state machine class
            // Find the StateBase_InAppBilling type first
            var stateBaseType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NLogin.NState.StateBase_InAppBilling");
            if (stateBaseType == null)
            {
                Console.WriteLine("ERROR: StateBase_InAppBilling type not found!");
                return;
            }
            Console.WriteLine($"Found type: {stateBaseType.FullName}");
            
            // Find the nested state machine class (compiler-generated for the coroutine)
            // It's typically named something like "<coRun>d__X"
            TypeDefinition stateMachineType = null;
            foreach (var nestedType in stateBaseType.NestedTypes)
            {
                if (nestedType.Name.StartsWith("<coRun>"))
                {
                    stateMachineType = nestedType;
                    break;
                }
            }
            
            if (stateMachineType == null)
            {
                Console.WriteLine("ERROR: coRun state machine type not found!");
                Console.WriteLine("Available nested types:");
                foreach (var nt in stateBaseType.NestedTypes)
                {
                    Console.WriteLine($"  - {nt.Name}");
                }
                return;
            }
            Console.WriteLine($"Found state machine: {stateMachineType.Name}");
            
            // Find the MoveNext method (this is where the actual coroutine code lives)
            var moveNextMethod = stateMachineType.Methods.FirstOrDefault(m => m.Name == "MoveNext");
            if (moveNextMethod == null)
            {
                Console.WriteLine("ERROR: MoveNext method not found!");
                return;
            }
            Console.WriteLine($"Found MoveNext method with {moveNextMethod.Body.Instructions.Count} instructions");
            
            // Find the EResultInit enum type
            var eResultInitType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.IAP.EResultInit");
            if (eResultInitType == null)
            {
                Console.WriteLine("WARNING: EResultInit type not found, trying to find it in nested namespaces...");
            }
            
            // Find the InAppBillingState enum (nested in StateBase_InAppBilling)
            var billingStateType = stateBaseType.NestedTypes.FirstOrDefault(t => t.Name == "InAppBillingState");
            if (billingStateType == null)
            {
                Console.WriteLine("WARNING: InAppBillingState enum not found!");
            }
            
            var il = moveNextMethod.Body.GetILProcessor();
            var instructions = moveNextMethod.Body.Instructions;
            
            // Strategy: Find the comparison with EResultInit.Success and the branch that leads to
            // the error popup code. We want to make the failure case jump to the success path instead.
            //
            // Looking for pattern:
            //   ... call get_TupleResultInit
            //   ... call get_Item1  (gets EResultInit value)
            //   ldc.i4.1  (EResultInit.Success = 1)
            //   beq/bne.un -> error_handling or success_path
            //
            // We'll change the branch so that when init fails, we still go to success path.
            
            Console.WriteLine("\nStrategy: Find all branch points that lead to WaitPopup error paths and redirect them");
            
            // Print out key IL instructions for debugging
            Console.WriteLine("\nKey IL instructions in MoveNext:");
            for (int i = 0; i < instructions.Count; i++)
            {
                var instr = instructions[i];
                // Print brfalse/brtrue/beq/bne instructions
                if (instr.OpCode.FlowControl == FlowControl.Cond_Branch)
                {
                    Console.WriteLine($"  IL_{i:X4} (0x{instr.Offset:X4}): {instr}");
                }
                // Print WaitPopup constructions
                if (instr.OpCode == OpCodes.Newobj)
                {
                    var methodRef = instr.Operand as MethodReference;
                    if (methodRef != null && methodRef.DeclaringType.Name == "WaitPopup")
                    {
                        Console.WriteLine($"  IL_{i:X4} (0x{instr.Offset:X4}): {instr} <-- WaitPopup");
                    }
                }
                // Print ldstr for context
                if (instr.OpCode == OpCodes.Ldstr)
                {
                    var str = instr.Operand as string;
                    if (str != null && (str.Contains("Success") || str.Contains("BILLING") || str.Contains("FAILED")))
                    {
                        Console.WriteLine($"  IL_{i:X4} (0x{instr.Offset:X4}): ldstr \"{str}\"");
                    }
                }
            }
            
            // SIMPLER APPROACH: Find the "Success all" target and make all early error paths go there
            // 
            // Find instruction: ldstr "[{0}] Success all!!!!"
            // That's the success path we want everything to go to
            
            Instruction successAllTarget = null;
            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].OpCode == OpCodes.Ldstr)
                {
                    var str = instructions[i].Operand as string;
                    if (str != null && str.Contains("Success all"))
                    {
                        successAllTarget = instructions[i];
                        Console.WriteLine($"\nFound success target at IL_{i:X4}: ldstr \"{str}\"");
                        break;
                    }
                }
            }
            
            if (successAllTarget == null)
            {
                Console.WriteLine("ERROR: Could not find 'Success all' target instruction!");
            }
            else
            {
                // Strategy: Find ALL brfalse instructions that check for null/failure and redirect
                // the failing paths to successAllTarget
                //
                // Looking at the C# code:
                // - Line 77: if (CheckInAppBilling == null) -> show error
                // - Line 91: if (TupleResultInit.Item1 != Success) -> inner checks -> show error  
                // - Line 118: if (TupleResultQueryInventory.Item1 == Success) -> success, else -> error
                //
                // The key insight is that the "else" block (starting at line 111) eventually leads to
                // the Success path at line 119-123 if everything goes well.
                //
                // Simplest approach: Find the first brfalse after TupleResultQueryInventory and 
                // make it always branch to success
                
                int patchCount = 0;
                
                // First, patch the TupleResultInit check
                // Pattern: ldfld TupleResultInit, ldfld Item1, brfalse IL_02a0
                // We want to change brfalse to unconditional br (but need to pop the value)
                
                for (int i = 0; i < instructions.Count - 3; i++)
                {
                    var instr = instructions[i];
                    
                    if (instr.OpCode == OpCodes.Ldfld)
                    {
                        var fieldRef = instr.Operand as FieldReference;
                        if (fieldRef != null && fieldRef.Name == "TupleResultInit")
                        {
                            // Check if next is ldfld Item1 and then brfalse
                            if (i + 2 < instructions.Count &&
                                instructions[i + 1].OpCode == OpCodes.Ldfld &&
                                instructions[i + 2].OpCode == OpCodes.Brfalse)
                            {
                                var item1Field = instructions[i + 1].Operand as FieldReference;
                                if (item1Field != null && item1Field.Name == "Item1")
                                {
                                    var brTarget = instructions[i + 2].Operand as Instruction;
                                    Console.WriteLine($"\nFound TupleResultInit.Item1 check at IL_{i:X4}");
                                    Console.WriteLine($"  brfalse target: {brTarget}");
                                    
                                    // Change: pop the CheckInAppBilling, nop, unconditional br to success
                                    il.Replace(instr, il.Create(OpCodes.Pop)); // pop CheckInAppBilling
                                    il.Replace(instructions[i + 1], il.Create(OpCodes.Nop));
                                    il.Replace(instructions[i + 2], il.Create(OpCodes.Br, successAllTarget));
                                    
                                    Console.WriteLine($"  Patched: unconditional jump to Success all");
                                    patchCount++;
                                    break; // Only patch the first TupleResultInit check
                                }
                            }
                        }
                    }
                }
                
                Console.WriteLine($"\nTotal patches applied: {patchCount}");
            }
            
            // Now patch the QueryInventory check as well
            // This shows "Cannot make any payments" popup
            Console.WriteLine("\nSearching for QueryInventory check...");
            
            // Looking at the IL structure for QueryInventory:
            //   call get_instance()  <- pushes LoginManager
            //   ldfld CheckInAppBilling  <- pops LoginManager, pushes CheckInAppBilling  
            //   ldfld TupleResultQueryInventory  <- pops CheckInAppBilling, pushes Tuple
            //   ldfld Item1  <- pops Tuple, pushes Item1 (EResultQueryInventory enum)
            //   brfalse IL_043e  <- when Item1 == 0 (Success), jump to success
            //
            // We want to unconditionally go to success
            // Strategy: Replace the ldfld Item1 with a pop (removes Tuple)
            //           Replace brfalse with br to success (no stack change needed)
            
            int queryPatchCount = 0;
            for (int i = 0; i < instructions.Count - 5; i++)
            {
                var instr = instructions[i];
                
                // Look for pattern: ldfld TupleResultQueryInventory followed by ldfld Item1 followed by brfalse
                if (instr.OpCode == OpCodes.Ldfld)
                {
                    var fieldRef = instr.Operand as FieldReference;
                    if (fieldRef != null && fieldRef.Name == "TupleResultQueryInventory")
                    {
                        // Check next instructions
                        if (i + 2 < instructions.Count)
                        {
                            var item1Instr = instructions[i + 1];
                            var branchInstr = instructions[i + 2];
                            
                            if (item1Instr.OpCode == OpCodes.Ldfld && branchInstr.OpCode == OpCodes.Brfalse)
                            {
                                var item1Field = item1Instr.Operand as FieldReference;
                                if (item1Field != null && item1Field.Name == "Item1")
                                {
                                    Console.WriteLine($"Found QueryInventory check at IL_{i:X4}");
                                    var successTarget = branchInstr.Operand as Instruction;
                                    Console.WriteLine($"  brfalse target (success path): {successTarget}");
                                    
                                    // Stack before our changes:
                                    //   ... CheckInAppBilling on stack
                                    //   ldfld TupleResultQueryInventory → pops CheckInAppBilling, pushes Tuple
                                    //   ldfld Item1 → pops Tuple, pushes Item1
                                    //   brfalse → pops Item1, maybe branches
                                    //
                                    // We want to unconditionally branch to success
                                    // Replace:
                                    //   ldfld TupleResultQueryInventory → pop (removes CheckInAppBilling from stack)
                                    //   ldfld Item1 → nop
                                    //   brfalse → br success
                                    
                                    Console.WriteLine($"  Patching: Making unconditional jump to success path");
                                    
                                    il.Replace(instr, il.Create(OpCodes.Pop)); // pop CheckInAppBilling
                                    il.Replace(item1Instr, il.Create(OpCodes.Nop)); // nop
                                    il.Replace(branchInstr, il.Create(OpCodes.Br, successTarget)); // unconditional branch
                                    
                                    queryPatchCount++;
                                    Console.WriteLine($"  Changed to unconditional jump to success at {successTarget}");
                                    
                                    // Only patch once
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            
            if (queryPatchCount > 0)
            {
                Console.WriteLine($"Successfully patched {queryPatchCount} QueryInventory check(s)");
            }
            else
            {
                Console.WriteLine("QueryInventory patch skipped (handled by TupleResultInit patch)");
            }
        }

        /// <summary>
        /// Prompts the user to enter the game client root directory if not provided via CLI.
        /// </summary>
        static string PromptForClientPath()
        {
            string defaultPath = @"D:\client";
            Console.Write($"Enter game client path [default: {defaultPath}]: ");
            string? input = Console.ReadLine()?.Trim().TrimEnd('\\', '/');
            if (string.IsNullOrEmpty(input))
            {
                input = defaultPath;
            }
            Console.WriteLine($"Using client path: {input}");
            return input;
        }
    }
}
