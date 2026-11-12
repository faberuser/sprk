using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class ManageGuildMemberComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickManageButton(ManageGuildMemberComponent comp);

		void OnClickAcceptJoin(ManageGuildMemberComponent comp);

		void OnClickRejectJoin(ManageGuildMemberComponent comp);
	}

	private enum ComponentType
	{
		GuildMember,
		WaitingPlayer
	}

	public const string PrefabPath = "Prefab2/Component/Guild/ManageGuildMemberComponent";

	public static PrefabAuto<ManageGuildMemberComponent> PrefabAuto = new PrefabAuto<ManageGuildMemberComponent>("Prefab2/Component/Guild/ManageGuildMemberComponent");

	public const string PrefabPathWaiting = "Prefab2/Component/Guild/ManageWaitingMemberComponent";

	public static PrefabAuto<ManageGuildMemberComponent> PrefabAutoWaiting = new PrefabAuto<ManageGuildMemberComponent>("Prefab2/Component/Guild/ManageWaitingMemberComponent");

	public UILabel Label_TeamLevel;

	public UILabel Label_Nick;

	public UITexture Texture_AvatarHero;

	public UIWidget Widget_Activity;

	public UIWidget Widget_Login;

	public UILabel Label_Rank;

	public UILabel Label_ActivityNum;

	public UILabel Label_LoginTimeStr;

	private Listener _listener;

	private ComponentType _type;

	public GuildMemberInfo MemberInfo { get; private set; }

	public GuildJoinRequestPlayerInfo WaitingInfo { get; private set; }

	public void Open(GuildMemberInfo info, Listener listener)
	{
		_type = ComponentType.GuildMember;
		MemberInfo = info;
		_listener = listener;
		SetupCommonInfo(MemberInfo.TeamLevel, MemberInfo.Nick, MemberInfo.AvatarHeroIndex, MemberInfo.LastLoginTime);
		SetupManageInfo();
	}

	public void Open(GuildJoinRequestPlayerInfo info, Listener listener)
	{
		_type = ComponentType.WaitingPlayer;
		WaitingInfo = info;
		_listener = listener;
		SetupCommonInfo(info.TeamLevel, info.Nick, info.AvatarHeroIndex, info.LastLoginTime);
	}

	public void Refresh()
	{
		if (_type == ComponentType.GuildMember)
		{
			GuildMemberInfo guildMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Get(MemberInfo.AccountId);
			if (guildMemberInfo != null)
			{
				MemberInfo = guildMemberInfo;
				SetupCommonInfo(MemberInfo.TeamLevel, MemberInfo.Nick, MemberInfo.AvatarHeroIndex, MemberInfo.LastLoginTime);
				SetupManageInfo();
			}
		}
		else if (_type == ComponentType.WaitingPlayer)
		{
			GuildJoinRequestPlayerInfo guildJoinRequestPlayerInfo = GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager.Get(WaitingInfo.AccountId);
			if (guildJoinRequestPlayerInfo != null)
			{
				WaitingInfo = guildJoinRequestPlayerInfo;
				SetupCommonInfo(WaitingInfo.TeamLevel, WaitingInfo.Nick, WaitingInfo.AvatarHeroIndex, WaitingInfo.LastLoginTime);
			}
		}
	}

	public void OnClickManageButton()
	{
		if (_listener != null)
		{
			_listener.OnClickManageButton(this);
		}
	}

	public void OnClickAcceptJoin()
	{
		if (_listener != null)
		{
			_listener.OnClickAcceptJoin(this);
		}
	}

	public void OnClickRejectJoin()
	{
		if (_listener != null)
		{
			_listener.OnClickRejectJoin(this);
		}
	}

	public void OnClickWaitingComponent()
	{
		InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(WaitingInfo.AccountId, WaitingInfo.AvatarHeroIndex, WaitingInfo.TeamLevel, WaitingInfo.Nick, InspectUserCategory.General);
		SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
	}

	private void SetupCommonInfo(int teamLevel, string nick, int avatarIndex, string lastLoginTime)
	{
		UIHelper.SetLabelText(Label_TeamLevel, "Lv " + teamLevel);
		UIHelper.SetLabelText(Label_Nick, nick);
		string iconPath = UIAvatarHelper.GetIconPath(avatarIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_AvatarHero, plainTexture);
		}
		Vespa_String_Extension_Method2.TryParseNetworkTime2(lastLoginTime, out var o_dateTime);
		int num = (int)(NGame2.NUI.NWindow.Common.GetUTCNow() - o_dateTime).TotalDays;
		if (num == 0)
		{
			UIHelper.SetLabelText(Label_LoginTimeStr, LocalizationManager.GetString(Guild.GUILD_LASTLOGINTIME_TODAY));
			return;
		}
		UIHelper.SetLabelText(Label_LoginTimeStr, LocalizationManager.GetString(Guild.GUILD_LASTLOGINTIME_DAY, num));
	}

	private void SetupManageInfo(bool setupInform = true)
	{
		if (setupInform)
		{
			UIHelper.SetLabelText(Label_Rank, GetGuildRankString(MemberInfo.Rank));
			UIHelper.SetLabelText(Label_ActivityNum, MemberInfo.AccActivityPoint.ToString());
		}
	}

	public static string GetGuildRankString(int rank)
	{
		return rank switch
		{
			3 => LocalizationManager.GetString(Guild.GUILD_RANK_MEMBER), 
			2 => LocalizationManager.GetString(Guild.GUILD_RANK_ADMIN), 
			1 => LocalizationManager.GetString(Guild.GUILD_RANK_MASTER), 
			4 => LocalizationManager.GetString(Guild.GUILD_RANK_NEW_MEMBER), 
			5 => LocalizationManager.GetString(Guild.GUILD_RANK_GHOST_MEMBER), 
			_ => "INVALIDRANK", 
		};
	}
}
