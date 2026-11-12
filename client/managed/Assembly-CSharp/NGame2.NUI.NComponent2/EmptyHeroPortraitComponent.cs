using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class EmptyHeroPortraitComponent : UIScrollItemBehaviour, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		void ViewDetail(int HeroIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Portrait/EmptyHeroPortrait";

	public static PrefabAuto<EmptyHeroPortraitComponent> PrefabAuto = new PrefabAuto<EmptyHeroPortraitComponent>("Resources/Prefab2/Component/Portrait/EmptyHeroPortrait");

	public UISimpleGrid HeroGrid;

	public HeroPortraitComponent.PrefabSelector PortraitSelector = new HeroPortraitComponent.PrefabSelector(HeroPortraitComponent.PrefabSelector.EPrefabType.Short);

	private HeroPortraitComponent _heroPortrait;

	private Listener _listener;

	public CreatureData CreatureData { get; private set; }

	public void Open(CreatureData creatureData, Listener listener)
	{
		Reset();
		CreatureData = creatureData;
		_listener = listener;
		Setup();
	}

	private void Reset()
	{
		FactoryBase.SafeReleaseComponent(ref _heroPortrait);
	}

	private void Setup()
	{
		_heroPortrait = UIHelper.AddGridItem(HeroGrid, PortraitSelector.PrefabAuto);
		_heroPortrait.Open(CreatureData, this);
		_heroPortrait.ShowCreatureName();
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (_listener != null)
		{
			_listener.ViewDetail(CreatureData.Index);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
		if (_listener != null)
		{
			_listener.ViewDetail(CreatureData.Index);
		}
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}
}
