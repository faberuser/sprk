using System;

namespace NShared;

public class SkillModifier : SnapshotObject
{
	[Flags]
	public enum ModifyType
	{
		None = 0,
		Set = 1,
		Add = 2,
		Sub = 4,
		All = 7
	}

	private readonly SnapshotObjectMember<ModifyType> _modifyTypeHolder;

	private readonly SnapshotObjectMember<int> _skillIndexHolder;

	private readonly SnapshotObjectMember<int> _operationNoHolder;

	private readonly SnapshotObjectMember<OperationFieldType> _fieldTypeHolder;

	private readonly SnapshotObjectMember<int> _fieldIndexHolder;

	private readonly SnapshotObjectMember<string> _valueHolder;

	private readonly SnapshotObjectMember<long> _numberValueHolder;

	public ModifyType Type
	{
		get
		{
			return _modifyTypeHolder.Get();
		}
		set
		{
			_modifyTypeHolder.Set(value);
		}
	}

	public int SkillIndex
	{
		get
		{
			return _skillIndexHolder.Get();
		}
		set
		{
			_skillIndexHolder.Set(value);
		}
	}

	public int OperationNo
	{
		get
		{
			return _operationNoHolder.Get();
		}
		set
		{
			_operationNoHolder.Set(value);
		}
	}

	public OperationFieldType FieldType
	{
		get
		{
			return _fieldTypeHolder.Get();
		}
		set
		{
			_fieldTypeHolder.Set(value);
		}
	}

	public int FieldIndex
	{
		get
		{
			return _fieldIndexHolder.Get();
		}
		set
		{
			_fieldIndexHolder.Set(value);
		}
	}

	public string Value
	{
		get
		{
			return _valueHolder.Get();
		}
		set
		{
			_valueHolder.Set(value);
		}
	}

	public long NumberValue
	{
		get
		{
			return _numberValueHolder.Get();
		}
		set
		{
			_numberValueHolder.Set(value);
		}
	}

	public SkillModifier()
	{
		_modifyTypeHolder = new SnapshotObjectMember<ModifyType>(this, ModifyType.None);
		_skillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_operationNoHolder = new SnapshotObjectMember<int>(this, 0);
		_fieldTypeHolder = new SnapshotObjectMember<OperationFieldType>(this, OperationFieldType.None);
		_fieldIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_valueHolder = new SnapshotObjectMember<string>(this);
		_numberValueHolder = new SnapshotObjectMember<long>(this, 0L);
	}

	public void Init()
	{
		InitModifyType();
		InitNumberValue();
	}

	private void InitModifyType()
	{
		if (_modifyTypeHolder.Get() == ModifyType.None)
		{
			_modifyTypeHolder.Set(ModifyType.Set);
		}
	}

	private void InitNumberValue()
	{
		if (string.IsNullOrEmpty(_valueHolder.Get()))
		{
			_numberValueHolder.Set(0L);
			return;
		}
		long result = 0L;
		if (long.TryParse(_valueHolder.Get(), out result))
		{
			_numberValueHolder.Set(result);
		}
		else
		{
			_numberValueHolder.Set(0L);
		}
	}
}
