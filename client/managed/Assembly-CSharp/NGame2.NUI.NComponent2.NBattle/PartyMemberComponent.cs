using NGameAsset;
using NShared.NParty;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NBattle;

public class PartyMemberComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/Battle/PartyMemberComponent";

	public static PrefabAuto<PartyMemberComponent> PrefabAuto = new PrefabAuto<PartyMemberComponent>("Prefab2/Component/Battle/PartyMemberComponent");

	public UITexture Texture_AvatarHero;

	public UISprite[] Sprite_PartySymbols;

	public UILabel Label_Nick;

	public void Open(MemberInfo memberInfo)
	{
		if (memberInfo != null)
		{
			UIHelper.SetLabelText(Label_Nick, memberInfo.Nick);
			SetupAvatarHero(memberInfo.AvatarHeroIndex);
			int memberIndex = memberInfo.MemberIndex;
			SetupPartySymbol(memberIndex);
		}
	}

	private void SetupAvatarHero(int AvatarHeroIndex)
	{
		if (AvatarHeroIndex != 0)
		{
			string battlePortraitPath = UIAvatarHelper.GetBattlePortraitPath(AvatarHeroIndex);
			if (!String_Extension_Method.IsNullOrEmpty(battlePortraitPath))
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(battlePortraitPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_AvatarHero, plainTexture);
			}
		}
	}

	private void SetupPartySymbol(int memberIndex)
	{
		for (int i = 0; i < Sprite_PartySymbols.Length; i++)
		{
			UIHelper.EnableGameObject(Sprite_PartySymbols[i], i == memberIndex);
		}
	}
}
