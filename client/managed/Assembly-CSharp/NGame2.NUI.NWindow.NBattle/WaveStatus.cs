using System;
using NGame2.NBattleField;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class WaveStatus : BaseWindow
{
	[Serializable]
	public class RootPositionByBossType
	{
		public MonsterGradeType GradeType;

		public Vector3 Position;
	}

	[Serializable]
	public class RootPosition
	{
		public BattleType BattleType;

		public Vector3 Position;

		public RootPositionByBossType[] SpecifiedBossTypePositions;
	}

	[Serializable]
	public class DifficultyInfo
	{
		public string Difficulty;

		public Transform Root;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/WaveStatus";

	public RootPosition[] RootPositionsWhenBossStatusOpened;

	public UILabel WaveLabel;

	public DifficultyInfo[] DifficultyInfos;

	private RootPosition _currentRootPositionInfo;

	private Func<bool> _checkBossStatusOpen;

	private Func<MonsterGradeType> _getBossMonsterGradeType;

	private bool _isBossStatusOpened;

	private int _currentWaveIndex;

	private int _currentMaxWaveIndex;

	private CampaignDungeonData _dungeonData;

	public void Open(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario, Func<bool> checkBossStatusOpen, Func<MonsterGradeType> getGradeTypeFunc)
	{
		base.Open();
		_checkBossStatusOpen = checkBossStatusOpen;
		_getBossMonsterGradeType = getGradeTypeFunc;
		Init(chapterIndex, dungeonIndex, difficulty, isScenario);
		InitCurrentBattleTypePositionInfo();
	}

	private void Init(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario)
	{
		if (isScenario)
		{
			SetDifficulty("Story");
			return;
		}
		CampaignDungeonData campaignDungeonData = (_dungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex));
		if (campaignDungeonData != null && campaignDungeonData.NoDifficulty)
		{
			SetDifficulty("Wave");
			return;
		}
		if (campaignDungeonData != null && campaignDungeonData.BattleType == BattleType.Eclipse)
		{
			SetDifficulty("Wave");
			return;
		}
		if (campaignDungeonData != null && campaignDungeonData.BattleType == BattleType.UnderPrison)
		{
			SetDifficulty("Wave");
			return;
		}
		if (campaignDungeonData != null && campaignDungeonData.BattleType == BattleType.TreasureHouse)
		{
			SetDifficulty("Wave");
			return;
		}
		if (campaignDungeonData != null && campaignDungeonData.BattleType == BattleType.KarmaRaid)
		{
			SetDifficulty("Wave");
			return;
		}
		if ((campaignDungeonData != null && campaignDungeonData.BattleType == BattleType.EventDungeon) || (campaignDungeonData != null && campaignDungeonData.BattleType == BattleType.SquareSideQuest))
		{
			SetDifficulty(difficulty.ToString());
			return;
		}
		CampaignWaveDataContainer.WaveCollectionData waveCollection = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(chapterIndex, dungeonIndex);
		if (waveCollection != null && waveCollection.DifficultyDic.Count > 1)
		{
			SetDifficulty(difficulty.ToString());
			return;
		}
		if (difficulty != DungeonDifficultyType.Normal)
		{
			SetDifficulty(difficulty.ToString());
			return;
		}
		bool flag = true;
		if (campaignDungeonData != null && !campaignDungeonData.BattleDefineCode.IsNullOrEmpty())
		{
			flag = campaignDungeonData.BattleDefineCode.Equals("Tutorial") || campaignDungeonData.BattleDefineCode.Equals("Tutorial2");
		}
		if (flag)
		{
			SetDifficulty("Story");
		}
		else
		{
			SetDifficulty(difficulty.ToString());
		}
	}

	private void InitCurrentBattleTypePositionInfo()
	{
		if (_dungeonData == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RootPositionsWhenBossStatusOpened); i++)
		{
			if (RootPositionsWhenBossStatusOpened[i].BattleType == _dungeonData.BattleType)
			{
				_currentRootPositionInfo = RootPositionsWhenBossStatusOpened[i];
				break;
			}
		}
	}

	public void SetDifficulty(string difficulty)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(DifficultyInfos); i++)
		{
			UIHelper.EnableGameObject(DifficultyInfos[i].Root, DifficultyInfos[i].Difficulty == difficulty);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_currentWaveIndex = 0;
		_currentRootPositionInfo = null;
		_isBossStatusOpened = false;
		_checkBossStatusOpen = null;
		WindowRoot.transform.localPosition = Vector3.zero;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			Close();
			return;
		}
		SetRootPosition(Vespa_System_Action_Extension_Method.fire(_checkBossStatusOpen), Vespa_System_Action_Extension_Method.fire(_getBossMonsterGradeType));
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (battleCache.WaveIndex != _currentWaveIndex || battleCache.MaxWaveIndex != _currentMaxWaveIndex)
		{
			string text = battleCache.WaveIndex + " / " + battleCache.TotalWaveIndex;
			UIHelper.SetLabelText(WaveLabel, text);
			_currentWaveIndex = battleCache.WaveIndex;
			if (_dungeonData != null && _dungeonData.BattleType == BattleType.Eclipse)
			{
				UIHelper.SetLabelText(WaveLabel, battleCache.MaxWaveIndex);
			}
			else if (_dungeonData != null && _dungeonData.BattleType == BattleType.KarmaRaid)
			{
				UIHelper.SetLabelText(WaveLabel, battleCache.MaxWaveIndex + 1);
			}
		}
	}

	private void SetRootPosition(bool isBossStatusOpen, MonsterGradeType bossType)
	{
		if (_currentRootPositionInfo == null || _isBossStatusOpened == isBossStatusOpen)
		{
			return;
		}
		_isBossStatusOpened = isBossStatusOpen;
		if (!isBossStatusOpen)
		{
			WindowRoot.transform.localPosition = Vector3.zero;
			return;
		}
		Vector3 position = _currentRootPositionInfo.Position;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_currentRootPositionInfo.SpecifiedBossTypePositions); i++)
		{
			if (_currentRootPositionInfo.SpecifiedBossTypePositions[i].GradeType == bossType)
			{
				position = _currentRootPositionInfo.SpecifiedBossTypePositions[i].Position;
			}
		}
		WindowRoot.transform.localPosition = position;
	}
}
