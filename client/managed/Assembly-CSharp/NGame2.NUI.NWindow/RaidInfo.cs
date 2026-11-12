using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RaidInfo : RaidInfoBase, IInspectorGuiDrawer
{
	public const string PrefabPath = "Resources/Prefab2/Window/Raid/RaidInfo";

	public UIWidget MonsterLevelRoot;

	public UILabel Lbl_HardMode;

	public UILabel Lbl_ChallengePrefix;

	public Transform Trs_RaidTitle;

	public Transform Trs_ChallengeRaidTitle;

	public Transform Trs_RaidButtons;

	public Transform Trs_ChallengeRaidButtons;

	public UIButton PrevGroupBossInfoButton;

	public UIButton NextGroupBossInfoButton;

	private RaidData _raidData;

	private List<RaidMultiInfoData> _raidMultiInfoDataList;

	private int _currentGroupBossInfoIndex;

	private bool CheckGroupBossInfo
	{
		get
		{
			if (_raidData == null)
			{
				return false;
			}
			if (_raidData.RaidMultiInfoGroupIndex <= 0)
			{
				return false;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(_raidMultiInfoDataList))
			{
				return _raidMultiInfoDataList.Count() > 1;
			}
			return false;
		}
	}

	private RaidMultiInfoData CurrentRaidMultiInfoData
	{
		get
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_raidMultiInfoDataList))
			{
				return null;
			}
			if (_currentGroupBossInfoIndex >= _raidMultiInfoDataList.Count)
			{
				return null;
			}
			return _raidMultiInfoDataList[_currentGroupBossInfoIndex];
		}
	}

	private int RaidMultiInfoForceWaveIndex
	{
		get
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_raidMultiInfoDataList))
			{
				return 0;
			}
			if (_raidMultiInfoDataList.Count <= _currentGroupBossInfoIndex)
			{
				return 0;
			}
			return _raidMultiInfoDataList[_currentGroupBossInfoIndex].WaveIndex;
		}
	}

	public void Open(Listener listener, RaidData raidData)
	{
		if (raidData == null)
		{
			VespaDebug.LogError("RaidInfo::Open / raidData == null");
			return;
		}
		_raidData = raidData;
		_raidMultiInfoDataList = GameRoot.Instance.RaidMultiInfoDataContainer.GetRaidMultiInfoDataListByGroupIndex(raidData.RaidMultiInfoGroupIndex);
		Open(listener, raidData, RaidMultiInfoForceWaveIndex);
		if (_raidData.RaidMultiInfoGroupIndex > 0)
		{
			SetNextGroupBossInfo(_currentGroupBossInfoIndex);
		}
		else
		{
			InitRaidInfo();
		}
	}

	public override void Reset()
	{
		base.Reset();
		_currentGroupBossInfoIndex = 0;
	}

	private void InitRaidInfo()
	{
		InitRaidLevel();
		InitRaidTitle();
		InitButtons();
		SetGroupBossInfoButtonState();
	}

	private void SetBossInfo(RaidData raidData)
	{
		if (raidData != null)
		{
			_raidData = raidData;
			Init(raidData, RaidMultiInfoForceWaveIndex);
			InitRaidInfo();
		}
	}

	public override string GetRaidName()
	{
		return _raidData.Name;
	}

	public override string GetMonsterTribe()
	{
		return GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(_dungeonData.ChapterIndex, _dungeonData.DungeonIndex, DungeonDifficultyType.Easy, _dungeonData.Scenario).Tribe;
	}

	public override int GetRewardIndex()
	{
		return _dungeonData.GetRewardIndexOfDifficulty(DungeonDifficultyType.Easy);
	}

	public override PrefabAuto<RaidSkillComponent> GetSkillComponentPrefab()
	{
		return RaidSkillComponent.PrefabAuto;
	}

	protected override void SetupRaidInfo()
	{
		base.SetupRaidInfo();
		if (CheckGroupBossInfo)
		{
			RaidMultiInfoData currentRaidMultiInfoData = CurrentRaidMultiInfoData;
			if (currentRaidMultiInfoData != null && !String_Extension_Method.IsNullOrEmpty(currentRaidMultiInfoData.Tactic))
			{
				string stringDirect = LocalizationManager.GetStringDirect(currentRaidMultiInfoData.Tactic);
				SprkRaidInfoScroll.SetText(RaidTacticLabel, stringDirect, option: false);
			}
		}
	}

	private void InitRaidLevel()
	{
		UIHelper.EnableGameObject(MonsterLevelRoot, enable: false);
		UIHelper.EnableGameObject(Lbl_HardMode, enable: false);
		UIHelper.EnableGameObject(Lbl_ChallengePrefix, IsChallengeRaid() || _raidData.Type == RaidType.GuildSuppress || _raidData.Type == RaidType.GuildSuppressSingle || _raidData.Type == RaidType.DragonNormal || _raidData.Type == RaidType.DragonHard || _raidData.Type == RaidType.DragonNormalSingle || _raidData.Type == RaidType.DragonHardSingle || _raidData.Type == RaidType.TechnoMagic || _raidData.Type == RaidType.TechnoMagicSingle || _raidData.Type == RaidType.TechnoEnchant || _raidData.Type == RaidType.TechnoEnchantSingle || _raidData.Type == RaidType.PunishmentRaid);
	}

	private void InitButtons()
	{
		UIHelper.EnableGameObject(Trs_ChallengeRaidButtons, IsChallengeRaid());
		UIHelper.EnableGameObject(Trs_RaidButtons, !IsChallengeRaid());
	}

	private void InitRaidTitle()
	{
		UIHelper.EnableGameObject(Trs_ChallengeRaidTitle, IsChallengeRaid());
		UIHelper.EnableGameObject(Trs_RaidTitle, !IsChallengeRaid());
	}

	private void SetGroupBossInfoButtonState()
	{
		UIHelper.EnableGameObject(PrevGroupBossInfoButton, CheckGroupBossInfo);
		UIHelper.EnableGameObject(NextGroupBossInfoButton, CheckGroupBossInfo);
	}

	private void SetNextGroupBossInfo(int nextIndex)
	{
		RaidData raidDataByMultiInfoData = GetRaidDataByMultiInfoData(nextIndex);
		if (raidDataByMultiInfoData != null)
		{
			SetBossInfo(raidDataByMultiInfoData);
		}
	}

	private bool IsChallengeRaid()
	{
		RaidType type = _raidData.Type;
		if (type == RaidType.Challenge || type == RaidType.ChallengeSingle)
		{
			return true;
		}
		return false;
	}

	private RaidData GetRaidDataByMultiInfoData(int nextIndex)
	{
		RaidMultiInfoData raidMultiInfoData = GetRaidMultiInfoData(nextIndex);
		if (raidMultiInfoData == null)
		{
			return null;
		}
		if (_raidData == null)
		{
			return null;
		}
		return GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(raidMultiInfoData.ChapterIndex, raidMultiInfoData.DungeonIndex);
	}

	private RaidMultiInfoData GetRaidMultiInfoData(int nextIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_raidMultiInfoDataList))
		{
			return null;
		}
		_currentGroupBossInfoIndex += nextIndex;
		if (_currentGroupBossInfoIndex < 0)
		{
			_currentGroupBossInfoIndex = _raidMultiInfoDataList.Count - 1;
		}
		if (_currentGroupBossInfoIndex >= _raidMultiInfoDataList.Count)
		{
			_currentGroupBossInfoIndex = 0;
		}
		return _raidMultiInfoDataList[_currentGroupBossInfoIndex];
	}

	public void OnClickPrevBossInfo()
	{
		if (CheckGroupBossInfo)
		{
			SetNextGroupBossInfo(-1);
		}
	}

	public void OnClickNextBossInfo()
	{
		if (CheckGroupBossInfo)
		{
			SetNextGroupBossInfo(1);
		}
	}

	public override RaidType GetRaidType()
	{
		return _raidData.Type;
	}
}
