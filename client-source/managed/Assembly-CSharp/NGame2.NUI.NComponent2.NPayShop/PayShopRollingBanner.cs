using System.Collections.Generic;
using NGameAsset;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopRollingBanner : RollingImagePanel
{
	public class BannerInfo
	{
		public int BannerIndex;

		public AssetTexture Texture;

		public PayShopBannerIndicatorComponent Indicator;
	}

	public interface IListener
	{
		void OnClick(int index);
	}

	public UISimpleGrid IndicatorGrid;

	private PayShopBannerIndicatorComponent _prevIndicator;

	private List<BannerInfo> _textureList = new List<BannerInfo>();

	private int _textureIndex;

	private IListener _listener;

	public void Open(IListener listener)
	{
		_listener = listener;
	}

	public void AddTexture(int bannerIndex, AssetTexture texture)
	{
		BannerInfo bannerInfo = new BannerInfo
		{
			BannerIndex = bannerIndex,
			Texture = texture
		};
		bannerInfo.Indicator = UIHelper.AddGridItem(IndicatorGrid, PayShopBannerIndicatorComponent.PrefabAuto);
		bannerInfo.Indicator.Open();
		_textureList.Add(bannerInfo);
		UIHelper.RepositionGrid(IndicatorGrid);
	}

	public void Play(int index = 0)
	{
		_textureIndex = index;
		SetTexture(CurrTexture, index);
		SetIndicator(index);
		ResetSwipe();
		base.Play();
	}

	public override void OnTouch()
	{
		base.OnTouch();
		if (_listener != null)
		{
			BannerInfo bannerInfo = GetBannerInfo(_textureIndex);
			if (bannerInfo != null)
			{
				_listener.OnClick(bannerInfo.BannerIndex);
			}
		}
	}

	private void SetTexture(UITexture uiTexture, int index)
	{
		BannerInfo bannerInfo = GetBannerInfo(index);
		if (bannerInfo != null)
		{
			UIHelper.SetTextureInfo(uiTexture, bannerInfo.Texture.GetTextureInfo());
		}
		OnChagneSize();
	}

	public BannerInfo GetBannerInfo(int index)
	{
		if (__Extension_Method_ICollection__.COUNT(_textureList) == 0)
		{
			return null;
		}
		if (__Extension_Method_ICollection__.COUNT(_textureList) <= index || 0 > index)
		{
			return null;
		}
		return _textureList[index];
	}

	protected override bool SetAutoNextImage()
	{
		base.SetAutoNextImage();
		_textureIndex = GetNextIndex(isIncrease: true);
		SetTexture(NextTexture, _textureIndex);
		return true;
	}

	protected int GetNextIndex(bool isIncrease)
	{
		int num = (isIncrease ? (_textureIndex + 1) : (_textureIndex - 1));
		if (__Extension_Method_ICollection__.COUNT(_textureList) <= num)
		{
			num = 0;
		}
		else if (num < 0)
		{
			num = __Extension_Method_ICollection__.COUNT(_textureList) - 1;
		}
		return num;
	}

	protected override void SetNextImage()
	{
		int nextIndex = GetNextIndex(isIncrease: true);
		SetTexture(NextTexture, nextIndex);
	}

	protected override void SetPrevImage()
	{
		int nextIndex = GetNextIndex(isIncrease: false);
		SetTexture(NextTexture, nextIndex);
	}

	protected override void SetDragNextImage()
	{
		base.SetDragNextImage();
		_textureIndex = GetNextIndex(_isForwardSwipeDirection);
	}

	protected override void OnImageChange()
	{
		base.OnImageChange();
		SetIndicator(_textureIndex);
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

	public void Clear()
	{
		foreach (BannerInfo texture in _textureList)
		{
			FactoryBase.SafeReleaseComponent(ref texture.Indicator);
		}
		_textureList.Clear();
	}
}
