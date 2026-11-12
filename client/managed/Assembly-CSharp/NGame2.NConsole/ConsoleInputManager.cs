using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NConsole.NClientCheat;
using NGame2.NContentsInfo;
using NGame2.NGuild;
using NGame2.NMatch;
using NGame2.NUI.NManager;
using NShared;
using NShared.Cheat;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NConsole;

public class ConsoleInputManager : SingletonAuto<ConsoleInputManager>
{
	private readonly Dictionary<string, ICheatCommand> _cheatCommandDictionary = new Dictionary<string, ICheatCommand>();

	public ConsoleInputManager()
	{
		AddClientCheat(new Help());
		AddClientCheat(new MoveChapter());
		AddClientCheat(new Item());
		AddClientCheat(new SpawnMonster());
	}

	private void AddClientCheat(ICheatCommand cheatCommand)
	{
		string key = cheatCommand.GetType().Name.ToLower();
		_cheatCommandDictionary.Remove(key);
		_cheatCommandDictionary.Add(key, cheatCommand);
	}

	public void OnConsoleInput(string input)
	{
		OnConsoleInput(input, null);
	}

	public void OnConsoleInput(string input, Action<string> onResponse)
	{
		if (string.IsNullOrEmpty(input))
		{
			return;
		}
		string[] array = input.Split(new char[1] { ';' });
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			string input2 = array[i].Trim();
			string clientMessage = ProcessClientCheat(input2);
			string.IsNullOrEmpty(clientMessage);
			RequestCheatToServer(input2, delegate(string serverMessage)
			{
				Vespa_System_Action_Extension_Method.fire(onResponse, clientMessage + "\n" + serverMessage);
			});
		}
	}

	public static void RequestCheatToServer(string input, Action<string> onResponse = null)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			Vespa_System_Action_Extension_Method.fire(onResponse, null);
			return;
		}
		AccountManager accountManager = GlobalAutoBehaviour<AccountManager>.instance;
		if (accountManager == null)
		{
			Vespa_System_Action_Extension_Method.fire(onResponse, null);
			return;
		}
		NShared.Cheat.Request request = new NShared.Cheat.Request
		{
			Command = input
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.Ping, delegate(NShared.Cheat.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onResponse, null);
			}
			else if (response.Result == ResultType.NoPrivilege)
			{
				Vespa_System_Action_Extension_Method.fire(onResponse, null);
			}
			else if (response.Result != ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onResponse, response.Message);
			}
			else
			{
				if (response.DungeonInfos != null)
				{
					ChapterDungeonInfo[] dungeonInfos = response.DungeonInfos;
					foreach (ChapterDungeonInfo chapterDungeonInfo in dungeonInfos)
					{
						GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.CheckNewClearNode(chapterDungeonInfo.ChapterIndex, chapterDungeonInfo.DungeonIndex);
					}
				}
				accountManager.UserManager.Apply(response.CurrencyResults);
				accountManager.ItemManager.Apply(response.ItemResults);
				accountManager.EquipItemManager.Apply(response.EquipItemResults);
				accountManager.EquipItemManager.ApplyRemoved(response.RemovedEquipItemSlotIndices);
				accountManager.UserManager.Apply(response.FriendshipPointResult);
				accountManager.UserManager.Apply(response.TeamExpResult);
				accountManager.UserManager.Apply(response.GuildPointResult);
				accountManager.TowerManager.Apply(response.TowerInfos);
				accountManager.CampaignManager.Apply(response.DungeonInfos);
				accountManager.TutorialManager.Apply(response.TutorialInfos);
				accountManager.UserManager.Apply(response.StaminaResults);
				accountManager.RaidManager.Apply(response.RaidInfos);
				accountManager.UserManager.Apply(response.MiscInfo);
				accountManager.PlayerCurrencyManager.Apply(response.CurrencyResults);
				accountManager.SoulWeaponManager.Apply(response.SoulWeaponInfo);
				accountManager.SetPlayerPurchaseTimeDurationInfo(response.PurchaseTimeDurations);
				accountManager.HeroFriendlyManager.Apply(response.HeroFriendlyInfo);
				accountManager.NPCFriendlyManager.Apply(response.NpcFriendlyInfo);
				accountManager.PayShopDiscountProductManager.Apply(response.PayShopDiscountInfos);
				accountManager.PlayerProductPurchaseManager.Apply(response.PlayerProductPurchaseInfo);
				if (response.MyGuildInfo != null)
				{
					accountManager.InitGuildInfo(response.MyGuildInfo);
				}
				if (response.RemovedHeroIndex != 0)
				{
					accountManager.HeroManager.Remove(response.RemovedHeroIndex);
				}
				if (GlobalAutoBehaviour<GuildManager>.isValidInstance)
				{
					UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
					if (myGuildManager != null)
					{
						myGuildManager.SetActivityPoint(response.GuildActivityPointResult);
						myGuildManager.SetSuppressPoint(response.GuildSuppressPointResult);
						myGuildManager.GuildRaidManager.Apply(response.GuildRaidInfo);
						myGuildManager.SetBuildingInfo(response.GuildBuildingInfo);
					}
				}
				GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.StartEffect();
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(response.HeroInfos))
				{
					HeroInfo[] heroInfos = response.HeroInfos;
					foreach (HeroInfo heroInfo in heroInfos)
					{
						accountManager.HeroManager.Remove(heroInfo.HeroIndex);
						accountManager.HeroManager.Add(new CachedHeroInfo(heroInfo));
					}
				}
				if (SingletonUpdater<MatchManager>.isValidInstance)
				{
					SingletonAuto<MatchArena>.instance.ApplyBattleInfo(MatchPvpType.Normal, response.BattleInfo);
				}
				accountManager.StepUpEquipGachamanager.Apply(response.StepUpEquipGachaInfo);
				accountManager.PetManager.Apply(response.PetInfos);
				accountManager.PetManager.Apply(response.PetMiscInfos);
				accountManager.PetGachaCountManager.Apply(response.PetGachaCountInfos);
				accountManager.PetExploreManager.Apply(response.PetExploreInfos);
				accountManager.OpenPunishmentRaidManager.Set(response.PunishmentRaidInfos);
				accountManager.OpenPunishmentRaidManager.Apply(response.OpenPunishmentRaidInfo);
				accountManager.UserManager.ApplyPlayerTreasureHouseInfo(response.PlayerTreasureHouseInfo);
				Vespa_System_Action_Extension_Method.fire(onResponse, response.Message);
			}
		});
	}

	private string ProcessClientCheat(string input)
	{
		string output = string.Empty;
		string[] array = input.Split(new char[1] { ' ' });
		if (__Extension_Method_ICollection__.COUNT(array) == 0)
		{
			return output;
		}
		string key = array[0].ToLower();
		ICheatCommand value = null;
		_cheatCommandDictionary.TryGetValue(key, out value);
		if (value == null)
		{
			return output;
		}
		value.Process(array, out output);
		return output;
	}

	public string GetHelpMessage(string command)
	{
		if (string.IsNullOrEmpty(command))
		{
			return GetAllHelpMessage();
		}
		ICheatCommand value = null;
		_cheatCommandDictionary.TryGetValue(command, out value);
		if (value == null)
		{
			return string.Empty;
		}
		return value.Help();
	}

	private string GetAllHelpMessage()
	{
		string text = "";
		foreach (ICheatCommand value in _cheatCommandDictionary.Values)
		{
			text += (string.IsNullOrEmpty(text) ? "" : "\n");
			text += value.Help();
		}
		return text;
	}
}
