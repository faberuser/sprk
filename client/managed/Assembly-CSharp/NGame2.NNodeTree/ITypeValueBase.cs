using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;

namespace NGame2.NNodeTree;

public interface ITypeValueBase
{
	string ValuePath { get; set; }

	Type GetValueType();

	object GetObjectValue(IVariable variable);

	ArgumentContainer GetArgumentContainer();

	ITypeValueBase CreateFromPrototype();

	bool LoadFromBodyJson(JsonData json);

	JsonData MakeBodyJson();
}
