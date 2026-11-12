using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class HeroStatComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/HeroStatComponent";

	public static PrefabAuto<HeroStatComponent> PrefabAuto = new PrefabAuto<HeroStatComponent>(PrefabPath);

	public UISprite SpriteHp;

	public UISprite SpriteMp;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.HeroStat));
	}

	public void Close()
	{
	}

	public void SetHPMPSprite(float hpRatio, float mpRatio)
	{
		UIHelper.SetSpriteFillAmount(SpriteHp, hpRatio);
		UIHelper.SetSpriteFillAmount(SpriteMp, mpRatio);
	}
}
