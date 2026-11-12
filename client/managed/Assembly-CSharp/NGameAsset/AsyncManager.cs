using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGameAsset;

public class AsyncManager : SingletonUpdater<AsyncManager>
{
	public interface IEntry
	{
		bool IsDone { get; }

		void Complete();
	}

	public class SingleEntry : IEntry
	{
		public IAsyncLoadBase AsyncLoad;

		public Action OnComplete;

		bool IEntry.IsDone
		{
			get
			{
				if (AsyncLoad == null)
				{
					return true;
				}
				return AsyncLoad.IsDone;
			}
		}

		void IEntry.Complete()
		{
			Vespa_System_Action_Extension_Method.fire(OnComplete);
			OnComplete = null;
		}
	}

	public class MultiEntry : IEntry, IAsyncLoadBase
	{
		public List<IAsyncLoadBase> AsyncList = new List<IAsyncLoadBase>();

		public Action OnComplete;

		private int _completeCount;

		bool IEntry.IsDone => IsDone();

		string IAsyncLoadBase.KeyDesc
		{
			get
			{
				string text = "MultiEntry: ";
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AsyncList); i++)
				{
					text = text + AsyncList[i].KeyDesc + ", ";
				}
				return text;
			}
		}

		bool IAsyncLoadBase.IsDone => IsDone();

		bool IAsyncLoadBase.IsValid => IsValid();

		string IAsyncLoadBase.LoadMessage => string.Empty;

		void IEntry.Complete()
		{
			Vespa_System_Action_Extension_Method.fire(OnComplete);
			OnComplete = null;
		}

		private bool IsDone()
		{
			for (int i = _completeCount; i < __Extension_Method_ICollection__.COUNT(AsyncList); i++)
			{
				if (!AsyncList[i].IsDone)
				{
					_completeCount = i;
					return false;
				}
			}
			return true;
		}

		private bool IsValid()
		{
			for (int i = _completeCount; i < __Extension_Method_ICollection__.COUNT(AsyncList); i++)
			{
				if (!AsyncList[i].IsValid)
				{
					return false;
				}
			}
			return true;
		}
	}

	private bool _allowForceSyncLoad;

	private bool _forceSyncLoad = true;

	public static int DelayFrame = 1;

	public int WwwCount;

	public int AsyncOperationCount;

	private List<IEntry> _list = new List<IEntry>();

	public bool ForceSyncLoad
	{
		get
		{
			if (_allowForceSyncLoad)
			{
				return _forceSyncLoad;
			}
			return false;
		}
		set
		{
			_forceSyncLoad = value;
		}
	}

	public int EntryCount => _list.Count;

	public void ConfigAsyncLoad(bool allowForceSyncLoad)
	{
		_allowForceSyncLoad = allowForceSyncLoad;
	}

	public void Add(IEntry entry)
	{
		_list.Add(entry);
	}

	public void Add(WwwAbstract www, Action onComplete)
	{
		VespaCoroutine.Start(coWwwAbstract(www, onComplete));
	}

	private IEnumerator coWwwAbstract(WwwAbstract www, Action onComplete)
	{
		while (!www.isDone)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public void Add(WWW www, Action onComplete)
	{
		VespaCoroutine.Start(coWWW(www, onComplete));
	}

	private IEnumerator coWWW(WWW www, Action onComplete)
	{
		WwwCount++;
		yield return www;
		Vespa_System_Action_Extension_Method.fire(onComplete);
		WwwCount--;
	}

	public void Add(AsyncOperation request, Action onComplete)
	{
		VespaCoroutine.Start(coRequest(request, onComplete));
	}

	private IEnumerator coRequest(AsyncOperation request, Action onComplete)
	{
		AsyncOperationCount++;
		yield return request;
		Vespa_System_Action_Extension_Method.fire(onComplete);
		AsyncOperationCount--;
	}

	public override void ChainUpdate()
	{
		CompleteAsyncListWithException();
	}

	private void CompleteAsyncListWithException()
	{
		try
		{
			CompleteAsyncList();
		}
		catch (Exception ex)
		{
			VespaDebug.LogError(ex.Message);
		}
	}

	private void CompleteAsyncList()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
		{
			IEntry entry = _list[i];
			if (entry.IsDone)
			{
				_list.RemoveAt(i);
				i--;
				entry.Complete();
			}
		}
	}
}
