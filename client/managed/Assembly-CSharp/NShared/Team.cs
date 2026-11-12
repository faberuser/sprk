using System.Collections.Generic;
using NShared.NUtil;

namespace NShared;

public class Team : SnapshotObject
{
	private readonly SnapshotObjectMember<TeamType> _typeHolder;

	private readonly SnapshotObjectMember<Team> _enemyTeamHolder;

	private readonly SnapshotObjectMember<bool> _rightDirectionHolder;

	private readonly SnapshotObjectMember<BattleCreature> _environmentCreatureHolder;

	public int Id => (int)Type;

	public Vector3 Direction
	{
		get
		{
			if (!RightDirection)
			{
				return Vector3.left;
			}
			return Vector3.right;
		}
	}

	public TeamType Type
	{
		get
		{
			return _typeHolder.Get();
		}
		set
		{
			_typeHolder.Set(value);
		}
	}

	public Team EnemyTeam
	{
		get
		{
			return _enemyTeamHolder.Get();
		}
		set
		{
			_enemyTeamHolder.Set(value);
		}
	}

	public bool RightDirection
	{
		get
		{
			return _rightDirectionHolder.Get();
		}
		set
		{
			_rightDirectionHolder.Set(value);
		}
	}

	public BattleCreature EnvironmentCreature => _environmentCreatureHolder.Get();

	public Team()
	{
		_typeHolder = new SnapshotObjectMember<TeamType>(this, TeamType.Left);
		_enemyTeamHolder = new SnapshotObjectMember<Team>(this);
		_rightDirectionHolder = new SnapshotObjectMember<bool>(this, value: false);
		_environmentCreatureHolder = new SnapshotObjectMember<BattleCreature>(this);
	}

	public void Init(TeamType type, bool rightDirection)
	{
		_typeHolder.Set(type);
		_rightDirectionHolder.Set(rightDirection);
	}

	public void SetEnemyTeam(Team team)
	{
		_enemyTeamHolder.Set(team);
	}

	public void InitEnvironmentCreature(BattleInstance battleInstance)
	{
		int environmentCreatureInex = BattleConstant.EnvironmentCreatureInex;
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(environmentCreatureInex);
		if (data == null)
		{
			return;
		}
		int[] indexList = ((battleInstance.DungeonData != null) ? battleInstance.DungeonData.DungeonSkillIndexList : null);
		List<int> dungeonSkillIndexListByTeam = GameRoot.Instance.CampaignDungeonSkillDataContainer.GetDungeonSkillIndexListByTeam(indexList, Type);
		bool flag = ((dungeonSkillIndexListByTeam != null && dungeonSkillIndexListByTeam.Count > 0) ? true : false);
		if (flag)
		{
			for (int i = 0; i < dungeonSkillIndexListByTeam.Count; i++)
			{
				int num = dungeonSkillIndexListByTeam[i];
				if (num != 0)
				{
					switch (i)
					{
					case 0:
						data.SkillIndex1 = num;
						break;
					case 1:
						data.SkillIndex2 = num;
						break;
					case 2:
						data.SkillIndex3 = num;
						break;
					}
				}
			}
		}
		CachedHeroInfo cachedHeroInfo = new CachedHeroInfo();
		cachedHeroInfo.HeroIndex = environmentCreatureInex;
		cachedHeroInfo.Level = 1;
		cachedHeroInfo.Star = 1;
		StatData equippedHeroStat = StatCalculator.GetEquippedHeroStat(cachedHeroInfo);
		BattleCreatureStockData battleCreatureStockData = new BattleCreatureStockData();
		battleCreatureStockData.CreatureData = data;
		battleCreatureStockData.CreateInfo = new BattleCreatureCreateInfo
		{
			IsDummy = true,
			HeroInfo = cachedHeroInfo,
			StatData = equippedHeroStat,
			HasDungeonSkill = flag
		};
		BattleCreature battleCreature = SnapshotObjectPool<BattleCreature>.Alloc();
		battleCreature.Init(battleInstance, battleCreatureStockData);
		battleCreature.InitSnapshot(0);
		battleCreature.OnCreated();
		_environmentCreatureHolder.Set(battleCreature);
	}
}
