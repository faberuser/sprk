using NGameAsset;

namespace NGame2.NUI.NWindow.NChatting;

public class ChattingUserContextMenu : BaseWindow
{
	public interface Listener
	{
		void Inspect(long accountId);

		void InviteFriend(long accountId, string name);

		void InviteParty(long accountId, string name);

		void Whisper(long accountId, string name);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Chatting/ChattingUserContextMenu";

	public UILabel NameLabel;

	public UILabel TeamLevelLabel;

	public UITexture PortraitTexture;

	public UIButton InspectButton;

	public UIButton InviteFriendButton;

	public UIButton InvitePartyButton;

	public UIButton WHisperButton;

	private Listener _listener;

	private ChatInfo _chatInfo;

	public void Open(Listener listener, ChatInfo chatInfo, bool canWhisper)
	{
		Open(delegate
		{
			Init(listener, chatInfo, canWhisper);
		}, null);
	}

	public void Init(Listener listener, ChatInfo chatInfo, bool canWhisper)
	{
		_listener = listener;
		_chatInfo = chatInfo;
		UIHelper.SetLabelText(NameLabel, chatInfo.SenderName);
		UIHelper.SetLabelText(TeamLevelLabel, chatInfo.SenderTeamLevel);
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(chatInfo.SenderAvaterIndex, ignoreAccount: true);
		UIHelper.ResetPlaneAssetTextureAsync(this, PortraitTexture, heroIcon);
		UIHelper.EnableButton(InspectButton, enable: true);
		UIHelper.EnableButton(InviteFriendButton, enable: true);
		UIHelper.EnableButton(InvitePartyButton, enable: true);
		UIHelper.EnableButton(WHisperButton, canWhisper);
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnClickInspect()
	{
		if (_listener != null)
		{
			_listener.Inspect(_chatInfo.SenderId);
		}
		Close();
	}

	public void OnClickInvitParty()
	{
		if (_listener != null)
		{
			_listener.InviteParty(_chatInfo.SenderId, _chatInfo.SenderName);
		}
		Close();
	}

	public void OnClickInviteFriend()
	{
		if (_listener != null)
		{
			_listener.InviteFriend(_chatInfo.SenderId, _chatInfo.SenderName);
		}
		Close();
	}

	public void OnClickWhisper()
	{
		if (_listener != null)
		{
			_listener.Whisper(_chatInfo.SenderId, _chatInfo.SenderName);
		}
		Close();
	}
}
