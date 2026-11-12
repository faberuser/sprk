using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;

namespace NGame2.NNodeTree.NContents.NExecute;

public class Wait : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;
	}

	public static string ContentsTypeName = "Execute/" + typeof(Wait).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<bool> IsWait;

	public Wait(NodeData node)
		: base(node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		CallerEnd = new Caller(this, "CallerEnd");
		IsWait = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		IsWait.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem i_executeItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		runtime.ExecuteContainer.Pending(this, i_executeItem);
	}

	private void ActionEnd(LocalItem localItem)
	{
		localItem.IsEnd = true;
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem && !IsWait.GetValue(localItem.Variable, defaultValue: false))
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
		IsWait.LoadFromJson(json["IsWait"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerInterval", CallerEnd);
		jsonData.AddField("IsWait", IsWait.MakeJson());
		return jsonData;
	}
}
