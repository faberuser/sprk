using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static bool PatchHeroInnButtons(ModuleDefinition module, TypeDefinition? componentType, TypeDefinition? gameObjectType)
    {
        // ================================================================
        // PATCH HeroInnViewButtonGroup (same as before)
        // ================================================================
        Console.WriteLine("\n=== Patching HeroInnViewButtonGroup ===");

        // Find the HeroInnViewButtonGroup type
        var buttonGroupType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroInnViewButtonGroup");
        if (buttonGroupType == null)
        {
            Console.WriteLine("ERROR: HeroInnViewButtonGroup type not found!");
            return false;
        }
        Console.WriteLine($"Found type: {buttonGroupType.FullName}");

        // Find the HeroInnViewButton type
        var buttonType = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroInnViewButton");
        if (buttonType == null)
        {
            Console.WriteLine("ERROR: HeroInnViewButton type not found!");
            return false;
        }

        // Find the SetListener method
        var setListenerMethod = buttonGroupType.Methods.FirstOrDefault(m => m.Name == "SetListener");
        if (setListenerMethod == null)
        {
            Console.WriteLine("ERROR: SetListener method not found!");
            return false;
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
                return false;
            }

            // Find the ActionType field in HeroInnViewButton
            var actionTypeField = buttonType.Fields.FirstOrDefault(f => f.Name == "ActionType");
            if (actionTypeField == null)
            {
                Console.WriteLine("ERROR: ActionType field not found!");
                return false;
            }

            if (componentType == null || gameObjectType == null)
            {
                Console.WriteLine("ERROR: Unity types not found!");
                return false;
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


        return true;
    }
}
