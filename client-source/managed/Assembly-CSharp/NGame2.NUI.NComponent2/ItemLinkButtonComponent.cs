using NGame2.NUI.NComponent2.NChatting;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ItemLinkButtonComponent : LinkFactoryComponent, IItemLink
{
	public interface Listener
	{
		void OnSubmit(ChatType chatType, LinkedItemInfo lastLinkedItem, long accountId = 0L, string userName = null);
	}

	public enum EItemLinkButtonType
	{
		None,
		Link,
		Share
	}

	public static PrefabAuto<ItemLinkButtonComponent> PrefabAuto = new PrefabAuto<ItemLinkButtonComponent>("Resources/Prefab2/Component/Icon/ItemLinkButtonComponent");

	public const string PrefabPath = "Resources/Prefab2/Component/Icon/ItemLinkButtonComponent";

	private static LinkedItemInfo lastLinkedItem = null;

	private LinkedItemInfo currentItemInfo;

	public GameObject ItemLinkButton;

	public UIButton Button_Link;

	public UIButton Button_Share;

	private WhisperUserComponent WhisperUserComponent;

	private ChatType _chatType = ChatType.Channel;

	private Listener _listener;

	public static void AddComponent(ref GameObject ItemLinkButtonRoot, ref ItemLinkButtonComponent component, EItemLinkButtonType ButtonType, ChatType chatType)
	{
		if (ItemLinkButtonRoot != null)
		{
			if (component == null)
			{
				component = PrefabAuto.Factory.Create(ItemLinkButtonRoot.transform);
			}
			component.Open(ButtonType);
			component.SetChatType(chatType);
		}
		SprkChatChannels.AddSendButton(component);
	}

	public void Open(EItemLinkButtonType buttonType, LinkedItemInfo equipItem = null)
	{
		_listener = SceneAutoBehaviour<ChattingManagement>.instance;
		UIHelper.EnableGameObject(Button_Link, enable: false);
		UIHelper.EnableGameObject(Button_Share, enable: false);
		switch (buttonType)
		{
		case EItemLinkButtonType.Link:
			UIHelper.EnableGameObject(Button_Link, enable: true);
			SetupEquipItemInfo(equipItem);
			break;
		case EItemLinkButtonType.Share:
			UIHelper.EnableGameObject(Button_Share, enable: true);
			break;
		case EItemLinkButtonType.None:
			break;
		}
	}

	public static void SetLastLinkedItem(LinkedItemInfo linkedItem)
	{
		if (linkedItem != null)
		{
			linkedItem = JsonMarshaler.Clone(linkedItem);
		}
		lastLinkedItem = linkedItem;
	}

	public void OnPressLinkButton()
	{
		if (currentItemInfo != null)
		{
			lastLinkedItem = JsonMarshaler.Clone(currentItemInfo);
			CenterMessage.Open(LocalizationManager.GetStringDirect("ITEM_LINK_MESSAGE"));
		}
	}

	public void OnPressShareButton()
	{
		if (lastLinkedItem != null && _listener != null)
		{
			long accountId = 0L;
			string userName = null;
			if (WhisperUserComponent != null)
			{
				accountId = WhisperUserComponent.AccountId;
				userName = WhisperUserComponent.UserName;
			}
			_listener.OnSubmit(_chatType, lastLinkedItem, accountId, userName);
			lastLinkedItem = null;
			currentItemInfo = null;
		}
	}

	public void SetChatType(ChatType chatType)
	{
		_chatType = chatType;
	}

	public void SetWhisperUserComponent(WhisperUserComponent _WhisperUserComponent)
	{
		WhisperUserComponent = _WhisperUserComponent;
	}

	public void SetupEquipItemInfo(LinkedItemInfo equipItem)
	{
		currentItemInfo = equipItem;
	}

	public void UpdateShareButtonState()
	{
		bool enable = false;
		if (lastLinkedItem != null)
		{
			enable = true;
		}
		UIHelper.EnableButton(Button_Share, enable, grayscaleIfDisable: true);
	}

	public void UpdateShareButtonState(bool enable)
	{
		if (lastLinkedItem != null)
		{
			UIHelper.EnableButton(Button_Share, enable, grayscaleIfDisable: true);
		}
		else
		{
			UIHelper.EnableButton(Button_Share, enable: false, grayscaleIfDisable: true);
		}
	}
}
