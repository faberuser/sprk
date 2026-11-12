using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class SwitchKey : ExecuteBase, IKey
{
	public class IndexedCaller : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public int IndexNo;

		public ContentsBase Contents;

		public string Value;

		public Caller Caller;

		public IndexedCaller(ContentsBase contents, int indexNo)
		{
			Contents = contents;
			IndexNo = indexNo;
			Value = string.Empty;
		}

		bool ILoadJson.Load(JsonData json)
		{
			IndexNo = json["IndexNo"].GETINT(IndexNo);
			Caller = new Caller(Contents, "Caller" + IndexNo);
			Caller.LOADFROMJSON(json["Caller"]);
			Value = json["Key"].GETSTRING(Value);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldLong("IndexNo", IndexNo);
			jsonData.AddFieldJsonMake("Caller", Caller);
			jsonData.AddFieldString("Key", Value);
			return jsonData;
		}
	}

	public class VariableInfo : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public string Key = string.Empty;

		bool ILoadJson.Load(JsonData json)
		{
			Key = json["Key"].GETSTRING(Key);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("Key", Key);
			return jsonData;
		}
	}

	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public Creature CachedCreature;

		public Dictionary<string, int> Dic = new Dictionary<string, int>();

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(SwitchKey).Name;

	public Callee CalleeStart;

	public ArgumentSingleGeneric<long> KeyReadUid;

	public List<VariableInfo> VariableList = new List<VariableInfo>();

	public List<IndexedCaller> CallerList = new List<IndexedCaller>();

	public Caller CallerDefault;

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public SwitchKey(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		KeyReadUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
		CallerDefault = new Caller(this, "CallerDefault");
	}

	public override void Wakeup(IVariable variable)
	{
		KeyReadUid.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		Dictionary<string, int> dictionary = null;
		if (KeyReadUid.TryGetValue(variable, out var value))
		{
			Creature battleCreature = runtime.SceneContainer.GetBattleCreature((int)value);
			if (battleCreature != null)
			{
				localItem.CachedCreature = battleCreature;
				dictionary = battleCreature.KeyStackDic;
			}
		}
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(CallerList); i++)
		{
			IndexedCaller indexedCaller = CallerList[i];
			if (indexedCaller != null && dictionary.ContainsKey(indexedCaller.Value) && dictionary[indexedCaller.Value] > 0)
			{
				indexedCaller.Caller.Call(runtime, localItem.Variable);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			CallerDefault.Call(runtime, localItem.Variable);
		}
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		KeyReadUid.LoadFromJson(json["KeyReadUid"]);
		CallerList.LoadFromJson(json["CallerList"], () => new IndexedCaller(this, 0));
		CallerDefault.LOADFROMJSON(json["CallerDefault"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddField("KeyReadUid", KeyReadUid.MakeJson());
		jsonData.AddFieldList("CallerList", CallerList);
		jsonData.AddFieldJsonMake("CallerDefault", CallerDefault);
		return jsonData;
	}
}
