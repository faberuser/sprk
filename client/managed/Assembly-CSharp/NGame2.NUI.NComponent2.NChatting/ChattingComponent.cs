using System;
using System.Collections.Generic;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NChatting;

public class ChattingComponent : LinkFactoryComponent, UICamera.IOnClickListener
{
	public interface Listener
	{
		void OnClick(ChattingComponent component);

		void OnClickName(ChattingComponent component);
	}

	private static PrefabAuto<ChattingComponent> prefabSystemChat = new PrefabAuto<ChattingComponent>("Prefab2/Component/Chatting/ChattingComponentSystem");

	private static PrefabAuto<ChattingComponent> prefabChat = new PrefabAuto<ChattingComponent>("Prefab2/Component/Chatting/ChattingComponent");

	private static PrefabAuto<ChattingComponent> prefabBattleSystemChat = new PrefabAuto<ChattingComponent>("Prefab2/Component/BattleChatting/BattleChattingComponentSystem");

	private static PrefabAuto<ChattingComponent> prefabBattleChat = new PrefabAuto<ChattingComponent>("Prefab2/Component/BattleChatting/BattleChattingComponent");

	private static PrefabAuto<ChattingComponent> prefabRaidPartySystemChat = new PrefabAuto<ChattingComponent>("Prefab2/Component/RaidPartyChatting/ChattingComponentSystem");

	private static PrefabAuto<ChattingComponent> prefabRaidPartyChat = new PrefabAuto<ChattingComponent>("Prefab2/Component/RaidPartyChatting/ChattingComponent");

	public const int RecentTimeSecond = 10;

	public const float CompBottomSpace = 13f;

	public UIWidget RootWidget;

	public Transform ChatRoot;

	public Transform NonChatRoot;

	public GameObject PortraitRoot;

	public UITexture PortraitTexture;

	public UILabel NameLabel;

	public UILabel ChatLabel;

	public UILabel SystemChatLabel;

	public UILabel TimeLabel;

	public UILabel TimeRecentLabel;

	public GameObject SplitterObject;

	public int ChatWidthMargin = 300;

	public UIButton Button_Chat;

	public UITable ChatTable;

	public UIBundleLoader EmoticonLoader;

	public GameObject DefaultIconObject;

	public UISprite WorldIconSprite;

	public UISprite PvPIconSprite;

	public UISprite PvEIconSprite;

	protected Listener _listener;

	protected bool _isRecent;

	protected DateTime _addedTime;

	protected bool _chatOnly;

	private Component _portraitFrameComponent;

	private ChatCase _chatCase;

	private PoolingDic<RankViewType, IconChatWindowData> dataDic;

	public ChatInfo ChatInfo { get; set; }

