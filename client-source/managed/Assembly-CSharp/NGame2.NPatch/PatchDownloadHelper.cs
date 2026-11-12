using NGame2.NDevice;
using NGame2.NUI;
using NShared;
using UnityEngine;

namespace NGame2.NPatch;

public class PatchDownloadHelper
{
	public static PatchDownloadSpecData GetRecommandSpecData()
	{
		if (DeviceHelper.IsCpuFamily_x86())
		{
			return GameRoot.Instance.PatchDownloadSpecDataContainer.GetRecommendedSpecData(PatchPlatform.x86, SystemInfo.systemMemorySize, UIHelper.IsATSCSupported());
		}
		return GameRoot.Instance.PatchDownloadSpecDataContainer.GetRecommendedSpecData(PatchPlatform.Android, SystemInfo.systemMemorySize, UIHelper.IsATSCSupported());
	}

	public static PatchPlatform GetCurrentPlatform()
	{
		if (DeviceHelper.IsCpuFamily_x86())
		{
			return PatchPlatform.x86;
		}
		return PatchPlatform.Android;
	}
}
