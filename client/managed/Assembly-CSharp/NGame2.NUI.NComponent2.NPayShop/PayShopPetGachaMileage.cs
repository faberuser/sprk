using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopPetGachaMileage : MonoBehaviour
{
	public interface IListener
	{
	}

	public GameObject Root;

	public UISprite SpriteProgressBar;

	public UILabel LabelProgress;

	public Transform RewardItemIconRoot;

	public UIWidget WidgetTooltip;

	private IListener _listener;

	private int _rewardIndex;

	private int _mileageAmountForReward = 100;

	private TooltipTextInfo _toolTip;

	private ItemIconComponent _rewardItemIconComponent;

	public void Setup(NewEquipGachaData data, int currentMileage, IListener listener)
	{
		_listener = listener;
		if (!IsShow(data))
		{
			Close();
			return;
		}
		_toolTip = new TooltipTextInfo
		{
			Desc = new StringProxy
			{
				Value = LocalizationManager.GetStringDirect("PET_MILEAGE_REWARD_DESC")
			}
		};
		_rewardIndex = data.RewardIndices[0];
		_mileageAmountForReward = data.MileageAmountForReward;
		SetRewardItemIconComponent(_rewardIndex);
		RefreshMileageProgress(currentMileage, data.MileageAmountForReward);
	}

	public void RefreshMileageProgress(int current, int max)
	{
		SetMileageProgressLabel(current, max);
		SetMileageProgressSprite(current, max);
	}

	public void Close()
	{
		if (_rewardItemIconComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _rewardItemIconComponent);
		}
	}

	private void SetRewardItemIconComponent(int rewardDataIndex)
	{
		if (RewardItemIconRoot == null)
		{
			return;
		}
		_rewardItemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(RewardItemIconRoot);
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardDataIndex);
		if (data != null)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.Item1Code);
			if (dataByCode != null)
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_rewardItemIconComponent.transform);
				_rewardItemIconComponent.Open(dataByCode);
			}
		}
	}

	private void SetMileageProgressLabel(int currentMileage, float maxMileage)
	{
		UIHelper.SetLabelText(LabelProgress, currentMileage + " / " + maxMileage);
	}

	private void SetMileageProgressSprite(int currentMileage, float maxMileage)
	{
		if (!(SpriteProgressBar == null))
		{
			float fillAmount = Mathf.Min(currentMileage, maxMileage) / maxMileage;
			UIHelper.SetSpriteFillAmount(SpriteProgressBar, fillAmount);
		}
	}

	public void OnShowTooltip()
	{
		DescTooltip.Open(WidgetTooltip, _toolTip);
	}

	public void OnHideTooltip()
	{
		DescTooltip.Hide(WidgetTooltip);
	}

	private bool IsShow(NewEquipGachaData data)
	{
		if (data != null)
		{
			return data.Type == EquipGachaType.Pet;
		}
		return false;
	}

	public void ShowUi(bool show)
	{
		UIHelper.EnableGameObject(Root, show);
	}
}
