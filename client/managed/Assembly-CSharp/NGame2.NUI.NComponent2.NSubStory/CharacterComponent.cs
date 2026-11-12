using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NSubStory;

public class CharacterComponent : LinkFactoryComponent, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		void OnClick(CharacterComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/SubStory/CharacterComponent";

	public static PrefabAuto<CharacterComponent> PrefabAuto = new PrefabAuto<CharacterComponent>("Prefab2/Component/SubStory/CharacterComponent");

	private Listener _listener;

	private CreatureData _creatureData;

	private HeroPortraitComponent _portrait;

	public UISprite Spr_Select;

	public Transform Trs_Root;

	public SubStoryData SubStoryData { get; private set; }

	public int HeroIndex { get; private set; }

	public void Select(bool select)
	{
		UIHelper.EnableGameObject(Spr_Select, select);
	}

	public bool Init(Listener listener, SubStoryData subStoryData, CachedHeroInfo heroInfo)
	{
		SubStoryData = subStoryData;
		_listener = listener;
		HeroIndex = heroInfo.HeroIndex;
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		FactoryBase.SafeReleaseComponent(ref _portrait);
		_portrait = HeroPortraitComponent.SmallPrefabAuto.Factory.Create(Trs_Root);
		_portrait.Open(_creatureData, this, HeroPortraitComponent.EPortraitMode.Icon, 0, 0, SubStoryData.CostumeIndex);
		_portrait.ShowStar(show: false);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_portrait.transform);
		return true;
	}

	public void SetGrayPortrait(bool grayScale)
	{
		if (_portrait != null)
		{
			_portrait.SetGrayScale(grayScale);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}
}
