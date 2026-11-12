using LitJson;
using NGame2.NBattleField;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa;

namespace NGame2.NNodeTree.NContents.NBattle;

public class FireEventSkill : BattleBase, IKey, IExecute
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public bool IsProcessed;
	}

	public static string ContentsTypeName = "Battle/" + typeof(FireEventSkill).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> FireCreatureUid;

	public ArgumentSingleGeneric<long> TargetCreatureUid;

	public int FireSkillIndex;

	public FireEventSkill(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		FireCreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
		TargetCreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
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
			int creatureUid = (int)FireCreatureUid.GetValue(variable);
			int manualTargetUid = (int)TargetCreatureUid.GetValue(variable);
			SingletonAuto<BattleFacade>.instance.FireEventSkillBattleLog(creatureUid, FireSkillIndex, manualTargetUid, delegate
			{
				localItem.IsProcessed = true;
			});
		}
		runtime.ExecuteContainer.Pending(this, localItem);
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

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		FireCreatureUid.LoadFromJson(json["FireCreatureUid"]);
		TargetCreatureUid.LoadFromJson(json["TargetCreatureUid"]);
		FireSkillIndex = json["FireSkillIndex"].GETINT(FireSkillIndex);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("FireCreatureUid", FireCreatureUid.MakeJson());
		jsonData.AddField("TargetCreatureUid", TargetCreatureUid.MakeJson());
		jsonData.AddFieldLong("FireSkillIndex", FireSkillIndex);
		return jsonData;
	}
}
