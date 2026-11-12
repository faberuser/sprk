using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PhaseStepRewardComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/PhaseStepReward/PhaseStepRewardComponent";

	public static PrefabAuto<PhaseStepRewardComponent> PrefabAuto = new PrefabAuto<PhaseStepRewardComponent>("Resources/Prefab2/Component/PhaseStepReward/PhaseStepRewardComponent");

	[Header("SettingInEditor")]
	public Transform CanGetRewardRoot;

	public Transform IsGetRewardRoot;

	public Transform IconRoot;

	public UILabel Label_Count;

	public TweenAlpha TweenAlpha;

	private ItemIconComponent _comp;

	public int Step { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClearItemIcon();
	}

	public void Open(RewardElementData data, int step, int minScore, bool isCanGetReward, bool isAlreadyGetReward)
	{
		Step = step;
		InitIcon(data);
		InitLabel(minScore);
		SetCanGetReward(isCanGetReward);
		SetGetReward(isAlreadyGetReward);
	}

	private void InitIcon(RewardElementData data)
	{
		CurrencyType currencyType = data.GetCurrencyType();
		if (currencyType != CurrencyType.None)
		{
			CurrencyIconComponent currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(IconRoot);
			int itemMin = data.ItemMin;
			CurrencyIconComponent.Listener listener = null;
			int num = itemMin;
			currencyIconComponent.Open(currencyType, num, listener);
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.ItemCode);
		if (dataByCode != null)
		{
			ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
			itemIconComponent.Open(dataByCode);
			int itemMax = data.ItemMax;
			Color? color = null;
			int num2 = itemMax;
			itemIconComponent.SetItemCount(num2, color);
			itemIconComponent.SetStar(data.ItemStar);
			itemIconComponent.ApplyCustomOption(data.ItemCustomOptionIndex);
			_comp = itemIconComponent;
		}
	}

	private void InitLabel(int minScore)
	{
		UIHelper.SetLabelText(Label_Count, LocalizationManager.GetStringDirect("21THANKSGIVING_EVENT_REWARD_KO_COUNT", minScore));
	}

	public void SetCanGetReward(bool isCanGetReward)
	{
		UIHelper.EnableGameObject(CanGetRewardRoot, isCanGetReward);
		if (!(TweenAlpha == null))
		{
			TweenAlpha.enabled = true;
			TweenAlpha.ResetToBeginning();
			TweenAlpha.PlayForward();
		}
	}

	public void SetGetReward(bool isAlreadyGetReward)
	{
		UIHelper.EnableGameObject(IsGetRewardRoot, isAlreadyGetReward);
		if (isAlreadyGetReward)
		{
			TweenAlpha.enabled = false;
		}
	}

	private void ClearItemIcon()
	{
		FactoryBase.ReleaseComponent(_comp);
	}
}
