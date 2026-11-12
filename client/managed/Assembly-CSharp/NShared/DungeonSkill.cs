namespace NShared;

public class DungeonSkill : SnapshotObject
{
	private readonly SnapshotObjectMember<int> _skillSlotIndexHolder;

	private readonly SnapshotObjectMember<int> _baseSkillIndexHolder;

	private readonly SnapshotObjectMember<int> _enhancedSkillIndexHolder;

	private readonly SnapshotObjectMember<int> _maxBaseStateCountHolder;

	private readonly SnapshotObjectMember<int> _maxEnhancedStateCountHolder;

	private readonly SnapshotObjectMember<int> _baseStateCountHolder;

	private readonly SnapshotObjectMember<int> _enhancedStateCountHolder;

	private readonly SnapshotObjectMember<int> _useSkillCountHolder;

	private readonly SnapshotObjectMember<SkillUseResult> _skillUseResult;

	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private string _baseCheckState;

	private string _enhancedCheckState;

	private int _maxUseSkillCount;

	public BattleCreature Owner => _ownerHolder.Get();

	public int SkillSlotIndex => _skillSlotIndexHolder.Get();

	public int BaseSkillIndex => _baseSkillIndexHolder.Get();

	public int EnhancedSkillIndex => _enhancedSkillIndexHolder.Get();

	public int MaxBaseStateCount => _maxBaseStateCountHolder.Get();

	public int MaxEnhancedStateCount => _maxEnhancedStateCountHolder.Get();

	public int BaseStateCount
	{
		get
		{
			if (_skillUseResult.Get() == SkillUseResult.UseSkillMaxCount || _skillUseResult.Get() == SkillUseResult.Cooltime)
			{
				return 0;
			}
			return _baseStateCountHolder.Get();
		}
	}

	public int EnhancedStateCount
	{
		get
		{
			if (_skillUseResult.Get() == SkillUseResult.UseSkillMaxCount || _skillUseResult.Get() == SkillUseResult.Cooltime || _skillUseResult.Get() == SkillUseResult.NotEnoughStateCount)
			{
				return 0;
			}
			return _enhancedStateCountHolder.Get();
		}
	}

	public int UseSkillCount => _useSkillCountHolder.Get();

	public SkillUseResult SkillUseResult => _skillUseResult.Get();

	public int UseableSkillCount
	{
		get
		{
			if (!IsValidUseCount())
			{
				return 0;
			}
			return _maxUseSkillCount - _useSkillCountHolder.Get();
		}
	}

	public DungeonSkill()
	{
		_skillSlotIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_baseSkillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_enhancedSkillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_maxBaseStateCountHolder = new SnapshotObjectMember<int>(this, 0);
		_maxEnhancedStateCountHolder = new SnapshotObjectMember<int>(this, 0);
		_baseStateCountHolder = new SnapshotObjectMember<int>(this, 0);
		_enhancedStateCountHolder = new SnapshotObjectMember<int>(this, 0);
		_useSkillCountHolder = new SnapshotObjectMember<int>(this, 0);
		_skillUseResult = new SnapshotObjectMember<SkillUseResult>(this, SkillUseResult.NotEnoughStateCount);
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
	}

	public bool Init(BattleCreature owner, int baseSkillIndex, int slotIndex)
	{
		if (owner == null)
		{
			return false;
		}
		CampaignDungeonSkillData data = GameRoot.Instance.CampaignDungeonSkillDataContainer.GetData(baseSkillIndex);
		if (data == null)
		{
			return false;
		}
		_baseCheckState = data.BaseCheckState;
		_enhancedCheckState = data.EnhancedCheckState;
		StateData data2 = GameRoot.Instance.StateDataContainer.GetData(_baseCheckState);
		if (data2 == null)
		{
			return false;
		}
		StateData data3 = GameRoot.Instance.StateDataContainer.GetData(_enhancedCheckState);
		if (data3 == null)
		{
			return false;
		}
		_skillSlotIndexHolder.Set(slotIndex);
		_baseSkillIndexHolder.Set(data.BaseSkillIndex);
		_enhancedSkillIndexHolder.Set(data.EnhancedSkillIndex);
		_maxBaseStateCountHolder.Set(data2.MaxStack);
		_maxEnhancedStateCountHolder.Set(data3.MaxStack);
		_baseStateCountHolder.Set(0);
		_enhancedStateCountHolder.Set(0);
		_useSkillCountHolder.Set(0);
		_ownerHolder.Set(owner);
		_maxUseSkillCount = data.MaxUseCount;
		return true;
	}

	public void Update()
	{
		UpdateState();
	}

	private void UpdateState()
	{
		if (CheckUseCount() && CheckCoolTime())
		{
			CheckStateCount();
		}
	}

	private bool CheckStateCount()
	{
		int stateStackCount = Owner.StateController.GetStateStackCount(_baseCheckState);
		_baseStateCountHolder.Set(stateStackCount);
		if (stateStackCount < _maxBaseStateCountHolder.Get())
		{
			_skillUseResult.Set(SkillUseResult.NotEnoughStateCount);
			_enhancedStateCountHolder.Set(0);
			return false;
		}
		int stateStackCount2 = Owner.StateController.GetStateStackCount(_enhancedCheckState);
		_enhancedStateCountHolder.Set(stateStackCount2);
		if (stateStackCount2 < _maxEnhancedStateCountHolder.Get())
		{
			_skillUseResult.Set(SkillUseResult.Success);
			return false;
		}
		_skillUseResult.Set(SkillUseResult.EnhancedSuccess);
		return true;
	}

	private bool CheckCoolTime()
	{
		if (Owner.SkillController.CooltimeManager.IsInCooltime(_baseSkillIndexHolder.Get()))
		{
			_skillUseResult.Set(SkillUseResult.Cooltime);
			if (_baseCheckState != null && _baseCheckState.Length > 0)
			{
				Owner.StateController.ClearState(_baseCheckState);
			}
			if (_enhancedCheckState != null && _enhancedCheckState.Length > 0)
			{
				Owner.StateController.ClearState(_enhancedCheckState);
			}
			return false;
		}
		return true;
	}

	private bool CheckUseCount()
	{
		if (!IsValidUseCount())
		{
			_skillUseResult.Set(SkillUseResult.UseSkillMaxCount);
			return false;
		}
		return true;
	}

	public bool IsValidUseCount()
	{
		if (_maxUseSkillCount != 0 && _maxUseSkillCount <= _useSkillCountHolder.Get())
		{
			return false;
		}
		return true;
	}

	public void IncreaseUseSkillCount()
	{
		int num = _useSkillCountHolder.Get();
		num++;
		_useSkillCountHolder.Set(num);
	}

	public int GetBaseStateRatioM()
	{
		if (_maxBaseStateCountHolder.Get() == 0)
		{
			return 1000;
		}
		return ((BaseStateCount < _maxBaseStateCountHolder.Get()) ? BaseStateCount : _maxBaseStateCountHolder.Get()) * 1000 / _maxBaseStateCountHolder.Get();
	}

	public int GetEnhancedStateRatioM()
	{
		if (_maxEnhancedStateCountHolder.Get() == 0)
		{
			return 1000;
		}
		return ((EnhancedStateCount < _maxEnhancedStateCountHolder.Get()) ? EnhancedStateCount : _maxEnhancedStateCountHolder.Get()) * 1000 / _maxEnhancedStateCountHolder.Get();
	}
}
