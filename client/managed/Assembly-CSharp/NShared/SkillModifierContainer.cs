using System.Collections.Generic;
using NGame2.NConfig;
using NVespa;

namespace NShared;

public class SkillModifierContainer : SnapshotObject
{
	private class PackedModifier
	{
		public bool hasSetValue;

		public long setValue;

		public string setStringValue;

		public bool isAddable;

		public long accumulatedModify;
	}

	private readonly SnapshotObjectMember<SnapshotList<SkillModifier>> _finalModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotList<SkillModifier>> _modifierListHolder;

	private readonly SnapshotObjectMember<SnapshotList<ISkillModifyListener>> _modifyListenerListHolder;

	private Dictionary<long, List<SkillModifier>> _modifiersCache = new Dictionary<long, List<SkillModifier>>();

	private Dictionary<long, PackedModifier> _packIntModifiersCache = new Dictionary<long, PackedModifier>();

	private Dictionary<long, PackedModifier> _packStringModifiersCache = new Dictionary<long, PackedModifier>();

	public static bool UseCacheOnBattleLogic { get; set; }

	public static bool CheckBattleIntegrity { get; set; }

	private long GetUniqueCode(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, SkillModifier.ModifyType modifyType)
	{
		return ((long)skillIndex << 32) | ((long)operationNo << 24) | ((long)fieldType << 12) | ((long)valueIndex << 6) | (long)modifyType;
	}

	public SkillModifierContainer()
	{
		_modifierListHolder = new SnapshotObjectMember<SnapshotList<SkillModifier>>(this);
		_finalModifierListHolder = new SnapshotObjectMember<SnapshotList<SkillModifier>>(this);
		_modifyListenerListHolder = new SnapshotObjectMember<SnapshotList<ISkillModifyListener>>(this);
	}

	public void Init(BattleCreature owner)
	{
		_modifierListHolder.Alloc();
		_finalModifierListHolder.Alloc();
		_modifyListenerListHolder.Alloc();
	}

	private void OpenErrorPopup(string error)
	{
		SingletonAuto<ClientDebugBattle>.instance.OpenErrorPopup(error);
	}

	public void AddModifier(SkillModifier modifier)
	{
		if (modifier != null)
		{
			_modifierListHolder.Get().Add(modifier);
			AnnounceModified(modifier);
		}
	}

	public void RemoveModifierList(List<SkillModifier> modifierList)
	{
		if (modifierList != null && modifierList.Count != 0)
		{
			for (int i = 0; i < modifierList.Count; i++)
			{
				SkillModifier modifier = modifierList[i];
				RemoveModifier(modifier);
			}
		}
	}

	public bool RemoveModifier(SkillModifier modifier)
	{
		if (modifier != null && _modifierListHolder.Get().Remove(modifier))
		{
			AnnounceModified(modifier);
			return true;
		}
		return false;
	}

	public void AddModifyListener(ISkillModifyListener listener)
	{
		if (listener != null)
		{
			_modifyListenerListHolder.Get().Add(listener);
		}
	}

	public bool RemoveModifyListener(ISkillModifyListener listener)
	{
		return _modifyListenerListHolder.Get().Remove(listener);
	}

	public void AnnounceModified(SkillModifier modifier)
	{
		ClearCache();
		for (int i = 0; i < _modifyListenerListHolder.Get().Count; i++)
		{
			_modifyListenerListHolder.Get()[i]?.OnModified(modifier.SkillIndex, modifier.OperationNo, modifier.FieldType, modifier.FieldIndex);
		}
	}

	public override void Deserialize(object s)
	{
		ClearCache();
		base.Deserialize(s);
	}

	private void ClearCache()
	{
		_modifiersCache.Clear();
		_packIntModifiersCache.Clear();
		_packStringModifiersCache.Clear();
	}

	public void CheckEquality(List<SkillModifier> cachedModifers, List<SkillModifier> modifiers, bool hit)
	{
		if ((cachedModifers == null && !hit) || (cachedModifers == null && modifiers == null))
		{
			return;
		}
		if (cachedModifers == null || modifiers == null)
		{
			OpenErrorPopup("Battle cache mismatch, null vs not null.");
			return;
		}
		if (cachedModifers.Count != modifiers.Count)
		{
			OpenErrorPopup("Battle cache mismatch, size wrong, " + cachedModifers.Count + " vs " + modifiers.Count);
			return;
		}
		for (int i = 0; i < modifiers.Count; i++)
		{
			if (modifiers[i] != cachedModifers[i])
			{
				VespaDebug.LogError("Battle cache mismatch, different item at " + i);
			}
		}
	}

