using System.Collections.Generic;
using NGame2.NBattleView;
using NGame2.NUtil;
using NShared;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class WorldBossStatus : BaseWindow
{
	public class ScoreContext
	{
		public bool _initialized;

		public long _initialTotalHp;

		public HashSet<int> _initialUids = new HashSet<int>();

		public long _currentTotalHp;

		public bool _isRequesting;

		public float _lastUpdatedTime;

		public void Reset()
		{
			_initialized = false;
			_initialTotalHp = 0L;
			_currentTotalHp = 0L;
			_isRequesting = false;
			_initialUids.Clear();
			_lastUpdatedTime = 0f;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/WorldBossStatus";

	public BossStatus.GradeInfo GradeInfo;

	public UILabel ScoreLabel;

	public UISimpleGrid BuffGrid;

	public Transform PatternTextAlarm;

	public Transform PatternImageAlarm;

	public UILabel PatternAlarmLabel;

	private int _chapterIndex;

	private int _dungeonIndex;

	private DungeonDifficultyType _difficulty;

	private Creature _boss;

	private BattleDefineData _battleDungeonData;

	private BattleScoreContext _scoreContext2 = new BattleScoreContext();

	private BattleStateUiController _stateUiController = new BattleStateUiController();

	public void Open(Creature bossCreature, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, BattleDefineData battleDungeonData)
	{
		Open(delegate
		{
			Init(bossCreature, chapterIndex, dungeonIndex, difficulty, battleDungeonData);
		}, null);
	}

	public void Init(Creature bossCreature, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, BattleDefineData battleDungeonData)
	{
		_boss = bossCreature;
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
		_difficulty = difficulty;
		_battleDungeonData = battleDungeonData;
		BossStatus.SetupIcon(this, _boss, GradeInfo);
		BossStatus.SetupPhase(GradeInfo);
		SetupBuff();
	}

	public override void Reset()
	{
		base.Reset();
		ResetBuff();
	}

	public override void Close()
	{
		base.Close();
		ResetBuff();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		BossStatus.UpdateHpGauge(_boss, GradeInfo, _battleDungeonData.HPPercent);
		UpdateScore(ScoreLabel, _scoreContext2);
		UpdateBuff();
	}

	public void UpdateScore(UILabel scoreLabel, BattleScoreContext scoreContext)
	{
		BattleScoreHelper.UpdateScore(scoreLabel, scoreContext);
	}

	private void SetupBuff()
	{
		_stateUiController.Open(_boss.Uid, BuffGrid);
	}

	private void ResetBuff()
	{
		_stateUiController.Reset();
	}

	private void UpdateBuff()
	{
		_stateUiController.ChainUpdate();
	}

	public Transform GetBottomDummy()
	{
		if (GradeInfo == null)
		{
			return null;
		}
		return GradeInfo.BottomDummy;
	}
}
