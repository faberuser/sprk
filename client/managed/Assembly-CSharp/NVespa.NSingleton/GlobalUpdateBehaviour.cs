using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NSingleton;

public class GlobalUpdateBehaviour : MonoBehaviour
{
	private static List<WeakRef<IUpdater>> _nativeList = new List<WeakRef<IUpdater>>();

	private static List<IUpdater> _unityList = new List<IUpdater>();

	private static List<WeakRef<ILateUpdater>> _nativeLateList = new List<WeakRef<ILateUpdater>>();

	private static List<ILateUpdater> _unityLateList = new List<ILateUpdater>();

	public static bool isValidInstance => GlobalAutoStatic<GlobalUpdateBehaviour>.isValidInstance;

	public static GlobalUpdateBehaviour instance => GlobalAutoStatic<GlobalUpdateBehaviour>.instance;

	public static void CreateInstance()
	{
		GlobalAutoStatic<GlobalUpdateBehaviour>.CreateInstance();
	}

	public static void Recreate()
	{
		if (isValidInstance)
		{
			GlobalAutoStatic<GlobalUpdateBehaviour>.ReleaseInstance();
			CreateInstance();
		}
	}

	public static void AddUpdater(object obj)
	{
		if (SystemStatus.IsApplicationQuit || Vespa_Common_Class_Extension_Method.IsUnityNull(obj))
		{
			return;
		}
		CreateInstance();
		bool flag = obj is UnityEngine.Object;
		if (obj is IUpdater updater)
		{
			if (flag)
			{
				_unityList.Add(updater);
			}
			else
			{
				WeakRef<IUpdater> weakRef = new WeakRef<IUpdater>();
				weakRef.Value = updater;
				_nativeList.Add(weakRef);
			}
		}
		if (obj is ILateUpdater lateUpdater)
		{
			if (flag)
			{
				_unityLateList.Add(lateUpdater);
				return;
			}
			WeakRef<ILateUpdater> weakRef2 = new WeakRef<ILateUpdater>();
			weakRef2.Value = lateUpdater;
			_nativeLateList.Add(weakRef2);
		}
	}

	private void Update()
	{
		SprkMissionHistory.Tick();
		SprkImageRecovery.TickGlobal();
		SprkStoryAuto.Tick();
		SprkConquest.Tick();
		SprkDispatch.Tick();
		for (int i = 0; i < _nativeList.Count; i++)
		{
			IUpdater value = _nativeList[i].Value;
			if (value == null)
			{
				_nativeList.RemoveAt(i);
				i--;
				continue;
			}
			try
			{
				value.ChainUpdate();
			}
			catch (Exception ex)
			{
				VespaDebug.LogError(ex.ToString());
			}
		}
		for (int j = 0; j < _unityList.Count; j++)
		{
			IUpdater updater = _unityList[j];
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(updater))
			{
				_unityList.RemoveAt(j);
				j--;
				continue;
			}
			try
			{
				updater.ChainUpdate();
			}
			catch (Exception ex2)
			{
				VespaDebug.LogError(ex2.ToString());
			}
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _nativeLateList.Count; i++)
		{
			ILateUpdater value = _nativeLateList[i].Value;
			if (value == null)
			{
				_nativeLateList.RemoveAt(i);
				i--;
				continue;
			}
			try
			{
				value.ChainLateUpdate();
			}
			catch (Exception ex)
			{
				VespaDebug.LogError(ex.ToString());
			}
		}
		for (int j = 0; j < _unityLateList.Count; j++)
		{
			ILateUpdater lateUpdater = _unityLateList[j];
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(lateUpdater))
			{
				_unityLateList.RemoveAt(j);
				j--;
				continue;
			}
			try
			{
				lateUpdater.ChainLateUpdate();
			}
			catch (Exception ex2)
			{
				VespaDebug.LogError(ex2.ToString());
			}
		}
	}
}
