using System;
using System.Collections.Generic;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NCodec;

public class CodecReadManager
{
	private Dictionary<string, EVideoSpecType> dicSpecType;

	public void InitSpecType()
	{
		dicSpecType = new Dictionary<string, EVideoSpecType>();
		dicSpecType.Add("high", EVideoSpecType.High);
		dicSpecType.Add("mid", EVideoSpecType.Mid);
		dicSpecType.Add("low", EVideoSpecType.Low);
	}

	public EVideoSpecType GetVideoSpecType(ref string[] refDeviceCodecs)
	{
		EVideoSpecType result = EVideoSpecType.Mid;
		IList<CodecSupportListData> allData = GameRoot.Instance.CodecSupportListContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return result;
		}
		foreach (CodecSupportListData item in allData)
		{
			if (!String_Extension_Method.IsNullOrEmpty(item.OS) && (item.OS != Application.platform.ToString() || (!String_Extension_Method.IsNullOrEmpty(item.OSVersion) && !IsContainOrdinalIgnoreCase(SystemInfo.operatingSystem, item.OSVersion)) || (!String_Extension_Method.IsNullOrEmpty(item.ModelManufacturer) && !IsContainOrdinalIgnoreCase(SystemInfo.deviceModel, item.ModelManufacturer))))
			{
				continue;
			}
			if (!String_Extension_Method.IsNullOrEmpty(item.CodecType))
			{
				bool flag = false;
				for (int i = 0; i < refDeviceCodecs.Length; i++)
				{
					if (IsContainOrdinalIgnoreCase(refDeviceCodecs[i], item.VideoType) && IsContainOrdinalIgnoreCase(refDeviceCodecs[i], item.CodecType))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			try
			{
				result = dicSpecType[item.TargetSpec];
			}
			catch (Exception obj)
			{
				VespaDebug.LogError(obj);
			}
			return result;
		}
		return result;
	}

	private bool IsContainOrdinalIgnoreCase(string InclusiveString, string FindString)
	{
		return InclusiveString.IndexOf(FindString, StringComparison.OrdinalIgnoreCase) >= 0;
	}
}
