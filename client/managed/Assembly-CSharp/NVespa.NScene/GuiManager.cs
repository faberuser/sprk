using System;
using System.Collections.Generic;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NVespa.NScene;

public class GuiManager : SceneAutoBehaviour<GuiManager>
{
	public class GuiAction
	{
		public WeakReference Owner;

		public Action DrawGui;
	}

	private IncreaseDic<string, List<GuiAction>> _dic = new IncreaseDic<string, List<GuiAction>>();

	public void Add(string actionName, object owner, Action guiAction)
	{
		if (owner != null)
		{
			actionName = (String_Extension_Method.IsNullOrEmpty(actionName) ? "(default)" : actionName);
			Remove(actionName, owner);
			GetGUIAction(actionName).Add(new GuiAction
			{
				Owner = new WeakReference(owner),
				DrawGui = guiAction
			});
		}
	}

	public void Remove(string actionName, object owner)
	{
		actionName = (String_Extension_Method.IsNullOrEmpty(actionName) ? "(default)" : actionName);
		List<GuiAction> gUIAction = GetGUIAction(actionName);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(gUIAction); i++)
		{
			if (gUIAction[i].Owner.Target == owner)
			{
				gUIAction.RemoveAt(i);
				break;
			}
		}
	}

	private void Start()
	{
		if (!GlobalAutoBehaviour<NVespa.NGlobal.GuiManager>.isValidInstance)
		{
			GlobalAutoBehaviour<NVespa.NGlobal.GuiManager>.CreateInstance();
		}
	}

	private void Update()
	{
		for (int i = 0; i < _dic.Count; i++)
		{
			List<GuiAction> valueByIndex = _dic.GetValueByIndex(i);
			for (int j = 0; j < valueByIndex.Count; j++)
			{
				if (Vespa_Common_Class_Extension_Method.IsUnityNull(valueByIndex[j].Owner.Target))
				{
					valueByIndex.RemoveAt(j);
					j--;
				}
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(valueByIndex))
			{
				_dic.RemoveByIndex(i);
				i--;
			}
		}
	}

	public void CollectView(ref PoolingList<string> actionNameList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_dic); i++)
		{
			actionNameList.Add(_dic.GetKeyByIndex(i));
		}
	}

	public List<GuiAction> GetGUIActionValid(string actionName)
	{
		actionName = (String_Extension_Method.IsNullOrEmpty(actionName) ? "(default)" : actionName);
		return _dic.GETVALUE(actionName, null);
	}

	private List<GuiAction> GetGUIAction(string actionName)
	{
		actionName = (String_Extension_Method.IsNullOrEmpty(actionName) ? "(default)" : actionName);
		if (_dic.TryGetValue(actionName, out var o_value))
		{
			return o_value;
		}
		o_value = new List<GuiAction>();
		_dic.Add(actionName, o_value);
		return o_value;
	}
}
