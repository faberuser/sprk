using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class EmptyHeroPortraitSmallComponent : UIScrollItemBehaviour
{
	public class HeroInfo
	{
		public int HeroIndex { get; set; }

		public int HeroStar { get; set; }

		public int HeroTranscend { get; set; }

		public HeroPortraitComponent.Listener IListener { get; set; }
	}

	public const string PATH_PREFAB = "Resources/Prefab2/Component/Portrait/EmptyHeroPortraitSmall";

	public const string PrefabPath = "Prefab2/Component/Portrait/EmptyHeroPortraitSmallComponent";

	public static PrefabAuto<EmptyHeroPortraitSmallComponent> PrefabAuto = new PrefabAuto<EmptyHeroPortraitSmallComponent>("Resources/Prefab2/Component/Portrait/EmptyHeroPortraitSmall");

	public static PrefabAuto<EmptyHeroPortraitSmallComponent> PrefabAuto2 = new PrefabAuto<EmptyHeroPortraitSmallComponent>("Prefab2/Component/Portrait/EmptyHeroPortraitSmallComponent");

	public HeroPortraitComponent HeroPortraitComponent { get; private set; }

	public void Open(HeroInfo heroInfo)
	{
		Init(heroInfo);
	}

	private void Init(HeroInfo heroInfo)
	{
		AddSmallHeroPortrait(heroInfo);
	}

	private void ReleaseComponent()
	{
		FactoryBase.ReleaseComponent(HeroPortraitComponent);
		HeroPortraitComponent = null;
	}

	private void AddSmallHeroPortrait(HeroInfo heroInfo)
	{
		ReleaseComponent();
		if (heroInfo != null)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroInfo.HeroIndex);
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
			bool owned = cachedHeroInfo != null;
			HeroPortraitComponent = HeroPortraitComponent.SmallPrefabAuto.Factory.Create(base.transform);
			HeroPortraitComponent.Open(data, heroInfo.IListener, HeroPortraitComponent.EPortraitMode.Icon, heroInfo.HeroStar, heroInfo.HeroTranscend);
			HeroPortraitComponent.SetOwned(owned);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(HeroPortraitComponent.transform);
		}
	}
}
