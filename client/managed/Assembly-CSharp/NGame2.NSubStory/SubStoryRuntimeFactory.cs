using System;
using System.Collections.Generic;
using NGame2.NTutorial.NRuntime;
using NVespa.NSingleton;

namespace NGame2.NSubStory;

public class SubStoryRuntimeFactory : SceneAutoBehaviour<SubStoryRuntimeFactory>
{
	private readonly Dictionary<string, Func<IEventRuntime>> _creatorDictionary = new Dictionary<string, Func<IEventRuntime>>();

	public SubStoryRuntimeFactory()
	{
		Register<SetFunctionality>();
		Register<PlayCutScene>();
		Register<WaitCutScene>();
		Register<PlayVideo>();
		Register<WaitVideo>();
		Register<PlayNodeTree>();
		Register<StopNodeTree>();
		Register<WaitNodeTree>();
		Register<NodeTreeNextFlow>();
		Register<NodeTreeSetFlow>();
	}

	private void Register<T>() where T : IEventRuntime, new()
	{
		string key = typeof(T).Name;
		_creatorDictionary.Remove(key);
		_creatorDictionary.Add(key, () => new T());
	}

	public IEventRuntime Create(string commandType)
	{
		if (String_Extension_Method.IsNullOrEmpty(commandType))
		{
			return null;
		}
		Func<IEventRuntime> value = null;
		_creatorDictionary.TryGetValue(commandType, out value);
		if (value == null)
		{
			return null;
		}
		return Vespa_System_Action_Extension_Method.fire(value);
	}
}