	public static PrefabAuto<ChattingComponent> GetPrefabAuto(ChatInfo chatInfo, bool isRaidPartyChat)
	{
		if (chatInfo == null)
		{
			return null;
		}
		if (isRaidPartyChat)
		{
			if (!chatInfo.IsSystem)
			{
				return prefabRaidPartyChat;
			}
			return prefabRaidPartySystemChat;
		}
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			if (chatInfo.IsSystem)
			{
				return prefabSystemChat;
			}
			return prefabChat;
		}
		if (chatInfo.IsSystem)
		{
			return prefabBattleSystemChat;
		}
		return prefabBattleChat;
	}

	public virtual void Open(ChatInfo chatInfo, int width, Listener listener, bool chatOnly)
	{
		ChatInfo = null;
		_listener = listener;
		_isRecent = false;
		_addedTime = NGame2.NUI.NWindow.Common.GetUTCNow();
		_chatOnly = chatOnly;
		ChatInfo = chatInfo;
		if ((bool)ChatLabel && ChatWidthMargin > 0)
		{
			ChatLabel.width = System.Math.Max(width - ChatWidthMargin, 0);
		}
		if ((bool)SystemChatLabel && ChatWidthMargin > 0)
		{
			SystemChatLabel.width = System.Math.Max(width - ChatWidthMargin, 0);
		}
		UIHelper.EnableGameObject(EmoticonLoader, enable: false);
		UIHelper.EnableGameObject(NonChatRoot, !chatOnly);
		if (chatInfo == null)
		{
			return;
		}
		SetChatCase();
		SetChatLabelEncoding(ref chatInfo.Chat);
		if (chatInfo.IsSystem)
		{
			SetSystemChat(chatInfo.Chat);
			if ((bool)SystemChatLabel)
			{
				SystemChatLabel.ForceUpdateAnchors(includeChildren: true);
			}
		}
		else
		{
			SetEmoticon(chatInfo.EmoticonIndex);
			SetPortraitTexture(chatInfo.SenderAvaterIndex);
			SetPortraitFrame();
			SetRankerIcon(chatInfo.ChatIconList);
			SetSenderName(chatInfo.SenderName);
			SetNameColor();
			SetChat(chatInfo.Chat);
			if ((bool)ChatLabel)
			{
				ChatLabel.ForceUpdateAnchors(includeChildren: true);
			}
		}
		SetTime(chatInfo.SendTime);
		UIHelper.EnableGameObject(SplitterObject, !chatInfo.IsSystem);
		if ((bool)ChatTable)
		{
			ChatTable.Reposition();
		}
		if ((bool)RootWidget)
		{
			RootWidget.ForceUpdateAnchors(includeChildren: true);
		}
		UIWidget component = GetComponent<UIWidget>();
		if (component == null)
		{
			VespaDebug.LogError("Cant Find Widget!!!");
		}
		else if (ChatInfo.EmoticonIndex == 0)
		{
			if (ChatLabel != null)
			{
				component.bottomAnchor.Set(ChatLabel.transform, 0f, -13f);
				EmoticonLoader.transform.localScale = Vector3.zero;
			}
		}
		else if (_chatOnly)
		{
			if (ChatLabel != null)
			{
				component.bottomAnchor.Set(ChatLabel.transform, 0f, -13f);
				EmoticonLoader.transform.localScale = Vector3.zero;
			}
		}
		else if (EmoticonLoader != null)
		{
			component.bottomAnchor.Set(EmoticonLoader.transform, 0f, -13f);
			EmoticonLoader.transform.localScale = Vector3.one;
		}
	}

	private void SetChatCase()
	{
		_chatCase = ChatCase.Default;
		if (ChatInfo.AdminLevel >= GetAdminRequireLevel())
		{
			_chatCase = ChatCase.Admin;
		}
		else if (ChatInfo.IsSystem)
		{
			_chatCase = ChatCase.System;
		}
		else if (IsItemLink())
		{
			_chatCase = ChatCase.ItemLink;
		}
	}

	private int GetAdminRequireLevel()
	{
		return SceneAutoBehaviour<ChattingManagement>.instance.GetAdminRequireLevel();
	}

	private void SetChatLabelEncoding(ref string message)
	{
		ChatCase[] bBCodeAllowCases = GetBBCodeAllowCases();
		bool flag = bBCodeAllowCases != null && Array.FindIndex(bBCodeAllowCases, (ChatCase definedCase) => definedCase == _chatCase) != -1;
		UIHelper.SetLabelEncoding(SystemChatLabel, flag);
		UIHelper.SetLabelEncoding(ChatLabel, flag);
		if (!flag)
		{
			message = NGUIText.StripSymbols(message);
		}
	}

	private ChatCase[] GetBBCodeAllowCases()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueArray<ChatCase>("AllowBBCodeChatCase");
	}

	private void SetPortraitFrame()
	{
		if (!(PortraitRoot == null) && !(PortraitTexture == null))
		{
			FactoryBase.SafeReleaseComponent(ref _portraitFrameComponent);
			if (_chatCase == ChatCase.Admin)
			{
				_portraitFrameComponent = PortraitFrameComponent.CreateComponent(PortraitTexture.transform, PortraitTexture.localSize.x);
			}
		}
	}

	private ChattingColorComponent.ColoredChatType ChatTypeToColoredType(ChatCase chatCase)
	{
		if (chatCase == ChatCase.Admin)
		{
			return ChattingColorComponent.ColoredChatType.Admin;
		}
		return ChattingColorComponent.ColoredChatType.None;
	}

	private void SetNameColor()
	{
		Color color = Color.white;
		TryGetNameColor(ChatCase.Default, out color);
		if (TryGetNameColor(_chatCase, out var color2))
		{
			color = color2;
		}
		UIHelper.SetLabelColor(NameLabel, color);
	}

	private bool TryGetNameColor(ChatCase type, out Color color)
	{
		color = Color.white;
		string value = GameRoot.Instance.ConstantDataContainer.GetValue("ChatNameColor_" + type);
		if (String_Extension_Method.IsNullOrEmpty(value))
		{
			return false;
		}
		color = NGUIText.ParseColor(value, 0);
		return true;
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void OnClickName()
	{
		if (_listener != null)
		{
			_listener.OnClickName(this);
		}
	}

	private void Update()
	{
		if (_isRecent && ChatInfo != null && (int)(NGame2.NUI.NWindow.Common.GetUTCNow() - _addedTime).TotalSeconds > 10)
		{
			SetTime(ChatInfo.SendTime);
			_isRecent = false;
		}
	}

	public void SetPortraitTexture(int avatarHeroIndex)
	{
		UIHelper.EnableGameObject(PortraitRoot, avatarHeroIndex > 0);
		if (avatarHeroIndex > 0)
		{
			string iconPath = UIAvatarHelper.GetIconPath(avatarHeroIndex);
			if (!String_Extension_Method.IsNullOrEmpty(iconPath))
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, PortraitTexture, plainTexture);
			}
		}
	}

	public void SetEmoticon(int emoticonIndex)
	{
		if (!_chatOnly && emoticonIndex != 0)
		{
			EmoticonData data = GameRoot.Instance.EmoticonDataContainer.GetData(emoticonIndex);
			if (data != null)
			{
				string assetPath = data.AssetPath;
				UIHelper.EnableGameObject(EmoticonLoader, enable: true);
				UIHelper.SetBundleTexture(EmoticonLoader, assetPath);
			}
		}
	}

	public void SetRankerIcon(int[] chatIconList)
	{
		InitRankerIcon();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(chatIconList))
		{
			return;
		}
		AddRankData(chatIconList);
		if (dataDic.Count > 0)
		{
			UIHelper.EnableGameObject(DefaultIconObject, enable: false);
			foreach (KeyValuePair<RankViewType, IconChatWindowData> item in dataDic)
			{
				IconChatWindowData value = item.Value;
				ActiveRankIconSprite(value.ViewType, value.IconFile);
			}
		}
		dataDic.Clear();
	}

	private void InitRankerIcon()
	{
		UIHelper.EnableGameObject(DefaultIconObject, enable: true);
		UIHelper.EnableGameObject(WorldIconSprite, enable: false);
		UIHelper.EnableGameObject(PvPIconSprite, enable: false);
		UIHelper.EnableGameObject(PvEIconSprite, enable: false);
	}

	private void AddRankData(int[] chatIconList)
	{
		IconChatWindowData data = GameRoot.Instance.IconChatWindowDataContainer.GetData(chatIconList[0]);
		if (data == null)
		{
			VespaDebug.LogError($"{chatIconList[0]} - RankData is null.");
			return;
		}
		dataDic.Add(data.ViewType, data);
		int num = chatIconList.Length;
		for (int i = 1; i < num; i++)
		{
			IconChatWindowData data2 = GameRoot.Instance.IconChatWindowDataContainer.GetData(chatIconList[i]);
			if (data2 == null)
			{
				VespaDebug.LogError($"{chatIconList[i]} - RankData is null.");
				dataDic.Clear();
				break;
			}
			IconChatWindowData vALUE = dataDic.GETVALUE(data2.ViewType, null);
			if (vALUE == null || vALUE.Order > data2.Order)
			{
				dataDic.Update(data2.ViewType, data2);
			}
		}
	}

	private void ActiveRankIconSprite(RankViewType rankViewType, string iconSpriteName)
	{
		UISprite uISprite = null;
		switch (rankViewType)
		{
		case RankViewType.Chat_WorldBossRank:
			uISprite = WorldIconSprite;
			break;
		case RankViewType.Chat_HonorArenaRank:
			uISprite = PvPIconSprite;
			break;
		case RankViewType.Chat_VictoryArenaRank:
			uISprite = PvEIconSprite;
			break;
		}
		if (!(uISprite == null))
		{
			UIHelper.EnableGameObject(uISprite, enable: true);
			UIHelper.SetSpriteName(uISprite, iconSpriteName);
		}
	}

	public void SetSenderName(string senderName)
	{
		UIHelper.EnableGameObject(NameLabel, !string.IsNullOrEmpty(senderName));
		UIHelper.SetLabelText(NameLabel, senderName);
	}

	private bool IsItemLink()
	{
		if (ChatInfo.LinkedItem != null && ChatInfo.LinkedItem.Length >= 1)
		{
			return Button_Chat != null;
		}
		return false;
	}

	public void SetChat(string chat)
	{
		UIHelper.EnableGameObject(ChatLabel, !string.IsNullOrEmpty(chat));
		UIHelper.SetLabelText(ChatLabel, chat);
		UIHelper.SetLabelColor(ChatLabel, GetChatLabelColor());
		bool flag = IsItemLink();
		if (flag)
		{
			string itemName = ItemNameBuilder.GetItemName(GameRoot.Instance.ItemDataContainer.GetData(ChatInfo.LinkedItem[0].EquipItemInfo.ItemIndex), ChatInfo.LinkedItem[0].EquipItemInfo);
			string stringDirect = LocalizationManager.GetStringDirect("CHAT_ITEM_SHARE_LINK", itemName);
			UIHelper.EnableGameObject(ChatLabel, !string.IsNullOrEmpty(stringDirect));
			UIHelper.SetLabelText(ChatLabel, stringDirect);
		}
		UIHelper.EnableGameObject(Button_Chat, flag);
	}

	private Color GetChatLabelColor()
	{
		Color result = Color.white;
		ChattingColorComponent chattingColorComponent = VespaResource.Load<ChattingColorComponent>("Prefab2/Component/Chatting/ChattingColorComponent");
		ChattingColorComponent.ColorInfo[] array = (chattingColorComponent ? chattingColorComponent.ColorInfos : null);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			if (array[i].ChatType == ChatTypeToColoredType(_chatCase))
			{
				result = array[i].ChatColor;
				break;
			}
		}
		return result;
	}

	public void SetSystemChat(string chat)
	{
		UIHelper.EnableGameObject(SystemChatLabel, !string.IsNullOrEmpty(chat));
		UIHelper.SetLabelText(SystemChatLabel, chat);
	}

	public void SetTime(string sendTime)
	{
		if (string.IsNullOrEmpty(sendTime))
		{
			UIHelper.EnableGameObject(TimeLabel, enable: false);
			UIHelper.EnableGameObject(TimeRecentLabel, enable: false);
			return;
		}
		Vespa_String_Extension_Method2.TryParseNetworkTime2NoCache(sendTime, out _addedTime);
		if ((int)(NGame2.NUI.NWindow.Common.GetUTCNow() - _addedTime).TotalSeconds < 10)
		{
			UIHelper.EnableGameObject(TimeLabel, enable: false);
			UIHelper.EnableGameObject(TimeRecentLabel, enable: true);
			_isRecent = true;
		}
		else
		{
			UIHelper.EnableGameObject(TimeLabel, enable: true);
			UIHelper.EnableGameObject(TimeRecentLabel, enable: false);
			DateTime dateTime = _addedTime.ToLocalTime();
			UIHelper.SetLabelText(TimeLabel, dateTime.ToString("HH:mm"));
		}
	}

	public int GetComponentSizeY()
	{
		UIWidget component = GetComponent<UIWidget>();
		if (component == null)
		{
			VespaDebug.LogError("Cant Find Widget!!!");
			return 0;
		}
		component.ResetAndUpdateAnchors();
		return (int)component.localSize.y;
	}

	public virtual int UpdatePos(int totalHeight)
	{
		Vector3 localPosition = base.gameObject.transform.localPosition;
		localPosition.y = totalHeight;
		base.gameObject.transform.localPosition = localPosition;
		int componentSizeY = GetComponentSizeY();
		return totalHeight - componentSizeY;
	}

	public void OnClickLinkedItem()
	{
		if (ChatInfo.LinkedItem != null && ChatInfo.LinkedItem.Length >= 1)
		{
			InspectEquipInfo instance = SceneAutoBehaviour<ChattingManagement>.instance.GetInstance<InspectEquipInfo>();
			if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
			{
				instance.DepthOrder = EDepthOrder.Topmost;
				instance.DepthOrder2 = 1;
			}
			instance.Open(null, null, ChatInfo.LinkedItem);
			instance.SetBlackBg(isEnable: true);
		}
	}

	public void OnClickLinkHouse()
	{
	}
}
