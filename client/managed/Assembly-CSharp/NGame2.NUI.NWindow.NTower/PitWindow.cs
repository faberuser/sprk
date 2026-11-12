using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NTower;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class PitWindow : BaseTower, PitStepComponent.Listener, ItemIconComponent.Listener
{
	public interface Listener
	{
		void OnResetPit(PitWindow caller);

		void OnClickStep(PitWindow caller, PitStepComponent component);

		int GetBossHpPercentage(PitWindow caller, TowerFloorData floorData);
	}

	[Serializable]
	public class RewardPivot
	{
		public UIWidget IconRoot;

		public UILabel GetLabel;

		public UILabel NextLabel;

		public GameObject[] NextEffects;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tower/PitWindow";

	public UILabel TitleLabel;

	public UIScrollGrid StepGrid;

	public RewardPivot[] RewardPivots;

	public Transform CostRoot;

	public Transform CostIconRoot;

	public float ClearEffectDelay = 1f;

	public float OpenEffectDelay = 2f;

	private Listener _listener;

	private List<PitStepComponent> _stepComponentList = new List<PitStepComponent>();

	private List<Component> _rewardComponentList = new List<Component>();

	private CurrencyIconComponent _battleCostComponent;

	public ResultCreatureInfo[] HeroInfos { get; private set; }

	public void Init(TowerData towerData, TowerInfo towerInfo, ResultCreatureInfo[] heroInfos, bool showClearEffect, bool returnFromBattle, Action onOpen, Listener listener)
	{
		Init(towerData, towerInfo);
		_listener = listener;
		HeroInfos = heroInfos;
		if (towerData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(towerData.Name);
			UIHelper.SetLabelText(TitleLabel, stringDirect);
		}
	}

	public void OnOpen(bool showClearEffect)
	{
		InitStepList();
		InitRewardList();
		InitBattleCost();
		CreateEffectInStepComponent(showClearEffect);
	}

	public override bool CanReset()
	{
		if (base.TowerInfo == null || string.IsNullOrEmpty(base.TowerInfo.CompletedTime))
		{
			return false;
		}
		return true;
	}

	public override void OnResetTower(TowerInfo towerInfo)
	{
		base.OnResetTower(towerInfo);
		InitTowerInfo(towerInfo);
		InitStepList();
		InitRewardList();
		InitBattleCost();
	}

	public override void Reset()
	{
		base.Reset();
		ClearStepList();
		ClearRewardList();
		ClearBattleCost();
	}

	private void CreateEffectInStepComponent(bool showEffect)
	{
		if (!showEffect)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_stepComponentList); i++)
			{
				PitStepComponent pitStepComponent = _stepComponentList[i];
				if (pitStepComponent.FloorData.Floor <= base.CompletedFloor)
				{
					pitStepComponent.DisplayClearEffect();
				}
			}
			return;
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_stepComponentList); j++)
		{
			PitStepComponent pitStepComponent2 = _stepComponentList[j];
			if (pitStepComponent2.FloorData.Floor < base.CompletedFloor)
			{
				pitStepComponent2.DisplayClearEffect();
			}
		}
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coCreateEnterEffect());
	}

	private IEnumerator coCreateEnterEffect()
	{
		yield return new WaitForSeconds(ClearEffectDelay);
		CreateClearEffect();
		yield return new WaitForSeconds(OpenEffectDelay);
		CreateOpenEffect();
	}

	private void CreateClearEffect()
	{
		PitStepComponent pitStepComponent = _stepComponentList.Find((PitStepComponent e) => e.FloorData != null && e.FloorData.Floor == base.CompletedFloor);
		if ((bool)pitStepComponent)
		{
			pitStepComponent.PlayClearEffect();
		}
	}

	private void CreateOpenEffect()
	{
		PitStepComponent pitStepComponent = _stepComponentList.Find((PitStepComponent e) => e.FloorData != null && e.FloorData.Floor == base.CompletedFloor + 1);
		if ((bool)pitStepComponent)
		{
			pitStepComponent.PlayOpenEffect();
		}
	}

	private void InitStepList()
	{
		ClearStepList();
		int num = 1;
		while (true)
		{
			TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(base.TowerData.TowerIndex, num);
			if (data == null)
			{
				break;
			}
			int bossHpPercentage = _listener.GetBossHpPercentage(this, data);
			PitStepComponent pitStepComponent = UIHelper.AddGridItem(StepGrid, PitStepComponent.PrefabAuto);
			pitStepComponent.Open(this, base.TowerData, data, base.CompletedFloor, bossHpPercentage);
			_stepComponentList.Add(pitStepComponent);
			num++;
		}
		UIHelper.RepositionGrid(StepGrid);
	}

	private void ClearStepList()
	{
		FactoryBase.ReleaseAndClearList(_stepComponentList);
	}

	private void InitRewardList()
	{
		ClearRewardList();
		int num = 1;
		while (true)
		{
			TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(base.TowerData.TowerIndex, num);
			if (data != null)
			{
				RewardPivot rewardPivot = RewardPivots[num - 1];
				Component item = TowerFloorComponent.CreateRewardComponent(data, isRepeating: false, rewardPivot.IconRoot.transform, this);
				_rewardComponentList.Add(item);
				int num2 = ((base.TowerInfo != null) ? base.TowerInfo.CompletedFloor : 0);
				bool flag = num <= num2;
				bool enable = num == num2 + 1;
				UIHelper.EnableGameObject(rewardPivot.GetLabel, flag);
				UIHelper.EnableGameObject(rewardPivot.NextLabel, enable);
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardPivot.NextEffects); i++)
				{
					UIHelper.EnableGameObject(rewardPivot.NextEffects[i], enable);
				}
				UIHelper.SetWidgetGrayScale(rewardPivot.IconRoot, flag);
				num++;
				continue;
			}
			break;
		}
	}

	private void ClearRewardList()
	{
		FactoryBase.ReleaseAndClearList(_rewardComponentList);
	}

	private void InitBattleCost()
	{
		ClearBattleCost();
		TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(base.TowerIndex, base.CompletedFloor + 1);
		if (data == null)
		{
			UIHelper.EnableGameObject(CostRoot, enable: false);
			return;
		}
		UIHelper.EnableGameObject(CostRoot, enable: true);
		data.GetReqStamina(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), out var staminaType, out var staminaValue);
		_battleCostComponent = CurrencyIconComponent.PrefabAuto2.Factory.Create(CostIconRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_battleCostComponent.transform);
		switch (staminaType)
		{
		case StaminaType.Gold:
			_battleCostComponent.Open(CurrencyType.Gold, staminaValue, null);
			break;
		case StaminaType.Gem:
			_battleCostComponent.Open(CurrencyType.Gem, staminaValue, null);
			break;
		default:
			VespaDebug.LogError("Unknown battle cost : " + staminaType);
			break;
		}
	}

	private void ClearBattleCost()
	{
		FactoryBase.SafeReleaseComponent(ref _battleCostComponent);
	}

	public void OnClickReset()
	{
		if (_listener != null)
		{
			_listener.OnResetPit(this);
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	void PitStepComponent.Listener.OnClickStep(PitStepComponent component)
	{
		if (_listener != null)
		{
			_listener.OnClickStep(this, component);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
	}
}
