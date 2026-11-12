using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;

namespace NGame2.NNodeTree.NContents.NExecute;

public class Delay : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase
	{
		public float BeginTime;

		public bool IsEnd;
	}

	public static string ContentsTypeName = "Execute/" + typeof(Delay).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<float> DelayTime;

	public Delay(NodeData node)
		: base(node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		CallerEnd = new Caller(this, "CallerEnd");
		DelayTime = new ArgumentSingleGeneric<float>(1f);
	}

	public override void Wakeup(IVariable variable)
	{
		DelayTime.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		runtime.ExecuteContainer.Pending(this, localItem);
	}

	private void ActionEnd(LocalItem localItem)
	{
		localItem.IsEnd = true;
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem && localItem.localTimer.time - localItem.BeginTime >= DelayTime.GetValue(localItem.Variable))
		{
			CallerEnd.Call(localItem.Runtime, localItem.Variable);
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
		CallerEnd.LOADFROMJSON(json["CallerInterval"]);
		DelayTime.LoadFromJson(json["DelayTime"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerInterval", CallerEnd);
		jsonData.AddField("DelayTime", DelayTime.MakeJson());
		return jsonData;
	}
}
