using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class MergeExecute : ExecuteBase, IKey, IKeyShared
{
	public class IndexedCallee : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public ContentsBase Contents;

		public int IndexNo;

		public Callee Callee;

		public Action<IndexedCallee, RuntimeInstance, IVariable> CalledAction;

		public IndexedCallee(ContentsBase contents, Action<IndexedCallee, RuntimeInstance, IVariable> calledAction)
		{
			Contents = contents;
			CalledAction = calledAction;
		}

		public IndexedCallee(ContentsBase contents, int indexNo, Action<IndexedCallee, RuntimeInstance, IVariable> calledAction)
		{
			Contents = contents;
			IndexNo = indexNo;
			CalledAction = calledAction;
			Callee = new Callee(Contents, "Callee" + IndexNo, delegate(RuntimeInstance runtimeInstance, IVariable variable)
			{
				Vespa_System_Action_Extension_Method.fire(CalledAction, this, runtimeInstance, variable);
			});
		}

		bool ILoadJson.Load(JsonData json)
		{
			IndexNo = json["IndexNo"].GETINT(IndexNo);
			Callee = new Callee(Contents, "Callee" + IndexNo, delegate(RuntimeInstance runtimeInstance, IVariable variable)
			{
				Vespa_System_Action_Extension_Method.fire(CalledAction, this, runtimeInstance, variable);
			});
			Callee.LOADFROMJSON(json["Callee"]);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldLong("IndexNo", IndexNo);
			jsonData.AddFieldJsonMake("Callee", Callee);
			return jsonData;
		}
	}

	public class LocalItem : ItemBase
	{
		public class CallCountData
		{
			public IndexedCallee Callee;

			public Queue<IVariable> VariableQueue = new Queue<IVariable>();
		}

		public bool IsEnd;

		public List<CallCountData> CallCountList = new List<CallCountData>();
	}

	public enum EMergeType
	{
		And,
		Or
	}

	public static string ContentsTypeName = "Execute/" + typeof(MergeExecute).Name;

	public EMergeType MergeType;

	public List<IndexedCallee> CalleeList = new List<IndexedCallee>();

	public Caller Caller;

	public MergeExecute(NodeData i_node)
		: base(i_node)
	{
		Caller = new Caller(this, "Caller");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	public void ActionStart(IndexedCallee indexedCallee, RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.GetSharedItem<LocalItem>(this);
		if (localItem == null)
		{
			localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
			for (int i = 0; i < CalleeList.Count; i++)
			{
				IndexedCallee callee = CalleeList[i];
				LocalItem.CallCountData callCountData = new LocalItem.CallCountData();
				callCountData.Callee = callee;
				localItem.CallCountList.Add(callCountData);
			}
		}
		bool flag = true;
		for (int j = 0; j < localItem.CallCountList.Count; j++)
		{
			LocalItem.CallCountData callCountData2 = localItem.CallCountList[j];
			if (callCountData2.Callee == indexedCallee)
			{
				callCountData2.VariableQueue.Enqueue(variable);
			}
			if (callCountData2.VariableQueue.Count == 0)
			{
				flag = false;
			}
		}
		if (!flag)
		{
			return;
		}
		MergeVariable mergeVariable = SmallPooling<MergeVariable>.CreateInstance();
		Vespa_Common_Class_Extension_Method.Interface((IVariable)mergeVariable).Open(runtime);
		for (int k = 0; k < localItem.CallCountList.Count; k++)
		{
			LocalItem.CallCountData callCountData3 = localItem.CallCountList[k];
			if (callCountData3.VariableQueue.Count != 0)
			{
				IVariable variable2 = callCountData3.VariableQueue.Dequeue();
				mergeVariable.Add(variable2);
			}
		}
		Caller.Call(runtime, mergeVariable);
		Vespa_Common_Class_Extension_Method.Interface((IVariable)mergeVariable).ReleaseSelf();
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		MergeType = json["MergeType"].GETENUM(MergeType);
		CalleeList.LoadFromJson(json["CalleeList"], () => new IndexedCallee(this, ActionStart));
		Caller.LOADFROMJSON(json["Caller"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldEnum("MergeType", MergeType);
		jsonData.AddFieldList("CalleeList", CalleeList);
		jsonData.AddFieldJsonMake("Caller", Caller);
		return jsonData;
	}
}
