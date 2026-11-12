using System.Collections.Generic;
using NGameAsset.NEffect;
using NVespa;

namespace NGameAsset;

public class EffectContainer : SingletonAuto<EffectContainer>
{
	private HashSet<EffectLoad> _set = new HashSet<EffectLoad>();

	public EffectContainer()
	{
		SingletonUpdater<AssetUpdateManager>.CreateInstance();
	}

	public IAsyncLoad<IEffect> CreateAsync(string code, int layer, ITransformData extraTransform = null)
	{
		EffectLoad effectLoad = new EffectLoad();
		effectLoad.Open(code, layer, extraTransform);
		_set.Add(effectLoad);
		return effectLoad;
	}

	public static void ReleaseEffect(IAsyncLoad<IEffect> asyncLoad, bool detachParent, bool immediatelyRelease = false)
	{
		if (!SingletonAuto<EffectContainer>.isValidInstance || asyncLoad == null)
		{
			return;
		}
		if (!(asyncLoad is EffectLoad effectLoad))
		{
			VespaDebug.LogError("Invalid Effect Async: " + asyncLoad.GetType().Name);
			asyncLoad.Value.Close(detachParent);
			return;
		}
		SingletonAuto<EffectContainer>.instance._set.Remove(effectLoad);
		effectLoad.Close(detachParent);
		if (!immediatelyRelease)
		{
			AssetUpdateManager.PendingReleaseFromEffectContainer(effectLoad);
		}
		else
		{
			effectLoad.Release();
		}
	}
}
