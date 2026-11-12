using System.Collections.Generic;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopStepUpGachaRewardPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopStepUpGachaRewardPopup";

	public UIScrollGrid ScrollReward;

	public UILabel LabelTitle;

	private List<PayShopStepUpGachaRewardPopupComponent> _listComponent = new List<PayShopStepUpGachaRewardPopupComponent>();

	public static void Open(StepUpGachaRewardData data)
	{
		WindowPrefab<PayShopStepUpGachaRewardPopup> windowPrefab = new WindowPrefab<PayShopStepUpGachaRewardPopup>("Resources/Prefab2/Window/PayShop/PayShopStepUpGachaRewardPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(data);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(StepUpGachaRewardData data)
	{
		if (data != null)
		{
			Open(delegate
			{
				SetTitleLabel(data);
				SetRewardItem(data);
			}, null);
		}
	}

	public override void Close()
	{
		base.Close();
	}

	public override void Reset()
	{
		ReleaseCompoent();
	}

	private void SetTitleLabel(StepUpGachaRewardData data)
	{
		int round = data.Round;
		int stepupGachaCount = GameRoot.Instance.StepUpGachaRewardDataContainer.GetStepupGachaCount(data.Index, round);
		int step = data.Step;
		int num = ((step > stepupGachaCount) ? ((step - 1) % stepupGachaCount + 1) : step);
		string stringDirect = LocalizationManager.GetStringDirect("STEPUP_STEP_TEXT", num);
		UIHelper.SetLabelText(LabelTitle, stringDirect);
	}

	private void SetRewardItem(StepUpGachaRewardData data)
	{
		SetDisCount(data.Discount);
		SetReward(data.RewardIndex);
		SetBuff(data.Buff);
		Reposition();
	}

	private void SetDisCount(int discount)
	{
		if (discount != 0)
		{
			PayShopStepUpGachaRewardPopupComponent payShopStepUpGachaRewardPopupComponent = CreateComponent();
			if (!(payShopStepUpGachaRewardPopupComponent == null))
			{
				payShopStepUpGachaRewardPopupComponent.Open(discount);
				_listComponent.Add(payShopStepUpGachaRewardPopupComponent);
			}
		}
	}

	private void SetReward(int rewardIndex)
	{
		if (rewardIndex == 0)
		{
			return;
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemList); i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			if (rewardElementData != null)
			{
				PayShopStepUpGachaRewardPopupComponent payShopStepUpGachaRewardPopupComponent = CreateComponent();
				if (!(payShopStepUpGachaRewardPopupComponent == null))
				{
					payShopStepUpGachaRewardPopupComponent.Open(rewardElementData);
					_listComponent.Add(payShopStepUpGachaRewardPopupComponent);
				}
			}
		}
	}

	private void SetBuff(string buff)
	{
		String_Extension_Method.IsNullOrEmpty(buff);
	}

	private void ReleaseCompoent()
	{
		FactoryBase.ReleaseAndClearList(_listComponent);
	}

	private PayShopStepUpGachaRewardPopupComponent CreateComponent()
	{
		return UIHelper.AddGridItem(ScrollReward, PayShopStepUpGachaRewardPopupComponent.PrefabAuto);
	}

	private void Reposition()
	{
		if (ScrollReward != null)
		{
			UIHelper.RepositionGrid(ScrollReward);
		}
	}
}
