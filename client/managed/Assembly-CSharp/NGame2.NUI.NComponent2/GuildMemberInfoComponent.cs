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

public class GuildMemberInfoComponent : UIScrollItemBehaviour
{
	public const string PrefabPath = "Prefab2/Component/Guild/GuildMemberInfoComponent";

	public static PrefabAuto<GuildMemberInfoComponent> PrefabAuto = new PrefabAuto<GuildMemberInfoComponent>("Prefab2/Component/Guild/GuildMemberInfoComponent");

	public UILabel Label_TeamLevel;

	public UILabel Label_Nick;

	public UITexture Texture_AvatarHero;

	public UILabel Label_Rank;

	public UILabel Label_ActivityNum;

	public UILabel Label_LastLogin;

	public GuildMemberInfo MemberInfo { get; private set; }

	public void Open(GuildMemberInfo info)
	{
		MemberInfo = info;
		SetupCommonInfo();
		SetupBasicInfo();
	}

	public void Refresh()
	{
		GuildMemberInfo guildMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Get(MemberInfo.AccountId);
		if (guildMemberInfo != null)
		{
			MemberInfo = guildMemberInfo;
			SetupCommonInfo();
			SetupBasicInfo();
		}
	}

	private void SetupCommonInfo()
	{
		UIHelper.SetLabelText(Label_TeamLevel, "Lv " + MemberInfo.TeamLevel);
		UIHelper.SetLabelText(Label_Nick, MemberInfo.Nick);
		string iconPath = UIAvatarHelper.GetIconPath(MemberInfo.AvatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_AvatarHero, plainTexture);
		}
	}

	private void SetupBasicInfo()
	{
		UIHelper.SetLabelText(Label_Rank, ManageGuildMemberComponent.GetGuildRankString(MemberInfo.Rank));
		UIHelper.SetLabelText(Label_ActivityNum, MemberInfo.AccActivityPoint.ToString());
		Vespa_String_Extension_Method2.TryParseNetworkTime2(MemberInfo.LastLoginTime, out var o_dateTime);
		int num = (int)(NGame2.NUI.NWindow.Common.GetUTCNow() - o_dateTime).TotalDays;
		if (num == 0)
		{
			UIHelper.SetLabelText(Label_LastLogin, LocalizationManager.GetString(Guild.GUILD_LASTLOGINTIME_TODAY));
			return;
		}
		UIHelper.SetLabelText(Label_LastLogin, LocalizationManager.GetString(Guild.GUILD_LASTLOGINTIME_DAY, num));
	}

	public void OnClickComponent()
	{
		InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(MemberInfo, InspectUserCategory.Guild);
		SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
	}
}
