using LitJson;

namespace NGame2.NNodeTree.NRuntime;

public interface IVariable
{
	void Open(RuntimeInstance runtime);

	IVariable CaptureSelf();

	void ReleaseSelf();

	RuntimeInstance GetRuntimeInstance();

	IVariableValueBase GetValueBase(StringKey key);

	JsonData DumpJson();
}
