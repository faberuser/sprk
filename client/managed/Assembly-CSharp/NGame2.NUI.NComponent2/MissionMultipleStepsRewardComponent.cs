using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MissionMultipleStepsRewardComponent : LinkFactoryComponent, InitOnCreate, ItemIconComponent.Listener, CurrencyIconComponent.Listener
{
	public interface IListener
	{
		void OnClickRewardItem();
	}

	public const string PrefabPath = "Prefab2/Component/QuestIntegration/MissionMultipleStepsRewardComponent";

	public static PrefabAuto<MissionMultipleStepsRewardComponent> PrefabAuto = new PrefabAuto<MissionMultipleStepsRewardComponent>("Prefab2/Component/QuestIntegration/MissionMultipleStepsRewardComponent");

	public UIWidget IconRootWidget;

	public UISimpleGrid RewardListGrid;

	public UILabel LabelReqProgress;

	public Transform GetRewardNoticeRoot;

	public Transform CompleteRewardRoot;

	private IListener _iListener;

	private bool _canGetReward;

	private int _questIndex;

	private int _step;

	private int _reqProgress;

	private ItemIconComponent _itemIconComponent;

	private CurrencyIconComponent _currencyIconComponent;

	public int ReqProgress => _reqProgress;

	public int Step => _step;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseComponents();
	}

	public void Open(IListener listener, RewardElementData rewardData, int questIndex, int step, int reqProgress)
	{
		ReleaseComponents();
		_iListener = listener;
		_questIndex = questIndex;
		_step = step;
		_reqProgress = reqProgress;
		_canGetReward = false;
		UIHelper.SetLabelText(LabelReqProgress, reqProgress);
		CurrencyType currencyType = rewardData.GetCurrencyType();
		if (currencyType != CurrencyType.None)
		{
			_currencyIconComponent = UIHelper.AddGridItem(RewardListGrid, CurrencyIconComponent.PrefabAuto);
			if (!(_currencyIconComponent == null))
			{
				CurrencyIconComponent currencyIconComponent = _currencyIconComponent;
				int itemMin = rewardData.ItemMin;
				currencyIconComponent.Open(currencyType, itemMin, this);
			}
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardData.ItemCode);
		if (dataByCode != null)
		{
			_itemIconComponent = UIHelper.AddGridItem(RewardListGrid, ItemIconComponent.PrefabAuto);
			if (!(_itemIconComponent == null))
			{
				_itemIconComponent.Open(dataByCode, this);
				ItemIconComponent itemIconComponent = _itemIconComponent;
				int itemMax = rewardData.ItemMax;
				Color? color = null;
				int num = itemMax;
				itemIconComponent.SetItemCount(num, color);
			}
		}
	}

	public void Refresh(bool cleared, bool complete)
	{
		_canGetReward = cleared && !complete;
		bool flag = complete;
		UIHelper.EnableGameObject(GetRewardNoticeRoot, _canGetReward);
		UIHelper.EnableGameObject(CompleteRewardRoot, flag);
		SetGrayScale(flag);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (_iListener != null && _canGetReward)
		{
			_iListener.OnClickRewardItem();
		}
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent itemIconComponent)
	{
		if (_iListener != null && _canGetReward)
		{
			_iListener.OnClickRewardItem();
		}
	}

	private void SetGrayScale(bool grayScale)
	{
		UIHelper.SetWidgetGrayScale(IconRootWidget, grayScale);
	}

	private void ReleaseComponents()
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		FactoryBase.SafeReleaseComponent(ref _currencyIconComponent);
	}
}
