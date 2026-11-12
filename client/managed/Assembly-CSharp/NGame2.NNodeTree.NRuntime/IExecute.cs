namespace NGame2.NNodeTree.NRuntime;

public interface IExecute
{
	void UpdateExecute(ItemBase itemBase);

	bool IsExecuteEnd(ItemBase itemBase);

	void RequestStopExecute(ItemBase itemBase, ERequestStop requestStop);
}
