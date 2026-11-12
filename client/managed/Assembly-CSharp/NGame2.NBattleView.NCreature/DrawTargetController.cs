using NGame2.NBattleField;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NGameAsset;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleView.NCreature;

public class DrawTargetController
{
	public class TargetRuntime
	{
		public int TargetCreatureUid;

		public IRuntime Runtime;
	}

	private PackLoad _packLoad;

	private AsyncManager.SingleEntry _asyncEntry;

	private TreeData _baseSkillTargetTree;

	private TreeData _skillManualTargetTree;

	private TargetRuntime _baseSkillTargetRuntime = new TargetRuntime();

	private TargetRuntime _skillManualTargetRuntime = new TargetRuntime();

	public Creature Owner { get; private set; }

	public DrawTargetController(Creature owner)
	{
		Owner = owner;
		_packLoad = SingletonAuto<PackContainer>.instance.GetPack("Common/Auto");
		if (_packLoad.IsDone)
		{
			CompletePackLoad();
		}
	}

	private void CompletePackLoad()
	{
		if (_packLoad.IsDone)
		{
			if (!_packLoad.ISVALID())
			{
				_packLoad = null;
				return;
			}
			_baseSkillTargetTree = _packLoad.Value.GetNodeTree("BaseSkillTarget");
			_skillManualTargetTree = _packLoad.Value.GetNodeTree("SkillManualTarget");
			_packLoad = null;
		}
	}

	public void Close()
	{
		ResetAllTargetRuntime();
	}

	private void ResetAllTargetRuntime()
	{
		RefreshTargetRuntime(null, 0, _baseSkillTargetRuntime);
		RefreshTargetRuntime(null, 0, _skillManualTargetRuntime);
	}

	public void ChainUpdate()
	{
		if (_packLoad != null)
		{
			CompletePackLoad();
		}
		UpdateTarget();
	}

	private void UpdateTarget()
	{
		BattleFacade.CreatureCacheData creatureCache = Owner.CreatureCache;
		if (!SingletonAuto<BattleFacade>.instance.IsBattleEnabled() || !creatureCache.Player.IsMe || Owner.IsDead)
		{
			ResetAllTargetRuntime();
			return;
		}
		if (creatureCache.ManualTargetUid != _baseSkillTargetRuntime.TargetCreatureUid)
		{
			RefreshTargetRuntime(_baseSkillTargetTree, creatureCache.ManualTargetUid, _baseSkillTargetRuntime);
		}
		if (creatureCache.ReservedSkillManualTargetUid != _skillManualTargetRuntime.TargetCreatureUid)
		{
			RefreshTargetRuntime(_skillManualTargetTree, creatureCache.ReservedSkillManualTargetUid, _skillManualTargetRuntime);
		}
	}

	private void RefreshTargetRuntime(TreeData treeData, int targetCreatureUid, TargetRuntime targetRuntime)
	{
		if (targetRuntime.Runtime != null)
		{
			targetRuntime.Runtime.RequestStop(ERequestStop.Force);
			targetRuntime.Runtime.ReleaseSelf();
			targetRuntime.Runtime = null;
		}
		targetRuntime.TargetCreatureUid = targetCreatureUid;
		if (treeData != null && targetCreatureUid != 0)
		{
			BuildChildRootRuntime buildChildRootRuntime = default(BuildChildRootRuntime);
			if (buildChildRootRuntime.Open(treeData))
			{
				buildChildRootRuntime.AddValue(VariableKeyword.Myself, (long)Owner.Uid);
				buildChildRootRuntime.AddValue(VariableKeyword.Target, (long)targetCreatureUid);
				targetRuntime.Runtime = buildChildRootRuntime.Build(null, Owner.ActionLocalTimer, null, Owner.gameObject.layer);
				SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(targetRuntime.Runtime.CaptureSelf(), immediatelyStart: false);
			}
		}
	}
}
