namespace NGame2.NNodeTree.NRuntime.NVariable;

public interface IValueFactory
{
	IVariableValueBase CreateValue(ITypeValueBase typeValueBase, IVariable variable, bool isDynamic);
}
