using System.Collections.Generic;
using System.Linq;
using NShared.NArea;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class ProjectileContainer : SnapshotObject
{
	private readonly SnapshotObjectMember<SnapshotList<SkillProjectileInfo>> _projectileInfoListHolder;

	protected SnapshotObjectMember<long> _lastProjectileUidHolder;

	private BattleInstance _battleInstance;

	protected SnapshotList<SkillProjectileInfo> ProjectileInfoList => _projectileInfoListHolder.Get();

	public ProjectileContainer()
	{
		_projectileInfoListHolder = new SnapshotObjectMember<SnapshotList<SkillProjectileInfo>>(this);
		_lastProjectileUidHolder = new SnapshotObjectMember<long>(this, 0L);
	}

	public void Init(BattleInstance battleInstance)
	{
		_battleInstance = battleInstance;
		_projectileInfoListHolder.Alloc();
	}

	public void Clear()
	{
		SkillProjectileInfo[] array = ProjectileInfoList.ToArray();
		foreach (SkillProjectileInfo projectileInfo in array)
		{
			DestroyProjectile(projectileInfo);
		}
	}

	private long AllocProjectileUid()
	{
		long num = _lastProjectileUidHolder.Get() + 1;
		_lastProjectileUidHolder.Set(num);
		return num;
	}

	public void AddProjectile(int projectileIndex, TeamType teamType, Vector3 offset)
	{
		SkillProjectileData data = GameRoot.Instance.SkillProjectileDataContainer.GetData(projectileIndex);
		if (data != null)
		{
			BattleCreature environmentCreature = ((teamType == TeamType.Left) ? _battleInstance.Snapshot.LeftTeam : _battleInstance.Snapshot.RightTeam).EnvironmentCreature;
			if (environmentCreature == null)
			{
				VespaDebug.LogError("Environment creature not found: " + teamType);
				return;
			}
			int groupIndex = environmentCreature.CreateInfo.GroupIndex;
			Vector3 value = new Vector3(0L, 0L, 0L);
			_battleInstance.Snapshot.WaveCenter.TryGetValue(groupIndex, out value);
			Vector3 originPosition = value + offset;
			AddProjectile(data, environmentCreature, 1, originPosition, Vector3.zero, 0, null);
		}
	}

	public void AddProjectile(SkillProjectileData projectileData, BattleCreature firedCreature, int skillLevel, Vector3 originPosition, Vector3 direction, int offsetMm, BattleCreature followTarget, int radiusMm = 0)
	{
		if (firedCreature == null || projectileData == null)
		{
			return;
		}
		int skillIndex = projectileData.SkillIndex;
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (data != null)
		{
			long num = AllocProjectileUid();
			Vector3 position = originPosition;
			if (offsetMm != 0)
			{
				position += firedCreature.TransformController.Direction * offsetMm / 1000L;
			}
			SkillProjectileInfo skillProjectileInfo = SnapshotObjectPool<SkillProjectileInfo>.Alloc();
			skillProjectileInfo.Init();
			skillProjectileInfo.Uid = num;
			skillProjectileInfo.FiredCreature = firedCreature;
			skillProjectileInfo.SkillData = data;
			skillProjectileInfo.ProjectileData = projectileData;
			skillProjectileInfo.SkillLevel = skillLevel;
			skillProjectileInfo.Direction = (direction.approximatelyZero ? direction : direction.normalized);
			skillProjectileInfo.Position = position;
			skillProjectileInfo.FollowTarget = followTarget;
			skillProjectileInfo.ElapsedTimeMs = 0L;
			skillProjectileInfo.SetRadiusMm = radiusMm;
			InitRemainTickIntervalTimeMs(skillProjectileInfo);
			ProjectileInfoList.Add(skillProjectileInfo);
			SpawnProjectile spawnProjectile = new SpawnProjectile();
			spawnProjectile.ProjectileUid = num;
			spawnProjectile.CreatureUid = firedCreature.Uid;
			spawnProjectile.SkillIndex = skillIndex;
			spawnProjectile.ProjectileIndex = projectileData.Index;
			spawnProjectile.FollowTargetUid = followTarget?.Uid ?? 0;
			spawnProjectile.Position = position;
			spawnProjectile.Direction = direction;
			_battleInstance.RaiseBattleEvent(spawnProjectile);
		}
	}

	private void InitRemainTickIntervalTimeMs(SkillProjectileInfo projectileInfo)
	{
		if (projectileInfo == null)
		{
			return;
		}
		SkillProjectileData projectileData = projectileInfo.ProjectileData;
		if (projectileData.TickIntervalMs != null && projectileData.TickIntervalMs.Length != 0)
		{
			int num = projectileData.TickIntervalMs[0];
			if (projectileData.TickIntervalMs.Length >= 2)
			{
				int max = projectileData.TickIntervalMs[1];
				num = projectileInfo.FiredCreature.NextRandom(num, max, "RandomProjectileInterval");
			}
			long currentTimeMs = _battleInstance.GlobalTimer.GetCurrentTimeMs();
			projectileInfo.NextTickTimeMs = currentTimeMs + num;
		}
	}

	private bool IsValidBattlePositionX(int groupIndex, long mx, int radiusMm)
	{
		ExtendArea battleAreaByGroupIndex = _battleInstance.BattleArea.GetBattleAreaByGroupIndex(groupIndex);
		if (battleAreaByGroupIndex == null)
		{
			return false;
		}
		if (mx >= battleAreaByGroupIndex.MinMx - radiusMm)
		{
			return mx <= battleAreaByGroupIndex.MaxMx + radiusMm;
		}
		return false;
	}

	public void Update(long currentGlobalTimeMs, int deltaTimeMs)
	{
		List<SkillProjectileInfo> list = new List<SkillProjectileInfo>();
		foreach (SkillProjectileInfo item in (IEnumerable<SkillProjectileInfo>)ProjectileInfoList)
		{
			if (!UpdateProjectile(currentGlobalTimeMs, deltaTimeMs, item))
			{
				list.Add(item);
			}
		}
		foreach (SkillProjectileInfo item2 in list)
		{
			DestroyProjectile(item2);
		}
	}

	private bool UpdateProjectile(long currentGlobalTimeMs, int deltaTimeMs, SkillProjectileInfo projectileInfo)
	{
		if (projectileInfo == null)
		{
			return false;
		}
		BattleCreature firedCreature = projectileInfo.FiredCreature;
		SkillData skillData = projectileInfo.SkillData;
		int skillLevel = projectileInfo.SkillLevel;
		SkillProjectileData projectileData = projectileInfo.ProjectileData;
		BattleCreature followTarget = projectileInfo.FollowTarget;
		int groupIndex = projectileInfo.FiredCreature.CreateInfo.GroupIndex;
		int num = projectileData.RadiusMm;
		if (projectileInfo.SetRadiusMm > 0 && projectileInfo.SetRadiusMm < num)
		{
			num = projectileInfo.SetRadiusMm;
		}
		projectileInfo.ElapsedTimeMs += deltaTimeMs;
		if (projectileData.SpeedMms != 0)
		{
			if (followTarget != null)
			{
				Vector3 vector = followTarget.TransformController.Position - projectileInfo.Position;
				projectileInfo.Direction = (vector.approximatelyZero ? vector : vector.normalized);
				groupIndex = followTarget.CreateInfo.GroupIndex;
			}
			Vector3 vector2 = projectileInfo.Direction * projectileData.SpeedMms / 1000L * deltaTimeMs / 1000L;
			Vector3 position = projectileInfo.Position + vector2;
			projectileInfo.Position = position;
		}
		Vector3 position2 = projectileInfo.Position;
		if (!IsValidBattlePositionX(groupIndex, position2.mx, num))
		{
			return false;
		}
		if (projectileData.DurationMs > 0 && projectileInfo.ElapsedTimeMs >= projectileData.DurationMs)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = projectileData.TickIntervalMs != null && projectileData.TickIntervalMs.Length != 0;
		if (flag2)
		{
			flag = currentGlobalTimeMs >= projectileInfo.NextTickTimeMs;
			if (flag)
			{
				InitRemainTickIntervalTimeMs(projectileInfo);
			}
		}
		bool flag3 = skillData.GetSkillTargetType(firedCreature).IsFriendly();
		for (int i = 0; i < _battleInstance.CreatureContainer.GetCreatureCount(); i++)
		{
			BattleCreature creatureByIndex = _battleInstance.CreatureContainer.GetCreatureByIndex(i);
			if (creatureByIndex.TeamController.IsFriendly(firedCreature.TeamController) != flag3)
			{
				continue;
			}
			bool flag4 = projectileInfo.TouchedCreatureList.Contains(creatureByIndex);
			if (num > 0 && !BattleCreatureContainer.IsInRadius(creatureByIndex, position2, num))
			{
				if (flag4)
				{
					projectileInfo.TouchedCreatureList.Remove(creatureByIndex);
					CancelProjectileSkill(firedCreature, creatureByIndex, skillData);
				}
				continue;
			}
			if (projectileData.Width > 0 && !BattleCreatureContainer.IsInWidth(creatureByIndex, position2, projectileInfo.Direction, projectileData.Width))
			{
				if (flag4)
				{
					projectileInfo.TouchedCreatureList.Remove(creatureByIndex);
					CancelProjectileSkill(firedCreature, creatureByIndex, skillData);
				}
				continue;
			}
			bool flag5 = false;
			if (flag2)
			{
				if (flag)
				{
					flag5 = true;
				}
			}
			else if (!flag4)
			{
				projectileInfo.TouchedCreatureList.Add(creatureByIndex);
				flag5 = true;
			}
			if (flag5)
			{
				firedCreature.SkillController.FireProjectileSkill(skillData, skillLevel, new BattleCreature[1] { creatureByIndex }, position2);
			}
		}
		return true;
	}

	private void DestroyProjectile(SkillProjectileInfo projectileInfo)
	{
		if (projectileInfo == null || !ProjectileInfoList.Remove(projectileInfo))
		{
			return;
		}
		foreach (BattleCreature item in (IEnumerable<BattleCreature>)projectileInfo.TouchedCreatureList)
		{
			CancelProjectileSkill(projectileInfo.FiredCreature, item, projectileInfo.SkillData);
		}
		DestroyProjectile destroyProjectile = new DestroyProjectile();
		destroyProjectile.ProjectileUid = projectileInfo.Uid;
		_battleInstance.RaiseBattleEvent(destroyProjectile);
	}

	private void CancelProjectileSkill(BattleCreature firedCreature, BattleCreature targetCreature, SkillData skillData)
	{
		if (firedCreature != null && targetCreature != null && skillData != null)
		{
			int index = skillData.Index;
			firedCreature.CancelMyOperation(targetCreature, passiveOnly: false, index);
		}
	}
}
