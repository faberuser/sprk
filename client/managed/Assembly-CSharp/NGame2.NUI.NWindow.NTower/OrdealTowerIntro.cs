using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NTower;
using NGameAsset;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class OrdealTowerIntro : BaseTower
{
	public interface Listener
	{
		void OnResetTower(OrdealTowerIntro caller);

		void OnEnterTower();

		void OnReceiveReward();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tower/OrdealTowerIntro";

	public Transform ItemRoot;

	public UIButton ButtonEnter;

	public UIButton ButtonReward;

	public UIButton ButtonReset;

	public UITexture TowerBackground;

	private Component _itemComponent;

	private Listener _listener;

	public void Init(TowerData towerData, TowerInfo towerInfo, Listener listener)
	{
		Init(towerData, towerInfo);
		_listener = listener;
		SetupItemIcon();
		LoadBackgroundTower();
		SetupButton();
	}

	public void Refresh(TowerInfo towerInfo)
	{
		Init(base.TowerData, towerInfo);
		SetupButton();
	}

	private void LoadBackgroundTower()
	{
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open("Tower/" + base.TowerData.TowerImage[0]);
		UIHelper.ResetPlaneAssetTextureAsync(this, TowerBackground, assetTexture);
	}

	private void ClearItemComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _itemComponent);
	}

	public override void Reset()
	{
		ClearItemComponent();
	}

	public void OnClickReset()
	{
		if (_listener != null)
		{
			_listener.OnResetTower(this);
		}
	}

	public void OnClickEnter()
	{
		if (_listener != null)
		{
			_listener.OnEnterTower();
		}
	}

	public void OnClickReceiveReward()
	{
		if (_listener != null)
		{
			_listener.OnReceiveReward();
		}
	}

	private void SetupItemIcon()
	{
		TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(base.TowerIndex, base.TowerData.FloorCountPerPage);
		if (data != null)
		{
			ClearItemComponent();
			_itemComponent = TowerFloorComponent.CreateRewardComponent<ItemIconComponent.Listener>(data, isRepeating: false, ItemRoot, null);
		}
	}

	private void SetupButton()
	{
		UIHelper.EnableGameObject(ButtonReward, enable: false);
		UIHelper.EnableGameObject(ButtonEnter, enable: false);
		UIHelper.EnableGameObject(ButtonReset, enable: false);
		bool flag = base.CompletedFloor >= base.MaxFloor;
		if (flag && base.TowerInfo != null && String_Extension_Method.IsNullOrEmpty(base.TowerInfo.ReceiveRewardTime))
		{
			UIHelper.EnableGameObject(ButtonReward, enable: true);
		}
		else if (flag)
		{
			UIHelper.EnableGameObject(ButtonReset, enable: true);
		}
		else
		{
			UIHelper.EnableGameObject(ButtonEnter, enable: true);
		}
	}

	public override bool CanReset()
	{
		if (base.TowerInfo == null || string.IsNullOrEmpty(base.TowerInfo.CompletedTime))
		{
			return false;
		}
		int maxFloor = BaseTower.GetMaxFloor(base.TowerData.TowerIndex);
		if (base.TowerInfo.CompletedFloor >= maxFloor && string.IsNullOrEmpty(base.TowerInfo.ReceiveRewardTime))
		{
			return false;
		}
		return true;
	}

	public override void OnResetTower(TowerInfo towerInfo)
	{
		base.OnResetTower(towerInfo);
		InitTowerInfo(towerInfo);
		SetupButton();
	}
}
