using NGameAsset;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HeroItemComponent : LinkFactoryComponent
{
	public static string PrefabPath = "Prefab2/Component/Icon/HeroItemPortrait";

	public static PrefabAuto<HeroItemComponent> PrefabAuto = new PrefabAuto<HeroItemComponent>(PrefabPath);

	public UISprite Sprite_Back;

	public UISprite Sprite_NoTailBack;

	public UITexture HeroImage;

	private CreatureData _creatureData;

	public void Open(CreatureData creatureData)
	{
		_creatureData = creatureData;
		SetHeroImage();
	}

	private void SetHeroImage()
	{
		sbyte startHeroStar = _creatureData.StartHeroStar;
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(_creatureData.Index, startHeroStar);
		UIHelper.ResetPlaneAssetTextureAsync(this, HeroImage, heroIcon);
	}

	public void SetBorderImage(bool TailExist)
	{
		UIHelper.EnableGameObject(Sprite_Back, TailExist);
		UIHelper.EnableGameObject(Sprite_NoTailBack, !TailExist);
	}
}
