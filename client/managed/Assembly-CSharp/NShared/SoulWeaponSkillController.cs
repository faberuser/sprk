using NVespa;

namespace NShared;

public class SoulWeaponSkillController : SnapshotObject
{
	private bool _equippedSoulWeapon;

	private int _maxUseSkillCount;

	private string _state;

	private SkillData _basicSkillData;

	private readonly SnapshotObjectMember<int> _maxStackCountHolder;

	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<bool> _useableHdoler;

	private readonly SnapshotObjectMember<SkillUseResult> _skillUseResult;

	private readonly SnapshotObjectMember<int> _stackCountHolder;

	private readonly SnapshotObjectMember<int> _useSkillCountHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	public int StackCount
	{
		get
		{
			if (!_equippedSoulWeapon)
			{
				return 0;
			}
			if (!IsValidUseCount())
			{
				return 0;
			}
			if (Owner.SkillController.CooltimeManager.IsInCooltime(_basicSkillData.Index))
			{
				return 0;
			}
			return _stackCountHolder.Get();
		}
	}

	public int MaxStackCount => _maxStackCountHolder.Get();

	public int StackRatioM => GetStackRatioM();

	public SkillData BasicSkillData => _basicSkillData;

	public bool EquippedSoulWeapon => _equippedSoulWeapon;

	public bool IsUseableSkill => _useableHdoler.Get();

	public SkillUseResult SkillUseResult => _skillUseResult.Get();

	public int UseableSkillCount
	{
		get
		{
			if (!IsValidUseCount())
			{
				return 0;
			}
			if (_maxUseSkillCount == 0)
			{
				return 1000;
			}
			return _maxUseSkillCount - _useSkillCountHolder.Get();
		}
	}

	public SoulWeaponSkillController()
	{
		_maxStackCountHolder = new SnapshotObjectMember<int>(this, 0);
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_useableHdoler = new SnapshotObjectMember<bool>(this, value: false);
		_stackCountHolder = new SnapshotObjectMember<int>(this, 0);
		_skillUseResult = new SnapshotObjectMember<SkillUseResult>(this, SkillUseResult.NotEnoughStack);
		_useSkillCountHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public void Init(BattleCreature owner)
	{
		if (owner.CreateInfo.HeroInfo == null)
		{
			return;
		}
		SoulWeaponInfo soulWeaponInfo = owner.CreateInfo.HeroInfo.GetSoulWeaponInfo();
		_equippedSoulWeapon = soulWeaponInfo != null;
		if (_equippedSoulWeapon)
		{
			SoulWeaponSkillData soulWeaponSkillData = soulWeaponInfo.GetSoulWeaponSkillData();
			if (soulWeaponSkillData == null)
			{
				InitError("SoulWeapon Skill Data Is Null");
				return;
			}
			_ownerHolder.Set(owner);
			int soulWeaponAddedMaxUseCount = owner.StatController.GetSoulWeaponAddedMaxUseCount();
			_maxUseSkillCount = soulWeaponSkillData.MaxUseCount + soulWeaponAddedMaxUseCount;
			InitSoulWepaonBasicSkill(soulWeaponSkillData.BasicSkillIndex);
			InitSoulWeaponState(soulWeaponSkillData.CheckState);
		}
	}

	private void InitError(string message)
	{
		VespaDebug.LogError($"[SoulWeaponController Init] {message}");
		_equippedSoulWeapon = false;
	}

	private void InitSoulWepaonBasicSkill(int skillIndex)
	{
		if (skillIndex == 0)
		{
			InitError("Basic Skill Index Is 0");
			return;
		}
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (data == null)
		{
			InitError("Basic Skill Data Is Null :Index ::" + skillIndex);
		}
		else
		{
			_basicSkillData = data;
		}
	}

	private void InitSoulWeaponState(string state)
	{
		if (string.IsNullOrEmpty(state))
		{
			InitError("State Is NullOrEmpty");
			return;
		}
		StateData data = GameRoot.Instance.StateDataContainer.GetData(state);
		if (data == null)
		{
			InitError("State Data Is Null : State :: " + state);
			return;
		}
		_state = state;
		_maxStackCountHolder.Set(data.MaxStack);
	}

	public void Update()
	{
		if (_equippedSoulWeapon)
		{
			UpdateState();
			bool data = CanUseSoulWeaponSkill();
			_useableHdoler.Set(data);
		}
	}

	public void UpdateState()
	{
		if (_equippedSoulWeapon)
		{
			if (Owner.StateController.IsInState(_state))
			{
				int stateMaxStackCount = Owner.StateController.GetStateMaxStackCount(_state, Owner);
				_maxStackCountHolder.Set(stateMaxStackCount);
			}
			if (Owner.SkillController.CooltimeManager.IsInCooltime(_basicSkillData.Index))
			{
				Owner.StateController.ClearState(_state);
			}
		}
	}

	public bool IsValidUseCount()
	{
		if (_maxUseSkillCount != 0 && _maxUseSkillCount <= _useSkillCountHolder.Get())
		{
			return false;
		}
		return true;
	}

	private bool CheckUseCount()
	{
		if (!IsValidUseCount())
		{
			return false;
		}
		return true;
	}

	private bool CheckStackCount()
	{
		int stateStackCount = Owner.StateController.GetStateStackCount(_state);
		_stackCountHolder.Set(stateStackCount);
		if (stateStackCount < _maxStackCountHolder.Get())
		{
			return false;
		}
		return true;
	}

	public bool CanUseSoulWeaponSkill()
	{
		if (!_equippedSoulWeapon)
		{
			return false;
		}
		SkillUseResult skillUseResult = Owner.SkillController.CanUseSkill(_basicSkillData, SkillUseOption.GetSoulWeaponSkillUseOption());
		_skillUseResult.Set(skillUseResult);
		if (skillUseResult == SkillUseResult.Died || skillUseResult == SkillUseResult.DisableSkillSlot)
		{
			return false;
		}
		if (!CheckUseCount())
		{
			_skillUseResult.Set(SkillUseResult.UseSkillMaxCount);
			return false;
		}
		if (!CheckStackCount())
		{
			_skillUseResult.Set(SkillUseResult.NotEnoughStack);
			return false;
		}
		if (skillUseResult != SkillUseResult.Success)
		{
			return false;
		}
		return true;
	}

	public int GetStackRatioM()
	{
		if (!_equippedSoulWeapon)
		{
			return 0;
		}
		if (_maxStackCountHolder.Get() == 0)
		{
			return 1000;
		}
		int num = StackCount;
		if (num >= _maxStackCountHolder.Get())
		{
			num = _maxStackCountHolder.Get();
		}
		return num * 1000 / _maxStackCountHolder.Get();
	}

	public void IncreaseUseSkillCount()
	{
		int num = _useSkillCountHolder.Get();
		num++;
		_useSkillCountHolder.Set(num);
	}

	public void ForcedAddStackCount(int count)
	{
		if (_equippedSoulWeapon)
		{
			StateData data = GameRoot.Instance.StateDataContainer.GetData(_state);
			DelegateStateModifier stateModifier = new DelegateStateModifier
			{
				StateData = data,
				FiredCreature = Owner
			};
			Owner.StateController.AddModifier(stateModifier, count);
		}
	}

	private void ResetUseableCount()
	{
		_useSkillCountHolder.Set(0);
	}
}