	public bool IsReadyModifier()
	{
		if (_modifierListHolder.Get() != null)
		{
			return _modifierListHolder.Get().Count > 0;
		}
		return false;
	}

	public void FindModifier(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, out List<SkillModifier> modifiers, SkillModifier.ModifyType modifyType = SkillModifier.ModifyType.All)
	{
		if (!UseCacheOnBattleLogic)
		{
			FindModifierInternal(skillIndex, operationNo, fieldType, valueIndex, out modifiers, modifyType);
			return;
		}
		long uniqueCode = GetUniqueCode(skillIndex, operationNo, fieldType, valueIndex, modifyType);
		List<SkillModifier> value = null;
		bool flag = _modifiersCache.TryGetValue(uniqueCode, out value);
		if (flag)
		{
			if (CheckBattleIntegrity)
			{
				FindModifierInternal(skillIndex, operationNo, fieldType, valueIndex, out modifiers, modifyType);
				CheckEquality(value, modifiers, flag);
			}
			modifiers = value;
		}
		else
		{
			FindModifierInternal(skillIndex, operationNo, fieldType, valueIndex, out modifiers, modifyType);
			_modifiersCache[uniqueCode] = modifiers;
		}
	}

	public void FindModifierInternal(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, out List<SkillModifier> modifiers, SkillModifier.ModifyType modifyType = SkillModifier.ModifyType.All)
	{
		modifiers = null;
		SnapshotList<SkillModifier> snapshotList = _modifierListHolder.Get();
		for (int i = 0; i < snapshotList.Count; i++)
		{
			SkillModifier skillModifier = snapshotList[i];
			if ((skillModifier.SkillIndex == 0 || skillModifier.SkillIndex == skillIndex) && (skillModifier.OperationNo == 0 || skillModifier.OperationNo == operationNo) && skillModifier.FieldType == fieldType && skillModifier.FieldIndex == valueIndex && (skillModifier.Type & modifyType) != SkillModifier.ModifyType.None)
			{
				modifiers = modifiers ?? new List<SkillModifier>();
				modifiers.Add(skillModifier);
			}
		}
	}

	private PackedModifier PackModifiers(List<SkillModifier> modifiers)
	{
		PackedModifier packedModifier = new PackedModifier();
		packedModifier.hasSetValue = false;
		for (int num = modifiers.Count - 1; num >= 0; num--)
		{
			SkillModifier skillModifier = modifiers[num];
			if (skillModifier.Type == SkillModifier.ModifyType.Set)
			{
				packedModifier.hasSetValue = true;
				packedModifier.setValue = skillModifier.NumberValue;
				break;
			}
		}
		packedModifier.accumulatedModify = 0L;
		for (int i = 0; i < modifiers.Count; i++)
		{
			SkillModifier skillModifier2 = modifiers[i];
			if (skillModifier2.Type == SkillModifier.ModifyType.Add)
			{
				packedModifier.accumulatedModify += skillModifier2.NumberValue;
			}
			else if (skillModifier2.Type == SkillModifier.ModifyType.Sub)
			{
				packedModifier.accumulatedModify -= skillModifier2.NumberValue;
			}
		}
		return packedModifier;
	}

	private PackedModifier PackStringModifiers(List<SkillModifier> modifiers, bool checkAddable)
	{
		PackedModifier packedModifier = new PackedModifier();
		packedModifier.hasSetValue = false;
		for (int num = modifiers.Count - 1; num >= 0; num--)
		{
			SkillModifier skillModifier = modifiers[num];
			if (skillModifier.Type == SkillModifier.ModifyType.Set)
			{
				packedModifier.hasSetValue = true;
				packedModifier.setStringValue = (string.IsNullOrEmpty(skillModifier.Value) ? string.Empty : skillModifier.Value);
				break;
			}
		}
		if (checkAddable)
		{
			long num2 = 0L;
			for (int i = 0; i < modifiers.Count; i++)
			{
				SkillModifier skillModifier2 = modifiers[i];
				if (skillModifier2.Type == SkillModifier.ModifyType.Add)
				{
					num2 += skillModifier2.NumberValue;
				}
				else if (skillModifier2.Type == SkillModifier.ModifyType.Sub)
				{
					num2 -= skillModifier2.NumberValue;
				}
			}
			packedModifier.accumulatedModify = num2;
		}
		return packedModifier;
	}

