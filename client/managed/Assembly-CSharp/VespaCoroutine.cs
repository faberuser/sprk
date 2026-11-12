using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

public class VespaCoroutine : SingletonAuto<VespaCoroutine>
{
	public enum ESceneType
	{
		Global,
		Scene,
		Unknown
	}

	public class RuntimeData
	{
		private EmptyBehaviour _behaviour;

		private MonoBehaviour _owner;

		private Coroutine _runningCo;

		private IEnumerator _enumerator;

		public Coroutine ExecuteCo { get; private set; }

		public bool IsEnd { get; private set; }

		public ESceneType SceneType { get; private set; }

		public RuntimeData(EmptyBehaviour behaviour, MonoBehaviour owner, ESceneType sceneType, string extra)
		{
			Init(behaviour, owner, sceneType, extra);
		}

		public RuntimeData(EmptyBehaviour behaviour, ESceneType sceneType, string extra)
		{
			Init(behaviour, behaviour, sceneType, extra);
		}

		public bool IsValid()
		{
			return !Vespa_Common_Class_Extension_Method.IsUnityNull(_owner);
		}

		public string GetDesc()
		{
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(_behaviour))
			{
				return "behaviour is null";
			}
			return _behaviour.Desc;
		}

		private void Init(EmptyBehaviour behaviour, MonoBehaviour owner, ESceneType sceneType, string extra)
		{
			_behaviour = behaviour;
			_owner = owner;
			SceneType = sceneType;
		}

		internal void Start(IEnumerator enumerator, Action onComplete)
		{
			IsEnd = false;
			ExecuteCo = _behaviour.StartCoroutine(coWait(enumerator, onComplete));
		}

		internal EmptyBehaviour Close()
		{
			EmptyBehaviour behaviour = _behaviour;
			ExecuteCo = null;
			_behaviour = null;
			_enumerator = null;
			_runningCo = null;
			_owner = null;
			IsEnd = true;
			return behaviour;
		}

		internal void Stop()
		{
			if (ExecuteCo != null && !(_behaviour == null))
			{
				if (_runningCo != null && _owner != null)
				{
					_owner.StopCoroutine(_runningCo);
				}
				_enumerator = null;
				_runningCo = null;
				if (ExecuteCo != null)
				{
					_behaviour.StopAllCoroutines();
					ExecuteCo = null;
				}
			}
		}

