using System;
using LitJson;

namespace NGame2.NNodeTree.NRuntime;

public class DefineVariable<T> : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsGlobalVariable;

	public StringKey Name = StringKey.Empty;

	public DefineVariable()
	{
	}

	public DefineVariable(StringKey name)
	{
		Name = name;
	}

	public void SetValue(ItemBase itemBase, T value)
	{
		if (!Name.IsNullOrEmpty())
		{
			LocalVariable localVariable = ((!IsGlobalVariable) ? itemBase.Variable : itemBase.Runtime.Variable);
			localVariable.AddValue(Name, value);
		}
	}

	public void SetFunc(ItemBase itemBase, Func<T> func)
	{
		if (!Name.IsNullOrEmpty())
		{
			LocalVariable localVariable = ((!IsGlobalVariable) ? itemBase.Variable : itemBase.Runtime.Variable);
			localVariable.AddFunc(Name, func);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsGlobalVariable = json["IsGlobalVariable"].GETBOOLEAN(IsGlobalVariable);
		Name = StringKey.CreateFromJson(json["Name"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsGlobalVariable", IsGlobalVariable);
		jsonData.AddField("Name", Name.MakeJson());
		return jsonData;
	}
}
