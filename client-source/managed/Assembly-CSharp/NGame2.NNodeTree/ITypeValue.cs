using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree;

public interface ITypeValue<T> : ITypeValueBase
{
	T GetValue(IVariable variable);

	bool TryGetValue(IVariable variable, out T o_value);
}
