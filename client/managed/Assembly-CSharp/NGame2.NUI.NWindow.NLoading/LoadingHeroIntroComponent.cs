using System.Collections.Generic;
using NGame2.NUI.NHelper;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NLoading;

public class LoadingHeroIntroComponent : MonoBehaviour, InitOnCreate, ILoadingComponent
{
	public const string PrefabPath = "Prefab2/Component/Loading/LoadingHeroIntroComponent";

	public static PrefabAuto<LoadingHeroIntroComponent> PrefabAuto = new PrefabAuto<LoadingHeroIntroComponent>("Prefab2/Component/Loading/LoadingHeroIntroComponent");

	public UIWidget HeroIntroDesc;

	public UILabel HeroTitleLabel;

	public UILabel HeroNameLabel;

	public Transform OneLineTipRoot;

	public UILabel OneLineTipLabel;

	public UITexture HeroBackgroundImageTexture;

	public List<LoadingDefault.AnchorWidgetInfo> ListAnchorWidgetInfo = new List<LoadingDefault.AnchorWidgetInfo>();

	private AssetTexture _imageAsset;

	public void Open(LoadingData loadingData)
	{
		string arg = loadingData.GetArg(1);
		string stringDirect = LocalizationManager.GetStringDirect(loadingData.GetArg(1));
		if (stringDirect == arg)
		{
			UIHelper.EnableGameObject(HeroIntroDesc, enable: false);
		}
		else
		{
			string[] array = stringDirect.Split(new char[1] { '\n' });
			UIHelper.SetLabelText(HeroTitleLabel, (array.Length >= 1) ? array[0] : string.Empty);
			UIHelper.SetLabelText(HeroNameLabel, (array.Length >= 2) ? array[1] : string.Empty);
		}
		UIHelper.EnableGameObject(OneLineTipRoot, enable: false);
		int argInt = loadingData.GetArgInt(0);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(argInt);
		if (data != null)
		{
			InitHeroBackground(data);
		}
	}

	private void InitHeroBackground(CreatureData creatureData)
	{
		if (creatureData == null)
		{
			SprkImageRecovery.WatchLoadingHero(this);
			return;
		}
		_imageAsset = UIHelper.GetHeroBackgroundImage(creatureData.CodeName);
		PlainAssetHolder plainAssetHolder = UIHelper.ResetPlaneAssetTextureAsync(this, HeroBackgroundImageTexture, _imageAsset);
		if (plainAssetHolder != null)
		{
			plainAssetHolder.ReleaseAssetOnClose = true;
		}
		SprkImageRecovery.WatchLoadingHero(this);
	}

	public bool IsCompleted()
	{
		if (_imageAsset != null && !_imageAsset.IsDone)
		{
			return false;
		}
		return true;
	}

	public UIWidget GetAnchorWidget(EAnchorPivot pivot)
	{
		return ListAnchorWidgetInfo.Find((LoadingDefault.AnchorWidgetInfo info) => info.Pivot == pivot)?.Widget;
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UIHelper.ClearPlaneAssetTexture(HeroBackgroundImageTexture);
		UIHelper.ReleaseTextureInfo(HeroBackgroundImageTexture);
		_imageAsset = null;
	}
}
