using NGameAsset;
using NShared.NParty;

namespace NGame2.NUI.NWindow;

public class PartyMemberContextMenu : BaseWindow
{
	public interface Listener
	{
		void Invite(MemberInfo memberInfo);

		void Ban(MemberInfo memberInfo);

		void Whisper(MemberInfo memberInfo);

		void Leave();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Dungeon/PartyMemberContextMenu";

	public UILabel NameLabel;

	public UILabel TeamLevelLabel;

	public UITexture PortraitTexture;

	public UIButton InviteButton;

	public UIButton BanButton;

	public UIButton WhisperButton;

	public UIButton LeaveButton;

	private Listener _listener;

	public MemberInfo MemberInfo { get; private set; }

	public void Open(Listener listener, MemberInfo memberInfo, MemberInfo myMemberInfo, bool canWhisper)
	{
		base.Open();
		Init(listener, memberInfo, myMemberInfo, canWhisper);
	}

	public void Init(Listener listener, MemberInfo memberInfo, MemberInfo myMemberInfo, bool canWhisper)
	{
		_listener = listener;
		MemberInfo = memberInfo;
		UIHelper.SetLabelText(NameLabel, memberInfo.Nick);
		UIHelper.SetLabelText(TeamLevelLabel, memberInfo.TeamLevel);
		AssetTexture plainTexture = UIHelper.GetPlainTexture(UIAvatarHelper.GetIconPath(memberInfo.AvatarHeroIndex));
		UIHelper.ResetPlaneAssetTextureAsync(this, PortraitTexture, plainTexture);
		bool isMaster = myMemberInfo.IsMaster;
		bool isMaster2 = myMemberInfo.IsMaster;
		bool enable = true;
		UIHelper.EnableButton(InviteButton, isMaster);
		UIHelper.EnableButton(BanButton, isMaster2);
		UIHelper.EnableButton(WhisperButton, canWhisper);
		UIHelper.EnableButton(LeaveButton, enable);
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnClickInvite()
	{
		if (_listener != null)
		{
			_listener.Invite(MemberInfo);
		}
		Close();
	}

	public void OnClickLeave()
	{
		if (_listener != null)
		{
			_listener.Leave();
		}
		Close();
	}

	public void OnClickBan()
	{
		if (_listener != null)
		{
			_listener.Ban(MemberInfo);
		}
		Close();
	}

	public void OnClickWhisper()
	{
		if (_listener != null)
		{
			_listener.Whisper(MemberInfo);
		}
		Close();
	}
}
