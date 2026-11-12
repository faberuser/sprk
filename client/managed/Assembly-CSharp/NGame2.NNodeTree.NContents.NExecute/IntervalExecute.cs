using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;

namespace NGame2.NNodeTree.NContents.NExecute;

public class IntervalExecute : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase
	{
		public float NextExecuteTime;

		public int ExecuteCount;

		public bool IsEnd;
	}

	public static string ContentsTypeName = "Execute/" + typeof(IntervalExecute).Name;

	public Callee CalleeStart;

	public Caller CallerInterval;

	public ArgumentSingleGeneric<float> IntervalTime;

	public int ExecuteCount = 1;

	public IntervalExecute(NodeData node)
		: base(node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		CallerInterval = new Caller(this, "CallerInterval");
		IntervalTime = new ArgumentSingleGeneric<float>(1f);
	}

	public override void Wakeup(IVariable variable)
	{
		IntervalTime.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.NextExecuteTime = runtime.LocalTimer.time;
		localItem.ExecuteCount = 0;
		runtime.ExecuteContainer.Pending(this, localItem);
	}

	private void ActionEnd(LocalItem localItem)
	{
		localItem.IsEnd = true;
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem { IsEnd: false } localItem))
		{
			return;
		}
		for (int i = 0; i < 100; i++)
		{
			if (localItem.NextExecuteTime > localItem.localTimer.time)
			{
				break;
			}
			if (localItem.ExecuteCount >= ExecuteCount)
			{
				break;
			}
			CallerInterval.Call(localItem.Runtime, localItem.Variable);
			localItem.NextExecuteTime += IntervalTime.GetValue(localItem.Variable);
			localItem.ExecuteCount++;
		}
		if (localItem.ExecuteCount >= ExecuteCount)
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
		CallerInterval.LOADFROMJSON(json["CallerInterval"]);
		IntervalTime.LoadFromJson(json["IntervalTime"]);
		ExecuteCount = json["ExecuteCount"].GETINT(ExecuteCount);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerInterval", CallerInterval);
		jsonData.AddField("IntervalTime", IntervalTime.MakeJson());
		jsonData.AddFieldLong("ExecuteCount", ExecuteCount);
		return jsonData;
	}
}
