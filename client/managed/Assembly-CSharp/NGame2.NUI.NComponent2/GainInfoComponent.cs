using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GainInfoComponent : UIScrollItemBehaviour
{
	public const string PrefabPath = "Prefab2/Component/Guild/HeadQuarter/GainInfoComponent";

	public static PrefabAuto<GainInfoComponent> PrefabAuto = new PrefabAuto<GainInfoComponent>("Prefab2/Component/Guild/HeadQuarter/GainInfoComponent");

	public UIBundleLoader Texture_Portrait;

	public UILabel Label_Name;

	public UILabel Label_Level;

	public UILabel Label_Rank;

	public UILabel Label_Resource1;

	public UILabel Label_Resource2;

	public UILabel Label_Resource3;

	public GuildMemberInfo MemberInfo { get; private set; }

	public void Open(GuildMemberInfo memberInfo)
	{
		if (memberInfo == null)
		{
			VespaDebug.LogError("memberInfo is null");
			return;
		}
		MemberInfo = memberInfo;
		SetPortrait();
		SetInfo();
		SetResorces();
	}

	private void SetPortrait()
	{
		string iconPath = UIAvatarHelper.GetIconPath(MemberInfo.AvatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			UIHelper.SetBundleTexture(Texture_Portrait, iconPath);
		}
	}

	private void SetInfo()
	{
		UIHelper.SetLabelText(Label_Name, MemberInfo.Nick);
		UIHelper.SetLabelText(Label_Level, "Lv." + MemberInfo.TeamLevel);
		string guildRankString = ManageGuildMemberComponent.GetGuildRankString(MemberInfo.Rank);
		UIHelper.SetLabelText(Label_Rank, guildRankString);
	}

	private void SetResorces()
	{
		UIHelper.SetLabelText(Label_Resource1, MemberInfo.AccWood);
		UIHelper.SetLabelText(Label_Resource2, MemberInfo.AccStone);
		UIHelper.SetLabelText(Label_Resource3, MemberInfo.AccMetal);
	}

	private string GetLocaleDirect(string key)
	{
		return LocalizationManager.GetStringDirect(key);
	}

	public void OnClickComponent()
	{
	}
}
