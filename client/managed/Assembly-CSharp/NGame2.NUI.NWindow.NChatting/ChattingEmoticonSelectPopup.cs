using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2.NChatting;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

public class ChattingEmoticonSelectPopup : BaseWindow, ChattingEmoticonComponent.Listener, ChattingEmoticonTabComponent.Listener
{
	public interface Listener
	{
		Vector3 GetRootPosition();

		bool CheckEnableChatting();

		ChatType GetChatType();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Chatting/ChattingEmoticonSelectPopup";

	private static WindowPrefab<ChattingEmoticonSelectPopup> _windowPrefab = new WindowPrefab<ChattingEmoticonSelectPopup>("Resources/Prefab2/Window/Chatting/ChattingEmoticonSelectPopup", GameUIRoot.ELayerType.UI);

	public UIScrollGrid EmoticonGrid;

	public UIScrollGrid EmotionTabGrid;

	private Action<EmoticonData> _onSelect;

	public GameObject DisableRoot;

	public UILabel Disable_Label;

	private bool _isInitializeComplete;

	private Listener _listener;

	private List<Component> _emoticonComponentList = new List<Component>();

	private List<ChattingEmoticonTabComponent> _emoticonTabComponentList = new List<ChattingEmoticonTabComponent>();

	public bool IsInitializeComplete => _isInitializeComplete;

	public static void Open(Listener listener, Action<EmoticonData> onSelect)
	{
		if (_windowPrefab == null || !_windowPrefab.IsValidInstance)
		{
			_windowPrefab = new WindowPrefab<ChattingEmoticonSelectPopup>("Resources/Prefab2/Window/Chatting/ChattingEmoticonSelectPopup", GameUIRoot.ELayerType.UI);
		}
		_windowPrefab.instance.OpenInternal(listener, onSelect);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
	}

	public static void SetEventAlpha(float alpha)
	{
		if (_windowPrefab != null && _windowPrefab.IsValidInstance && _windowPrefab.IsOpen())
		{
			_windowPrefab.instance.SetEventHide(alpha);
		}
	}

	public static void CloseWindow()
	{
		if (_windowPrefab != null && _windowPrefab.IsValidInstance && _windowPrefab.IsOpen())
		{
			_windowPrefab.instance.Close();
		}
	}

	public static bool IsOpenedWindow()
	{
		if (_windowPrefab == null || !_windowPrefab.IsValidInstance)
		{
			return false;
		}
		return _windowPrefab.IsOpen();
	}

	public static void CloseAndResetComponent()
	{
		if (_windowPrefab != null && _windowPrefab.IsValidInstance && _windowPrefab.instance.IsInitializeComplete)
		{
			_windowPrefab.instance.CloseAndReleaseComponent();
		}
	}

	private void OpenInternal(Listener listener, Action<EmoticonData> onSelect)
	{
		_onSelect = onSelect;
		_listener = listener;
		Open(delegate
		{
			CreateTabComponent();
			Setup();
		}, null);
	}

	private void Setup(int tabIndex = 0)
	{
		SetupPosition();
		if (!(EmotionTabGrid == null) && !_isInitializeComplete)
		{
			SetupSelectTab(tabIndex);
			SetupEmoticon();
			_isInitializeComplete = true;
		}
	}

