using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;

namespace NGame2.NNodeTree.NContents.NExecute;

public class ReadFromExternal : ExecuteBase, IKey
{
	public class VariableInfo : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public StringKey NameForLocal = StringKey.Empty;

		public StringKey NameForExternal = StringKey.Empty;

		bool ILoadJson.Load(JsonData json)
		{
			NameForLocal = StringKey.CreateFromJson(json["NameForLocal"]);
			NameForExternal = StringKey.CreateFromJson(json["NameForExternal"]);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddField("NameForLocal", NameForLocal.MakeJson());
			jsonData.AddField("NameForExternal", NameForExternal.MakeJson());
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(ReadFromExternal).Name;

	public Callee CalleeStart;

	public Caller CallerNext;

	public ArgumentSingleGeneric<long> ReadFromUid;

	public List<VariableInfo> VariableList = new List<VariableInfo>();

	public ReadFromExternal(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		CallerNext = new Caller(this, "CallerNext");
		ReadFromUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
	}

	public override void Wakeup(IVariable variable)
	{
		ReadFromUid.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		ItemBase itemBase = runtime.ItemContainer.CreateItem<ItemBase>(this, variable);
		CreatureVariable creatureVariable = null;
		if (ReadFromUid.TryGetValue(variable, out var value))
		{
			Creature battleCreature = runtime.SceneContainer.GetBattleCreature((int)value);
			if (battleCreature != null)
			{
				creatureVariable = battleCreature.Variable;
			}
		}
		if (creatureVariable != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(VariableList); i++)
			{
				VariableInfo variableInfo = VariableList[i];
				if (variableInfo != null && !variableInfo.NameForLocal.IsNullOrEmpty())
				{
					IVariableValueBase valueBase = creatureVariable.GetValueBase(variableInfo.NameForExternal);
					if (valueBase != null)
					{
						itemBase.Variable.AddVariableValue(variableInfo.NameForLocal, valueBase);
					}
				}
			}
		}
		CallerNext.Call(runtime, itemBase.Variable);
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerNext.LOADFROMJSON(json["CallerNext"]);
		ReadFromUid.LoadFromJson(json["ReadFromUid"]);
		JsonTools.ToList(json["VariableList"], VariableList);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerNext", CallerNext);
		jsonData.AddField("ReadFromUid", ReadFromUid.MakeJson());
		jsonData.AddFieldList("VariableList", VariableList);
		return jsonData;
	}
}
