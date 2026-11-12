using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class ManageMenu : BaseWindow
{
	public interface Listener
	{
		void OnClickDelegateMasterButton(GuildMemberInfo info);

		void OnClickSetAdminRankButton(GuildMemberInfo info);

		void OnClickSetMemberRankButton(GuildMemberInfo info);

		void OnClickKickButton(GuildMemberInfo info);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Guild/ManageMenu";

	public UILabel Label_Nick;

	public UIButton Button_DelegateMaster;

	public UIButton Button_SetAdminRank;

	public UIButton Button_SetMemberRank;

	public UIButton Button_KickMember;

	private Listener _listener;

	public GuildMemberInfo MemberInfo { get; private set; }

	public void Open(GuildMemberInfo info, Listener listener)
	{
		base.Open();
		Init(info, listener);
	}

	public void Init(GuildMemberInfo info, Listener listener)
	{
		_listener = listener;
		MemberInfo = info;
		SetupGuildMemberInfo();
		SetupButtons();
	}

	public override void Reset()
	{
		base.Reset();
		UIHelper.EnableButton(Button_DelegateMaster, enable: true);
		UIHelper.EnableButton(Button_SetAdminRank, enable: true);
		UIHelper.EnableButton(Button_KickMember, enable: true);
		UIHelper.EnableButton(Button_SetMemberRank, enable: true);
	}

	public void OnClickExitButton()
	{
		Close();
	}

	private void SetupGuildMemberInfo()
	{
		UIHelper.SetLabelText(Label_Nick, MemberInfo.Nick);
	}

	private void SetupButtons()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null || GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null)
		{
			return;
		}
		int num = 0;
		foreach (GuildMemberInfo item in GlobalAutoBehaviour<GuildManager>.instance.MemberManager)
		{
			if (item.Rank == 2)
			{
				num++;
			}
		}
		byte level = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Level;
		GuildLevelData data = GameRoot.Instance.GuildLevelDataContainer.GetData(level);
		bool flag = data != null && num < data.MaxAdmin;
		if (myMemberInfo.Rank == 1)
		{
			UIHelper.EnableButton(Button_DelegateMaster, MemberInfo.Rank > 1);
			UIHelper.EnableButton(Button_SetAdminRank, MemberInfo.Rank > 2 && flag);
			UIHelper.EnableButton(Button_SetMemberRank, MemberInfo.Rank != 3 && MemberInfo.Rank != 1);
			UIHelper.EnableButton(Button_KickMember, MemberInfo.Rank > 1);
		}
		else if (myMemberInfo.Rank == 2)
		{
			UIHelper.EnableButton(Button_DelegateMaster, enable: false);
			UIHelper.EnableButton(Button_SetAdminRank, enable: false);
			UIHelper.EnableButton(Button_SetMemberRank, enable: false);
			UIHelper.EnableButton(Button_KickMember, enable: false);
		}
	}

	public void OnClickDelegateMasterButton()
	{
		if (_listener != null)
		{
			LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
			string message = LocalizationManager.GetString(Guild.GUILD_CONFIRM_RANK, MemberInfo.Nick, ManageGuildMemberComponent.GetGuildRankString(1));
			MessagePopup.Open("", message, delegate
			{
				_listener.OnClickDelegateMasterButton(MemberInfo);
			}, null);
		}
	}

	public void OnClickSetAdminRankButton()
	{
		if (_listener != null)
		{
			LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
			string message = LocalizationManager.GetString(Guild.GUILD_CONFIRM_RANK, MemberInfo.Nick, ManageGuildMemberComponent.GetGuildRankString(2));
			MessagePopup.Open("", message, delegate
			{
				_listener.OnClickSetAdminRankButton(MemberInfo);
			}, null);
		}
	}

	public void OnClickSetMemberRankButton()
	{
		if (_listener != null)
		{
			LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
			string message = LocalizationManager.GetString(Guild.GUILD_CONFIRM_RANK, MemberInfo.Nick, ManageGuildMemberComponent.GetGuildRankString(3));
			MessagePopup.Open("", message, delegate
			{
				_listener.OnClickSetMemberRankButton(MemberInfo);
			}, null);
		}
	}

	public void OnClickKickButton()
	{
		if (_listener != null)
		{
			LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
			string message = LocalizationManager.GetString(Guild.GUILD_CONFIRM_KICK, MemberInfo.Nick);
			MessagePopup.Open("", message, delegate
			{
				_listener.OnClickKickButton(MemberInfo);
			}, null);
		}
	}
}
