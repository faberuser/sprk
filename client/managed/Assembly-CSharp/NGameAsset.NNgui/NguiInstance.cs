using System.Collections.Generic;
using NGameAsset.NNgui.NComponent;
using UnityEngine;

namespace NGameAsset.NNgui;

public class NguiInstance
{
	public List<WidgetInstance> WidgetList = new List<WidgetInstance>();

	public GameObject Go { get; private set; }

	public bool Open(GameObject prefabGo, NguiData data, Transform parentTm, int layer)
	{
		if (prefabGo == null)
		{
			return false;
		}
		if (parentTm == null)
		{
			return false;
		}
		Go = Object.Instantiate(prefabGo);
		Vespa_Unity_Transform_Extension_Method.ParentRelative(Go.transform, parentTm);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(Go, layer);
		for (int i = 0; i < data.NguiComponent.WidgetList.Count; i++)
		{
			WidgetData widgetData = data.NguiComponent.WidgetList[i];
			Transform transform = Go.transform.Find(widgetData.Path);
			if (!(transform == null))
			{
				WidgetInstance widgetInstance = new WidgetInstance();
				widgetInstance.Data = widgetData;
				widgetInstance.Go = transform.gameObject;
				widgetInstance.Widget = widgetInstance.Go.GetComponent<UIWidget>();
				WidgetList.Add(widgetInstance);
			}
		}
		return true;
	}

	public void Close()
	{
		Go.SetActive(value: false);
		Object.Destroy(Go);
		Go = null;
	}
}
