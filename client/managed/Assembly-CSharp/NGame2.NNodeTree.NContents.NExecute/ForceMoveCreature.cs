using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NExecute;

public class ForceMoveCreature : ExecuteBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public float BeginTime;

		public int Uid;

		public bool IsEnd;
	}

	public static string ContentsTypeName = "Execute/" + typeof(ForceMoveCreature).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public bool ApplyPosition;

	public ArgumentSingleGeneric<Vector3> Position;

	public bool ApplyDirection;

	public bool BlendDirection;

	public ArgumentSingleGeneric<Vector3> Direction;

	public DefineVariable<float> DefineFrame = new DefineVariable<float>();

	public ForceMoveCreature(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		CreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
		Position = new ArgumentSingleGeneric<Vector3>(VariableKeyword.Destination);
		Direction = new ArgumentSingleGeneric<Vector3>(VariableKeyword.Direction);
	}

	public override void Wakeup(IVariable variable)
	{
		CreatureUid.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = localItem.localTimer.time;
		localItem.Uid = (int)CreatureUid.GetValue(variable, 0L);
		DefineFrame.SetFunc(localItem, () => Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable))));
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem { IsEnd: false } localItem))
		{
			return;
		}
		float num = localItem.localTimer.time - localItem.BeginTime;
		float value = DurationTime.GetValue(localItem.Variable);
		Creature battleCreature = localItem.GetBattleCreature(localItem.Uid);
		if (battleCreature != null)
		{
			if (ApplyPosition)
			{
				battleCreature.SetBattlePos(Position.GetValue(localItem.Variable));
			}
			if (ApplyDirection)
			{
				Vector3 value2 = Direction.GetValue(localItem.Variable);
				battleCreature.RenderFacade.Action.SetDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(value2), BlendDirection);
			}
		}
		if (num >= value || StopThis.GetValue(localItem.Variable, defaultValue: false))
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
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		ApplyPosition = json["ApplyPosition"].GETBOOLEAN(ApplyPosition);
		Position.LoadFromJson(json["Position"]);
		ApplyDirection = json["ApplyDirection"].GETBOOLEAN(ApplyDirection);
		BlendDirection = json["BlendDirection"].GETBOOLEAN(BlendDirection);
		Direction.LoadFromJson(json["Direction"]);
		DefineFrame.LOADFROMJSON(json["DefineFrame"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldBool("ApplyPosition", ApplyPosition);
		jsonData.AddField("Position", Position.MakeJson());
		jsonData.AddFieldBool("ApplyDirection", ApplyDirection);
		jsonData.AddFieldBool("BlendDirection", BlendDirection);
		jsonData.AddField("Direction", Direction.MakeJson());
		jsonData.AddFieldJsonMake("DefineFrame", DefineFrame);
		return jsonData;
	}
}
