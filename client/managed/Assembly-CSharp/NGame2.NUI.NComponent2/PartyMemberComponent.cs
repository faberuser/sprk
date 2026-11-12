using NGameAsset;
using NShared.NParty;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class PartyMemberComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(PartyMemberComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Party/PartyMemberComponent";

	public static PrefabAuto<PartyMemberComponent> PrefabAuto = new PrefabAuto<PartyMemberComponent>("Prefab2/Component/Party/PartyMemberComponent");

	public UITexture AvatarTexture;

	public UILabel NickLabel;

	public UILabel TeamLevelLabel;

	private Listener _listener;

	public MemberInfo PartyMemberInfo { get; private set; }

	public void Open(Listener listener, MemberInfo partyMemberInfo)
	{
		if (partyMemberInfo != null)
		{
			PartyMemberInfo = partyMemberInfo;
			_listener = listener;
			SetAvatar(partyMemberInfo.AvatarHeroIndex);
			UIHelper.SetLabelText(NickLabel, partyMemberInfo.Nick);
			UIHelper.SetLabelText(TeamLevelLabel, partyMemberInfo.TeamLevel);
		}
	}

	public void SetAvatar(int avatarHeroIndex)
	{
		string iconPath = UIAvatarHelper.GetIconPath(avatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, AvatarTexture, plainTexture);
		}
	}

	public void OnClickPartyMember()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
