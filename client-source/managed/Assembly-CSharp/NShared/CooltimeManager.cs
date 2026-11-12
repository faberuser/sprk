using System;
using System.Collections.Generic;

namespace NShared;

public class CooltimeManager : SnapshotObject
{
	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<IndexableSnapshotDictionary<int, CooltimeTimer>> _cooltimeTimerDictionaryHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, ModCooltimeDurationInfo>> _modCooltimeDurationInfoDictionaryHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, ModCooltimeRatioInfo>> _modCooltimeRatioInfoDictionaryHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	protected IndexableSnapshotDictionary<int, CooltimeTimer> CooltimeTimerDictionary => _cooltimeTimerDictionaryHolder.Get();

	protected SnapshotDictionary<int, ModCooltimeDurationInfo> ModCooltimeDurationInfoDictionary => _modCooltimeDurationInfoDictionaryHolder.Get();

	protected SnapshotDictionary<int, ModCooltimeRatioInfo> ModCooltimeRatioInfoDictionary => _modCooltimeRatioInfoDictionaryHolder.Get();

	public CooltimeManager()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_cooltimeTimerDictionaryHolder = new SnapshotObjectMember<IndexableSnapshotDictionary<int, CooltimeTimer>>(this);
		_modCooltimeDurationInfoDictionaryHolder = new SnapshotObjectMember<SnapshotDictionary<int, ModCooltimeDurationInfo>>(this);
		_modCooltimeRatioInfoDictionaryHolder = new SnapshotObjectMember<SnapshotDictionary<int, ModCooltimeRatioInfo>>(this);
	}

	public void Init(BattleCreature owner)
	{
		_ownerHolder.Set(owner);
		_cooltimeTimerDictionaryHolder.Alloc();
		_modCooltimeDurationInfoDictionaryHolder.Alloc();
		_modCooltimeRatioInfoDictionaryHolder.Alloc();
	}

	public void Clear()
	{
		CooltimeTimerDictionary.Clear();
		ModCooltimeDurationInfoDictionary.Clear();
		ModCooltimeRatioInfoDictionary.Clear();
	}

	public void Update(int deltaTimeMs)
	{
		List<int> list = null;
		for (int i = 0; i < CooltimeTimerDictionary.Count; i++)
		{
			CooltimeTimer valueByIndex = CooltimeTimerDictionary.GetValueByIndex(i);
			valueByIndex.ElapsedTimeMs += deltaTimeMs;
			if (!IsInCooltime(valueByIndex))
			{
				list = list ?? new List<int>();
				list.Add(valueByIndex.SkillIndex);
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				int skillIndex = list[j];
				RemoveCooltimeTimer(skillIndex);
				DecModCooltimeDurationInfo(skillIndex);
			}
		}
	}

	public static int[] GetCoolTimeGroupSkillList(int skillId)
	{
		using TemporaryList<int> temporaryList = default(TemporaryList<int>);
		temporaryList.Add(skillId);
		if (GameRoot.isValidInstance && GameRoot.Instance.SkillDataContainer != null)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillId);
			if (data != null && data.CoolTimeGroup != null && data.CoolTimeGroup.Length != 0)
			{
				temporaryList.AddRange(data.CoolTimeGroup);
			}
		}
		return temporaryList.ToArray();
	}

	public void BeginCooltime(int skillId, int durationTimeMs, bool applyCoolTimeModifier)
	{
		int[] coolTimeGroupSkillList = GetCoolTimeGroupSkillList(skillId);
		foreach (int skillId2 in coolTimeGroupSkillList)
		{
			BeginCooltimeInternal(skillId2, durationTimeMs, applyCoolTimeModifier);
		}
	}

	private void BeginCooltimeInternal(int skillId, int durationTimeMs, bool applyCoolTimeModifier)
	{
		if (applyCoolTimeModifier)
		{
			durationTimeMs = Owner.SkillController.ModifierContainer.GetModifiedInt(skillId, 0, OperationFieldType.SkillCoolTimeMs, 0, durationTimeMs);
		}
		if (durationTimeMs <= 0)
		{
			DecModCooltimeDurationInfo(skillId);
			return;
		}
		CooltimeTimerDictionary.Remove(skillId);
		CooltimeTimerDictionary.TryAdd(skillId, new CooltimeTimer(skillId, durationTimeMs));
		OnCoolTime onCoolTime = SnapshotObjectPool<OnCoolTime>.Alloc();
		onCoolTime.TriggerCreature = Owner;
		onCoolTime.TargetCreatures = new BattleCreature[1] { Owner };
		onCoolTime.SkillIndex = skillId;
		onCoolTime.DurationTimeMs = durationTimeMs;
		Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onCoolTime);
	}

	public int GetCooltimeCount()
	{
		return CooltimeTimerDictionary.Count;
	}

	public int[] GetCooltimeSkillIndices()
	{
		using TemporaryList<int> temporaryList = default(TemporaryList<int>);
		for (int i = 0; i < CooltimeTimerDictionary.Count; i++)
		{
			CooltimeTimer valueByIndex = CooltimeTimerDictionary.GetValueByIndex(i);
			temporaryList.Add(valueByIndex.SkillIndex);
		}
		return (temporaryList.Count > 0) ? temporaryList.ToArray() : null;
	}

	private CooltimeTimer GetCooltimeTimer(int skillId)
	{
		CooltimeTimerDictionary.TryGetValue(skillId, out var value);
		return value;
	}

	public bool RemoveCooltimeTimer(int skillIndex)
	{
		return CooltimeTimerDictionary.Remove(skillIndex);
	}

	public bool IsInCooltime(int skillId)
	{
		CooltimeTimer cooltimeTimer = GetCooltimeTimer(skillId);
		return IsInCooltime(cooltimeTimer);
	}

	public bool IsInCooltime(CooltimeTimer cooltimeTimer)
	{
		if (cooltimeTimer == null)
		{
			return false;
		}
		int finalCooltimeDurationMs = GetFinalCooltimeDurationMs(cooltimeTimer);
		return cooltimeTimer.ElapsedTimeMs < finalCooltimeDurationMs;
	}

	public float GetCooltimeRatio(int skillId)
	{
		CooltimeTimer cooltimeTimer = GetCooltimeTimer(skillId);
		return GetCooltimeRatio(cooltimeTimer);
	}

	public float GetCooltimeRatio(CooltimeTimer cooltimeTimer)
	{
		if (cooltimeTimer == null)
		{
			return 1f;
		}
		int finalCooltimeDurationMs = GetFinalCooltimeDurationMs(cooltimeTimer);
		if (finalCooltimeDurationMs <= 0)
		{
			return 0f;
		}
		return Math.Min((float)cooltimeTimer.ElapsedTimeMs / (float)finalCooltimeDurationMs, 1f);
	}

	public bool GetCooltimeInfo(int skillId, out int durationMs, out int elapsedTimeMs)
	{
		durationMs = 0;
		elapsedTimeMs = 0;
		CooltimeTimer cooltimeTimer = GetCooltimeTimer(skillId);
		if (cooltimeTimer == null)
		{
			return false;
		}
		durationMs = GetFinalCooltimeDurationMs(cooltimeTimer);
		elapsedTimeMs = cooltimeTimer.ElapsedTimeMs;
		return true;
	}

	public void AddCooltime(int skillId, int durationMs)
	{
		int[] coolTimeGroupSkillList = GetCoolTimeGroupSkillList(skillId);
		foreach (int skillId2 in coolTimeGroupSkillList)
		{
			CooltimeTimer cooltimeTimer = GetCooltimeTimer(skillId2);
			if (cooltimeTimer != null)
			{
				cooltimeTimer.AddDurationMs += durationMs;
			}
		}
	}

	public void AddCooltimeR(int skillId, int ratio)
	{
		int[] coolTimeGroupSkillList = GetCoolTimeGroupSkillList(skillId);
		foreach (int skillId2 in coolTimeGroupSkillList)
		{
			CooltimeTimer cooltimeTimer = GetCooltimeTimer(skillId2);
			if (cooltimeTimer != null)
			{
				cooltimeTimer.AddDurationRatio += ratio;
			}
		}
	}

	private int GetFinalCooltimeDurationMs(CooltimeTimer cooltimeTimer)
	{
		if (cooltimeTimer == null)
		{
			return 0;
		}
		ModCooltimeDurationInfo modCooltimeDurationInfo = GetModCooltimeDurationInfo(cooltimeTimer.SkillIndex);
		ModCooltimeRatioInfo modCooltimeRatioInfo = GetModCooltimeRatioInfo(cooltimeTimer.SkillIndex);
		int num = modCooltimeDurationInfo?.DurationMs ?? 0;
		int num2 = modCooltimeRatioInfo?.RatioM ?? 0;
		int addedStat = Owner.StatController.GetAddedStat(StatType.Cooltime);
		num += addedStat;
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(cooltimeTimer.SkillIndex);
		if (data == null)
		{
			return 0;
		}
		int num3 = (int)((long)data.GetCoolTimeMs(Owner) * (long)(num2 + cooltimeTimer.AddDurationRatio) / 1000);
		int num4 = num + cooltimeTimer.AddDurationMs;
		return cooltimeTimer.InitDurationMs + (num3 + num4);
	}

	public void ModNextCooltimeDuration(int skillIndex, int durationMs, int count, bool checkExist)
	{
		int[] coolTimeGroupSkillList = GetCoolTimeGroupSkillList(skillIndex);
		foreach (int skillIndex2 in coolTimeGroupSkillList)
		{
			ModNextCooltimeDurationInternal(skillIndex2, durationMs, count, checkExist);
		}
	}

	private void ModNextCooltimeDurationInternal(int skillIndex, int durationMs, int count, bool checkExist)
	{
		if (!checkExist || IsInCooltime(skillIndex))
		{
			ModCooltimeDurationInfo modCooltimeDurationInfo = GetModCooltimeDurationInfo(skillIndex);
			if (modCooltimeDurationInfo == null)
			{
				modCooltimeDurationInfo = SnapshotObjectPool<ModCooltimeDurationInfo>.Alloc();
				modCooltimeDurationInfo.SkillIndex = skillIndex;
			}
			modCooltimeDurationInfo.DurationMs += durationMs;
			modCooltimeDurationInfo.Count += ((count <= 0) ? 1 : count);
			ModCooltimeDurationInfoDictionary.Remove(skillIndex);
			ModCooltimeDurationInfoDictionary.Add(skillIndex, modCooltimeDurationInfo);
		}
	}

	public void ModNextCooltimeRatio(int skillIndex, int ratioM, int count, bool checkExist)
	{
		int[] coolTimeGroupSkillList = GetCoolTimeGroupSkillList(skillIndex);
		foreach (int skillIndex2 in coolTimeGroupSkillList)
		{
			ModNextCooltimeRatioInternal(skillIndex2, ratioM, count, checkExist);
		}
	}

	public void ModNextCooltimeRatioInternal(int skillIndex, int ratioM, int count, bool checkExist)
	{
		if (!checkExist || IsInCooltime(skillIndex))
		{
			ModCooltimeRatioInfo modCooltimeRatioInfo = GetModCooltimeRatioInfo(skillIndex);
			if (modCooltimeRatioInfo == null)
			{
				modCooltimeRatioInfo = SnapshotObjectPool<ModCooltimeRatioInfo>.Alloc();
				modCooltimeRatioInfo.SkillIndex = skillIndex;
			}
			modCooltimeRatioInfo.SkillIndex = skillIndex;
			modCooltimeRatioInfo.RatioM += ratioM;
			modCooltimeRatioInfo.Count += ((count <= 0) ? 1 : count);
			ModCooltimeRatioInfoDictionary.Remove(skillIndex);
			ModCooltimeRatioInfoDictionary.Add(skillIndex, modCooltimeRatioInfo);
		}
	}

	private ModCooltimeDurationInfo GetModCooltimeDurationInfo(int skillIndex)
	{
		ModCooltimeDurationInfo value = null;
		ModCooltimeDurationInfoDictionary.TryGetValue(skillIndex, out value);
		return value;
	}

	private ModCooltimeRatioInfo GetModCooltimeRatioInfo(int skillIndex)
	{
		ModCooltimeRatioInfo value = null;
		ModCooltimeRatioInfoDictionary.TryGetValue(skillIndex, out value);
		return value;
	}

	private void RemoveModCooltimeDurationInfo(int skillIndex)
	{
		ModCooltimeDurationInfoDictionary.Remove(skillIndex);
	}

	private void RemoveModCooltimeRatioInfo(int skillIndex)
	{
		ModCooltimeRatioInfoDictionary.Remove(skillIndex);
	}

	public void DecModCooltimeDurationInfo(int skillIndex)
	{
		if (GetModCooltimeDurationInfo(skillIndex) != null)
		{
			RemoveModCooltimeDurationInfo(skillIndex);
		}
		if (GetModCooltimeRatioInfo(skillIndex) != null)
		{
			RemoveModCooltimeRatioInfo(skillIndex);
		}
	}
}
