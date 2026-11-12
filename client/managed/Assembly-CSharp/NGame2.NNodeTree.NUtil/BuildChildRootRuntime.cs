using System;
using NGame2.NNodeTree.NContents.NRoot;
using NGame2.NNodeTree.NRuntime;
using NVespa;

namespace NGame2.NNodeTree.NUtil;

public struct BuildChildRootRuntime
{
	private TreeData _tree;

	private ChildRoot _childRoot;

	private ChildRoot.RuntimeVariable _variable;

	public bool Open(TreeData tree)
	{
		_tree = tree;
		if (_tree == null)
		{
			return false;
		}
		_childRoot = _tree.ContentsRoot as ChildRoot;
		if (_childRoot == null)
		{
			return false;
		}
		_variable = SmallPooling<ChildRoot.RuntimeVariable>.CreateInstance();
		return true;
	}

	public void AddTypeValue(StringKey key, ITypeValueBase typeValue, IVariable variable)
	{
		if (_variable != null)
		{
			_variable.AddTypeValue(key, typeValue, variable);
		}
	}

	public void AddValue<T>(StringKey key, T value)
	{
		if (_variable != null)
		{
			_variable.AddValue(key, value);
		}
	}

	public void AddFunc<T>(StringKey key, Func<T> func)
	{
		if (_variable != null)
		{
			_variable.AddFunc(key, func);
		}
	}

	public bool TryGetValue<T>(StringKey key, out T value)
	{
		if (_variable == null)
		{
			value = default(T);
			return false;
		}
		return _variable.TryGetValue<T>(key, out value);
	}

	public RuntimeInstance Build(ISceneContainer sceneContainer, ILocalTimer parentTimer, IObserver observer, int layer)
	{
		if (_tree == null)
		{
			return null;
		}
		if (_childRoot == null)
		{
			return null;
		}
		if (_variable == null)
		{
			return null;
		}
		RuntimeInstance runtimeInstance = SmallPooling<RuntimeInstance>.CreateInstance();
		runtimeInstance.Open(sceneContainer, _tree, _variable, parentTimer, observer, layer, _childRoot);
		_tree = null;
		_childRoot = null;
		_variable = null;
		return runtimeInstance;
	}
}
