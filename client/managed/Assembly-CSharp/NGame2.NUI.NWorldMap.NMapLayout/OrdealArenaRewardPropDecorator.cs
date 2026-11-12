using NGame2.NOrdeal;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class OrdealArenaRewardPropDecorator : IPropDecorator
{
	protected PropComponent _propComponent;

	protected float _extraAlpha;

	protected Mediator _mapMediator;

	private const string StampPath = "Tween_GetStamp";

	private const string ItemRootPath = "RewardItemPos";

	private const string EffectBoosterPath = "Effect_Booster";

	private const string EffectRewardPath = "ObtainRewardEff";

	private UIWidget _widgetRoot;

	private GameObject _goStamp;

	private UIWidget _itemRoot;

	private GameObject _goBoosterEffect;

	private GameObject _goRewardEffect;

	private ItemIconComponent _itemIconComponent;

	private int _floor;

	private Transform _trans;

	void IPropDecorator.ChainUpdate()
	{
	}

	void IPropDecorator.Release()
	{
		UIHelper.EnableGameObject(_goBoosterEffect, enable: false);
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
	}

	void IPropDecorator.SetExtraAlpha(float alpha)
	{
		if (_widgetRoot != null)
		{
			_widgetRoot.alpha = alpha;
		}
	}

	void IPropDecorator.SetPropComponent(PropComponent prop, string[] propActionArgs, Mediator mapMediator)
	{
		_propComponent = prop;
		_mapMediator = mapMediator;
		_floor = ArgParser.GetValueInt(propActionArgs, 1);
		int valueInt = ArgParser.GetValueInt(propActionArgs, 2);
		SetUIComponent();
		SetItemIconComponent(valueInt);
		SetCompleted();
	}

	void IPropDecorator.Refresh()
	{
		SetCompleted();
	}

	private bool IsReciveReward()
	{
		if (!GlobalAutoBehaviour<OrdealArenaManager>.isValidInstance)
		{
			return false;
		}
		if (_floor == 0)
		{
			return false;
		}
		return GlobalAutoBehaviour<OrdealArenaManager>.instance.GetCompleteFloor() >= _floor;
	}

	private void SetUIComponent()
	{
		if (_trans == null)
		{
			_trans = _propComponent.transform;
		}
		if (_trans == null)
		{
			return;
		}
		if (_goStamp == null)
		{
			Transform transform = _trans.Find("Tween_GetStamp");
			_goStamp = ((transform != null) ? transform.gameObject : null);
		}
		if (_itemRoot == null)
		{
			Transform transform2 = _trans.Find("RewardItemPos");
			if (transform2 != null)
			{
				_itemRoot = SingletonAuto<LinkedTransform<UIWidget>>.instance.Get(transform2);
			}
		}
		if (_widgetRoot == null)
		{
			_widgetRoot = SingletonAuto<LinkedTransform<UIWidget>>.instance.Get(_trans);
		}
		if (_goBoosterEffect == null)
		{
			Transform transform3 = _trans.Find("Effect_Booster");
			_goBoosterEffect = ((transform3 != null) ? transform3.gameObject : null);
		}
		if (_goRewardEffect == null)
		{
			Transform transform4 = _trans.Find("ObtainRewardEff");
			_goRewardEffect = ((transform4 != null) ? transform4.gameObject : null);
		}
	}

	private void SetItemIconComponent(int index)
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		if (_itemRoot == null || index == 0)
		{
			return;
		}
		MatchTierType selectedTier = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedTier();
		OrdealArenaRewardAreaData data = GameRoot.Instance.OrdealArenaRewardAreaDataContainer.GetData(index, selectedTier);
		if (data == null)
		{
			return;
		}
		int rewardIndex = data.RewardIndex;
		if (rewardIndex == 0)
		{
			return;
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		HashString item1Code = data2.Item1Code;
		int item1CountMin = data2.Item1CountMin;
		long count = item1CountMin;
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item1Code);
		if (dataByCode != null)
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(_itemRoot.transform);
			if (_itemIconComponent != null)
			{
				_itemIconComponent.ResetPosition();
				_itemIconComponent.Open(dataByCode);
				_itemIconComponent.SetItemCount(count);
			}
		}
	}

	private void SetCompleted()
	{
		bool flag = IsReciveReward();
		UIHelper.EnableGameObject(_goStamp, flag);
		UIHelper.EnableGameObject(_goBoosterEffect, !flag);
		UIHelper.EnableGameObject(_goRewardEffect, !flag);
		UIHelper.SetWidgetGrayScale(_itemRoot, flag);
	}
}
