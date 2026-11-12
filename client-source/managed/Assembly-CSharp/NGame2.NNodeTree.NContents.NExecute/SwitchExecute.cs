using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public abstract class SwitchExecute<T> : ExecuteBase, IKey
{
	public class IndexedCaller : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public int IndexNo;

		public ContentsBase Contents;

		public T Value;

		public Caller Caller;

		public IndexedCaller(ContentsBase contents, int indexNo)
		{
			Contents = contents;
			IndexNo = indexNo;
		}

		bool ILoadJson.Load(JsonData json)
		{
			IndexNo = json["IndexNo"].GETINT(IndexNo);
			Caller = new Caller(Contents, "Caller" + IndexNo);
			Caller.LOADFROMJSON(json["Caller"]);
			ILoadFunctor<T> loadFunctorIfExist = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T>();
			if (loadFunctorIfExist == null)
			{
				VespaDebug.LogError("LoadFunctor not exist: " + typeof(T).Name);
				return false;
			}
			Value = loadFunctorIfExist.CreateFromJson(json["Value"]);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldLong("IndexNo", IndexNo);
			jsonData.AddFieldJsonMake("Caller", Caller);
			IMakeFunctor<T> makeFunctor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T>();
			if (makeFunctor == null)
			{
				VespaDebug.LogError("MakeFunctor not exist: " + typeof(T).Name);
				return null;
			}
			jsonData.AddField("Value", makeFunctor.MakeJson(Value));
			return jsonData;
		}
	}

	public Callee CalleeStart;

	public ArgumentSingleGeneric<T> Condition;

	public List<IndexedCaller> CallerList = new List<IndexedCaller>();

	public Caller CallerDefault;

	public SwitchExecute(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		Condition = new ArgumentSingleGeneric<T>(default(T));
		CallerDefault = new Caller(this, "CallerDefault");
	}

	public override void Wakeup(IVariable variable)
	{
		Condition.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		T value = Condition.GetValue(variable, default(T));
		IEqualityComparer<T> equalityComparer = SingletonAuto<EqualityComparerCache>.instance.Get<T>();
		if (equalityComparer == null)
		{
			VespaDebug.LogError("Comparer not found: " + typeof(T).Name);
			return;
		}
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(CallerList); i++)
		{
			IndexedCaller indexedCaller = CallerList[i];
			if (indexedCaller != null && equalityComparer.Equals(indexedCaller.Value, value))
			{
				indexedCaller.Caller.Call(runtime, variable);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			CallerDefault.Call(runtime, variable);
		}
	}

	internal override string GetContentsTypeName()
	{
		return "Execute/" + GetType().Name;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		Condition.LoadFromJson(json["Condition"]);
		CallerList.LoadFromJson(json["CallerList"], () => new IndexedCaller(this, 0));
		CallerDefault.LOADFROMJSON(json["CallerDefault"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddField("Condition", Condition.MakeJson());
		jsonData.AddFieldList("CallerList", CallerList);
		jsonData.AddFieldJsonMake("CallerDefault", CallerDefault);
		return jsonData;
	}
}