	public string GetModifiedString(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, string originalValue, bool checkAddable = false)
	{
		if (!UseCacheOnBattleLogic)
		{
			return GetModifiedStringLegacy(skillIndex, operationNo, fieldType, valueIndex, originalValue, checkAddable);
		}
		PackedModifier value = null;
		SkillModifier.ModifyType modifyType = ((!checkAddable) ? SkillModifier.ModifyType.Set : SkillModifier.ModifyType.All);
		long uniqueCode = GetUniqueCode(skillIndex, operationNo, fieldType, valueIndex, modifyType);
		if (!_packStringModifiersCache.TryGetValue(uniqueCode, out value))
		{
			List<SkillModifier> modifiers = null;
			FindModifier(skillIndex, operationNo, fieldType, valueIndex, out modifiers, modifyType);
			value = new PackedModifier();
			if (modifiers != null && modifiers.Count > 0)
			{
				value = PackStringModifiers(modifiers, checkAddable);
			}
			_packStringModifiersCache[uniqueCode] = value;
		}
		string text = originalValue;
		if (value.hasSetValue)
		{
			text = value.setStringValue;
		}
		if (checkAddable && value.accumulatedModify != 0L)
		{
			long result = 0L;
			if (string.IsNullOrEmpty(text) || long.TryParse(text, out result))
			{
				text = (result + value.accumulatedModify).ToString();
			}
		}
		if (CheckBattleIntegrity)
		{
			string modifiedStringLegacy = GetModifiedStringLegacy(skillIndex, operationNo, fieldType, valueIndex, originalValue, checkAddable);
			if (modifiedStringLegacy != text)
			{
				OpenErrorPopup("Battle cache, modify string mismatch, skill =  " + skillIndex + "," + text + " vs " + modifiedStringLegacy);
			}
		}
		return text;
	}

	public string GetModifiedStringLegacy(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, string originalValue, bool checkAddable = false)
	{
		List<SkillModifier> modifiers = null;
		FindModifier(skillIndex, operationNo, fieldType, valueIndex, out modifiers, (!checkAddable) ? SkillModifier.ModifyType.Set : SkillModifier.ModifyType.All);
		if (modifiers == null || modifiers.Count == 0)
		{
			return originalValue;
		}
		string text = originalValue;
		for (int num = modifiers.Count - 1; num >= 0; num--)
		{
			SkillModifier skillModifier = modifiers[num];
			if (skillModifier.Type == SkillModifier.ModifyType.Set)
			{
				text = (string.IsNullOrEmpty(skillModifier.Value) ? string.Empty : skillModifier.Value);
				break;
			}
		}
		if (checkAddable)
		{
			long result = 0L;
			if (string.IsNullOrEmpty(text) || long.TryParse(text, out result))
			{
				bool flag = false;
				for (int i = 0; i < modifiers.Count; i++)
				{
					SkillModifier skillModifier2 = modifiers[i];
					if (skillModifier2.Type == SkillModifier.ModifyType.Add)
					{
						result += skillModifier2.NumberValue;
						flag = true;
					}
					else if (skillModifier2.Type == SkillModifier.ModifyType.Sub)
					{
						result -= skillModifier2.NumberValue;
						flag = true;
					}
				}
				if (flag)
				{
					text = result.ToString();
				}
			}
		}
		return text;
	}

	public long GetModifiedInt(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, long originalValue)
	{
		if (!UseCacheOnBattleLogic)
		{
			return GetModifiedIntLegacy(skillIndex, operationNo, fieldType, valueIndex, originalValue);
		}
		PackedModifier value = null;
		long uniqueCode = GetUniqueCode(skillIndex, operationNo, fieldType, valueIndex, SkillModifier.ModifyType.All);
		if (!_packIntModifiersCache.TryGetValue(uniqueCode, out value))
		{
			List<SkillModifier> modifiers = null;
			FindModifier(skillIndex, operationNo, fieldType, valueIndex, out modifiers);
			value = ((modifiers == null || modifiers.Count <= 0) ? new PackedModifier() : PackModifiers(modifiers));
			_packIntModifiersCache[uniqueCode] = value;
		}
		long num = originalValue;
		if (value.hasSetValue)
		{
			num = value.setValue;
		}
		num += value.accumulatedModify;
		if (CheckBattleIntegrity)
		{
			long modifiedIntLegacy = GetModifiedIntLegacy(skillIndex, operationNo, fieldType, valueIndex, originalValue);
			if (modifiedIntLegacy != num)
			{
				OpenErrorPopup("Battle cache, modify int mismatch, skill =  " + skillIndex + "," + num + " vs " + modifiedIntLegacy);
			}
		}
		return num;
	}