	private void ChangeTab(int tabIndex)
	{
		if (!(EmotionTabGrid == null) && IsEnableSelectTab(tabIndex))
		{
			SetupSelectTab(tabIndex);
			SetupEmoticon();
			_isInitializeComplete = true;
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateDisableState();
	}

	private void UpdateDisableState()
	{
		if (!ShowDiableLabel())
		{
			SetupDisableState(show: false);
			return;
		}
		SetupDisableState(show: true);
		string stringDirect = LocalizationManager.GetStringDirect("CHATTING.CHAT_REMAIN_ENABLE_TIME", GetRemainChatTime().ToString());
		UIHelper.SetLabelText(Disable_Label, stringDirect);
	}

	private void SetupPosition()
	{
		if (_listener != null)
		{
			base.transform.position = _listener.GetRootPosition();
		}
	}

	private bool ShowDiableLabel()
	{
		if (_listener == null)
		{
			return false;
		}
		return _listener.CheckEnableChatting();
	}

	private int GetRemainChatTime()
	{
		if (!ShowDiableLabel())
		{
			return 0;
		}
		ChatType chatType = _listener.GetChatType();
		return SceneAutoBehaviour<ChattingManagement>.instance.GetChatRemainTime(chatType);
	}

	private void SetupDisableState(bool show)
	{
		if (DisableRoot != null && DisableRoot.activeSelf != show)
		{
			UIHelper.EnableGameObject(DisableRoot, show);
		}
	}

	public override void Close()
	{
		base.Close();
	}

	public void CloseAndReleaseComponent()
	{
		base.Close();
		ResetWindowOriginState();
	}

	private void ReleaseComponent()
	{
		ClearEmoticonComponent();
		ClearEmoticonTabComponent();
		SetupDisableState(show: false);
		_isInitializeComplete = false;
	}

	private void ClearEmoticonComponent()
	{
		FactoryBase.ReleaseAndClearList(_emoticonComponentList);
	}

	private void ClearEmoticonTabComponent()
	{
		FactoryBase.ReleaseAndClearList(_emoticonTabComponentList);
	}

	private bool IsEnableSelectTab(int index)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_emoticonTabComponentList); i++)
		{
			_ = _emoticonTabComponentList[i];
			if (_emoticonTabComponentList[i].Index == index)
			{
				return !_emoticonTabComponentList[i].IsSelect;
			}
		}
		return false;
	}

	private void SetupSelectTab(int tabIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_emoticonTabComponentList); i++)
		{
			ChattingEmoticonTabComponent chattingEmoticonTabComponent = _emoticonTabComponentList[i];
			if (tabIndex == 0 && i == 0)
			{
				chattingEmoticonTabComponent.SetSelect(isSelect: true);
			}
			else
			{
				chattingEmoticonTabComponent.SetSelect(chattingEmoticonTabComponent.Index == tabIndex);
			}
		}
	}

	private int GetSelectTabIndex()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_emoticonTabComponentList); i++)
		{
			if (_emoticonTabComponentList[i].IsSelect)
			{
				return _emoticonTabComponentList[i].Index;
			}
		}
		return 0;
	}

	private void CreateTabComponent()
	{
		if (EmotionTabGrid == null || !__Extension_Method_ICollection__.IsNullOrEmpty(_emoticonTabComponentList))
		{
			return;
		}
		IList<EmoticonTabData> tabList = GameRoot.Instance.EmotionTabDataContainer.GetTabList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(tabList); i++)
		{
			ChattingEmoticonTabComponent chattingEmoticonTabComponent = EmotionTabGrid.AddGridItem(ChattingEmoticonTabComponent.PrefabAuto);
			if (chattingEmoticonTabComponent == null)
			{
				VespaDebug.LogError("Failed to create emoticon tab component. index=" + tabList[i].Index);
				continue;
			}
			EmoticonTabData emoticonTabData = tabList[i];
			chattingEmoticonTabComponent.Open(this, emoticonTabData.Index, emoticonTabData.AssetPath);
			_emoticonTabComponentList.Add(chattingEmoticonTabComponent);
		}
		UIHelper.RepositionGrid(EmotionTabGrid);
	}

	private void SetupEmoticon()
	{
		if (!(EmoticonGrid == null))
		{
			ClearEmoticonComponent();
			CreateEmoticon(GetSelectTabIndex());
		}
	}

	private void CreateEmoticon(int tabIndex)
	{
		IList<EmoticonData> datasByTabIndex = GameRoot.Instance.EmoticonDataContainer.GetDatasByTabIndex(tabIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(datasByTabIndex))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(datasByTabIndex); i++)
		{
			EmoticonData emoticonData = datasByTabIndex[i];
			ChattingEmoticonComponent chattingEmoticonComponent = EmoticonGrid.AddGridItem(ChattingEmoticonComponent.PrefabAuto);
			if (chattingEmoticonComponent == null)
			{
				VespaDebug.LogError("Failed to create emoticon component. index=" + emoticonData.Index);
				continue;
			}
			chattingEmoticonComponent.Open(this, emoticonData);
			_emoticonComponentList.Add(chattingEmoticonComponent);
		}
		EmoticonGrid.RepositionGrid();
	}

	void ChattingEmoticonTabComponent.Listener.OnClickEmoticonTab(ChattingEmoticonTabComponent component)
	{
		ChangeTab(component.Index);
	}

	void ChattingEmoticonComponent.Listener.OnClickEmoticon(ChattingEmoticonComponent component)
	{
		if (GetRemainChatTime() <= 0)
		{
			Vespa_System_Action_Extension_Method.fire(_onSelect, component.EmoticonData);
			Close();
		}
	}
}
