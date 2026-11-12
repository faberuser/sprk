using System.Collections.Generic;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NUtil;
using NVespa;
using NVespa.NSingleton;
using PigeonCoopToolkit.Effects.Trails;

namespace NGame2.NNodeTree;

public class RuntimeGlobalManager : SceneAutoBehaviour<RuntimeGlobalManager>, InitOnCreate, IInspectorGuiDrawer
{
	private struct PendingRuntime
	{
		public IRuntime Runtime;

		public int PendingFrame;
	}

	private HandlerContainer _handlerContainer = new HandlerContainer();

	private IncreaseDic<string, PackLoad> _packDic = new IncreaseDic<string, PackLoad>();

	private List<IRuntime> _runtimeList = new List<IRuntime>(128);

	private List<IRuntime> _releaseList = new List<IRuntime>(128);

	private Queue<PendingRuntime> _pendingQueue;

	private List<PackLoad> _packLoadingList = new List<PackLoad>();

	void InitOnCreate.InitializeSingleton()
	{
		_pendingQueue = new Queue<PendingRuntime>(10);
		SceneAutoBehaviour<HandlerVespa>.CreateInstance();
		LoadNodeTree("Common/Auto");
		LoadNodeTree("Common/DropItem");
		AddRuntime(null, immediatelyStart: false);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			_handlerContainer.Clear();
		}
	}

	public PackLoad LoadNodeTree(string pathName)
	{
		string arg = pathName;
		if (PackHelper.FixPathName(ref pathName))
		{
			VespaDebug.LogError($"Invalid PathName: {arg} -> {pathName}");
		}
		PackLoad vALUE = _packDic.GETVALUE(pathName, null);
		if (vALUE != null)
		{
			return vALUE;
		}
		vALUE = SingletonAuto<PackContainer>.instance.GetPack(pathName);
		if (vALUE == null)
		{
			return null;
		}
		_packDic.Add(pathName, vALUE);
		if (vALUE.IsDone)
		{
			AddHandlerFromPack(vALUE);
		}
		else
		{
			_packLoadingList.Add(vALUE);
		}
		return vALUE;
	}

	private void AddHandlerFromPack(PackLoad packLoad)
	{
		if (!packLoad.IsDone || !packLoad.ISVALID())
		{
			return;
		}
		for (int i = 0; i < packLoad.Value.TreeList.Count; i++)
		{
			if (packLoad.Value.TreeList[i].Tree.ContentsRoot is Holder.ICollector collector)
			{
				_handlerContainer.AddCollector(collector);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(packLoad.Value.LinkList); j++)
		{
			LinkNodeTree2 linkNodeTree = packLoad.Value.LinkList[j];
			if (!String_Extension_Method.IsNullOrEmpty(linkNodeTree.PackPathName))
			{
				LoadNodeTree(linkNodeTree.PackPathName);
			}
		}
	}

	public void ClearAll()
	{
		_packLoadingList.Clear();
		for (int i = 0; i < _runtimeList.Count; i++)
		{
			IRuntime runtime = _runtimeList[i];
			runtime.RequestStop(ERequestStop.Force);
			runtime.Close();
			runtime.ReleaseSelf();
		}
		_runtimeList.Clear();
		while (_pendingQueue.Count > 0)
		{
			IRuntime runtime2 = _pendingQueue.Peek().Runtime;
			_pendingQueue.Dequeue();
			runtime2.Close();
			runtime2.ReleaseSelf();
		}
		for (int j = 0; j < _releaseList.Count; j++)
		{
			_releaseList[j].ReleaseSelf();
		}
		_releaseList.Clear();
	}

	public bool AddRuntime(IRuntime runtime, bool immediatelyStart)
	{
		if (runtime == null)
		{
			return false;
		}
		if (!immediatelyStart)
		{
			PendingRuntime item = new PendingRuntime
			{
				Runtime = runtime,
				PendingFrame = GlobalTimer.frameCount
			};
			_pendingQueue.Enqueue(item);
			return true;
		}
		if (!runtime.Start())
		{
			runtime.Close();
			_releaseList.Add(runtime);
			return false;
		}
		_runtimeList.Add(runtime);
		return true;
	}

	public void ChainUpdate()
	{
		for (int i = 0; i < _packLoadingList.Count; i++)
		{
			PackLoad packLoad = _packLoadingList[i];
			if (packLoad.IsDone)
			{
				AddHandlerFromPack(packLoad);
				_packLoadingList.RemoveAt(i);
			}
		}
		int frameCount = GlobalTimer.frameCount;
		while (_pendingQueue.Count > 0)
		{
			PendingRuntime pendingRuntime = _pendingQueue.Peek();
			if (pendingRuntime.PendingFrame >= frameCount)
			{
				break;
			}
			IRuntime runtime = pendingRuntime.Runtime;
			_pendingQueue.Dequeue();
			if (!runtime.Start())
			{
				runtime.Close();
				runtime.ReleaseSelf();
			}
			else
			{
				_runtimeList.Add(runtime);
			}
		}
		for (int j = 0; j < _runtimeList.Count; j++)
		{
			IRuntime runtime2 = _runtimeList[j];
			runtime2.Update();
			if (runtime2.IsEnd())
			{
				_runtimeList.RemoveAt(j);
				runtime2.Close();
				runtime2.ReleaseSelf();
				j--;
			}
		}
		Container.instance.ChainUpdate();
		for (int k = 0; k < _releaseList.Count; k++)
		{
			_releaseList[k].ReleaseSelf();
		}
		_releaseList.Clear();
	}
}
