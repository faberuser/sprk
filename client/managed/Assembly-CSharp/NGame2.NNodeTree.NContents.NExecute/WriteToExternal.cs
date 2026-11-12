using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class WriteToExternal : ExecuteBase, IKey
{
	public class VariableInfo : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public StringKey Name = StringKey.Empty;

		public float Lifetime = 1f;

		bool ILoadJson.Load(JsonData json)
		{
			Name = StringKey.CreateFromJson(json["Name"]);
			Lifetime = json["Lifetime"].GETFLOAT(Lifetime);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddField("Name", Name.MakeJson());
			jsonData.AddFieldDouble("Lifetime", Lifetime);
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(WriteToExternal).Name;

	public Callee CalleeStart;

	public Caller CallerNext;

	public ArgumentSingleGeneric<long> WriteToUid;

	public ArgumentContainer ArgumentContainer = new ArgumentContainer();

	public Dictionary<ArgumentHolder, VariableInfo> VariableDic = new Dictionary<ArgumentHolder, VariableInfo>();

	public WriteToExternal(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		CallerNext = new Caller(this, "CallerNext");
		WriteToUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
	}

	public override void Wakeup(IVariable variable)
	{
		WriteToUid.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		ItemBase itemBase = runtime.ItemContainer.CreateItem<ItemBase>(this, variable);
		CreatureVariable creatureVariable = null;
		if (WriteToUid.TryGetValue(variable, out var value))
		{
			Creature battleCreature = runtime.SceneContainer.GetBattleCreature((int)value);
			if (battleCreature != null)
			{
				creatureVariable = battleCreature.Variable;
			}
		}
		if (creatureVariable != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ArgumentContainer.HolderList); i++)
			{
				ArgumentHolder argumentHolder = ArgumentContainer.HolderList[i];
				VariableInfo vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(VariableDic, argumentHolder, null);
				if (vALUE != null && !vALUE.Name.IsNullOrEmpty())
				{
					IVariableValueBase valueBase = SingletonAuto<TypeFactory>.instance.CreateVariableValue(argumentHolder.Value, variable, isDynamic: false);
					creatureVariable.AddValueBase(vALUE.Name, valueBase, vALUE.Lifetime);
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
		WriteToUid.LoadFromJson(json["WriteToUid"]);
		ArgumentContainer.LoadFromJson(json["ArgumentContainer"], delegate(ArgumentHolder holder, JsonData jsonValue)
		{
			JsonData jsonData = jsonValue["VariableInfo"];
			if (!jsonData.isNone())
			{
				VariableInfo variableInfo = new VariableInfo();
				variableInfo.LOADFROMJSON(jsonData);
				VariableDic[holder] = variableInfo;
			}
		});
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerNext", CallerNext);
		jsonData.AddField("WriteToUid", WriteToUid.MakeJson());
		JsonData value = ArgumentContainer.MakeJson(delegate(ArgumentHolder holder, JsonData jsonValue)
		{
			VariableInfo vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(VariableDic, holder, null);
			if (vALUE != null)
			{
				jsonValue.AddFieldJsonMake("VariableInfo", vALUE);
			}
		});
		jsonData.AddField("ArgumentContainer", value);
		return jsonData;
	}
}