		private IEnumerator coWait(IEnumerator enumerator, Action onComplete)
		{
			_enumerator = enumerator;
			_runningCo = _owner.StartCoroutine(enumerator);
			yield return _runningCo;
			Vespa_System_Action_Extension_Method.fire(onComplete);
			IsEnd = true;
			SingletonAuto<VespaCoroutine>.instance.OnEnd(this);
		}
	}

	private Transform _coroutineRoot;

	private Dictionary<Coroutine, RuntimeData> _dic = new Dictionary<Coroutine, RuntimeData>();

	private Queue<EmptyBehaviour> _unusedQueue = new Queue<EmptyBehaviour>();

	public static Coroutine StartThis(MonoBehaviour behaviour, IEnumerator enumerator, string extra = null)
	{
		return SingletonAuto<VespaCoroutine>.instance.FollowCo(behaviour, enumerator, extra);
	}

	public static Coroutine Start(MonoBehaviour behaviour, IEnumerator enumerator)
	{
		return StartThis(behaviour, enumerator);
	}

	public static Coroutine StartScene(IEnumerator enumerator)
	{
		return StartThis(SceneAutoManager.SingletonBehaviour, enumerator);
	}

	public static void Stop(MonoBehaviour behaviour, IList<Coroutine> coroutineList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(coroutineList); i++)
		{
			Stop(behaviour, coroutineList[i]);
		}
	}

	public static void Stop(MonoBehaviour behaviour, Coroutine coroutine)
	{
		if (!SingletonAuto<VespaCoroutine>.instance.StopCo(coroutine) && behaviour != null && coroutine != null)
		{
			behaviour.StopCoroutine(coroutine);
		}
	}

	public static void StopScene(Coroutine coroutine)
	{
		Stop(SceneAutoManager.SingletonBehaviour, coroutine);
	}

	public static void StopAll(MonoBehaviour behaviour)
	{
		behaviour.StopAllCoroutines();
	}

	public static RuntimeData StartRuntime(IEnumerator enumerator, string extra = null)
	{
		return SingletonAuto<VespaCoroutine>.instance.StartCo(enumerator, null, ESceneType.Unknown, extra);
	}

	public static RuntimeData StartRuntimeGlobal(IEnumerator enumerator, string extra = null)
	{
		return SingletonAuto<VespaCoroutine>.instance.StartCo(enumerator, null, ESceneType.Global, extra);
	}

	public static Coroutine Start(IEnumerator enumerator, string extra = null)
	{
		return SingletonAuto<VespaCoroutine>.instance.StartCo(enumerator, null, ESceneType.Unknown, extra)?.ExecuteCo;
	}

	public static Coroutine StartGlobal(IEnumerator enumerator, string extra = null)
	{
		return SingletonAuto<VespaCoroutine>.instance.StartCo(enumerator, null, ESceneType.Global, extra)?.ExecuteCo;
	}

	public static Coroutine Start(IEnumerator enumerator, Action onComplete, string extra = null)
	{
		return SingletonAuto<VespaCoroutine>.instance.StartCo(enumerator, onComplete, ESceneType.Scene, extra)?.ExecuteCo;
	}

	public static Coroutine StartGlobal(IEnumerator enumerator, Action onComplete, string extra = null)
	{
		return SingletonAuto<VespaCoroutine>.instance.StartCo(enumerator, onComplete, ESceneType.Global, extra)?.ExecuteCo;
	}

	public static void Stop(Coroutine coroutine)
	{
		if (coroutine != null)
		{
			SingletonAuto<VespaCoroutine>.instance.StopCo(coroutine);
		}
	}

	public static void Stop(RuntimeData runtime)
	{
		if (runtime != null)
		{
			SingletonAuto<VespaCoroutine>.instance.StopCo(runtime);
		}
	}

	private static IEnumerator coWaitRuntime(RuntimeData runtime)
	{
		yield return runtime.ExecuteCo;
		SingletonAuto<VespaCoroutine>.instance.OnEnd(runtime);
	}

	public VespaCoroutine()
	{
		GameObject gameObject = new GameObject("VespaCoroutine");
		_coroutineRoot = gameObject.transform;
		_coroutineRoot.parent = GlobalAutoManager.SingletonComponent.transform;
	}

	public void BeforeSceneChange()
	{
		List<RuntimeData> list = new List<RuntimeData>();
		foreach (KeyValuePair<Coroutine, RuntimeData> item in _dic)
		{
			RuntimeData value = item.Value;
			if (value.SceneType == ESceneType.Scene)
			{
				list.Add(value);
			}
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			RuntimeData runtime = list[i];
			StopCo(runtime);
		}
		list.Clear();
	}

	public void OnSceneChanged()
	{
		List<RuntimeData> list = new List<RuntimeData>();
		foreach (KeyValuePair<Coroutine, RuntimeData> item in _dic)
		{
			RuntimeData value = item.Value;
			if (!value.IsValid())
			{
				list.Add(value);
			}
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			RuntimeData runtime = list[i];
			StopCo(runtime);
		}
		list.Clear();
		while (_unusedQueue.Count > 0)
		{
			EmptyBehaviour emptyBehaviour = _unusedQueue.Dequeue();
			if (emptyBehaviour != null)
			{
				VespaObject.DestroyGo(emptyBehaviour);
			}
		}
	}

	private Coroutine FollowCo(MonoBehaviour behaviour, IEnumerator enumerator, string extra)
	{
		if (!VespaDebug.IsUnityMainThread())
		{
			VespaDebug.LogError("is Not UnityThread");
			return null;
		}
		RuntimeData runtimeData = new RuntimeData(CreateMonoBehaviour(), behaviour, ESceneType.Unknown, extra);
		runtimeData.Start(enumerator, null);
		if (runtimeData.IsEnd || runtimeData.ExecuteCo == null)
		{
			return null;
		}
		_dic.Add(runtimeData.ExecuteCo, runtimeData);
		return runtimeData.ExecuteCo;
	}

	private RuntimeData StartCo(IEnumerator enumerator, Action onComplete, ESceneType sceneType, string extra)
	{
		if (!VespaDebug.IsUnityMainThread())
		{
			VespaDebug.LogError("is Not UnityThread");
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return null;
		}
		RuntimeData runtimeData = new RuntimeData(CreateMonoBehaviour(), sceneType, extra);
		runtimeData.Start(enumerator, onComplete);
		if (runtimeData.IsEnd || runtimeData.ExecuteCo == null)
		{
			return null;
		}
		_dic.Add(runtimeData.ExecuteCo, runtimeData);
		return runtimeData;
	}

	private bool StopCo(Coroutine coroutine)
	{
		if (coroutine == null)
		{
			return false;
		}
		if (!_dic.TryGetValue(coroutine, out var value))
		{
			return false;
		}
		_dic.Remove(coroutine);
		value.Stop();
		EmptyBehaviour emptyBehaviour = value.Close();
		if (emptyBehaviour != null)
		{
			_unusedQueue.Enqueue(emptyBehaviour);
		}
		return true;
	}

	private void StopCo(RuntimeData runtime)
	{
		if (runtime.ExecuteCo != null)
		{
			_dic.Remove(runtime.ExecuteCo);
		}
		runtime.Stop();
		EmptyBehaviour emptyBehaviour = runtime.Close();
		if (emptyBehaviour != null)
		{
			_unusedQueue.Enqueue(emptyBehaviour);
		}
	}

	private void OnEnd(RuntimeData runtime)
	{
		if (runtime != null)
		{
			if (runtime.ExecuteCo != null)
			{
				_dic.Remove(runtime.ExecuteCo);
			}
			EmptyBehaviour emptyBehaviour = runtime.Close();
			if (emptyBehaviour != null)
			{
				_unusedQueue.Enqueue(emptyBehaviour);
			}
		}
	}

	private EmptyBehaviour CreateMonoBehaviour()
	{
		while (_unusedQueue.Count > 0)
		{
			EmptyBehaviour emptyBehaviour = _unusedQueue.Dequeue();
			if (!(emptyBehaviour == null) && !(emptyBehaviour.gameObject == null))
			{
				return emptyBehaviour;
			}
		}
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = _coroutineRoot;
		return gameObject.AddComponent<EmptyBehaviour>();
	}
}
