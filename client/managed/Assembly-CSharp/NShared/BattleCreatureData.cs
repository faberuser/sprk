using System;
using System.Collections.Generic;
using NVespa;

namespace NShared;

public class BattleCreatureData : SnapshotObject
{
	private readonly SnapshotObjectMember<SnapshotLinkedList<ICreatureDataModifier>> _setDictionary;

	private readonly SnapshotObjectMember<SnapshotLinkedList<ICreatureDataModifier>> _modDictionary;

	private readonly SnapshotObjectMember<CreatureData> _creatureDataHolder;

	public CreatureData CreatureData => _creatureDataHolder.Get();

	public BattleCreatureData()
	{
		_setDictionary = new SnapshotObjectMember<SnapshotLinkedList<ICreatureDataModifier>>(this);
		_modDictionary = new SnapshotObjectMember<SnapshotLinkedList<ICreatureDataModifier>>(this);
		_creatureDataHolder = new SnapshotObjectMember<CreatureData>(this, null, GameRoot.Instance.CreatureDataContainer);
	}

	public void Init(CreatureData creatureData)
	{
		_creatureDataHolder.Set(creatureData);
		_setDictionary.Alloc();
		_modDictionary.Alloc();
	}

	public void BeginSetCreatureDataModifier(ICreatureDataModifier modifier)
	{
		_setDictionary.Get().AddLast(modifier);
	}

	public bool EndSetCreatureDataModifier(ICreatureDataModifier modifier)
	{
		return _setDictionary.Get().Remove(modifier);
	}

	public void BeginModCreatureDataModifier(ICreatureDataModifier modifier)
	{
		_modDictionary.Get().AddLast(modifier);
	}

	public bool EndModCreatureDataModifier(ICreatureDataModifier modifier)
	{
		return _modDictionary.Get().Remove(modifier);
	}

	private bool HasArrayValue<T>(CreatureDataFieldType fieldType, T value, Func<object, T> valueConverter, Func<T, T, bool> equalityFunc)
	{
		if (value == null)
		{
			return false;
		}
		foreach (ICreatureDataModifier item in (IEnumerable<ICreatureDataModifier>)_modDictionary.Get())
		{
			if (item.GetFieldType() == fieldType)
			{
				T arg = valueConverter(item.GetValue());
				if (equalityFunc(value, arg))
				{
					return true;
				}
			}
		}
		using ArrayPool<T>.AutoArrayPoolReleaser autoArrayPoolReleaser = ArrayPool<T>.AllocAuto(20);
		int num = 0;
		for (int i = 0; i < 20; i++)
		{
			object fieldData = CreatureData.GetFieldData(fieldType, i);
			if (fieldData == null)
			{
				break;
			}
			num = i + 1;
			autoArrayPoolReleaser[i] = valueConverter(fieldData);
		}
		foreach (ICreatureDataModifier item2 in (IEnumerable<ICreatureDataModifier>)_setDictionary.Get())
		{
			if (item2.GetFieldType() == fieldType)
			{
				object value2 = item2.GetValue();
				int valueIndex = item2.GetValueIndex();
				if (valueIndex >= num)
				{
					Action<object> logError = VespaDebug.LogError;
					string[] obj = new string[10]
					{
						"valueIndex is out of range. fieldType=",
						fieldType.ToString(),
						" value=",
						null,
						null,
						null,
						null,
						null,
						null,
						null
					};
					T val = value;
					obj[3] = val?.ToString();
					obj[4] = " setValue=";
					obj[5] = value2?.ToString();
					obj[6] = " valueIndex=";
					obj[7] = valueIndex.ToString();
					obj[8] = " arrayLength=";
					obj[9] = num.ToString();
					logError(string.Concat(obj));
				}
				else
				{
					autoArrayPoolReleaser[valueIndex] = valueConverter(value2);
				}
			}
		}
		if (!autoArrayPoolReleaser.Exist(value, equalityFunc, num))
		{
			return false;
		}
		return true;
	}

	private bool HasArrayValue(CreatureDataFieldType fieldType, string value)
	{
		return HasArrayValue(fieldType, value, delegate(object v)
		{
			if (v == null)
			{
				return (string)null;
			}
			return (v.GetType() == typeof(string)) ? ((string)v) : v.ToString();
		}, (string l, string r) => string.Equals(l, r, StringComparison.OrdinalIgnoreCase));
	}

	public T GetFieldValue<T>(CreatureDataFieldType fieldType, int valueIndex, Func<object, T> valueConverter)
	{
		foreach (ICreatureDataModifier item in (IEnumerable<ICreatureDataModifier>)_setDictionary.Get())
		{
			if (item.GetFieldType() == fieldType && item.GetValueIndex() == valueIndex)
			{
				object value = item.GetValue();
				return valueConverter(value);
			}
		}
		return valueConverter(CreatureData.GetFieldData(fieldType, valueIndex));
	}

	public int GetFieldValueInt(CreatureDataFieldType fieldType, int valueIndex)
	{
		return GetFieldValue(fieldType, valueIndex, delegate(object v)
		{
			if (v == null)
			{
				return 0;
			}
			if (v.GetType() == typeof(int))
			{
				return (int)v;
			}
			int result = 0;
			int.TryParse(v.ToString(), out result);
			return result;
		});
	}

	public bool CanIgnoreState(string stateType)
	{
		return HasArrayValue(CreatureDataFieldType.IgnoreCc, stateType);
	}

	public bool IsAvoidSkillTargetType(string skillTargetType)
	{
		return HasArrayValue(CreatureDataFieldType.AvoidSkillTargetType, skillTargetType);
	}
}
