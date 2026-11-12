using LitJson;
using NGame2.NBattleView;
using NGame2.NBattleView.NCreature;
using NGame2.NBattleView.NCreature.NHandler;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NBattle;

public class MoveToPosition : BattleBase, IKey, IExecute
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public EventMove EventMove;
	}

	public static string ContentsTypeName = "Battle/" + typeof(MoveToPosition).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<Vector3> Position;

	public ArgumentSingleGeneric<float> ApproachDistance;

	public bool WaitMoveEnd;

	public MoveToPosition(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		CreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
		Position = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
		ApproachDistance = new ArgumentSingleGeneric<float>(1f);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		int uid = (int)CreatureUid.GetValue(variable);
		Vector3 value = Position.GetValue(variable);
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		Creature battleCreature = localItem.GetBattleCreature(uid);
		if (battleCreature == null)
		{
			ActionEnd(localItem);
			return;
		}
		localItem.EventMove = new EventMove(battleCreature, value);
		battleCreature.NodeTreeController.SetExternalHandler(localItem.EventMove);
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
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			if (!WaitMoveEnd)
			{
				ActionEnd(localItem);
			}
			else if (localItem.EventMove == null)
			{
				ActionEnd(localItem);
			}
			else if (Vespa_Common_Class_Extension_Method.Interface((IHandler)localItem.EventMove).IsEnd())
			{
				ActionEnd(localItem);
			}
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
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		Position.LoadFromJson(json["Position"]);
		ApproachDistance.LoadFromJson(json["ApproachDistance"]);
		WaitMoveEnd = json["WaitMoveEnd"].GETBOOLEAN(WaitMoveEnd);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("Position", Position.MakeJson());
		jsonData.AddField("ApproachDistance", ApproachDistance.MakeJson());
		jsonData.AddFieldBool("WaitMoveEnd", WaitMoveEnd);
		return jsonData;
	}
}
