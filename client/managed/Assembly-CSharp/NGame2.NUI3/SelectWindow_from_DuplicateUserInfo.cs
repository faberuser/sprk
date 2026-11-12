using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using NGame2.NUI3.NSelectWindow_from_DuplicateUserInfo;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI3;

internal class SelectWindow_from_DuplicateUserInfo : BaseWindow, IInspectorGuiDrawer
{
	public const string PrefabPath = "Resources/Prefab3/Select_from_DuplicateUserInfo/Select_from_DuplicateUserInfo";

	private static GlobalWindowPrefab<NGame2.NUI3.SelectWindow_from_DuplicateUserInfo> _windowPrefab = new GlobalWindowPrefab<NGame2.NUI3.SelectWindow_from_DuplicateUserInfo>("Resources/Prefab3/Select_from_DuplicateUserInfo/Select_from_DuplicateUserInfo", GlobalUIRoot.ELayerType.GlobalUI);

	public UITable ScrollTable;

	public UserInfo_Entry_Component FirstEntry;

	private Action<long> _onSelect;

	private List<UserInfo_Entry_Component> _entryList = new List<UserInfo_Entry_Component>();

	public static async UniTask<long> OpenGlobalAsync(UserInfo[] duplicateUserInfos)
	{
		if (__Extension_Method_ICollection__.COUNT(duplicateUserInfos) <= 0)
		{
			return 0L;
		}
		bool isSelected = false;
		long selectedAccountId = 0L;
		_windowPrefab.instance.OpenInternal(duplicateUserInfos, delegate(long v)
		{
			selectedAccountId = v;
			isSelected = true;
		});
		await UniTask.WaitWhile(() => !isSelected);
		return selectedAccountId;
	}

	protected void OpenInternal(UserInfo[] duplicateUserInfos, Action<long> onSelect)
	{
		base.Open();
		_onSelect = onSelect;
		if (!_entryList.Contains(FirstEntry))
		{
			_entryList.Insert(0, FirstEntry);
		}
		while (_entryList.Count < __Extension_Method_ICollection__.COUNT(duplicateUserInfos))
		{
			UserInfo_Entry_Component userInfo_Entry_Component = UnityEngine.Object.Instantiate(FirstEntry);
			Vespa_Unity_Transform_Extension_Method.ParentRelative(userInfo_Entry_Component.transform, ScrollTable.transform);
			_entryList.Add(userInfo_Entry_Component);
		}
		while (_entryList.Count > __Extension_Method_ICollection__.COUNT(duplicateUserInfos))
		{
			UnityEngine.Object.Destroy(_entryList[_entryList.Count - 1].gameObject);
			_entryList.RemoveAt(_entryList.Count - 1);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(duplicateUserInfos); i++)
		{
			UserInfo_Entry_Component userInfo_Entry_Component2 = _entryList[i];
			UserInfo userInfo = duplicateUserInfos[i];
			userInfo_Entry_Component2.InitUi(userInfo);
		}
		ScrollTable.Reposition();
	}

	public void OnClick_UserInfo_Entry(UserInfo_Entry_Component component)
	{
		_entryList.IndexOf(component);
		Vespa_System_Action_Extension_Method.fire(_onSelect, component.UserInfo.AccountId);
		_onSelect = null;
		base.Close();
	}

	public void OnClickClose()
	{
		Vespa_System_Action_Extension_Method.fire(_onSelect, 0L);
		_onSelect = null;
		base.Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}
}
