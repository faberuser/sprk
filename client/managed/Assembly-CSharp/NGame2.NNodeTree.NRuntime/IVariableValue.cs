namespace NGame2.NNodeTree.NRuntime;

public interface IVariableValue<T> : IVariableValueBase
{
	T GetValue();
}
