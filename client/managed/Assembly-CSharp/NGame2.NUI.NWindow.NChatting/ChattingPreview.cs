using System;
using System.Collections.Generic;
using NGame2.NCutScene;
using NGame2.NGuild;
using NGame2.NScene;
using NGame2.NUI.NComponent2.NChatting;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

public class ChattingPreview : BaseWindow
{
	[Serializable]
	public class ChattingPreviewComponent
	{
		public Transform ComponentRoot;

		public UILabel SenderNameLabel;

		public UILabel MessageLabel;

		public UILabelAligner Aligner;

		public UISprite BG;
	}

	public class ChattingPreviewContainer
	{
		public ChattingColorComponent.ColoredChatType chatType;

		public bool isSystem;

		public string senderName;

		public string message;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Chatting/ChattingPreview";

	private static GlobalWindowPrefab<ChattingPreview> _windowPrefab = new GlobalWindowPrefab<ChattingPreview>("Resources/Prefab2/Window/Chatting/ChattingPreview", GlobalUIRoot.ELayerType.GlobalUI);

	private static ChattingColorComponent.ColorInfo[] ColorInfos = null;

	private static LoadingManager.ESceneType[] VisibleSceneTypes = new LoadingManager.ESceneType[2]
	{
		LoadingManager.ESceneType.Lobby,
		LoadingManager.ESceneType.Battle2
	};

	public UIPanel Panel;

	public float ShowDuration = 5f;

	public int MessageWidth = 600;

	private float _openedTime;

	private float _currentAlpha = 1f;

	private static bool _disabledChating = false;

	private ChatInfo _chatInfo;

	public ChattingPreviewComponent[] PreviewComponents;

	private List<ChattingPreviewContainer> _chattingPreviewContainer = new List<ChattingPreviewContainer>();

	public static void Open(ChatInfo chatInfo)
	{
		if (chatInfo == null || !GlobalAutoBehaviour<LoadingManager>.isValidInstance)
		{
			return;
		}
		LoadingManager instance = GlobalAutoBehaviour<LoadingManager>.instance;
		if (instance == null)
		{
			return;
		}
		LoadingManager.ESceneType currentSceneType = instance.CurrentSceneType;
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(VisibleSceneTypes); i++)
		{
			if (VisibleSceneTypes[i] == currentSceneType)
			{
				flag = true;
				break;
			}
		}
		if (flag && !_disabledChating)
		{
			_windowPrefab.instance.OpenInternal(chatInfo);
		}
	}

	public static void OnSceneChanged()
	{
		if (!_windowPrefab.IsOpen())
		{
			_windowPrefab.ReleaseWindow();
			_windowPrefab.ReleasePrefab();
		}
	}

	private void OpenComponent(ChattingPreviewContainer container, ChattingPreviewComponent component)
	{
		Color chatColor = Color.white;
		Color outlineColor = Color.black;
		GetColor(container, out outlineColor, out chatColor);
		SetMessage(container, component, outlineColor, chatColor);
		SetName(container, component, outlineColor, chatColor);
		if (component.Aligner != null)
		{
			component.Aligner.Align();
		}
		if (component.BG != null)
		{
			component.BG.width = component.Aligner.TotalWidth;
		}
	}

