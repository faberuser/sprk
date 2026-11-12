using NGame2.NBattleField;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle.NTactic;

public class TimerSkillTacticComponent : TacticComponent, InitOnCreate
{
	public Transform Root;

	public UILabel RemainTimeLabel;

	public Transform[] StepRoot;

	private float _lastUpdatedTime;

	private int _maxStep;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UIHelper.SetLabelColor(RemainTimeLabel, Color.black);
	}

	public override void Open(CampaignTacticData tacticData)
	{
		_lastUpdatedTime = 0f;
		_maxStep = __Extension_Method_ICollection__.COUNT(tacticData.TacticValue) / 2;
		Init();
	}

	private void Init()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StepRoot); i++)
		{
			UIHelper.EnableGameObject(StepRoot[i], enable: false);
		}
	}

	private void Update()
	{
		if (!(Time.time < _lastUpdatedTime + 0.3f))
		{
			_lastUpdatedTime = Time.time;
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(UpdateThreaded);
		}
	}

	private void UpdateThreaded(BattleInstance battleInstance)
	{
		if (battleInstance.Snapshot.TacticManager.Tactic is TimerSkill timerSkill)
		{
			int step = timerSkill.Step;
			int remainTimeMs = timerSkill.RemainNextStepTimeMs;
			SingletonUpdater<Executer>.instance.NextRun(delegate
			{
				SetStepStatus(step, remainTimeMs);
			});
		}
	}

	private void SetStepStatus(int step, int remainTimeMs)
	{
		if (step >= _maxStep && remainTimeMs < 0)
		{
			UIHelper.SetLabelColor(RemainTimeLabel, Color.red);
			SetTime(0);
			SetStep(step);
		}
		else
		{
			SetTime((remainTimeMs + 999) / 1000);
			SetStep(step);
		}
	}

	private void SetTime(int time)
	{
		int num = time / 60;
		int num2 = time % 60;
		UIHelper.SetLabelText(RemainTimeLabel, $"{num}:{num2:D2}");
	}

	private void SetStep(int step)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StepRoot); i++)
		{
			UIHelper.EnableGameObject(StepRoot[i], i < step);
		}
	}
}
