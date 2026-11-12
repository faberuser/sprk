using NGame2.NUI.NHelper.NGuild;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressStatusPopupRankComponent : GuildSuppressStatusBoardUserComponent
{
	public new const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressStatusPopupRankComponent";

	public new static PrefabAuto<GuildSuppressStatusPopupRankComponent> PrefabAuto = new PrefabAuto<GuildSuppressStatusPopupRankComponent>("Prefab2/Component/GuildSuppress/GuildSuppressStatusPopupRankComponent");

	public UITexture HeroTexture;

	public override void OnRelease()
	{
		base.OnRelease();
		UIHelper.ClearPlaneAssetTexture(HeroTexture);
	}

	public void Open(int rank, int heroIndex, bool isMe, string name, long score)
	{
		Open(rank, isMe, name, score);
		SetHeroPortrait(heroIndex);
	}

	private void SetHeroPortrait(int heroIndex)
	{
		GuildSuppressHelper.SetHeroPortrait(heroIndex, isBattlePortrait: false, HeroTexture, this);
	}
}
