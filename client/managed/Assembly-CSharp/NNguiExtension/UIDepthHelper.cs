using NVespa;
using NVespa.NPrefab2.NEvent;
using UnityEngine;

namespace NNguiExtension;

public class UIDepthHelper : MonoBehaviour, IDepthHelper, IReleaseEvent
{
	private IncreaseDic<UIWidget, int> _backupDic = new IncreaseDic<UIWidget, int>();

	public static void RearrangeWidgetDepth(GameObject go, int beginDepth)
	{
		UIDepthHelper uIDepthHelper = go.GetComponent<UIDepthHelper>();
		if (uIDepthHelper == null)
		{
			uIDepthHelper = go.AddComponent<UIDepthHelper>();
		}
		uIDepthHelper.RearrangeWidgetDepth(beginDepth);
	}

	void IReleaseEvent.OnRelease()
	{
		for (int i = 0; i < _backupDic.Count; i++)
		{
			UIWidget keyByIndex = _backupDic.GetKeyByIndex(i);
			if (!(keyByIndex == null))
			{
				keyByIndex.depth = _backupDic.GetValueByIndex(i);
			}
		}
		_backupDic.Clear();
	}

	public int RearrangeWidgetDepth(int beginDepth)
	{
		return RearrangeWidgetDepth(base.transform, beginDepth);
	}

	private int RearrangeWidgetDepth(Transform tm, int beginDepth)
	{
		int num = beginDepth;
		UIWidget component = tm.gameObject.GetComponent<UIWidget>();
		if (component != null)
		{
			if (!_backupDic.ContainsKey(component))
			{
				_backupDic.Add(component, component.depth);
			}
			component.depth = num;
			num += Mathf.Max(1, component.reserveDepth);
		}
		for (int i = 0; i < tm.childCount; i++)
		{
			num = RearrangeWidgetDepth(tm.GetChild(i), num);
		}
		return num;
	}
}
