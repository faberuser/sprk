using System;
using System.Collections.Generic;
using NGame2.NBattleField;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BattleScoreContext
{
	public bool Initialized { get; set; }

	public long InitialTotalHp { get; set; }

	private HashSet<int> _initializeUids { get; set; }

	public long CurrentTotalHp { get; set; }

	public bool IsReserved { get; set; }

	public bool IsUnDead { get; set; }

	public float LastUpdateTime { get; set; }

	public long TotalGivedDamage { get; set; }

	public BattleScoreContext()
	{
		Initialized = false;
		InitialTotalHp = 0L;
		_initializeUids = new HashSet<int>();
		CurrentTotalHp = 0L;
		IsReserved = false;
		LastUpdateTime = 0f;
		TotalGivedDamage = 0L;
	}

	public void Reset()
	{
		Initialized = false;
		InitialTotalHp = 0L;
		CurrentTotalHp = 0L;
		IsReserved = false;
		LastUpdateTime = 0f;
		TotalGivedDamage = 0L;
		IsUnDead = false;
		ClearUids();
	}

	private void ClearUids()
	{
		_initializeUids.Clear();
	}

	private bool CanUpdate()
	{
		if (Time.time - LastUpdateTime < 0.3f)
		{
			return false;
		}
		LastUpdateTime = Time.time;
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return false;
		}
		if (IsReserved)
		{
			return false;
		}
		return true;
	}

	public void Update()
	{
		if (CanUpdate())
		{
			return;
		}
		IsReserved = true;
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			IsReserved = false;
			BattleCreatureList allCreaturesOfTeam = battleInstance.CreatureContainer.GetAllCreaturesOfTeam(1);
			if (allCreaturesOfTeam.Count != 0)
			{
				if (!Initialized)
				{
					Initialized = true;
					InitialTotalHp = 0L;
					IsUnDead = false;
					ClearUids();
					for (int i = 0; i < allCreaturesOfTeam.Count; i++)
					{
						BattleCreature battleCreature = allCreaturesOfTeam[i];
						int uid = battleCreature.Uid;
						_initializeUids.Add(uid);
						InitialTotalHp += battleCreature.StatController.Hp;
						IsUnDead |= battleCreature.StatController.GetMinHp() > 0;
					}
				}
				long num = 0L;
				long num2 = 0L;
				for (int j = 0; j < allCreaturesOfTeam.Count; j++)
				{
					BattleCreature battleCreature2 = allCreaturesOfTeam[j];
					int uid2 = battleCreature2.Uid;
					if (_initializeUids.Contains(uid2))
					{
						long hp = battleCreature2.StatController.Hp;
						num += hp;
						if (IsUnDead)
						{
							long num3 = battleInstance.Snapshot.StatisticsManager.TotalDamagedMeter(uid2);
							num2 += num3;
						}
					}
				}
				CurrentTotalHp = num;
				TotalGivedDamage = num2;
			}
		});
	}

	public long GetScore()
	{
		if (IsUnDead)
		{
			return TotalGivedDamage;
		}
		return System.Math.Max(InitialTotalHp - CurrentTotalHp, 0L);
	}
}
