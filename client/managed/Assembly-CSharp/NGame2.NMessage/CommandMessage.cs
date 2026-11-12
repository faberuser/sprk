using System.Collections.Generic;
using NGame2.NNodeTree;
using NVespa;

namespace NGame2.NMessage;

public class CommandMessage : SmallPooling<CommandMessage>, ISmallPoolingItem, IMessage
{
	public string Command;

	public List<object> ArgList = new List<object>();

	public ISceneContainer TargetScene;

	public bool IsDummy { get; set; }

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		Command = string.Empty;
		ArgList.Clear();
	}

	void IMessage.ReleaseSelf()
	{
		SmallPooling<CommandMessage>.ReleaseInstance(this);
	}

	public void Open(string command)
	{
		Command = command;
	}

	public void Open(string command, params object[] args)
	{
		Command = command;
		ArgList.AddRange(args);
	}

	public void AddArg(object arg)
	{
		ArgList.Add(arg);
	}

	public void AddArgs(IEnumerable<object> args)
	{
		ArgList.AddRange(args);
	}

	public bool IsValidScene(ISceneContainer scene)
	{
		if (TargetScene == null)
		{
			return true;
		}
		return TargetScene == scene;
	}

	public T GetArg<T>(int index) where T : class
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(ArgList, index))
		{
			return ArgList[index] as T;
		}
		return null;
	}

	public bool TryGetArgValue<T>(int index, out T oValue) where T : struct
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(ArgList, index))
		{
			oValue = default(T);
			return false;
		}
		object obj = ArgList[index];
		if (obj == null)
		{
			oValue = default(T);
			return false;
		}
		oValue = (T)obj;
		return true;
	}

	public T GetArgValue<T>(int index) where T : struct
	{
		if (TryGetArgValue<T>(index, out var oValue))
		{
			return oValue;
		}
		return default(T);
	}
}
