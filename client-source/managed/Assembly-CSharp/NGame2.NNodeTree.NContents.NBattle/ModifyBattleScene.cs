using System;
using LitJson;
using NGame2.NBattleField;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NShared;
using NShared.NCampaign.NCampaignState;
using NVespa;
using NVespa.NUtil;

namespace NGame2.NNodeTree.NContents.NBattle;

public class ModifyBattleScene : BattleBase, IKey, IKeyFinalize, IExecute
{
	public enum EModifyType
	{
		MakeAllCreatureDummy,
		ClearAllCreatureDummy,
		MakeCreatureDummy,
		ClearCreatureDummy,
		DisableSyncPosition,
		EnableSyncPosition,
		CancelAllSkill,
		CancelSkill,
		DestroyAllCreature,
		DisableWinDecider,
		EnableWinDecider,
		DisableGiveDamage,
		EnableGiveDamage,
		AllResistCcOn,
		AllResistCcOff,
		MakeAllNoDie,
		MakeTeamNoDie,
		MakeCreatureNoDie,
		ClearAllNoDie,
		DecideWinner
	}

	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public bool IsProcessed;
	}

	public static string ContentsTypeName = "Battle/" + typeof(ModifyBattleScene).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public EModifyType ModifyType;

	public ArgumentSingleGeneric<long> CreatureUid;

	public TeamType TeamType;

	public bool RestoreOnEnd;

	public ModifyBattleScene(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		CreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			localItem.IsProcessed = true;
		}
		else
		{
			ProcessModify(ModifyType, variable, delegate
			{
				localItem.IsProcessed = true;
			});
		}
		runtime.ExecuteContainer.Pending(this, localItem);
	}

	private void ProcessModify(EModifyType modifyType, IVariable variable, Action onModify)
	{
		Executer globalExecuter = SingletonUpdater<Executer>.instance;
		if (modifyType == EModifyType.MakeAllCreatureDummy)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						battleInstance.CreatureContainer.GetCreatureByIndex(i).CreateInfo.IsDummy = true;
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.ClearAllCreatureDummy)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						battleInstance.CreatureContainer.GetCreatureByIndex(i).CreateInfo.IsDummy = false;
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.MakeCreatureDummy)
		{
			int creatureUid = (int)CreatureUid.GetValue(variable);
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						BattleCreature creatureByIndex = battleInstance.CreatureContainer.GetCreatureByIndex(i);
						if (creatureByIndex.Uid == creatureUid)
						{
							creatureByIndex.CreateInfo.IsDummy = true;
						}
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.ClearCreatureDummy)
		{
			int creatureUid2 = (int)CreatureUid.GetValue(variable);
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						BattleCreature creatureByIndex = battleInstance.CreatureContainer.GetCreatureByIndex(i);
						if (creatureByIndex.Uid == creatureUid2)
						{
							creatureByIndex.CreateInfo.IsDummy = false;
						}
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.DisableSyncPosition)
		{
			ISceneContainer sceneContainer = variable.GetRuntimeInstance().SCENECONTAINER();
			if (sceneContainer != null)
			{
				for (int num = 0; num < sceneContainer.GetCreatureCount(); num++)
				{
					sceneContainer.GetCreatureByIndex(num).NodeTreeController.SyncPosition.DisableLock++;
				}
			}
			Vespa_System_Action_Extension_Method.fire(onModify);
		}
		else if (modifyType == EModifyType.EnableSyncPosition)
		{
			ISceneContainer sceneContainer2 = variable.GetRuntimeInstance().SCENECONTAINER();
			if (sceneContainer2 != null)
			{
				for (int num2 = 0; num2 < sceneContainer2.GetCreatureCount(); num2++)
				{
					sceneContainer2.GetCreatureByIndex(num2).NodeTreeController.SyncPosition.DisableLock--;
				}
			}
			Vespa_System_Action_Extension_Method.fire(onModify);
		}
		else if (modifyType == EModifyType.CancelAllSkill)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						BattleCreature creatureByIndex = battleInstance.CreatureContainer.GetCreatureByIndex(i);
						creatureByIndex.OperationController.Clear(clearPassive: false);
						creatureByIndex.StateController.Clear();
						creatureByIndex.SkillController.StopUsingDurationalSkill(isCancel: true);
						creatureByIndex.SkillController.CancelReservedSkill();
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.CancelSkill)
		{
			int creatureUid3 = (int)CreatureUid.GetValue(variable);
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						BattleCreature creatureByIndex = battleInstance.CreatureContainer.GetCreatureByIndex(i);
						if (creatureByIndex.Uid == creatureUid3)
						{
							creatureByIndex.OperationController.Clear(clearPassive: false);
							creatureByIndex.StateController.Clear();
							creatureByIndex.SkillController.StopUsingDurationalSkill(isCancel: true);
							creatureByIndex.SkillController.CancelReservedSkill();
						}
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.DestroyAllCreature)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						BattleCreature creatureByIndex = battleInstance.CreatureContainer.GetCreatureByIndex(0);
						if (creatureByIndex == null)
						{
							break;
						}
						battleInstance.CreatureContainer.Destroy(creatureByIndex.Uid, raiseEvent: true);
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.DisableWinDecider || modifyType == EModifyType.EnableWinDecider)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate
			{
				WavePlaying.DisableWinDecider = modifyType == EModifyType.DisableWinDecider;
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.DisableGiveDamage || modifyType == EModifyType.EnableGiveDamage)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				battleInstance.DisableGiveDamage = modifyType == EModifyType.DisableGiveDamage;
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.AllResistCcOn || modifyType == EModifyType.AllResistCcOff)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				battleInstance.AllResistCc = modifyType == EModifyType.AllResistCcOn;
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.MakeAllNoDie)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						battleInstance.CreatureContainer.GetCreatureByIndex(i).CreateInfo.ForceNoDie = true;
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.MakeTeamNoDie)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						BattleCreature creatureByIndex = battleInstance.CreatureContainer.GetCreatureByIndex(i);
						if (creatureByIndex.TeamController.TeamType == TeamType)
						{
							creatureByIndex.CreateInfo.ForceNoDie = true;
						}
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.MakeCreatureNoDie)
		{
			int creatureUid4 = (int)CreatureUid.GetValue(variable);
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						BattleCreature creatureByIndex = battleInstance.CreatureContainer.GetCreatureByIndex(i);
						if (creatureByIndex.Uid == creatureUid4)
						{
							creatureByIndex.CreateInfo.ForceNoDie = true;
						}
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.ClearAllNoDie)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				using (new SnapshotWritableBlock())
				{
					int creatureCount = battleInstance.CreatureContainer.GetCreatureCount();
					for (int i = 0; i < creatureCount; i++)
					{
						battleInstance.CreatureContainer.GetCreatureByIndex(i).CreateInfo.ForceNoDie = false;
					}
				}
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else if (modifyType == EModifyType.DecideWinner)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate
			{
				WavePlaying.ForceDecideWinner = modifyType == EModifyType.DecideWinner;
				WavePlaying.WinnerTeamType = TeamType;
				globalExecuter.NextRun(this, delegate
				{
					Vespa_System_Action_Extension_Method.fire(onModify);
				});
			});
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onModify);
		}
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			CallerEnd.Call(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false, IsProcessed: not false } localItem)
		{
			ActionEnd(localItem);
		}
	}

	bool IExecute.IsExecuteEnd(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem localItem))
		{
			return true;
		}
		return localItem.IsEnd;
	}

	void IExecute.RequestStopExecute(ItemBase itemBase, ERequestStop requestStop)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem);
		}
	}

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
		if (itemBase is LocalItem localItem && RestoreOnEnd)
		{
			if (ModifyType == EModifyType.MakeAllCreatureDummy)
			{
				ProcessModify(EModifyType.ClearAllCreatureDummy, localItem.Variable, null);
			}
			else if (ModifyType == EModifyType.MakeCreatureDummy)
			{
				ProcessModify(EModifyType.ClearCreatureDummy, localItem.Variable, null);
			}
			else if (ModifyType == EModifyType.DisableWinDecider)
			{
				ProcessModify(EModifyType.EnableWinDecider, localItem.Variable, null);
			}
			else if (ModifyType == EModifyType.DisableGiveDamage)
			{
				ProcessModify(EModifyType.EnableGiveDamage, localItem.Variable, null);
			}
			else if (ModifyType == EModifyType.AllResistCcOn)
			{
				ProcessModify(EModifyType.AllResistCcOff, localItem.Variable, null);
			}
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		ModifyType = json["ModifyType"].GETENUM(ModifyType);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		TeamType = json["TeamType"].GETENUM(TeamType);
		RestoreOnEnd = json["RestoreOnEnd"].GETBOOLEAN(RestoreOnEnd);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddFieldEnum("ModifyType", ModifyType);
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddFieldEnum("TeamType", TeamType);
		jsonData.AddFieldBool("RestoreOnEnd", RestoreOnEnd);
		return jsonData;
	}
}
