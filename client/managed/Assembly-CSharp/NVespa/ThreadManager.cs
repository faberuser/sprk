using System;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NThreadManager;
using UnityEngine;

namespace NVespa;

public class ThreadManager : SingletonUpdater<ThreadManager>
{
	public enum MainThreadInterceptType
	{
		None,
		Single,
		Total,
		DisableThread,
		DisableThread_Single
	}

	public enum EEntryType
	{
		None,
		LitJson
	}

	public interface IThreadInstance
	{
		void Notify(string notify);
	}

	public interface IPlatformManager
	{
		string Dump(bool detail = false);

		void Enqueue(Action action, Action onComplete);

		void Enqueue(Action<IThreadInstance> action, Action onComplete);

		void ChainUpdate();
	}

	public static MainThreadInterceptType MainThreadIntercept;

	private IPlatformManager _entryNoneManager;

	private IPlatformManager _entryLitJsonManager;

	public ThreadManager()
	{
		_ = MainThreadIntercept;
		_entryNoneManager = new AndroidManager(4);
		_entryLitJsonManager = new AndroidManager(1);
		GlobalAutoBehaviour<GuiManager>.instance.Add("ThreadManager", this, delegate
		{
			MainThreadIntercept = EnumTools.IngamePopup("MainThreadIntercept", MainThreadIntercept);
			GUILayout.Label(Dump(detail: true));
			if (GameRoot.Instance.JsonLoader is JsonAssetAsyncLoader jsonAssetAsyncLoader)
			{
				GUILayout.Label(jsonAssetAsyncLoader.Dump());
			}
		});
	}

	public string Dump(bool detail = false)
	{
		return $"None: {_entryNoneManager.Dump(detail)}\nLitJson: {_entryLitJsonManager.Dump(detail)}";
	}

	public void Enqueue(Action action, Action onComplete)
	{
		Enqueue(action, onComplete, EEntryType.None);
	}

	public void Enqueue(Action action, Action onComplete, EEntryType entryType)
	{
		if (entryType == EEntryType.LitJson)
		{
			_entryLitJsonManager.Enqueue(action, onComplete);
		}
		else
		{
			_entryNoneManager.Enqueue(action, onComplete);
		}
	}

	public void Enqueue(Action<IThreadInstance> action, Action onComplete, EEntryType entryType = EEntryType.None)
	{
		if (entryType == EEntryType.LitJson)
		{
			_entryLitJsonManager.Enqueue(action, onComplete);
		}
		else
		{
			_entryNoneManager.Enqueue(action, onComplete);
		}
	}

	public override void ChainUpdate()
	{
		_entryNoneManager.ChainUpdate();
		_entryLitJsonManager.ChainUpdate();
	}
}
