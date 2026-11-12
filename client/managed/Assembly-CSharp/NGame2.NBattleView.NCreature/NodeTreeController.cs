using System.Collections.Generic;
using NGame2.NBattleView.NCreature.NHandler;
using NGame2.NNodeTree;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleView.NCreature;

public class NodeTreeController
{
	public class RuntimeLock
	{
		internal IRuntime Runtime;

		internal HashSet<long> LockSet = new HashSet<long>();

		public void Lock(long uid)
		{
			LockSet.Add(uid);
		}

		internal void Release(long uid)
		{
			if (Runtime != null)
			{
				LockSet.Remove(uid);
				if (LockSet.Count == 0)
				{
					Runtime.RequestStop(ERequestStop.Normal);
					Runtime.ReleaseSelf();
					Runtime = null;
				}
			}
		}
	}

	public SyncPosition SyncPosition;

	private List<IRuntime> _runtimeList = new List<IRuntime>();

	private IncreaseDic<object, RuntimeLock> _runtimeLockDic = new IncreaseDic<object, RuntimeLock>();

	public IHandler ExternalHandler { get; private set; }

	public IRuntime Main { get; private set; }

	public Creature Creature { get; private set; }

	public NodeTreeController(Creature creature)
	{
		Creature = creature;
		SyncPosition = new SyncPosition(Creature);
	}

	public void Close()
	{
		ReleaseMainRuntime(requestStop: true);
		for (int i = 0; i < _runtimeList.Count; i++)
		{
			_runtimeList[i].RequestStop(ERequestStop.Normal);
			_runtimeList[i].ReleaseSelf();
		}
		_runtimeList.Clear();
		for (int j = 0; j < _runtimeLockDic.Count; j++)
		{
			RuntimeLock valueByIndex = _runtimeLockDic.GetValueByIndex(j);
			if (valueByIndex.Runtime != null)
			{
				valueByIndex.Runtime.RequestStop(ERequestStop.Normal);
				valueByIndex.Runtime.ReleaseSelf();
			}
		}
		_runtimeLockDic.Clear();
	}

	public RuntimeLock GetRuntimeLock(object obj)
	{
		return _runtimeLockDic.GETVALUE(obj, null);
	}

	public void LockRuntime(object obj, long uid, IRuntime runtime, bool immediatelyStart)
	{
		if (SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(runtime, immediatelyStart))
		{
			RuntimeLock vALUE = _runtimeLockDic.GETVALUE(obj, null);
			if (vALUE != null)
			{
				vALUE.Runtime.RequestStop(ERequestStop.Force);
				_runtimeLockDic.RemoveSlow(obj);
			}
			RuntimeLock runtimeLock = new RuntimeLock();
			runtimeLock.Runtime = runtime.CaptureSelf();
			runtimeLock.Lock(uid);
			_runtimeLockDic.Add(obj, runtimeLock);
		}
	}

	public void ReleaseRuntimeLock(object obj, long uid)
	{
		RuntimeLock vALUE = _runtimeLockDic.GETVALUE(obj, null);
		if (vALUE != null)
		{
			vALUE.Release(uid);
			if (vALUE.LockSet.Count == 0)
			{
				_runtimeLockDic.RemoveSlow(obj);
			}
		}
	}

	public void AddRuntime(IRuntime runtime, bool immediatelyStart)
	{
		if (SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(runtime, immediatelyStart))
		{
			_runtimeList.Add(runtime.CaptureSelf());
		}
	}

	public void SetMainRuntime(IRuntime runtime)
	{
		if (Vespa_Common_Class_Extension_Method.EQUALS(Main, runtime))
		{
			runtime.ReleaseSelf();
			return;
		}
		ReleaseExternalHandler();
		ReleaseMainRuntime(requestStop: true);
		Main = runtime;
		Main.Start();
	}

	private void ReleaseMainRuntime(bool requestStop)
	{
		if (Main != null)
		{
			if (requestStop)
			{
				Main.RequestStop(ERequestStop.Normal);
			}
			Main.Close();
			Main.ReleaseSelf();
			Main = null;
		}
	}

	public void SetExternalHandler(IHandler handler)
	{
		ReleaseMainRuntime(requestStop: true);
		if (!Vespa_Common_Class_Extension_Method.EQUALS(ExternalHandler, handler))
		{
			ReleaseExternalHandler();
			ExternalHandler = handler;
			if (ExternalHandler != null)
			{
				ExternalHandler.Begin();
			}
		}
	}

	public void ReleaseExternalHandler()
	{
		if (ExternalHandler != null)
		{
			ExternalHandler.Cancel();
			ExternalHandler = null;
		}
	}

	public void ChainUpdate()
	{
		if (ExternalHandler != null)
		{
			ExternalHandler.ChainUpdate();
			if (ExternalHandler.IsEnd())
			{
				ExternalHandler = null;
			}
		}
		if (Main != null)
		{
			Main.Update();
			if (Main.IsEnd())
			{
				ReleaseMainRuntime(requestStop: false);
			}
		}
		if (ExternalHandler == null && Main == null)
		{
			SyncPosition.Sync();
		}
		else
		{
			SyncPosition.Reset();
		}
	}
}
