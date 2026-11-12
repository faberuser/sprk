using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NGameAsset;
using NVespa;

namespace NShared.NJit;

public class TableRoot
{
	private static TableRoot _instance;

	private bool _patchDownLoadComplete;

	private Dictionary<Type, IJitDataContainer> _dic = new Dictionary<Type, IJitDataContainer>();

	public static TableRoot Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new TableRoot();
			}
			return _instance;
		}
	}

	public static bool PatchDownloadComplete
	{
		get
		{
			if (_instance == null)
			{
				return false;
			}
			return _instance._patchDownLoadComplete;
		}
	}

	private TableRoot()
	{
	}

	public void Reset()
	{
		lock (_dic)
		{
			_dic.Clear();
		}
	}

	public TDataContainer GetDataContainer<TDataContainer>() where TDataContainer : class, IJitDataContainer, new()
	{
		lock (_dic)
		{
			Type typeFromHandle = typeof(TDataContainer);
			if (_dic.TryGetValue(typeFromHandle, out var value))
			{
				if (!value.IsLoaded)
				{
					JitOption.JitLogError($"{typeof(TDataContainer)} Table is Not Ready");
					return null;
				}
				return value as TDataContainer;
			}
			if (!CanLoadTable<TDataContainer>())
			{
				JitOption.JitLogError($"{typeof(TDataContainer)} Table Cannot Load");
				return null;
			}
			TDataContainer val = new TDataContainer();
			_dic.Add(typeof(TDataContainer), val);
			val.Load();
			return val;
		}
	}

	public JitDataContainerAbstract GetDataContainer_by_Type(Type type)
	{
		return GetType().GetMethod("GetDataContainer", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(type).Invoke(this, null) as JitDataContainerAbstract;
	}

	public static bool CanLoadTable<TDataContainer>() where TDataContainer : class, IJitDataContainer, new()
	{
		if (typeof(IData_from_PreloadInAppBytes).IsAssignableFrom(typeof(TDataContainer)))
		{
			return true;
		}
		if (!UnityCache.isGamePlaying)
		{
			return true;
		}
		if (Define.LocalAssetLoading.Value)
		{
			return true;
		}
		if (PatchDownloadComplete)
		{
			return true;
		}
		return false;
	}

	public void SetPatchDownLoadComplete(bool isComplete)
	{
		_patchDownLoadComplete = isComplete;
	}

	public static void RebuildJit_by_Type(Type dataContainerType)
	{
		try
		{
			typeof(TableRoot).GetMethod("RebuildJit", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(dataContainerType).Invoke(null, null);
		}
		catch (Exception ex)
		{
			JitOption.JitLogError(ex.ToString());
		}
	}

	public static void RebuildJit<T>() where T : IJitDataContainer, new()
	{
		using (new ForceRebuildJit())
		{
			Instance.Reset();
			MethodInfo method = typeof(TableRoot).GetMethod("GetDataContainer");
			if (method == null || !method.IsGenericMethodDefinition)
			{
				JitOption.JitLogError("TableRoot.instance.GetDataContainer not exist");
				return;
			}
			Type typeFromHandle = typeof(T);
			MethodInfo methodInfo = method.MakeGenericMethod(typeFromHandle);
			TableRoot instance = Instance;
			object[] emptyTypes = Type.EmptyTypes;
			if (methodInfo.Invoke(instance, emptyTypes) == null)
			{
				JitOption.JitLogError($"TableRoot.instance.GetDataContainer<{typeFromHandle}> result is null");
			}
		}
	}

	public static void RebuildAllJit()
	{
		JitOption.Max_countInHashed_from_JitDictionary = 0;
		JitOption.AppendLogToFile(string.Empty, "\n\n");
		JitOption.JitLog("[[ -- RebuildAllJit starting...... -- ]]");
		using (new ForceRebuildJit())
		{
			Instance.Reset();
			MethodInfo method = typeof(TableRoot).GetMethod("GetDataContainer");
			if (method == null || !method.IsGenericMethodDefinition)
			{
				JitOption.JitLogError("TableRoot.instance.GetDataContainer not exist");
				return;
			}
			new StringBuilder();
			List<Type> dataContainerList = UsingReflection.GetDataContainerList();
			for (int i = 0; i < dataContainerList.Count; i++)
			{
				Type type = dataContainerList[i];
				MethodInfo methodInfo = method.MakeGenericMethod(type);
				TableRoot instance = Instance;
				object[] emptyTypes = Type.EmptyTypes;
				if (methodInfo.Invoke(instance, emptyTypes) == null)
				{
					JitOption.JitLogError($"TableRoot.instance.GetDataContainer<{type}> result is null");
				}
			}
		}
	}

	public static void LoadAll()
	{
		Instance.Reset();
		MethodInfo method = typeof(TableRoot).GetMethod("GetDataContainer");
		if (method == null || !method.IsGenericMethodDefinition)
		{
			JitOption.JitLogError("TableRoot.instance.GetDataContainer not exist");
			return;
		}
		List<Type> dataContainerList = UsingReflection.GetDataContainerList();
		for (int i = 0; i < dataContainerList.Count; i++)
		{
			Type type = dataContainerList[i];
			MethodInfo methodInfo = method.MakeGenericMethod(type);
			TableRoot instance = Instance;
			object[] emptyTypes = Type.EmptyTypes;
			if (methodInfo.Invoke(instance, emptyTypes) == null)
			{
				JitOption.JitLogError($"TableRoot.instance.GetDataContainer<{type}> result is null");
			}
		}
	}

	public static List<TablePathDefine> GetTablePathDefine_for_PreloadInAppBytes()
	{
		List<TablePathDefine> list = new List<TablePathDefine>();
		MethodInfo method = typeof(TableRoot).GetMethod("GetDataContainer");
		if (method == null || !method.IsGenericMethodDefinition)
		{
			JitOption.JitLogError("TableRoot.instance.GetDataContainer not exist");
			return list;
		}
		List<Type> dataContainerList = UsingReflection.GetDataContainerList();
		for (int i = 0; i < dataContainerList.Count; i++)
		{
			Type type = dataContainerList[i];
			if (typeof(IData_from_PreloadInAppBytes).IsAssignableFrom(type) && Activator.CreateInstance(type) is JitDataContainerAbstract jitDataContainerAbstract)
			{
				list.Add(jitDataContainerAbstract.PathDefine);
			}
		}
		return list;
	}
}
