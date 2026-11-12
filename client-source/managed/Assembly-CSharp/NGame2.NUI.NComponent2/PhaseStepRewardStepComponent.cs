using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PhaseStepRewardStepComponent : LinkFactoryComponent, InitOnCreate
{
	private const string PrefabPath = "Resources/Prefab2/Component/PhaseStepReward/PhaseStepRewardStepComponent";

	public static PrefabAuto<PhaseStepRewardStepComponent> PrefabAuto = new PrefabAuto<PhaseStepRewardStepComponent>("Resources/Prefab2/Component/PhaseStepReward/PhaseStepRewardStepComponent");

	[Header("SettingInEditor")]
	public UIGrid Grid_RewardItem;

	public UILabel Label_Step;

	public UILabel Label_Count;

	public Component Sprite_Completed;

	private List<ItemIconComponent> _itemIconComponentList = new List<ItemIconComponent>();

	private List<CurrencyIconComponent> _currencyIconComponentList = new List<CurrencyIconComponent>();

	public int Step { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClearItemIconList();
	}

	public void Open(int step, int minScore, int maxScore, int rewardIndex, bool isAlreadyGetReward = false)
	{
		Step = step;
		SetLabels(step, minScore, maxScore);
		SetRewards(rewardIndex, isAlreadyGetReward);
	}

	private void SetLabels(int step, int minScore, int maxScore)
	{
		UIHelper.SetLabelText(Label_Step, LocalizationManager.GetStringDirect("21THANKSGIVING_EVENT_REWARD_STEP", step));
		string text = ((maxScore == -1) ? $"{minScore}~" : $"{minScore}~{maxScore}");
		UIHelper.SetLabelText(Label_Count, LocalizationManager.GetStringDirect("21THANKSGIVING_EVENT_REWARD_KO_COUNT", text));
	}

	private void SetRewards(int rewardIndex, bool isAlreadyGetReward)
	{
		SetGetReward(isAlreadyGetReward);
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null)
		{
			VespaDebug.LogError("RewardData is null - " + rewardIndex);
			return;
		}
		foreach (RewardElementData rewardItem in data.GetRewardItemList())
		{
			CurrencyType currencyType = rewardItem.GetCurrencyType();
			if (currencyType != CurrencyType.None)
			{
				CurrencyIconComponent currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(Grid_RewardItem.transform);
				int itemMin = rewardItem.ItemMin;
				CurrencyIconComponent.Listener listener = null;
				int num = itemMin;
				currencyIconComponent.Open(currencyType, num, listener);
				_currencyIconComponentList.Add(currencyIconComponent);
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardItem.ItemCode);
			if (dataByCode != null)
			{
				_ = rewardItem.ItemMin;
				_ = rewardItem.ItemMax;
				ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(Grid_RewardItem.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
				itemIconComponent.Open(dataByCode);
				int itemMax = rewardItem.ItemMax;
				Color? color = null;
				int num2 = itemMax;
				itemIconComponent.SetItemCount(num2, color);
				itemIconComponent.SetStar(rewardItem.ItemStar);
				itemIconComponent.ApplyCustomOption(rewardItem.ItemCustomOptionIndex);
				_itemIconComponentList.Add(itemIconComponent);
			}
		}
		Grid_RewardItem.Reposition();
	}

	public void SetGetReward(bool isAlreadyGetReward)
	{
		UIHelper.EnableGameObject(Sprite_Completed, isAlreadyGetReward);
	}

	private void ClearItemIconList()
	{
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
		FactoryBase.ReleaseAndClearList(_currencyIconComponentList);
	}
}
