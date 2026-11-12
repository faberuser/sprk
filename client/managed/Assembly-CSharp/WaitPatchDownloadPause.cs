using NGameAsset.NPatch;
using NVespa.NSingleton;
using UnityEngine;

public class WaitPatchDownloadPause : CustomYieldInstruction
{
	public override bool keepWaiting
	{
		get
		{
			if (!GlobalAutoBehaviour<PatchDownloader>.isValidInstance)
			{
				return false;
			}
			return GlobalAutoBehaviour<PatchDownloader>.instance.State == PatchDownloader.DownloadState.Pause;
		}
	}
}
