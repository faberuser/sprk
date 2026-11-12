using System.Collections.Generic;
using NVespa;
using NVespa.NSingleton;
using NVespa.NTask;
using UnityEngine;

namespace NGame2.NLobby;

public class LobbyTaskFactory : SingletonAuto<LobbyTaskFactory>
{
	private readonly Dictionary<string, ILobbyTaskCreator> _creatorDictionary = new Dictionary<string, ILobbyTaskCreator>();

	public void Run(string taskType, string[] taskValues)
	{
		if (!SceneAutoBehaviour<ActivityManager>.instance.Run(taskType, taskValues))
		{
			Debug.LogError("deprecated lobbytaskfactory");
			CreateTask(taskType, taskValues)?.Commit();
		}
	}

	private ITask CreateTask(string taskType, string[] taskValues)
	{
		if (string.IsNullOrEmpty(taskType))
		{
			return null;
		}
		ILobbyTaskCreator value = null;
		_creatorDictionary.TryGetValue(taskType, out value);
		return value?.Create(taskValues);
	}

	private void Register<T>() where T : ILobbyTaskCreator, new()
	{
		string name = typeof(T).Name;
		T val = new T();
		_creatorDictionary.Add(name, val);
	}
}
