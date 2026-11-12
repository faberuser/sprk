using System;
using UnityEngine;

namespace NGameAsset.NBundleManager;

public class BundleLoadThreaded : BundleLoadBase
{
	public AssetBundle Bundle;

	protected override void OnClose()
	{
		base.OnClose();
		Bundle = null;
	}

	public override void Load(string bundlePath, Action<LoadResult> onLoad)
	{
		Tools.ReadBytesAsync(bundlePath, delegate(byte[] bytes)
		{
			LoadResult loadResult = new LoadResult();
			if (bytes == null)
			{
				loadResult.Error = "Bundle Load Fail: " + bundlePath;
				Vespa_System_Action_Extension_Method.fire(onLoad, loadResult);
			}
			else
			{
				Bundle = AssetBundle.LoadFromMemory(bytes);
				if (Bundle == null)
				{
					loadResult.Error = "Bundle Load Fail: " + bundlePath;
					Vespa_System_Action_Extension_Method.fire(onLoad, loadResult);
				}
				else
				{
					loadResult.AssetBundle = Bundle;
					Vespa_System_Action_Extension_Method.fire(onLoad, loadResult);
				}
			}
		});
	}
}
