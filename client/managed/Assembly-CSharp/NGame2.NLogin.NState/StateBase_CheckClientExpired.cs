using System.Collections;
using NGame2.NScene;
using NGameAsset.NPatch;
using NShared.NLocalization;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NLogin.NState;

public class StateBase_CheckClientExpired : StateBase
{
	public override void OnBegin()
	{
		base.OnBegin();
		StartCoroutine(coEntry());
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
		StateOwner.SetNextState(new StateBase_SetupLoginEnvironment());
	}

	public override IEnumerator coRun()
	{
		int remainSecond = 0;
		if (StateBase.ClientConfig.GetRemainExpireSecond(out remainSecond) && remainSecond <= 0)
		{
			IoHelper.ClearDirectoryRecursive(PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, ""));
			yield return new WaitErrorReport(NShared.NLocalization.System.APP_EXPIRED, $"App expires at : {StateBase.ClientConfig.ExpireDate}", delegate
			{
				GlobalAutoBehaviour<LoadingManager>.instance.Quit();
			});
		}
	}
}
