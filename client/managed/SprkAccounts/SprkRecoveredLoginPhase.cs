using System.Collections;
using System.Linq;
using NGame2.NUI;
using NGameAsset.NEffect;
using UnityEngine;

public sealed class SprkRecoveredLoginPhase : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return null;
		UIGame3DWidgetFromRes[] componentsInChildren = GetComponentsInChildren<UIGame3DWidgetFromRes>(includeInactive: true);
		foreach (UIGame3DWidgetFromRes uIGame3DWidgetFromRes in componentsInChildren)
		{
			if (uIGame3DWidgetFromRes.gameObject.activeInHierarchy && uIGame3DWidgetFromRes.EntryList.Any((UIGame3DWidgetFromRes.EntryData e) => e.ResourceObject is OpenDataFromResource && ((OpenDataFromResource)e.ResourceObject).Code.StartsWith("UI.Title.Logo")))
			{
				uIGame3DWidgetFromRes.PlayPhase("Play");
			}
		}
	}
}
