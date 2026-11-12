using System;
using UnityEngine;

namespace NGameAsset.NBundleManager;

public class BundleLoadFromFile : BundleLoadBase
{
	protected override void OnClose()
	{
		base.OnClose();
	}

	public override void Load(string bundlePath, Action<LoadResult> onLoad)
	{
		LoadResult loadResult = new LoadResult();
		try
		{
			loadResult.AssetBundle = AssetBundle.LoadFromFile(bundlePath);
		}
		catch (Exception ex)
		{
			loadResult.Error = ex.Message;
		}
		Vespa_System_Action_Extension_Method.fire(onLoad, loadResult);
	}
}