	public long GetModifiedIntLegacy(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, long originalValue)
	{
		List<SkillModifier> modifiers = null;
		FindModifier(skillIndex, operationNo, fieldType, valueIndex, out modifiers);
		if (modifiers == null || modifiers.Count == 0)
		{
			return originalValue;
		}
		long num = originalValue;
		for (int num2 = modifiers.Count - 1; num2 >= 0; num2--)
		{
			SkillModifier skillModifier = modifiers[num2];
			if (skillModifier.Type == SkillModifier.ModifyType.Set)
			{
				num = skillModifier.NumberValue;
				break;
			}
		}
		for (int i = 0; i < modifiers.Count; i++)
		{
			SkillModifier skillModifier2 = modifiers[i];
			if (skillModifier2.Type == SkillModifier.ModifyType.Add)
			{
				num += skillModifier2.NumberValue;
			}
			else if (skillModifier2.Type == SkillModifier.ModifyType.Sub)
			{
				num -= skillModifier2.NumberValue;
			}
		}
		return num;
	}

	public int GetModifiedInt(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, int originalValue)
	{
		return (int)GetModifiedInt(skillIndex, operationNo, fieldType, valueIndex, (long)originalValue);
	}

	public short GetModifiedInt(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, short originalValue)
	{
		return (short)GetModifiedInt(skillIndex, operationNo, fieldType, valueIndex, (long)originalValue);
	}

	public byte GetModifiedInt(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, byte originalValue)
	{
		return (byte)GetModifiedInt(skillIndex, operationNo, fieldType, valueIndex, (long)originalValue);
	}

	public sbyte GetModifiedInt(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, sbyte originalValue)
	{
		return (sbyte)GetModifiedInt(skillIndex, operationNo, fieldType, valueIndex, (long)originalValue);
	}

	public bool GetModifiedBool(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, bool originalValue = false)
	{
		return GetModifiedT(skillIndex, operationNo, fieldType, valueIndex, originalValue);
	}

	public T GetModifiedT<T>(int skillIndex, int operationNo, OperationFieldType fieldType, int valueIndex, T defaultValue) where T : struct
	{
		string modifiedString = GetModifiedString(skillIndex, operationNo, fieldType, valueIndex, null);
		if (modifiedString == null)
		{
			return defaultValue;
		}
		return BaseJsonMarshaler.To<T>(modifiedString);
	}

	private bool IsFinalModifier(OperationFieldType fieldType)
	{
		if (fieldType == OperationFieldType.FinalRadiusMm)
		{
			return true;
		}
		return false;
	}

	public void AddFinalModifier(SkillModifier finalModifier)
	{
		if (finalModifier != null)
		{
			if (!IsFinalModifier(finalModifier.FieldType))
			{
				VespaDebug.LogError("Invalid FinalSkillModifier FieldType :" + finalModifier.FieldType);
			}
			else
			{
				_finalModifierListHolder.Get().Add(finalModifier);
			}
		}
	}

	public void RemoveFinalModifer(SkillModifier finalModifier)
	{
		if (finalModifier != null)
		{
			if (!IsFinalModifier(finalModifier.FieldType))
			{
				VespaDebug.LogError("Invalid FinalSkillModifier FieldType :" + finalModifier.FieldType);
			}
			else
			{
				_finalModifierListHolder.Get().Remove(finalModifier);
			}
		}
	}

	public int GetFinalModifierInt(OperationFieldType fieldType, int originValue)
	{
		int count = _finalModifierListHolder.Get().Count;
		if (count == 0)
		{
			return originValue;
		}
		for (int num = count - 1; num >= 0; num--)
		{
			SkillModifier skillModifier = _finalModifierListHolder.Get()[num];
			if (skillModifier.FieldType == fieldType && skillModifier.NumberValue < originValue)
			{
				return (int)skillModifier.NumberValue;
			}
		}
		return originValue;
	}
}
