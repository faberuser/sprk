using System.Collections.Generic;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NEffect;
using NVespa;

public static class Game2_Scene_IResourcePreloader_ExtensionMethod
{
	public static void AddAsyncLoadList<T>(this IResourcePreloader This, List<T> asyncLoadList) where T : IAsyncLoadBase
	{
		if (This != null && asyncLoadList != null)
		{
			for (int i = 0; i < asyncLoadList.Count; i++)
			{
				This.AddAsyncLoad(asyncLoadList[i]);
			}
		}
	}

	public static void ReserveCacheEffect(this IResourcePreloader This, string code)
	{
		if (This == null || String_Extension_Method.IsNullOrEmpty(code))
		{
			return;
		}
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(code);
		if (builder != null)
		{
			This.ExecuteAction(This.EffectPreloadType, delegate
			{
				This.AddAsyncLoad(builder.CreateOpenData());
			});
		}
	}

	public static void ReserveCacheSound(this IResourcePreloader This, string code)
	{
		if (This != null && !String_Extension_Method.IsNullOrEmpty(code))
		{
			This.ExecuteAction(This.SoundPreloadType, delegate
			{
				This.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound(code));
			});
		}
	}
}
