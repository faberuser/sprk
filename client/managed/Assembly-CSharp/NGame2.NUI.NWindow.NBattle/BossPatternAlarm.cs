using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BossPatternAlarm
{
	private Holder _receiverHolder = new Holder();

	private float _elapsedPatternAlarmTime;

	private BossWarningData _bossWarningData;

	private string _alarmText = string.Empty;

	private Creature _boss;

	private Transform _trsTextAlarm;

	private Transform _trsImageAlarm;

	private UILabel _label;

	private Animator _imageAnimator;

	private string _animName;

	private IBattleTactic _tactic;

	private int _phase;

	private int _lastPhase;

	private bool _isBegined;

	public void Init(Creature boss, Transform trsTextAlarm, UILabel label, Transform trsImageAlarm, Animator imageAnimator, string animName)
	{
		_trsTextAlarm = trsTextAlarm;
		_trsImageAlarm = trsImageAlarm;
		_label = label;
		_imageAnimator = imageAnimator;
		_animName = animName;
		_boss = boss;
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			_tactic = battleInstance.Snapshot.TacticManager.Tactic;
			_phase = battleInstance.Snapshot.Phase;
		});
	}

	public void Close()
	{
		_receiverHolder.Clear();
	}

	public void Update()
	{
		if (!GameOptionData.Render_EnableBossPatternAlarm || _boss == null)
		{
			return;
		}
		if (_receiverHolder.GetCount() == 0)
		{
			SetupPatternAlarm();
		}
		if (_bossWarningData == null)
		{
			return;
		}
		if (_boss.IsDead)
		{
			EndPatternAlarm();
			return;
		}
		BossCcGauge bossCcGauge = _tactic as BossCcGauge;
		if (!_bossWarningData.CantCancel && bossCcGauge != null && bossCcGauge.CurGauge == 0)
		{
			EndPatternAlarm();
			return;
		}
		if (_lastPhase != _phase)
		{
			_lastPhase = _phase;
			EndPatternAlarm();
			return;
		}
		_elapsedPatternAlarmTime += Time.deltaTime;
		float num = _bossWarningData.DelayTime * 0.001f;
		float num2 = _bossWarningData.DurationTime * 0.001f;
		if (!(_elapsedPatternAlarmTime < num))
		{
			if (!_isBegined)
			{
				BeginPatternAlarm();
			}
			if (!(_elapsedPatternAlarmTime < num + num2))
			{
				EndPatternAlarm();
				_elapsedPatternAlarmTime = 0f;
			}
		}
	}

	public void OnMessageChangeState(CallbackMessage<ChangeState> message)
	{
		BossWarningData bossWarningData = GetBossWarningData(message.Callback.SkillIndex, BossWarningData.EType.Buff);
		if (!IsPriority(bossWarningData))
		{
			return;
		}
		ResetPatternAlarm();
		Debug.LogWarning("OnMessageChangeState : " + bossWarningData.SkillIndex);
		if (message.Callback.StackCount == bossWarningData.BuffStartStackCount)
		{
			_alarmText = LocalizationManager.GetStringDirect(bossWarningData.BuffStartWarningOption);
		}
		else
		{
			if (message.Callback.StackCount != bossWarningData.BuffCrashStackCount)
			{
				_alarmText = string.Empty;
				return;
			}
			_alarmText = LocalizationManager.GetStringDirect(bossWarningData.BuffCrashWarningOption);
		}
		_bossWarningData = bossWarningData;
		_elapsedPatternAlarmTime = 0f;
	}

	private BossWarningData GetBossWarningData(int skillIndex, BossWarningData.EType targetType)
	{
		BossWarningData data = GameRoot.Instance.BossWarningDataContainer.GetData(skillIndex);
		if (data == null)
		{
			return null;
		}
		if (data.WarningTargetType != targetType)
		{
			return null;
		}
		return data;
	}

	private bool IsPriority(BossWarningData newBossWarningData)
	{
		if (newBossWarningData == null)
		{
			return false;
		}
		int priority = newBossWarningData.Priority;
		int num = ((_bossWarningData != null) ? _bossWarningData.Priority : int.MaxValue);
		if (priority > num)
		{
			return false;
		}
		return true;
	}

	public void OnMessageFireSkill(CallbackMessage<BeginFireSkill> message)
	{
		BossWarningData bossWarningData = GetBossWarningData(message.Callback.SkillId, BossWarningData.EType.Skill);
		if (IsPriority(bossWarningData))
		{
			Debug.LogWarning("OnMessageFireSkill : " + bossWarningData.SkillIndex);
			ResetPatternAlarm();
			_bossWarningData = bossWarningData;
			_alarmText = LocalizationManager.GetStringDirect(_bossWarningData.SkillWarningOption);
			_elapsedPatternAlarmTime = 0f;
		}
	}

	private void BeginPatternAlarm()
	{
		_isBegined = true;
		Debug.LogWarning("BeginPatternAlarm : " + _bossWarningData.SkillIndex);
		if (_bossWarningData.SkillWarningType == BossWarningData.EWarningType.Image)
		{
			UIHelper.EnableGameObject(_trsImageAlarm, enable: true);
			_imageAnimator.Play(_animName);
		}
		else
		{
			UIHelper.EnableGameObject(_trsTextAlarm, enable: true);
			string stringDirect = LocalizationManager.GetStringDirect(_alarmText);
			UIHelper.SetLabelText(_label, stringDirect);
		}
	}

	private void ResetPatternAlarm()
	{
		UIHelper.EnableGameObject(_trsTextAlarm, enable: false);
		UIHelper.EnableGameObject(_trsImageAlarm, enable: false);
		_bossWarningData = null;
		_isBegined = false;
		_imageAnimator.StopPlayback();
	}

	private void EndPatternAlarm()
	{
		Debug.LogWarning("EndPatternAlarm : " + _bossWarningData.SkillIndex);
		ResetPatternAlarm();
	}

	private void SetupPatternAlarm()
	{
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.CreateInfo.IsBoss)
			{
				CreatureVerifier verifier = new CreatureVerifier(creatureCacheByIndex.Uid);
				_receiverHolder.AddByCreatureUid<CallbackMessage<BeginFireSkill>>(verifier, OnMessageFireSkill, creatureCacheByIndex.Uid);
				_receiverHolder.AddByCreatureUid<CallbackMessage<ChangeState>>(verifier, OnMessageChangeState, creatureCacheByIndex.Uid);
			}
		}
	}
}
