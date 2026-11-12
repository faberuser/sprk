using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class DeviceModelDataContainer : JitDataContainerDefault<int, DeviceModelData>
{
	public const string TABLENAME = "DeviceModelTable";

	public static List<Action<bool>> ObserverLisrt = new List<Action<bool>>();

	public override TablePathDefine PathDefine => new TablePathDefine("DeviceModelTable");

	public static void AddObserver(Action<bool> act)
	{
		if (!ObserverLisrt.Contains(act))
		{
			ObserverLisrt.Add(act);
		}
	}

	public static void RemoveObserver(Action<bool> act)
	{
		if (ObserverLisrt.Contains(act))
		{
			ObserverLisrt.Remove(act);
		}
	}

	protected override void OnEndLoad_Final()
	{
		for (int i = 0; i < ObserverLisrt.Count; i++)
		{
			ObserverLisrt[i]?.Invoke(obj: true);
		}
	}

	public string GetMemorySpec(string deviceModel)
	{
		DeviceModelData data = GetData(deviceModel, (DeviceModelData deviceModelData) => string.IsNullOrEmpty(deviceModelData.ForceMemorySpec));
		if (data == null)
		{
			return string.Empty;
		}
		return data.ForceMemorySpec;
	}

	public DeviceStyleType GetDeviceStyleType(string deviceModel)
	{
		return GetData(deviceModel, (DeviceModelData data) => data.DeviceStyle == DeviceStyleType.None)?.DeviceStyle ?? DeviceStyleType.None;
	}

	public DeviceModelData GetData(string deviceModel, Func<DeviceModelData, bool> IgnoreFunc = null)
	{
		foreach (DeviceModelData allDatum in GetAllData())
		{
			if ((IgnoreFunc == null || !IgnoreFunc(allDatum)) && allDatum.CompareDeviceModel(deviceModel))
			{
				return allDatum;
			}
		}
		return null;
	}
}
