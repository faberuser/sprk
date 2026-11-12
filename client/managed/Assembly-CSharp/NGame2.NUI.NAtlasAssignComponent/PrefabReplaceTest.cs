using System;
using NNguiExtension;
using UnityEngine;

namespace NGame2.NUI.NAtlasAssignComponent;

public class PrefabReplaceTest : MonoBehaviour
{
	[ContextMenu("Save")]
	public void TestSave()
	{
		LoopUISpriteSelectorAssignComponent(delegate(UISpriteSelectorAtlasAssigner assigner)
		{
			assigner.Save();
		});
		LoopUISpriteAssignComponent(delegate(UISpriteAtlasAssigner assigner)
		{
			assigner.Save();
		});
	}

	[ContextMenu("Assign")]
	public void TestAssign()
	{
		LoopUISpriteSelectorAssignComponent(delegate(UISpriteSelectorAtlasAssigner assigner)
		{
			assigner.Assign();
		});
		LoopUISpriteAssignComponent(delegate(UISpriteAtlasAssigner assigner)
		{
			assigner.Assign();
		});
	}

	private void LoopUISpriteAssignComponent(Action<UISpriteAtlasAssigner> action)
	{
		UISprite[] componentsInChildren = base.gameObject.GetComponentsInChildren<UISprite>(includeInactive: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
		{
			UISprite uISprite = componentsInChildren[i];
			UISpriteAtlasAssigner componentAuto = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<UISpriteAtlasAssigner>(uISprite.gameObject);
			if (componentAuto.Target == null)
			{
				componentAuto.Target = uISprite;
			}
			Vespa_System_Action_Extension_Method.fire(action, componentAuto);
		}
	}

	private void LoopUISpriteSelectorAssignComponent(Action<UISpriteSelectorAtlasAssigner> action)
	{
		UISpriteSelector[] componentsInChildren = base.gameObject.GetComponentsInChildren<UISpriteSelector>(includeInactive: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
		{
			UISpriteSelector uISpriteSelector = componentsInChildren[i];
			UISpriteSelectorAtlasAssigner componentAuto = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<UISpriteSelectorAtlasAssigner>(uISpriteSelector.gameObject);
			if (componentAuto.Target == null)
			{
				componentAuto.Target = uISpriteSelector;
			}
			Vespa_System_Action_Extension_Method.fire(action, componentAuto);
		}
	}
}
