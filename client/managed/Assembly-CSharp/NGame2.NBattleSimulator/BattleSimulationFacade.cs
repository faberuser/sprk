using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NPet;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleSimulator;

public static class BattleSimulationFacade
{
	public static List<BattleSimulationInstance> BattleSimulatorList = new List<BattleSimulationInstance>(5);

	public static BattleSimulationInstance RunBattleSimulation(string uid, int[] heroIndices, int chapterIndex, int dungeonIndex, int dungeonDifficulty, bool enableBattleLog = false)
	{
		BattleSimulationAccountInfo battleSimulationAccountInfo = new BattleSimulationAccountInfo();
		battleSimulationAccountInfo.UserInfo = new CachedUserInfo
		{
			AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId
		};
		battleSimulationAccountInfo.HeroInfos = new Dictionary<int, CachedHeroInfo>();
		int i = 0;
		for (int num = heroIndices.Length; i < num; i++)
		{
			int key = heroIndices[i];
			battleSimulationAccountInfo.HeroInfos[key] = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(key);
		}
		battleSimulationAccountInfo.AccountBuffs = GlobalAutoBehaviour<AccountManager>.instance.GetActiveAccountBuffDataBaseList().ToArray();
		GuildSkillDataBase[] guildSkills = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildSkillLevelDataList().ToArray();
		battleSimulationAccountInfo.GuildSkills = guildSkills;
		ClassBuffWrapper classBuffWrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
		if (classBuffWrapper != null)
		{
			List<ClassBuffData> classBuffDataListByClassBuffInfo = classBuffWrapper.GetClassBuffDataListByClassBuffInfo();
			if (classBuffDataListByClassBuffInfo != null)
			{
				ClassBuffDataBase[] classBuffDataBases = classBuffDataListByClassBuffInfo.ToArray();
				battleSimulationAccountInfo.ClassBuffDataBases = classBuffDataBases;
			}
		}
		List<DefaultExtraStatData> list = new List<DefaultExtraStatData>();
		List<DefaultExtraStatData> petStatDataBaseList = GlobalAutoBehaviour<PetManager>.instance.GetPetStatDataBaseList();
		SumExtraStatData(list, petStatDataBaseList);
		List<PetStatDataBase> petContentsStatDataBaseList = GlobalAutoBehaviour<PetManager>.instance.GetPetContentsStatDataBaseList();
		battleSimulationAccountInfo.ExtraStatDatas = list?.ToArray();
		battleSimulationAccountInfo.PetStatDataBases = petContentsStatDataBaseList?.ToArray();
		BattleSimulationInfo battleSimulationInfo = new BattleSimulationInfo();
		battleSimulationInfo.AccountInfos = new BattleSimulationAccountInfo[1] { battleSimulationAccountInfo };
		battleSimulationInfo.ChapterIndex = chapterIndex;
		battleSimulationInfo.DungeonIndex = dungeonIndex;
		battleSimulationInfo.DungeonDifficulty = dungeonDifficulty;
		BattleSimulationInstance battleInst = new BattleSimulationInstance();
		lock (BattleSimulatorList)
		{
			BattleSimulatorList.Add(battleInst);
		}
		battleInst.Init(uid, battleSimulationInfo);
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			battleInst.BattleStart(enableBattleLog);
			do
			{
				if (battleInst.IsBattleStopped)
				{
					lock (BattleSimulatorList)
					{
						BattleSimulatorList.Remove(battleInst);
						break;
					}
				}
				battleInst.Update();
			}
			while (!battleInst.IsBattleEnd);
		}, (Action)null, ThreadManager.EEntryType.None);
		return battleInst;
	}

	public static void RemoveInstance(BattleSimulationInstance ins)
	{
		ins.Stop();
		lock (BattleSimulatorList)
		{
			BattleSimulatorList.Remove(ins);
		}
	}

	public static BattleSimulationInstance RunBattleSimulationTest(string uid, int[] heroIndices, int chapterIndex, int dungeonIndex, int dungeonDifficulty, int repeatCount = 1, Action<BattleSimulationResultInfo> callback = null)
	{
		BattleSimulationAccountInfo battleSimulationAccountInfo = new BattleSimulationAccountInfo();
		battleSimulationAccountInfo.UserInfo = new CachedUserInfo
		{
			AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId
		};
		battleSimulationAccountInfo.HeroInfos = new Dictionary<int, CachedHeroInfo>();
		int i = 0;
		for (int num = heroIndices.Length; i < num; i++)
		{
			int key = heroIndices[i];
			battleSimulationAccountInfo.HeroInfos[key] = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(key);
		}
		battleSimulationAccountInfo.AccountBuffs = GlobalAutoBehaviour<AccountManager>.instance.GetActiveAccountBuffDataBaseList().ToArray();
		GuildSkillDataBase[] guildSkills = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildSkillLevelDataList().ToArray();
		battleSimulationAccountInfo.GuildSkills = guildSkills;
		ClassBuffWrapper classBuffWrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
		if (classBuffWrapper != null)
		{
			List<ClassBuffData> classBuffDataListByClassBuffInfo = classBuffWrapper.GetClassBuffDataListByClassBuffInfo();
			if (classBuffDataListByClassBuffInfo != null)
			{
				ClassBuffDataBase[] classBuffDataBases = classBuffDataListByClassBuffInfo.ToArray();
				battleSimulationAccountInfo.ClassBuffDataBases = classBuffDataBases;
			}
		}
		List<DefaultExtraStatData> list = new List<DefaultExtraStatData>();
		List<DefaultExtraStatData> petStatDataBaseList = GlobalAutoBehaviour<PetManager>.instance.GetPetStatDataBaseList();
		SumExtraStatData(list, petStatDataBaseList);
		List<PetStatDataBase> petContentsStatDataBaseList = GlobalAutoBehaviour<PetManager>.instance.GetPetContentsStatDataBaseList();
		battleSimulationAccountInfo.ExtraStatDatas = list?.ToArray();
		battleSimulationAccountInfo.PetStatDataBases = petContentsStatDataBaseList?.ToArray();
		BattleSimulationInfo battleSimulationInfo = new BattleSimulationInfo();
		battleSimulationInfo.AccountInfos = new BattleSimulationAccountInfo[1] { battleSimulationAccountInfo };
		battleSimulationInfo.ChapterIndex = chapterIndex;
		battleSimulationInfo.DungeonIndex = dungeonIndex;
		battleSimulationInfo.DungeonDifficulty = dungeonDifficulty;
		BattleSimulationInstance battleInst = new BattleSimulationInstance();
		battleInst.Init(uid, battleSimulationInfo);
		battleInst.BattleStart();
		lock (BattleSimulatorList)
		{
			SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
			{
				do
				{
					battleInst.Update();
				}
				while (!battleInst.IsBattleEnd);
				Vespa_System_Action_Extension_Method.fire(callback, battleInst.GetBattleSimulationResultInfo());
			}, (Action)null, ThreadManager.EEntryType.None);
		}
		return battleInst;
	}

	public static void StopAllBattleSimulation()
	{
		lock (BattleSimulatorList)
		{
			int i = 0;
			for (int count = BattleSimulatorList.Count; i < count; i++)
			{
				BattleSimulatorList[i]?.Stop();
			}
			BattleSimulatorList.Clear();
		}
	}

	private static void SumExtraStatData(List<DefaultExtraStatData> extraStatDataList, List<DefaultExtraStatData> targetStatDataList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(targetStatDataList); i++)
		{
			DefaultExtraStatData statDataBase = targetStatDataList[i];
			if (statDataBase != null)
			{
				int num = extraStatDataList.FindIndex((DefaultExtraStatData statData) => statData.OptionType == statDataBase.OptionType);
				if (num == -1)
				{
					DefaultExtraStatData item = new DefaultExtraStatData
					{
						OptionType = statDataBase.OptionType,
						OptionValue = statDataBase.OptionValue
					};
					extraStatDataList.Add(item);
				}
				else
				{
					extraStatDataList[num].OptionValue += statDataBase.OptionValue;
				}
			}
		}
	}
}
