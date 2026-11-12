using System;
using System.Collections;
using UnityEngine;

namespace NGame2.NUI.NUIEffect;

public static class UIEffectUtility
{
	public static void PlayEffectAndCallback(string uiEffectPrefabName, string[] uiEffectValues, Transform root, Action callback)
	{
		VespaCoroutine.Start(coPlayEffectAndCallback(uiEffectPrefabName, uiEffectValues, root, callback));
	}

	private static IEnumerator coPlayEffectAndCallback(string uiEffectPrefabName, string[] uiEffectValues, Transform root, Action callback)
	{
		if (String_Extension_Method.IsNullOrEmpty(uiEffectPrefabName))
		{
			Vespa_System_Action_Extension_Method.fire(callback);
			yield break;
		}
		GameObject gameObject = VespaResource.Load<GameObject>("Prefab2/UIEffect/" + uiEffectPrefabName);
		if (gameObject == null)
		{
			Vespa_System_Action_Extension_Method.fire(callback);
			yield break;
		}
		GameObject effectObject = UnityEngine.Object.Instantiate(gameObject);
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(effectObject.transform, root);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(effectObject, root.gameObject.layer, forceRecursive: true);
		effectObject.SetActive(value: true);
		BaseUIEffect component = effectObject.GetComponent<BaseUIEffect>();
		if ((bool)component)
		{
			component.Play(uiEffectValues);
		}
		yield return null;
		while (!Vespa_Common_Class_Extension_Method.IsUnityNull(effectObject) && effectObject.activeSelf)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(callback);
	}
}
