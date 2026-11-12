using System;
using System.Collections;
using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopEquipGachaStarterPickupProgress : MonoBehaviour
{
	public UIClipSprite Gauge_Bar;

	public UILabel Label_StarterPickup_Count;

	public UIWidget MileageRewardWidget;

	public float StaterGaugeDelay = 0.2f;

	public float StaterGaugeRuntime = 0.2f;

	private ItemIconComponent _staterRewardIcon;

	private NewEquipGachaData _gachaData;

	public void Open(NewEquipGachaData gachaData)
	{
		_gachaData = gachaData;
		Init();
	}

	private void Init()
	{
		StarterPickupEquipGachaInfo starterPickupEquipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.StarterGachaManager.Get(_gachaData.Index);
		if (starterPickupEquipGachaInfo == null)
		{
			return;
		}
		int gachaCount = starterPickupEquipGachaInfo.GachaCount;
		int starterMileage = _gachaData.StarterMileage;
		SetProgress(gachaCount, starterMileage);
		if (__Extension_Method_ICollection__.COUNT(_gachaData.RewardIndices) <= 0)
		{
			VespaDebug.LogError("RewardIndices is null");
			return;
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(_gachaData.RewardIndices[0]);
		if (data == null)
		{
			VespaDebug.LogError("rewardData is null - reward Index :" + _gachaData.RewardIndices[0]);
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.Item1Code);
		if (dataByCode == null)
		{
			VespaDebug.LogError("itemData is null - reward Item1Code :" + data.Item1Code);
			return;
		}
		_staterRewardIcon = ItemIconComponent.PrefabAuto.Factory.Create(MileageRewardWidget.transform);
		_staterRewardIcon.Open(dataByCode);
		_staterRewardIcon.ResetPosition();
		SetEnableRewardIcon(starterPickupEquipGachaInfo);
	}

	private void SetProgress(float value, float max)
	{
		string stringDirect = LocalizationManager.GetStringDirect("PAYSHOP_STARTER_PICKUP_MILEAGE_COUNT", (int)value, (int)max);
		UIHelper.SetLabelText(Label_StarterPickup_Count, stringDirect);
		UIHelper.SetSpriteFillAmount(Gauge_Bar, value / max);
	}

	public void IncreaseAnimation(Action callback)
	{
		VespaCoroutine.Start(this, coIncreaseAnimation(callback));
	}

	private IEnumerator coIncreaseAnimation(Action callback)
	{
		StarterPickupEquipGachaInfo starterGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.StarterGachaManager.Get(_gachaData.Index);
		int currVal = starterGachaInfo.GachaCount;
		int prevVal = currVal - 1;
		float elapsTime = 0f;
		while (elapsTime <= StaterGaugeRuntime)
		{
			float fillAmount = Mathf.Lerp(prevVal, currVal, elapsTime / StaterGaugeRuntime) / (float)_gachaData.StarterMileage;
			UIHelper.SetSpriteFillAmount(Gauge_Bar, fillAmount);
			elapsTime += Time.deltaTime;
			yield return null;
		}
		SetProgress(currVal, _gachaData.StarterMileage);
		SetEnableRewardIcon(starterGachaInfo);
		Vespa_System_Action_Extension_Method.fire(callback);
	}

	public void Clear()
	{
		ReleaseRewardIcon();
	}

	private void ReleaseRewardIcon()
	{
		FactoryBase.SafeReleaseComponent(ref _staterRewardIcon);
	}

	private void SetEnableRewardIcon(StarterPickupEquipGachaInfo starterGachaInfo)
	{
		int gachaCount = starterGachaInfo.GachaCount;
		int starterMileage = _gachaData.StarterMileage;
		UIHelper.SetWidgetGrayScale(MileageRewardWidget, starterGachaInfo.IsTakeLastReward || gachaCount >= starterMileage);
	}
}
