using NGame2.NUI.NHelper;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NLoading;

public class LoadingGamePlayTipComponent : MonoBehaviour, InitOnCreate, ILoadingComponent
{
	public const string PrefabPath = "Prefab2/Component/Loading/LoadingGamePlayTipComponent";

	public static PrefabAuto<LoadingGamePlayTipComponent> PrefabAuto = new PrefabAuto<LoadingGamePlayTipComponent>("Prefab2/Component/Loading/LoadingGamePlayTipComponent");

	public Transform OneLineTipRoot;

	public UILabel OneLineTipLabel;

	public UITexture BackgroundImageTexture;

	public UITexture HeroTex;

	private AssetTexture _backgroundImageAsset;

	private AssetTexture _heroImageAsset;

	public void Open(LoadingData loadingData)
	{
		string arg = loadingData.GetArg(0);
		InitBackgroundImage(arg);
		string arg2 = loadingData.GetArg(1);
		InitTipText(arg2);
		InitHero(loadingData);
	}

	private void InitBackgroundImage(string tipName)
	{
		if (_backgroundImageAsset != null)
		{
			_backgroundImageAsset.Close();
			_backgroundImageAsset = null;
		}
		_backgroundImageAsset = UIHelper.GetGamePlayTipImage(tipName);
		PlainAssetHolder plainAssetHolder = UIHelper.ResetPlaneAssetTextureAsync(this, BackgroundImageTexture, _backgroundImageAsset);
		if (plainAssetHolder != null)
		{
			plainAssetHolder.ReleaseAssetOnClose = true;
		}
		SprkImageRecovery.WatchLoadingTip(this);
	}

	private void InitTipText(string key)
	{
		if (String_Extension_Method.IsNullOrEmpty(key))
		{
			UIHelper.EnableGameObject(OneLineTipRoot, enable: false);
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect(key);
		if (stringDirect == key)
		{
			UIHelper.EnableGameObject(OneLineTipRoot, enable: false);
			return;
		}
		UIHelper.EnableGameObject(OneLineTipRoot, enable: true);
		UIHelper.SetLabelText(OneLineTipLabel, stringDirect);
	}

	private void InitHero(LoadingData loadingData)
	{
		if (_heroImageAsset != null)
		{
			_heroImageAsset.Close();
			_heroImageAsset = null;
		}
		string heroCode = loadingData.HeroCode;
		if (String_Extension_Method.IsNullOrEmpty(heroCode))
		{
			UIHelper.EnableGameObject(HeroTex, enable: false);
			return;
		}
		string heroFullPortraitPath = UIHelper.GetHeroFullPortraitPath(heroCode);
		_heroImageAsset = UIHelper.GetPlainTexture(heroFullPortraitPath);
		UIHelper.ResetPlaneAssetTextureAsync(this, HeroTex, _heroImageAsset);
		if (__Extension_Method_ICollection__.COUNT(loadingData.HeroPosOffset) >= 2)
		{
			float x = loadingData.HeroPosOffset[0];
			float y = loadingData.HeroPosOffset[1];
			HeroTex.transform.localPosition = new Vector3(x, y);
		}
		int heroHeight = loadingData.HeroHeight;
		int heroWidth = loadingData.HeroWidth;
		HeroTex.height = heroHeight;
		HeroTex.width = heroWidth;
	}

	public bool IsCompleted()
	{
		if (_backgroundImageAsset != null && !_backgroundImageAsset.ISDONE())
		{
			return false;
		}
		if (_heroImageAsset != null && !_heroImageAsset.ISDONE())
		{
			return false;
		}
		return true;
	}

	public UIWidget GetAnchorWidget(EAnchorPivot pivot)
	{
		return null;
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UIHelper.ReleaseTextureInfo(BackgroundImageTexture);
		_backgroundImageAsset = null;
		UIHelper.ReleaseTextureInfo(HeroTex);
		_heroImageAsset = null;
	}
}
