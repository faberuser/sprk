using System;
using System.Collections.Generic;
using NGame2.NChatting;
using NGame2.NUI.NComponent2.NChatting;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

public class ChattingListComponentBase : LinkFactoryComponent, ChattingComponent.Listener
{
	public interface Listener
	{
		void OnClickChatUser(ChatInfo chatInfo);

		void OnClickEmoticonSelect();
	}

	public UIInput Input;

	public Transform ScrollPanelRoot;

	public UIScrollView ScrollView;

	public bool IsRaidParty;

	public UIButton EmoticonSelectButton;

	private Listener _listener;

	private List<ChattingComponent> _componentList = new List<ChattingComponent>();

	public bool IsOpen { get; private set; }

	public void Open(Listener listener)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_listener = listener;
		IsOpen = true;
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		IsOpen = false;
		Reset();
	}

	protected void UpdateEmoticonSelectButtonState()
	{
		if ((bool)EmoticonSelectButton && (bool)Input)
		{
			bool flag = EmoticonSelectButton.state == UIButtonColor.State.Normal;
			bool flag2 = UIHelper.IsEnabledGameObjet(Input);
			if (flag2 != flag)
			{
				UIHelper.EnableButton(EmoticonSelectButton, flag2);
			}
		}
	}

	protected void CreateDefaultChat()
	{
	}

	public virtual void Reset()
	{
		ClearChat();
	}

	public void ClearChat()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public ChattingComponent GetChattingComponent(ChatInfo chatInfo)
	{
		foreach (ChattingComponent component in _componentList)
		{
			if (component.ChatInfo == chatInfo)
			{
				return component;
			}
		}
		return null;
	}

	public void AddChat(ChatInfo chatInfo)
	{
		PrefabAuto<ChattingComponent> prefabAuto = null;
		if (prefabAuto == null)
		{
			prefabAuto = ChattingComponent.GetPrefabAuto(chatInfo, IsRaidParty);
			if (prefabAuto == null)
			{
				return;
			}
		}
		AddChat(chatInfo, prefabAuto);
	}

	public void AddChat(ChatInfo chatInfo, PrefabAuto<ChattingComponent> prefabAuto)
	{
		AddChat(chatInfo, () => prefabAuto.Factory.Create(ScrollPanelRoot));
	}

	public void AddChat(ChatInfo chatInfo, Func<ChattingComponent> factory)
	{
		if (chatInfo != null && (!string.IsNullOrEmpty(chatInfo.Chat) || chatInfo.EmoticonIndex != 0))
		{
			if (chatInfo.EmoticonIndex != 0 && !String_Extension_Method.IsNullOrEmpty(chatInfo.Chat))
			{
				ChattingComponent component = factory();
				string chat = chatInfo.Chat;
				chatInfo.Chat = string.Empty;
				AddChat(chatInfo, component, chatOnly: false);
				Reposition();
				ChattingComponent component2 = factory();
				chatInfo.Chat = chat;
				AddChat(chatInfo, component2, chatOnly: true);
			}
			else
			{
				ChattingComponent component3 = factory();
				AddChat(chatInfo, component3, chatOnly: false);
			}
		}
	}

	public void AddChat(ChatInfo chatInfo, ChattingComponent component, bool chatOnly)
	{
		AddChatInternal(chatInfo, component, chatOnly);
	}

	public void AddChat(IEnumerable<ChatInfo> chatInfos)
	{
		if (chatInfos == null)
		{
			return;
		}
		foreach (ChatInfo chatInfo in chatInfos)
		{
			AddChat(chatInfo);
		}
	}

	private void AddChatInternal(ChatInfo chatInfo, ChattingComponent component, bool chatOnly)
	{
		float z = ScrollView.panel.finalClipRegion.z;
		component.Open(chatInfo, (int)z, this, chatOnly);
		int i = 0;
		while (_componentList.Count > ChattingManager.MaxChatCount)
		{
			for (; _componentList[i].ChatInfo.isAlwaysShow; i++)
			{
			}
			ChattingComponent comp = _componentList[i];
			_componentList.RemoveAt(i);
			FactoryBase.SafeReleaseComponent(ref comp);
		}
		_componentList.Add(component);
	}

	public void Reposition(bool forceReset = false)
	{
		int totalHeight = 0;
		foreach (ChattingComponent component in _componentList)
		{
			totalHeight = component.UpdatePos(totalHeight);
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_componentList))
		{
			float num = Vespa_Generic_IList_Extension_Method.GetLast(_componentList).GetComponent<Transform>().localPosition[1];
			float y = ScrollView.GetComponent<UIPanel>().finalClipRegion.y;
			float num2 = ScrollView.GetComponent<UIPanel>().finalClipRegion.w / 2f;
			float num3 = y - num2;
			if (num > num3 || forceReset)
			{
				ScrollView.ResetPosition();
			}
		}
	}

	void ChattingComponent.Listener.OnClick(ChattingComponent component)
	{
		if (component == null)
		{
			return;
		}
		ChatInfo chatInfo = component.ChatInfo;
		if (chatInfo != null)
		{
			if (chatInfo.Action != null && chatInfo.ActionData != null)
			{
				chatInfo.Action.Run(chatInfo.ActionData);
			}
			else if (chatInfo.OnClick != null)
			{
				chatInfo.OnClick(chatInfo);
			}
		}
	}

	void ChattingComponent.Listener.OnClickName(ChattingComponent component)
	{
		if (_listener != null)
		{
			_listener.OnClickChatUser(component.ChatInfo);
		}
	}

	public void OnClickEmoticonSelect()
	{
		if (_listener != null)
		{
			_listener.OnClickEmoticonSelect();
		}
	}
}
