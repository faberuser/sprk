using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PhaseStepRewardInfoPopup : BaseWindow
{
	public interface Listener
	{
		void OnClickGetReward(int chapterIndex, int dungeonIndex, string Type);
	}

	public class PhaseRewardStepInfo
	{
		public int DailyAchievementStep = -1;

		public int GeneralAchievementStep = -1;

		public int TotalCount = -1;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PhaseStepReward/PhaseStepRewardInfoPopup";

	[Header("SettingInEditor")]
	public UIGrid Grid_Count;

	public UIGrid Grid_Step;

	public UIProgressBar ProgressBar_Gauge;

	public UIGrid Grid_Gauge;

	private List<PhaseStepRewardStepComponent> _stepCompList = new List<PhaseStepRewardStepComponent>();

	private List<PhaseStepRewardComponent> _compList = new List<PhaseStepRewardComponent>();

	private KoCountComponent _koComp;

	private Dictionary<PhaseStepRewardType, List<PhaseStepRewardData>> RewardDataDic;

	private Listener _listener;

	private PhaseRewardStepInfo _info;

	private int _chapterIndex;

	private int _dungeonIndex;

	public void Open(Listener listener, PhaseRewardStepInfo info, int chapterIndex, int dungeonIndex)
	{
		_listener = listener;
		_info = info;
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
		base.Open(delegate
		{
			Init();
		});
	}

	private void Init()
	{
		InitRewardData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(RewardDataDic))
		{
			VespaDebug.LogError("RewardDataDic is null.");
			return;
		}
		InitScoreInfo();
		InitGridReward();
		InitGaugeReward();
	}

	private void InitRewardData()
	{
		List<PhaseStepRewardData> phaseStepDataList = GameRoot.Instance.PhaseStepRewardDataContainer.GetPhaseStepDataList(_chapterIndex, _dungeonIndex);
		if (phaseStepDataList == null)
		{
			VespaDebug.LogError($"RewardDataList is null - {_chapterIndex},{_dungeonIndex}");
			return;
		}
		RewardDataDic = new Dictionary<PhaseStepRewardType, List<PhaseStepRewardData>>();
		foreach (PhaseStepRewardData item in phaseStepDataList)
		{
			if (item != null && item.Type != PhaseStepRewardType.None)
			{
				List<PhaseStepRewardData> value = null;
				if (RewardDataDic.TryGetValue(item.Type, out value))
				{
					value.Add(item);
					continue;
				}
				value = new List<PhaseStepRewardData> { item };
				RewardDataDic.Add(item.Type, value);
			}
		}
	}

	private void InitScoreInfo()
	{
		_koComp = KoCountComponent.PrefabAuto.Factory.Create(Grid_Count.transform);
		Grid_Count.AddChild(_koComp.transform);
		_koComp.Open(_info.TotalCount);
	}

	private void InitGridReward()
	{
		List<PhaseStepRewardData> value = null;
		if (RewardDataDic.TryGetValue(PhaseStepRewardType.Daily, out value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				PhaseStepRewardStepComponent phaseStepRewardStepComponent = PhaseStepRewardStepComponent.PrefabAuto.Factory.Create(Grid_Step.transform);
				PhaseStepRewardData phaseStepRewardData = value[i];
				int maxScore = ((i == value.Count - 1) ? (-1) : (value[i + 1].ReqValue - 1));
				bool isAlreadyGetReward = _info.DailyAchievementStep >= phaseStepRewardData.Step;
				phaseStepRewardStepComponent.Open(phaseStepRewardData.Step, phaseStepRewardData.ReqValue, maxScore, phaseStepRewardData.RewardIndex, isAlreadyGetReward);
				_stepCompList.Add(phaseStepRewardStepComponent);
			}
			Grid_Step.Reposition();
		}
	}

	private void InitGaugeReward()
	{
		List<PhaseStepRewardData> value = null;
		if (!RewardDataDic.TryGetValue(PhaseStepRewardType.General, out value))
		{
			return;
		}
		int num = 1;
		for (int i = 0; i < value.Count; i++)
		{
			PhaseStepRewardData phaseStepRewardData = value[i];
			RewardData data = GameRoot.Instance.RewardDataContainer.GetData(phaseStepRewardData.RewardIndex);
			if (data == null)
			{
				continue;
			}
			List<RewardElementData> rewardItemList = data.GetRewardItemList();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(rewardItemList))
			{
				RewardElementData rewardElementData = rewardItemList[0];
				if (rewardElementData != null)
				{
					PhaseStepRewardComponent phaseStepRewardComponent = PhaseStepRewardComponent.PrefabAuto.Factory.Create(Grid_Gauge.transform);
					bool flag = _info.GeneralAchievementStep >= phaseStepRewardData.Step;
					bool flag2 = flag || _info.TotalCount >= phaseStepRewardData.ReqValue;
					num = (flag2 ? phaseStepRewardData.Step : num);
					phaseStepRewardComponent.Open(rewardElementData, phaseStepRewardData.Step, phaseStepRewardData.ReqValue, flag2, flag);
					_compList.Add(phaseStepRewardComponent);
				}
			}
		}
		float value2 = (float)(num - 1) / (float)(Vespa_Generic_IList_Extension_Method.GetLast(value).Step - 1);
		ProgressBar_Gauge.value = value2;
		Grid_Gauge.Reposition();
	}

	public void Refresh()
	{
		foreach (PhaseStepRewardStepComponent stepComp in _stepCompList)
		{
			bool getReward = _info.DailyAchievementStep >= stepComp.Step;
			stepComp.SetGetReward(getReward);
		}
		foreach (PhaseStepRewardComponent comp in _compList)
		{
			bool getReward2 = _info.GeneralAchievementStep >= comp.Step;
			comp.SetGetReward(getReward2);
		}
	}

	public override void Close()
	{
		ClearComponentList();
		base.Close();
	}

	private void ClearComponentList()
	{
		FactoryBase.ReleaseAndClearList(_stepCompList);
		FactoryBase.ReleaseAndClearList(_compList);
		FactoryBase.ReleaseComponent(_koComp);
	}

	public void OnClickGetGeneralReward()
	{
		if (_listener != null)
		{
			string type = PhaseStepRewardType.General.ToString();
			_listener.OnClickGetReward(_chapterIndex, _dungeonIndex, type);
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}
}
