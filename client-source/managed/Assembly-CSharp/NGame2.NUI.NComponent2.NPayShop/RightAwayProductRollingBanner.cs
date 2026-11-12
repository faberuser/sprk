using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class RightAwayProductRollingBanner : RollingImagePanel
{
	[Serializable]
	public class LabelData
	{
		public UILabel Label_CurRemainTime;

		public UILabel Label_CurCount;
	}

	public class BannerInfo
	{
		public int ProductIndex;

		public string BannerUrl;

		public string EndTime;

		public int Count;

		public PayShopBannerIndicatorComponent Indicator;
	}

	public interface IListener
	{
		void OnClick(int productIndex);
	}

	public UISimpleGrid IndicatorGrid;

	private PayShopBannerIndicatorComponent _prevIndicator;

	public BoxCollider BoxCollider;

	[Header("[Current Label]")]
	public LabelData CurrentLabels;

	[Header("[Next Label]")]
	public LabelData NextLabels;

	private List<BannerInfo> _bannerInfoList = new List<BannerInfo>();

	private int _curBannerInfoListIndex;

	private int _curProductIndex;

	private bool _playStart;

	private BannerInfo _curBannerInfo;

	private BannerInfo _nextBannerInfo;

	private RightAwayProductInfo[] _rightAwayProductInfos;

	private IListener _listener;

	public void Open(IListener listener)
	{
		_listener = listener;
	}

	public void Clear()
	{
		_playStart = false;
		CurrTexture.mainTexture = null;
		NextTexture.mainTexture = null;
		_curBannerInfo = null;
		_nextBannerInfo = null;
		foreach (BannerInfo bannerInfo in _bannerInfoList)
		{
			FactoryBase.SafeReleaseComponent(ref bannerInfo.Indicator);
		}
		_bannerInfoList.Clear();
	}

	public void AddBannerInfo(RightAwayProductInfo rightAwayProductInfo)
	{
		if (rightAwayProductInfo != null && (String_Extension_Method.IsNullOrEmpty(rightAwayProductInfo.EndTime) || !(GetBannerRemainTime(rightAwayProductInfo.EndTime).TotalSeconds <= 0.0)) && !String_Extension_Method.IsNullOrEmpty(rightAwayProductInfo.BannerPath))
		{
			LobbyManagement.AddRightAwayBannerDic(rightAwayProductInfo.Index, rightAwayProductInfo.BannerPath);
			BannerInfo bannerInfo = new BannerInfo
			{
				ProductIndex = rightAwayProductInfo.Index,
				BannerUrl = rightAwayProductInfo.BannerPath,
				EndTime = rightAwayProductInfo.EndTime,
				Count = rightAwayProductInfo.LimitedQuantity
			};
			bannerInfo.Indicator = UIHelper.AddGridItem(IndicatorGrid, PayShopBannerIndicatorComponent.PrefabAuto);
			bannerInfo.Indicator.Open();
			_bannerInfoList.Add(bannerInfo);
			UIHelper.RepositionGrid(IndicatorGrid);
		}
	}

	public void RefreshBannerInfo()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_bannerInfoList); i++)
		{
			BannerInfo bannerInfo = _bannerInfoList[i];
			RightAwayProductInfo rightAwayProductInfo = GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.Get(bannerInfo.ProductIndex);
			if (rightAwayProductInfo == null)
			{
				VespaDebug.LogError($"RightAwayProductInfo {bannerInfo.ProductIndex} is null");
				continue;
			}
			bannerInfo.EndTime = rightAwayProductInfo.EndTime;
			bannerInfo.Count = rightAwayProductInfo.LimitedQuantity;
		}
	}

	public bool CheckBannerReset(RightAwayProductInfo[] changeProductInfo)
	{
		if (__Extension_Method_ICollection__.COUNT(changeProductInfo) != __Extension_Method_ICollection__.COUNT(_rightAwayProductInfos))
		{
			_rightAwayProductInfos = changeProductInfo;
			return true;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rightAwayProductInfos); i++)
		{
			int index = _rightAwayProductInfos[i].Index;
			int index2 = changeProductInfo[i].Index;
			if (index != index2)
			{
				_rightAwayProductInfos = changeProductInfo;
				return true;
			}
		}
		return false;
	}

	public void StartPlay(int index = 0)
	{
		_curBannerInfoListIndex = index;
		_curBannerInfo = GetBannerInfo(index);
		_curProductIndex = GetCurProductIndex();
		_playStart = true;
	}

	private void PlayBanner()
	{
		SetTexture(CurrTexture, _curBannerInfoListIndex);
		SetIndicator(_curBannerInfoListIndex);
		ResetSwipe();
		Play();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateStartPlay();
		UpdateLabels();
	}

	private void UpdateStartPlay()
	{
		if (!_playStart)
		{
			return;
		}
		if (_curBannerInfo != null)
		{
			AssetTexture newBannerAssetTexture = LobbyManagement.GetNewBannerAssetTexture(_curBannerInfo.ProductIndex);
			if (newBannerAssetTexture != null && !newBannerAssetTexture.IsDone)
			{
				return;
			}
		}
		PlayBanner();
		_playStart = false;
	}

	private void UpdateLabels()
	{
		SetLabels(CurrentLabels, _curBannerInfo);
		SetLabels(NextLabels, _nextBannerInfo);
	}

	private void SetLabels(LabelData labelData, BannerInfo bannerInfo)
	{
		if (bannerInfo == null)
		{
			UIHelper.EnableGameObject(labelData.Label_CurRemainTime, enable: false);
			UIHelper.EnableGameObject(labelData.Label_CurCount, enable: false);
			return;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		TimeSpan ts = Convert.ToDateTime(bannerInfo.EndTime) - currentServerUTCTime;
		string stringDirect = LocalizationManager.GetStringDirect("RIGHTAWAY_SECOND_PRODUCT_REMAINING_TIME_TEXT", UIHelper.GetRemainTimeLabelText(ts));
		UIHelper.EnableGameObject(labelData.Label_CurRemainTime, ts.TotalSeconds > 0.0);
		UIHelper.SetLabelText(labelData.Label_CurRemainTime, stringDirect);
		int count = bannerInfo.Count;
		string stringDirect2 = LocalizationManager.GetStringDirect("RIGHTAWAY_SECOND_PRODUCT_REMAINING_COUNT_TEXT", count.ToString());
		UIHelper.EnableGameObject(labelData.Label_CurCount, count > 0);
		if (count > 0)
		{
			UIHelper.SetLabelText(labelData.Label_CurCount, stringDirect2);
		}
	}

	private void SetTexture(UITexture uiTexture, int index)
	{
		BannerInfo bannerInfo = GetBannerInfo(index);
		if (bannerInfo == null)
		{
			UIHelper.SetTextureInfo(uiTexture, null);
			OnChagneSize();
			return;
		}
		AssetTexture newBannerAssetTexture = LobbyManagement.GetNewBannerAssetTexture(bannerInfo.ProductIndex);
		if (newBannerAssetTexture != null)
		{
			UIHelper.SetTextureInfo(uiTexture, newBannerAssetTexture.GetTextureInfo());
		}
		else
		{
			UIHelper.SetTextureInfo(uiTexture, null);
		}
		OnChagneSize();
	}

	public BannerInfo GetBannerInfo(int index)
	{
		if (__Extension_Method_ICollection__.COUNT(_bannerInfoList) == 0)
		{
			return null;
		}
		if (__Extension_Method_ICollection__.COUNT(_bannerInfoList) <= index || 0 > index)
		{
			return null;
		}
		return _bannerInfoList[index];
	}

	private TimeSpan GetBannerRemainTime(string endTimeStr)
	{
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		return Convert.ToDateTime(endTimeStr) - currentServerUTCTime;
	}

	private int GetCurProductIndex()
	{
		if (__Extension_Method_ICollection__.COUNT(_bannerInfoList) == 0)
		{
			return -1;
		}
		if (__Extension_Method_ICollection__.COUNT(_bannerInfoList) <= _curBannerInfoListIndex || 0 > _curBannerInfoListIndex)
		{
			return -1;
		}
		return _bannerInfoList[_curBannerInfoListIndex].ProductIndex;
	}

	protected int GetNextIndex(int index, bool isIncrease)
	{
		int num = (isIncrease ? (index + 1) : (index - 1));
		if (__Extension_Method_ICollection__.COUNT(_bannerInfoList) <= num)
		{
			num = 0;
		}
		else if (num < 0)
		{
			num = __Extension_Method_ICollection__.COUNT(_bannerInfoList) - 1;
		}
		BannerInfo bannerInfo = GetBannerInfo(num);
		if (bannerInfo != null && !String_Extension_Method.IsNullOrEmpty(bannerInfo.EndTime) && GetBannerRemainTime(bannerInfo.EndTime).TotalSeconds <= 0.0)
		{
			FactoryBase.SafeReleaseComponent(ref bannerInfo.Indicator);
			_bannerInfoList.Remove(bannerInfo);
			UIHelper.RepositionGrid(IndicatorGrid);
			return GetNextIndex(index, isIncrease);
		}
		return num;
	}

	protected override bool SetAutoNextImage()
	{
		base.SetAutoNextImage();
		int curProductIndex = _curProductIndex;
		_curBannerInfoListIndex = GetNextIndex(_curBannerInfoListIndex, isIncrease: true);
		_nextBannerInfo = GetBannerInfo(_curBannerInfoListIndex);
		_curProductIndex = GetCurProductIndex();
		SetTexture(NextTexture, _curBannerInfoListIndex);
		return curProductIndex != _curProductIndex;
	}

	protected override void SetNextImage()
	{
		int nextIndex = GetNextIndex(_curBannerInfoListIndex, isIncrease: true);
		_nextBannerInfo = GetBannerInfo(nextIndex);
		SetTexture(NextTexture, nextIndex);
	}

	protected override void SetPrevImage()
	{
		int nextIndex = GetNextIndex(_curBannerInfoListIndex, isIncrease: false);
		_nextBannerInfo = GetBannerInfo(nextIndex);
		SetTexture(NextTexture, nextIndex);
	}

	protected override void SetDragNextImage()
	{
		base.SetDragNextImage();
		_curBannerInfoListIndex = GetNextIndex(_curBannerInfoListIndex, _isForwardSwipeDirection);
		_nextBannerInfo = GetBannerInfo(_curBannerInfoListIndex);
		_curProductIndex = GetCurProductIndex();
	}

	protected override void OnImageChange()
	{
		base.OnImageChange();
		_curBannerInfo = _nextBannerInfo;
		_enableSwipe = __Extension_Method_ICollection__.COUNT(_bannerInfoList) > 1;
		SetIndicator(_curBannerInfoListIndex);
		UpdateLabels();
		SetColliderEnable();
	}

	private void SetIndicator(int index)
	{
		if (_prevIndicator != null)
		{
			_prevIndicator.SetOn(On: false);
		}
		BannerInfo bannerInfo = GetBannerInfo(index);
		if (bannerInfo != null && bannerInfo.Indicator != null)
		{
			bannerInfo.Indicator.SetOn(On: true);
			_prevIndicator = bannerInfo.Indicator;
		}
	}

	public void SetColliderEnable()
	{
		bool isEnable = __Extension_Method_ICollection__.COUNT(_bannerInfoList) > 0;
		UIHelper.EnableCollider(BoxCollider, isEnable);
	}

	public override void OnTouch()
	{
		base.OnTouch();
		if (_listener != null)
		{
			BannerInfo bannerInfo = GetBannerInfo(_curBannerInfoListIndex);
			if (bannerInfo != null)
			{
				_listener.OnClick(bannerInfo.ProductIndex);
			}
		}
	}
}
