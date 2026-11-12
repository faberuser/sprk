using System;
using LitJson;

namespace NGame2.NNodeTree.NRuntime;

public interface IVariableValueBase
{
	Type GetValueType();

	JsonData DumpJson();
}
