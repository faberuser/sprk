using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;

namespace NGame2.NNodeTree.NContents.NExecute;

public class Calculate : ExecuteBase, IKey
{
	public class VariableInfo : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public StringKey Name = StringKey.Empty;

		public bool IsDynamic;

		public bool IsGlobal;

		bool ILoadJson.Load(JsonData json)
		{
			Name = StringKey.CreateFromJson(json["Name"]);
			IsDynamic = json["IsDynamic"].GETBOOLEAN(IsDynamic);
			IsGlobal = json["IsGlobal"].GETBOOLEAN(IsGlobal);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddField("Name", Name.MakeJson());
			jsonData.AddFieldBool("IsDynamic", IsDynamic);
			jsonData.AddFieldBool("IsGlobal", IsGlobal);
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(Calculate).Name;

	public Callee CalleeStart;

	public Caller CallerNext;

	public ArgumentContainer ArgumentContainer = new ArgumentContainer();

	public Dictionary<ArgumentHolder, VariableInfo> VariableDic = new Dictionary<ArgumentHolder, VariableInfo>();

	public Calculate(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		CallerNext = new Caller(this, "CallerNext");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		ItemBase itemBase = runtime.ItemContainer.CreateItem<ItemBase>(this, variable);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ArgumentContainer.HolderList); i++)
		{
			ArgumentHolder argumentHolder = ArgumentContainer.HolderList[i];
			VariableInfo vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(VariableDic, argumentHolder, null);
			if (vALUE != null && !vALUE.Name.IsNullOrEmpty())
			{
				if (!vALUE.IsGlobal)
				{
					itemBase.Variable.AddTypeValue(vALUE.Name, argumentHolder.Value, vALUE.IsDynamic);
				}
				else
				{
					runtime.Variable.AddTypeValue(vALUE.Name, argumentHolder.Value, vALUE.IsDynamic);
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