	private void GetColor(ChattingPreviewContainer container, out Color outlineColor, out Color chatColor)
	{
		outlineColor = Color.black;
		chatColor = Color.white;
		if (ColorInfos == null)
		{
			if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
			{
				ChattingColorComponent chattingColorComponent = VespaResource.Load<ChattingColorComponent>("Prefab2/Component/Chatting/ChattingColorComponent");
				ColorInfos = (chattingColorComponent ? chattingColorComponent.PreviewColorInfos : new ChattingColorComponent.ColorInfo[0]);
			}
			else
			{
				ChattingColorComponent chattingColorComponent2 = VespaResource.Load<ChattingColorComponent>("Prefab2/Component/BattleChatting/BattleChattingColorComponent");
				ColorInfos = (chattingColorComponent2 ? chattingColorComponent2.PreviewColorInfos : new ChattingColorComponent.ColorInfo[0]);
			}
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ColorInfos); i++)
		{
			if (ColorInfos[i].ChatType == container.chatType)
			{
				chatColor = ColorInfos[i].ChatColor;
			}
		}
		string value = GameRoot.Instance.ConstantDataContainer.GetValue("ChatPreviewColor_" + container.chatType);
		if (!String_Extension_Method.IsNullOrEmpty(value))
		{
			chatColor = NGUIText.ParseColor(value, 0);
		}
		string value2 = GameRoot.Instance.ConstantDataContainer.GetValue("ChatPreviewOutlineColor_" + container.chatType);
		if (!String_Extension_Method.IsNullOrEmpty(value2))
		{
			outlineColor = NGUIText.ParseColor(value2, 0);
		}
	}

	private void SetName(ChattingPreviewContainer container, ChattingPreviewComponent component, Color outlineColor, Color chatColor)
	{
		UIHelper.SetLabelColor(component.SenderNameLabel, chatColor);
		UIHelper.SetLabelEffectColor(component.SenderNameLabel, outlineColor);
		if (!container.isSystem && !string.IsNullOrEmpty(container.senderName))
		{
			UIHelper.SetLabelText(component.SenderNameLabel, container.senderName + " : ");
		}
		else
		{
			UIHelper.SetLabelText(component.SenderNameLabel, "");
		}
	}

	private void SetMessage(ChattingPreviewContainer container, ChattingPreviewComponent component, Color outlineColor, Color chatColor)
	{
		string fitEllipsisString = UIHelper.GetFitEllipsisString(container.message, MessageWidth, component.MessageLabel, component.MessageLabel.fontSize);
		UIHelper.SetLabelColor(component.MessageLabel, chatColor);
		UIHelper.SetLabelEffectColor(component.MessageLabel, outlineColor);
		UIHelper.SetLabelText(component.MessageLabel, fitEllipsisString);
		if (_chatInfo.LinkedItem != null && _chatInfo.LinkedItem.Length >= 1)
		{
			string itemName = ItemNameBuilder.GetItemName(GameRoot.Instance.ItemDataContainer.GetData(_chatInfo.LinkedItem[0].EquipItemInfo.ItemIndex), _chatInfo.LinkedItem[0].EquipItemInfo);
			string stringDirect = LocalizationManager.GetStringDirect("CHAT_ITEM_SHARE_LINK", itemName);
			Color chatColor2 = Color.white;
			Color outlineColor2 = Color.black;
			container.chatType = ChattingColorComponent.ColoredChatType.None;
			GetColor(container, out outlineColor2, out chatColor2);
			UIHelper.SetLabelColor(component.MessageLabel, chatColor2);
			UIHelper.SetLabelEffectColor(component.MessageLabel, outlineColor2);
			UIHelper.SetLabelText(component.MessageLabel, stringDirect);
		}
	}

	protected void OpenInternal(ChatInfo chatInfo)
	{
		base.Open();
		_chatInfo = chatInfo;
		ChatType chatType = chatInfo.ChatType;
		bool isSystem = chatInfo.IsSystem;
		bool flag = chatInfo.AdminLevel >= 2;
		long senderId = chatInfo.SenderId;
		string senderName = chatInfo.SenderName;
		string text = chatInfo.Chat;
		ChattingColorComponent.ColoredChatType chatType2 = ChattingColorComponent.ColoredChatType.None;
		if (isSystem)
		{
			chatType2 = ChattingColorComponent.ColoredChatType.System;
		}
		else if (flag)
		{
			chatType2 = ChattingColorComponent.ColoredChatType.Admin;
		}
		else
		{
			switch (chatType)
			{
			case ChatType.Whisper:
				chatType2 = ChattingColorComponent.ColoredChatType.Whisper;
				break;
			case ChatType.Party:
				chatType2 = ChattingColorComponent.ColoredChatType.Party;
				break;
			case ChatType.Guild:
			{
				chatType2 = ChattingColorComponent.ColoredChatType.Guild;
				NGame2.NGuild.GuildMemberManager guildMemberManager = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance.MemberManager : null);
				if (guildMemberManager != null)
				{
					GuildMemberInfo guildMemberInfo = guildMemberManager.Get(senderId);
					if (guildMemberInfo != null && guildMemberInfo.Rank == 1)
					{
						chatType2 = ChattingColorComponent.ColoredChatType.GuildMaster;
					}
				}
				break;
			}
			}
		}
		if (!isSystem && (chatInfo.LinkedItem == null || chatInfo.LinkedItem.Length < 1))
		{
			text = NGUIText.StripSymbols(text);
		}
		EmoticonData emoticonData = ((chatInfo.EmoticonIndex != 0) ? GameRoot.Instance.EmoticonDataContainer.GetData(chatInfo.EmoticonIndex) : null);
		if (emoticonData != null)
		{
			text = "(" + emoticonData.Keyword + ")" + text;
		}
		_chattingPreviewContainer.Add(new ChattingPreviewContainer
		{
			chatType = chatType2,
			isSystem = isSystem,
			senderName = senderName,
			message = text
		});
		while (_chattingPreviewContainer.Count > PreviewComponents.Length)
		{
			_chattingPreviewContainer.RemoveAt(0);
		}
		for (int i = 0; i < _chattingPreviewContainer.Count; i++)
		{
			UIHelper.EnableGameObject(PreviewComponents[i].ComponentRoot, enable: true);
			OpenComponent(_chattingPreviewContainer[i], PreviewComponents[i]);
		}
		for (int j = _chattingPreviewContainer.Count; j < PreviewComponents.Length; j++)
		{
			UIHelper.EnableGameObject(PreviewComponents[j].ComponentRoot, enable: false);
		}
		_openedTime = Time.time;
		_currentAlpha = GetTargetAlpha();
		if ((bool)Panel)
		{
			Panel.ResetAndUpdateAnchors();
			Panel.alpha = _currentAlpha;
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateAlpha();
		UpdateTimeEnded();
	}

	private void UpdateAlpha()
	{
		if (!(Panel == null))
		{
			float targetAlpha = GetTargetAlpha();
			if (!Mathf.Approximately(_currentAlpha, targetAlpha))
			{
				float num = Mathf.Min(Time.deltaTime * 3f, 0.1f);
				_currentAlpha = ((targetAlpha > _currentAlpha) ? (_currentAlpha + num) : (_currentAlpha - num));
				_currentAlpha = Mathf.Clamp01(_currentAlpha);
				Panel.alpha = _currentAlpha;
			}
		}
	}

	private void UpdateTimeEnded()
	{
		if (Time.time - _openedTime >= ShowDuration)
		{
			Close();
		}
	}

	private bool IsCutScenePlaying()
	{
		if (!SceneAutoBehaviour<CutSceneManager>.isValidInstance)
		{
			return false;
		}
		CutSceneManager instance = SceneAutoBehaviour<CutSceneManager>.instance;
		bool num = instance != null && instance.IsPlaying;
		bool flag = (GlobalTimer.pauseFlag & GlobalTimer.EPauseFlag.CutScene) != 0;
		return num || flag;
	}

	private bool IsVideoPlaying()
	{
		if (!SingletonUpdater<VideoManager>.isValidInstance)
		{
			return false;
		}
		return VideoManager.IsPlaying();
	}

	private float GetTargetAlpha()
	{
		bool num = IsCutScenePlaying();
		bool flag = IsVideoPlaying();
		if (num || flag)
		{
			return 0f;
		}
		return 1f;
	}

	public static void AbledChating()
	{
		_disabledChating = false;
	}

	public void DisabledChating()
	{
		_disabledChating = true;
		Close();
	}
}
