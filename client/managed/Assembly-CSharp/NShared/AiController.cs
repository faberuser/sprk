using NShared.NAi.NTactic;

namespace NShared;

public class AiController : SnapshotObject
{
	private readonly SnapshotObjectMember<AiContext> _contextHolder;

	private readonly SnapshotObjectMember<IAiTactic> _tacticHolder;

	private readonly SnapshotObjectMember<bool> _isActiveHolder;

	private readonly SnapshotObjectMember<bool> _isPersistentActiveHolder;

	private readonly SnapshotObjectMember<int> _forcedAiModeCountHolder;

	public BattleCreature Owner => _contextHolder.Get().Creature;

	public AiController()
	{
		_contextHolder = new SnapshotObjectMember<AiContext>(this);
		_tacticHolder = new SnapshotObjectMember<IAiTactic>(this);
		_isActiveHolder = new SnapshotObjectMember<bool>(this, value: false);
		_isPersistentActiveHolder = new SnapshotObjectMember<bool>(this, value: false);
		_forcedAiModeCountHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public void Init(BattleCreature owner)
	{
		_contextHolder.Alloc().Init(owner);
	}

	public bool IsActive()
	{
		if (!_isActiveHolder.Get())
		{
			return _isPersistentActiveHolder.Get();
		}
		return true;
	}

	public void Update(int deltaTimeMs)
	{
		if (Owner.BattleInstance.IsBattleEnabled() && !Owner.IsResting && Owner.SkillController.CanUpdateTimer() && IsActive() && !Owner.IsDummyCreature())
		{
			_tacticHolder.Get()?.Update(_contextHolder.Get());
		}
	}

	public void Start(bool isSmartAi = false)
	{
		_isActiveHolder.Set(data: true);
		WaitRandomSkillTactic waitRandomSkillTactic = new WaitRandomSkillTactic();
		waitRandomSkillTactic.InitSmartAi(isSmartAi);
		_tacticHolder.Set(waitRandomSkillTactic);
	}

	public void StartForcedAutoMode(bool isSmartAi = false)
	{
		int num = _forcedAiModeCountHolder.Get() + 1;
		_forcedAiModeCountHolder.Set(num);
		if (num <= 1)
		{
			Start(isSmartAi);
		}
	}

	public void StopForcedAutoMode()
	{
		int num = _forcedAiModeCountHolder.Get() - 1;
		_forcedAiModeCountHolder.Set(num);
		if (num <= 0)
		{
			_forcedAiModeCountHolder.Set(0);
			Stop();
		}
	}

	public void SetFirstSkill(int skillSlotIndex)
	{
		int skillIndexBySlot = Owner.CreateInfo.HeroInfo.CreatureData.GetSkillIndexBySlot(skillSlotIndex);
		if (skillIndexBySlot != 0)
		{
			IAiTactic aiTactic = _tacticHolder.Get();
			if (aiTactic != null && aiTactic is WaitRandomSkillTactic waitRandomSkillTactic)
			{
				waitRandomSkillTactic.SetFirstSkillIndex(skillIndexBySlot);
			}
		}
	}

	public void StartPersistentAi(bool set)
	{
		_isPersistentActiveHolder.Set(set);
		if (set && !_isActiveHolder.Get())
		{
			Start();
		}
	}

	public void Stop()
	{
		_isActiveHolder.Set(data: false);
	}

	public int GetAutoAiSkillIndex()
	{
		WaitRandomSkillTactic waitRandomSkillTactic = _tacticHolder.Get() as WaitRandomSkillTactic;
		if (_isActiveHolder.Get() && waitRandomSkillTactic != null && waitRandomSkillTactic.SkillData != null)
		{
			return waitRandomSkillTactic.SkillData.Index;
		}
		return 0;
	}
}
