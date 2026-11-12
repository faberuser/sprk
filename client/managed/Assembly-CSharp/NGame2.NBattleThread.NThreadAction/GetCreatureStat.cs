using System.Collections.Generic;
using NGame2.NBattleField;
using NShared;
using NShared.NCampaign;
using NShared.NOperationRuntime;
using NVespa;
using UnityEngine;

namespace NGame2.NBattleThread.NThreadAction;

public class GetCreatureStat : IThreadAction
{
	public class CreatureStatData
	{
		public class ValueData
		{
			public long RawValue;

			public long FinalValue;

			public bool IsOverrideStat;

			public long OverrideValue;
		}

		public long Hp;

		public long Mp;

		public long Shield;

		public IncreaseDic<StatType, ValueData> StatDic = new IncreaseDic<StatType, ValueData>(StatTypeComparer.Default);
	}

	public static bool IsEnabled = false;

	public static bool HideValueZero = true;

	public static bool Foldout = false;

	public int CreatureUid;

	private CreatureStatData _stat;

	private CreatureStatData _threadStat;

	public bool IsWaiting { get; set; }

	public GetCreatureStat(int creatureUid)
	{
		CreatureUid = creatureUid;
	}

	public void ChainUpdate()
	{
		if (IsEnabled && !IsWaiting)
		{
			if (_stat == null)
			{
				_stat = new CreatureStatData();
			}
			if (_threadStat == null)
			{
				_threadStat = new CreatureStatData();
			}
			Math.Swap(ref _stat, ref _threadStat);
			SingletonAuto<BattleFacade>.instance.AddThreadSync(this);
		}
	}

	void IThreadAction.Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater)
	{
		if (_threadStat == null)
		{
			return;
		}
		BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
		if (creature != null)
		{
			_threadStat.Hp = creature.StatController.Hp;
			_threadStat.Mp = creature.StatController.Mp;
			_threadStat.StatDic.Clear();
			_threadStat.Shield = GetShieldHp(creature);
			EnumLookup<StatType>.ValueData[] valueArray = EnumLookup<StatType>.ValueArray;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
			{
				EnumLookup<StatType>.ValueData valueData = valueArray[i];
				long finalStatNoCache = creature.StatController.GetFinalStatNoCache(valueData.EnumValue, allowNegative: true, applyAttenuation: false);
				long finalStatNoCache2 = creature.StatController.GetFinalStatNoCache(valueData.EnumValue, allowNegative: true, applyAttenuation: true);
				long statValue = 0L;
				bool overrideStat = creature.StatController.GetOverrideStat(valueData.EnumValue, out statValue);
				_threadStat.StatDic.Add(valueData.EnumValue, new CreatureStatData.ValueData
				{
					RawValue = finalStatNoCache,
					FinalValue = finalStatNoCache2,
					IsOverrideStat = overrideStat,
					OverrideValue = statValue
				});
			}
		}
	}

	public void DrawToggleGUI()
	{
		IsEnabled = GUILayout.Toggle(IsEnabled, "Is Enabled");
		HideValueZero = GUILayout.Toggle(HideValueZero, "Hide Value Zero");
		if (_stat == null)
		{
			return;
		}
		GUILayout.Label("Hp : " + Vespa_Long_Extension_Method.ToCommaString(_stat.Hp));
		GUILayout.Label("Mp : " + Vespa_Long_Extension_Method.ToCommaString(_stat.Mp));
		GUILayout.Label("Shield : " + Vespa_Long_Extension_Method.ToCommaString(_stat.Shield));
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_stat.StatDic); i++)
		{
			KeyValuePair<StatType, CreatureStatData.ValueData> keyValuePairByIndex = _stat.StatDic.GetKeyValuePairByIndex(i);
			StatType key = keyValuePairByIndex.Key;
			CreatureStatData.ValueData value = keyValuePairByIndex.Value;
			if (!HideValueZero || value.RawValue != 0L || value.FinalValue != 0L)
			{
				string empty = string.Empty;
				empty = ((value.RawValue != value.FinalValue) ? (Vespa_String_Extension_Method.GetEnumString(key) + " : " + StatData.GetStatDisplayValue(key, value.RawValue, value.FinalValue, " => ", showSign: true, isCommaString: true)) : (Vespa_String_Extension_Method.GetEnumString(key) + " : " + StatData.GetStatDisplayValue(key, value.RawValue, showSign: true, isCommaString: true)));
				if (value.IsOverrideStat)
				{
					empty = empty + " (OverrideStat : " + StatData.GetStatDisplayValue(key, value.OverrideValue, showSign: true, isCommaString: true) + ")";
				}
				GUILayout.Label(empty);
			}
		}
	}

	private long GetShieldHp(BattleCreature battleCreature)
	{
		long num = 0L;
		LinkedListNode<IDamageModifier> linkedListNode = battleCreature.StatController.GetDamageModifierList(DamagePipelinePosition.Final).First;
		while (linkedListNode != null)
		{
			LinkedListNode<IDamageModifier> next = linkedListNode.Next;
			IDamageModifier value = linkedListNode.Value;
			if (value.GetModifiedType() == DamageModifiedType.Shield && value.IsValid())
			{
				if (value is HpShield hpShield)
				{
					num += hpShield.ShieldHp;
				}
				if (value is IShield shield)
				{
					num += shield.GetShieldHp();
				}
			}
			linkedListNode = next;
		}
		return num;
	}
}
