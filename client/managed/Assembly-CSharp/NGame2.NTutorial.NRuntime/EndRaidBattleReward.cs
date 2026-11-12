using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.GetMultiplayReward;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class EndRaidBattleReward : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime, MultiplayReward.Listener
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	private void GetArgsForCommand(TutorialData tutorialData, string command, List<int> commandArgs, int step = 1)
	{
		commandArgs.Clear();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(tutorialData.Argument))
		{
			return;
		}
		for (int i = 0; i < tutorialData.Argument.Length; i++)
		{
			if (string.Compare(TutorialUtility.GetArg(tutorialData, i), command) != 0)
			{
				continue;
			}
			int num = 0;
			for (int j = i + 1; j < tutorialData.Argument.Length; j++)
			{
				int argInt = TutorialUtility.GetArgInt(tutorialData, j);
				if (argInt == 0 && num % step == 0)
				{
					break;
				}
				num++;
				commandArgs.Add(argInt);
			}
			break;
		}
	}

	private void SetExpResultInfo(int addedExp, int heroIdx, HeroExpResultInfo info)
	{
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIdx);
		info.HeroIndex = heroIdx;
		info.AddBoosterValue = 0;
		info.NewValue = heroInfo.Exp + addedExp;
		info.OldLevel = heroInfo.Level;
		info.NewLevel = heroInfo.Level;
		while (true)
		{
			if (info.NewLevel > GameRoot.Instance.CreatureLevelDataContainer.MaxLevel)
			{
				info.NewLevel = (short)GameRoot.Instance.CreatureLevelDataContainer.MaxLevel;
				info.NewValue = 0;
				break;
			}
			long localExp = GameRoot.Instance.CreatureLevelDataContainer.GetData(info.NewLevel).LocalExp;
			if (info.NewValue < localExp)
			{
				break;
			}
			info.NewValue -= (int)localExp;
			short newLevel = (short)(info.NewLevel + 1);
			info.NewLevel = newLevel;
		}
		info.AddValue = info.NewValue - heroInfo.Exp;
	}

	private HeroExpResultInfo[] GetHeroExpResult(TutorialData tutorialData)
	{
		List<int> list = new List<int>();
		GetArgsForCommand(tutorialData, "AddHeroExp", list);
		if (list.Count == 0 || list.Count == 1)
		{
			return null;
		}
		HeroExpResultInfo[] array = new HeroExpResultInfo[list.Count - 1];
		for (int i = 1; i < list.Count; i++)
		{
			array[i - 1] = new HeroExpResultInfo();
			SetExpResultInfo(list[0], list[i], array[i - 1]);
		}
		return array;
	}

	private ItemResultInfo[] GetItemResult(TutorialData tutorialData)
	{
		List<int> list = new List<int>();
		GetArgsForCommand(tutorialData, "AddItem", list);
		if (list.Count == 0 || list.Count == 1)
		{
			return null;
		}
		ItemResultInfo[] array = new ItemResultInfo[list.Count / 2];
		for (int i = 0; i < list.Count / 2; i++)
		{
			array[i] = new ItemResultInfo();
			array[i].ItemIndex = list[i * 2];
			array[i].AddCount = list[i * 2 + 1];
			array[i].NewCount = list[i * 2 + 1] + GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(array[i].ItemIndex);
		}
		return array;
	}

	private EquipItemInfo[] GetEquipInfos(TutorialData tutorialData)
	{
		List<int> list = new List<int>();
		GetArgsForCommand(tutorialData, "AddEquipItem", list, 3);
		if (list.Count == 0)
		{
			return null;
		}
		EquipItemInfo[] array = new EquipItemInfo[list.Count / 3];
		for (int i = 0; i < list.Count / 3; i++)
		{
			array[i] = new EquipItemInfo();
			array[i].ItemIndex = list[i * 3];
			array[i].Star = list[i * 3 + 1];
			array[i].Level = list[i * 3 + 2];
		}
		return array;
	}

	private GoldResultInfo[] GetGoldInfos(TutorialData tutorialData)
	{
		List<int> list = new List<int>();
		GetArgsForCommand(tutorialData, "AddGold", list);
		if (list.Count == 0)
		{
			return null;
		}
		GoldResultInfo[] obj = new GoldResultInfo[1]
		{
			new GoldResultInfo()
		};
		GoldResultInfo obj2 = obj[0];
		int num = list[0];
		obj2.AddValue = num;
		return obj;
	}

	private GemResultInfo[] GetGemInfos(TutorialData tutorialData)
	{
		List<int> list = new List<int>();
		GetArgsForCommand(tutorialData, "AddGem", list, 2);
		if (list.Count == 0)
		{
			return null;
		}
		GemResultInfo[] array = new GemResultInfo[1]
		{
			new GemResultInfo()
		};
		GemResultInfo obj = array[0];
		int num = list[0];
		obj.AddValue = num;
		if (list.Count == 2)
		{
			array[0].FirstDiffClear = (byte)list[1];
		}
		return array;
	}

	private RaidPointResultInfo[] GetRaidPointInfos(TutorialData tutorialData)
	{
		List<int> list = new List<int>();
		GetArgsForCommand(tutorialData, "AddRaidPoint", list, 2);
		if (list.Count == 0)
		{
			return null;
		}
		RaidPointResultInfo[] obj = new RaidPointResultInfo[1]
		{
			new RaidPointResultInfo()
		};
		RaidPointResultInfo obj2 = obj[0];
		int num = list[0];
		obj2.AddValue = num;
		return obj;
	}

	private RewardItemInfo[] GetRewardItemInfos(TutorialData tutorialData)
	{
		List<int> list = new List<int>();
		GetArgsForCommand(tutorialData, "AddReward", list, 3);
		if (list.Count == 0)
		{
			return null;
		}
		RewardItemInfo[] array = new RewardItemInfo[list.Count / 3];
		for (int i = 0; i < list.Count / 3; i++)
		{
			array[i] = new RewardItemInfo();
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(list[i * 3]);
			array[i].Code = data.Code;
			array[i].Count = list[i * 3 + 1];
			array[i].Star = list[i * 3 + 2];
		}
		return array;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntimeImmediate)this).RunImmediate(window, tutorialData);
		yield break;
	}

	void ITutorialRuntimeImmediate.RunImmediate(TutorialWindow window, TutorialData tutorialData)
	{
		HeroExpResultInfo[] heroExpResult = GetHeroExpResult(tutorialData);
		ItemResultInfo[] itemResult = GetItemResult(tutorialData);
		EquipItemInfo[] equipInfos = GetEquipInfos(tutorialData);
		GoldResultInfo[] goldInfos = GetGoldInfos(tutorialData);
		GemResultInfo[] gemInfos = GetGemInfos(tutorialData);
		RaidPointResultInfo[] raidPointInfos = GetRaidPointInfos(tutorialData);
		RewardItemInfo[] rewardItemInfos = GetRewardItemInfos(tutorialData);
		SceneAutoBehaviour<EndCampaign>.instance.TutorialEndRaidBattleReward(this, heroExpResult, itemResult, equipInfos, goldInfos, gemInfos, raidPointInfos, rewardItemInfos);
	}

	bool MultiplayReward.Listener.IsPartyValid()
	{
		return true;
	}

	void MultiplayReward.Listener.OnAbandon(MultiplayReward caller)
	{
	}

	void MultiplayReward.Listener.OnBid(MultiplayReward caller)
	{
	}

	void MultiplayReward.Listener.OnRewardCalculationEnd(MultiplayReward caller, NShared.GetMultiplayReward.Response response)
	{
	}

	void MultiplayReward.Listener.RequestGetReward()
	{
	}
}
