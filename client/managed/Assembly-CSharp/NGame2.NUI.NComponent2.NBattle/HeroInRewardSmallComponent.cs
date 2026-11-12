using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NBattle;

public class HeroInRewardSmallComponent : HeroInRewardComponent
{
	public new const string PrefabPath = "Prefab2/Component/Battle/HeroInRewardSmallComponent";

	public new static PrefabAuto<HeroInRewardSmallComponent> PrefabAuto = new PrefabAuto<HeroInRewardSmallComponent>("Prefab2/Component/Battle/HeroInRewardSmallComponent");

	public UIBundleLoader BundleTexPortrait;

	public UIWidget ParentWidget;

	public UIGame3DWidget LevelUpEffect;

	public override bool Open(CachedHeroInfo heroInfo, string nick, long accountId, bool isMercenary, bool isDead, HeroExpResultInfo expResultInfo, FlaskEndCampaignResultInfo flaskResultInfo, bool isPlaySound = false)
	{
		if (!SetupUI(heroInfo, isMercenary, isDead, expResultInfo, flaskResultInfo, isPlaySound))
		{
			return false;
		}
		SetPortraitTexture();
		return true;
	}

	protected override bool SetupUI(CachedHeroInfo heroInfo, bool isMercenary, bool isDead, HeroExpResultInfo expResultInfo, FlaskEndCampaignResultInfo flaskResultInfo, bool isPlaySound)
	{
		return base.SetupUI(heroInfo, isMercenary, isDead, expResultInfo, flaskResultInfo, isPlaySound);
	}

	protected override void PlayLevelupEffect()
	{
		LevelUpEffect.Play(null);
		UIHelper.EnableGameObject(AddExpLabel, enable: false);
		FlaskSourceIconInfo[] flaskSourceIcons = FlaskSourceIcons;
		for (int i = 0; i < flaskSourceIcons.Length; i++)
		{
			UIHelper.EnableGameObject(flaskSourceIcons[i].Icon, enable: false);
		}
	}

	private void SetPortraitTexture()
	{
		string heroIconPath = UIHelper.GetHeroIconPath(base.HeroInfo);
		UIHelper.SetBundleTexture(BundleTexPortrait, heroIconPath);
	}

	private void SetGrayScaleWidget(bool grayScale)
	{
		ParentWidget.isGrayscale = grayScale;
	}
}
