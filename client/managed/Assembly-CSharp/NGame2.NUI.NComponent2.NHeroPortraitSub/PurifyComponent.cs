using System;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public sealed class PurifyComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/PurifyComponent";

	public static PrefabAuto<PurifyComponent> PrefabAuto = new PrefabAuto<PurifyComponent>(PrefabPath);

	public UIBundleLoader PurifyBundleTex;

	public UIBundleLoader TranscendBundleTex;

	public UIWidget PurifySeparator;

	public UIWidget PurifyGaugeRoot;

	public UIWidget StarBlinkRoot;

	public UIWidget PurifyPercentRoot;

	public float FullGaugeBlinkTime;

	public AnimationCurve FullGaugeBlinkCurve = new AnimationCurve();

	public UISprite[] StarBlinks;

	public UILabel Label_PurifyPercent;

	private bool _bTranscend;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		PurifyGaugeRoot.SetAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.PurifyGauge), 0, 0, 0, 0);
		StarBlinkRoot.SetAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.StarBlink), 0, 0, 0, 0);
		PurifyPercentRoot.SetAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.PurifyPercent), 0, 0, 0, 0);
		PurifyGaugeRoot.ForceUpdateAnchors(includeChildren: true);
		StarBlinkRoot.ForceUpdateAnchors(includeChildren: true);
		PurifyPercentRoot.ForceUpdateAnchors(includeChildren: true);
		_bTranscend = owner.HeroInfo.Transcended > 0;
		UIHelper.EnableGameObject(PurifyBundleTex, !_bTranscend);
		UIHelper.EnableGameObject(StarBlinkRoot, !_bTranscend);
		UIHelper.EnableGameObject(TranscendBundleTex, _bTranscend);
	}

	public void Close()
	{
	}

	private void Update()
	{
		if (!_bTranscend)
		{
			UpdateStarBlink();
		}
	}

	private void UpdateStarBlink()
	{
		if (base.Owner.HeroInfo != null && base.Owner.HeroInfo.Star >= 2)
		{
			float num = Mathf.Repeat(GlobalTimer.time, FullGaugeBlinkTime) / FullGaugeBlinkTime;
			float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(FullGaugeBlinkCurve, num, num);
			if (StarBlinks[base.Owner.HeroInfo.Star - 2] != null && StarBlinks[base.Owner.HeroInfo.Star - 2].gameObject.activeSelf)
			{
				StarBlinks[base.Owner.HeroInfo.Star - 2].alpha = alpha;
			}
		}
	}

	public void SetPurified(float purified)
	{
		UIBundleLoader uIBundleLoader = (_bTranscend ? TranscendBundleTex : PurifyBundleTex);
		sbyte star = ((base.Owner.HeroInfo == null) ? base.Owner.CreatureData.StartHeroStar : base.Owner.HeroInfo.Star);
		string heroPortraitPath = UIHelper.GetHeroPortraitPath(base.Owner.CreatureData.CodeName, star, base.Owner.HeroInfo.CostumeIndex);
		UIHelper.SetBundleTexture(uIBundleLoader, heroPortraitPath);
		UIHelper.SetTextureFillAmount(uIBundleLoader.UITEX(), 1f - purified);
		if (uIBundleLoader.UITEX() != null && PurifySeparator != null)
		{
			uIBundleLoader.UITEX().RemoveFromPanel();
			float num = (float)uIBundleLoader.UITEX().height * purified - (float)uIBundleLoader.UITEX().height * 0.5f;
			PurifySeparator.transform.localPosition = Vector3.down * num;
		}
		for (int i = 0; i < StarBlinks.Length; i++)
		{
			UIHelper.EnableGameObject(StarBlinks[i], i == base.Owner.HeroInfo.Star - 2);
		}
		int num2 = Convert.ToInt32(purified * 100f);
		UIHelper.SetLabelText(Label_PurifyPercent, LocalizationManager.GetString(HeroPurify.PURIFY_PERCENT, num2));
	}
}
