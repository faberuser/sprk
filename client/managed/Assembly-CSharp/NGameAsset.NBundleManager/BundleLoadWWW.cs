using System;
using NVespa;
using UnityEngine;

namespace NGameAsset.NBundleManager;

public class BundleLoadWWW : BundleLoadBase
{
	public WWW www;

	protected override void OnClose()
	{
		base.OnClose();
		if (www != null)
		{
			www.Dispose();
			www = null;
		}
	}

	public override void Load(string bundlePath, Action<LoadResult> onLoad)
	{
		www = new WWW("file://" + bundlePath);
		SingletonUpdater<AsyncManager>.instance.Add(www, delegate
		{
			LoadResult loadResult = new LoadResult
			{
				Error = www.error
			};
			if (!String_Extension_Method.IsNullOrEmpty(loadResult.Error))
			{
				Debug.LogError("Bundle Load Fail: " + bundlePath + " error=" + loadResult.Error);
			}
			else if (www.assetBundle == null)
			{
				loadResult.Error = "Bundle Loaded but null: " + bundlePath;
			}
			loadResult.AssetBundle = www.assetBundle;
			Vespa_System_Action_Extension_Method.fire(onLoad, loadResult);
		});
	}
}
