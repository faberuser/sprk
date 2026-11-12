using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NGame2.NLogin;
using NShared;
using NShared.NContentsDefine;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OfficialChannelWindow : BaseWindow
{
	[Serializable]
	public class ChannelUiData
	{
		public UIWidget Root;

		public UIButton Button;

		public UISprite IconSprite;

		public UILabel ChannelName;

		public UISprite NewMarkSprite;

		public NShared.NContentsDefine.OfficialChannel.Channel ChannelData { get; private set; }

		public void Set(NShared.NContentsDefine.OfficialChannel.Channel channelData, bool newMarkVisible)
		{
			ChannelData = channelData;
			if (ChannelData != null)
			{
				UIHelper.SetSpriteName(IconSprite, ChannelData.Icon);
				UIHelper.SetLabelText(ChannelName, LocalizationManager.GetStringDirect(ChannelData.Name_Local));
				UIHelper.EnableGameObject(NewMarkSprite, channelData.IsNewVisible && newMarkVisible);
			}
		}

		public void DisableNewMark()
		{
			UIHelper.EnableGameObject(NewMarkSprite, enable: false);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Lobby/OfficialChannelWindow";

	public static LocalStorageInt LastPatchNoteIndex = new LocalStorageInt(0, "OfficialChannelWindow_LastPatchNoteIndex");

	public UITable ChannelTable;

	public List<ChannelUiData> ChannelUiList;

	private int _patchNoteIndex;

	public override void Open()
	{
		base.Open();
		if (!AccountManager.IsLogin() || !SingletonUpdater<LoginManager>.isValidInstance)
		{
			return;
		}
		_patchNoteIndex = NGame2.NContentsDefine.OfficialChannel.GetPatchNoteIndex();
		bool newMarkVisible = false;
		if (_patchNoteIndex > 0 && _patchNoteIndex > LastPatchNoteIndex.Value)
		{
			newMarkVisible = true;
		}
		string serverName = SingletonUpdater<LoginManager>.instance.ServerName;
		string selectedServer;
		NShared.NContentsDefine.OfficialChannel.Setting setting = ContentsDefine.OfficialChannel.GetSetting(serverName, out selectedServer);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ChannelUiList); i++)
		{
			ChannelUiData channelUiData = ChannelUiList[i];
			if (i >= __Extension_Method_ICollection__.COUNT(setting.Channels))
			{
				channelUiData.Set(null, newMarkVisible: false);
				UIHelper.EnableGameObject(channelUiData.Root, enable: false);
				continue;
			}
			NShared.NContentsDefine.OfficialChannel.Channel channel = setting.Channels[i];
			UIHelper.EnableGameObject(channelUiData.Root, channel.IsEnable);
			if (channel.IsEnable)
			{
				channelUiData.Set(channel, newMarkVisible);
			}
		}
		if (ChannelTable != null)
		{
			ChannelTable.Reposition();
		}
	}

	public void OnClickChannel(UIButton button)
	{
		ChannelUiData channelUiByButton = GetChannelUiByButton(button);
		if (channelUiByButton == null)
		{
			VespaDebug.LogError("Invalid Channel Button");
			return;
		}
		string serverName = SingletonUpdater<LoginManager>.instance.ServerName;
		string selectedServer;
		NShared.NContentsDefine.OfficialChannel.Setting setting = ContentsDefine.OfficialChannel.GetSetting(serverName, out selectedServer);
		NShared.NContentsDefine.OfficialChannel.Channel channelData = channelUiByButton.ChannelData;
		if (channelData.IsNewVisible)
		{
			if (_patchNoteIndex > 0 && LastPatchNoteIndex.Value < _patchNoteIndex)
			{
				LastPatchNoteIndex.Value = _patchNoteIndex;
			}
			channelUiByButton.DisableNewMark();
		}
		if (setting.OpenInAppBrowser)
		{
			OfficialChannelWebView.OpenGlobal(channelData.Url);
		}
		else
		{
			Application.OpenURL(channelData.Url);
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	private ChannelUiData GetChannelUiByButton(UIButton button)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ChannelUiList); i++)
		{
			if (ChannelUiList[i].Button == button)
			{
				return ChannelUiList[i];
			}
		}
		return null;
	}
}
