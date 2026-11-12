using System;
using System.Collections.Generic;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public sealed class AchievementHeroComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void SelectHero(int HeroIndex);
	}

	public interface IDataProvider
	{
		bool AchievementCompleted(int HeroIndex);

		int GetRewardableCount(int HeroIndex);
	}

	public const string PrefabPath = "Prefab2/Component/Achievement/AchievementHeroComponent";

	public static PrefabAuto<AchievementHeroComponent> PrefabAuto = new PrefabAuto<AchievementHeroComponent>("Prefab2/Component/Achievement/AchievementHeroComponent");

	public UIBundleLoader BundleTex_Portrait;

	public List<HeroPortraitComponent.ClassTagInfo> ClassTagInfos;

	public UILabel Label_Level;

	public UIWidget WidgetCompleted;

	public UILabel Label_HeroName;

	public UISprite Sprite_LevelBack;

	public UIWidget WidgetCleared;

	public UILabel Label_Cleared;

	public UISprite Sprite_HeroFrame;

	public UISpriteSelector StarSelector;

	public UISpriteSelector TranscendStarSelector;

	public UILabel Label_Transcend;

	public List<UIWidget> HeroFrameList;

	private Listener _listener;

	private IDataProvider _dataProvider;

	public CachedHeroInfo HeroInfo { get; private set; }

	public int HeroIndex { get; private set; }

	public void Open(Listener listener, IDataProvider dataProvider, CachedHeroInfo heroInfo)
	{
		_listener = listener;
		HeroInfo = heroInfo;
		HeroIndex = heroInfo.HeroIndex;
		_dataProvider = dataProvider;
		SetGrayScale(isGrayScale: false);
		SetupHero();
		SetHeroFrame();
	}

	public void OpenNotOwned(int heroIndex)
	{
		_listener = null;
		HeroInfo = null;
		_dataProvider = null;
		HeroIndex = heroIndex;
		SetGrayScale(isGrayScale: true);
		SetupNotOwnedHero(heroIndex);
		SetHeroFrame();
	}

	private void SetGrayScale(bool isGrayScale)
	{
		Action<UISprite> SetGrayScale = delegate(UISprite sprite)
		{
			if (sprite != null)
			{
				sprite.isGrayscale = isGrayScale;
			}
		};
		ClassTagInfos.ForEach(delegate(HeroPortraitComponent.ClassTagInfo info)
		{
			SetGrayScale(info.Sprite_ClassTag);
		});
		if (StarSelector != null)
		{
			UIHelper.SetWidgetGrayScale(StarSelector.GetTargetSprite(), isGrayScale);
		}
		UIHelper.SetBundleTextureGrayscale(BundleTex_Portrait, isGrayScale);
		UIHelper.SetWidgetGrayScale(Sprite_LevelBack, isGrayScale);
		UIHelper.SetWidgetGrayScale(Sprite_HeroFrame, isGrayScale);
	}

	private void SetupHero()
	{
		if (HeroInfo == null)
		{
			return;
		}
		CreatureData creatureData = GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
		if (creatureData != null)
		{
			string heroPortraitPath = UIHelper.GetHeroPortraitPath(creatureData.CodeName, HeroInfo.Star, HeroInfo.CostumeIndex);
			UIHelper.SetBundleTexture(BundleTex_Portrait, heroPortraitPath);
			UIHelper.SetLabelText(Label_HeroName, creatureData.Name);
			UIHelper.EnableGameObject(StarSelector, HeroInfo.Transcended == 0);
			UIHelper.EnableGameObject(TranscendStarSelector, HeroInfo.Transcended > 0);
			if (HeroInfo.Transcended == 0 && StarSelector != null)
			{
				StarSelector.SelectInt(HeroInfo.Star);
			}
			if (HeroInfo.Transcended > 0 && TranscendStarSelector != null)
			{
				TranscendStarSelector.SelectInt(HeroInfo.Transcended);
			}
			ClassTagInfos.ForEach(delegate(HeroPortraitComponent.ClassTagInfo info)
			{
				UIHelper.EnableGameObject(info.Sprite_ClassTag, info.tagType == creatureData.TagType);
			});
			UIHelper.EnableGameObject(Label_Level, enable: true);
			UIHelper.SetLabelText(Label_Level, "Lv " + HeroInfo.Level);
			UIHelper.EnableGameObject(WidgetCompleted, _dataProvider.AchievementCompleted(HeroInfo.HeroIndex));
			int rewardableCount = _dataProvider.GetRewardableCount(HeroInfo.HeroIndex);
			UIHelper.EnableGameObject(WidgetCleared, rewardableCount > 0);
			if (rewardableCount > 0)
			{
				UIHelper.SetLabelText(Label_Cleared, rewardableCount);
			}
		}
	}

	private void SetupNotOwnedHero(int HeroIndex)
	{
		CreatureData creatureData = GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);
		if (creatureData != null)
		{
			string heroPortraitPath = UIHelper.GetHeroPortraitPath(creatureData.CodeName, creatureData.StartHeroStar);
			UIHelper.SetBundleTexture(BundleTex_Portrait, heroPortraitPath);
			UIHelper.SetLabelText(Label_HeroName, creatureData.Name);
			ClassTagInfos.ForEach(delegate(HeroPortraitComponent.ClassTagInfo info)
			{
				UIHelper.EnableGameObject(info.Sprite_ClassTag, info.tagType == creatureData.TagType);
			});
			UIHelper.EnableGameObject(StarSelector, enable: true);
			UIHelper.EnableGameObject(TranscendStarSelector, enable: false);
			if (StarSelector != null)
			{
				StarSelector.SelectInt(creatureData.StartHeroStar);
			}
			UIHelper.EnableGameObject(Label_Level, enable: false);
			UIHelper.EnableGameObject(WidgetCompleted, enable: false);
			UIHelper.EnableGameObject(WidgetCleared, enable: false);
		}
	}

	public void SetHeroFrame()
	{
		if (HeroInfo == null)
		{
			return;
		}
		for (int i = 0; i < HeroFrameList.Count; i++)
		{
			UIWidget component = HeroFrameList[i];
			if (HeroInfo.Transcended == i)
			{
				UIHelper.EnableGameObject(component, enable: true);
			}
			else
			{
				UIHelper.EnableGameObject(component, enable: false);
			}
		}
	}

	public void OnClickHero()
	{
		if (_listener != null)
		{
			_listener.SelectHero(HeroInfo.HeroIndex);
		}
	}
}
