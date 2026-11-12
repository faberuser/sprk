using System;
using NVespa;
using UnityEngine;

namespace NGameAsset.NBundleManager;

public class BundleLoadFromFileAsync : BundleLoadBase
{
	public AssetBundle Bundle;

	protected override void OnClose()
	{
		base.OnClose();
	}

	public override void Load(string bundlePath, Action<LoadResult> onLoad)
	{
		if (SingletonUpdater<AsyncManager>.instance.ForceSyncLoad)
		{
			LoadResult loadResult = new LoadResult();
			try
			{
				loadResult.AssetBundle = AssetBundle.LoadFromFile(bundlePath);
				return;
			}
			catch (Exception ex)
			{
				loadResult.Error = ex.Message;
				return;
			}
			finally
			{
				Vespa_System_Action_Extension_Method.fire(onLoad, loadResult);
			}
		}
		AssetBundleCreateRequest request;
		try
		{
			request = AssetBundle.LoadFromFileAsync(bundlePath);
		}
		catch (Exception ex2)
		{
			LoadResult loadResult2 = new LoadResult();
			loadResult2.Error = ex2.Message;
			Vespa_System_Action_Extension_Method.fire(onLoad, loadResult2);
			return;
		}
		SingletonUpdater<AsyncManager>.instance.Add(request, delegate
		{
			Bundle = request.assetBundle;
			LoadResult loadResult3 = new LoadResult();
			if (Bundle == null)
			{
				loadResult3.Error = "Bundle Load Fail: " + bundlePath;
				Vespa_System_Action_Extension_Method.fire(onLoad, loadResult3);
			}
			else
			{
				loadResult3.AssetBundle = Bundle;
				Vespa_System_Action_Extension_Method.fire(onLoad, loadResult3);
			}
		});
	}
}
