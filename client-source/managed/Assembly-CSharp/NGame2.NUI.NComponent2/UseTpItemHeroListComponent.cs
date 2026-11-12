using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class UseTpItemHeroListComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickHero(CachedHeroInfo heroInfo);
	}

	public const string PrefabPath = "Prefab2/Component/Hero/UseTpItemHeroListComponent";

	public static PrefabAuto<UseTpItemHeroListComponent> PrefabAuto = new PrefabAuto<UseTpItemHeroListComponent>("Prefab2/Component/Hero/UseTpItemHeroListComponent");

	public UISimpleGrid HeroSmallPortraitGrid;

	public UILabel LabelMax;

	public UILabel LabelHeroLevel;

	public UILabel LabelHeroName;

	public UILabel LabelTp;

	public GameObject GameObjectDisable;

	private HeroPortraitComponent _heroPortrait;

	private IListener _iListener;

	private bool _isMax;

	public CachedHeroInfo HeroInfo { get; private set; }

	public void Open(CachedHeroInfo heroInfo, IListener listener)
	{
		_iListener = listener;
		HeroInfo = heroInfo;
		SetupHeroPortrait();
		SetHeroInfo();
		SetHeroTp();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		HeroInfo = null;
		BoxCollider component = _heroPortrait.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.enabled = true;
		}
		FactoryBase.SafeReleaseComponent(ref _heroPortrait);
	}

	private void SetupHeroPortrait()
	{
		_heroPortrait = UIHelper.AddGridItem(HeroSmallPortraitGrid, HeroPortraitComponent.SmallPrefabAuto);
		_heroPortrait.Open(HeroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
		_heroPortrait.SetLevel(show: false);
		BoxCollider component = _heroPortrait.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.enabled = false;
		}
		UIHelper.RepositionGrid(HeroSmallPortraitGrid);
	}

	public void SetHeroInfo()
	{
		UIHelper.SetLabelText(LabelHeroLevel, "Lv" + HeroInfo.Level);
		CreatureData creatureData = HeroInfo.CreatureData;
		UIHelper.SetLabelText(LabelHeroName, (creatureData != null) ? creatureData.Name : string.Empty);
	}

	public void SetHeroTp()
	{
		int currentTotalTrancendPoint = HeroInfo.GetCurrentTotalTrancendPoint();
		int usableTranscendSkillPoint = HeroInfo.GetUsableTranscendSkillPoint(HeroInfo.SkillPage);
		UIHelper.SetLabelText(LabelTp, $"{usableTranscendSkillPoint:00}/{currentTotalTrancendPoint:00}");
		short transcendPoint = HeroInfo.TranscendPoint;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxExtraTranscendPoint");
		_isMax = transcendPoint >= valueInt;
		UIHelper.EnableGameObject(GameObjectDisable, _isMax);
		UIHelper.EnableGameObject(LabelMax, _isMax);
	}

	public void OnClickUse()
	{
		if (!_isMax && _iListener != null)
		{
			_iListener.OnClickHero(HeroInfo);
		}
	}
}
