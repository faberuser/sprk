using System;
using NGameAsset.NBundleManager;
using NVespa;

namespace NGameAsset;

public class BundleManager : SingletonAuto<BundleManager>
{
	public interface ILoader
	{
		int LoadingCount { get; }

		int BundleCount { get; }

		bool IsReady { get; }

		bool IsLoadShader { get; set; }

		bool IsLoadPack { get; set; }

		BundleInfoContainer InfoContainer { get; set; }

		IBundleLoad LockBundle(string bundleFileName);

		void ReleaseBundle(IBundleLoad bundleLoad, bool destroyImmediately);

		void DestroyUnused(bool unusedAll);

		void ListenOnReady(Action onReady);

		string GetSummary();

		void Load();
	}

	public bool IsReady => Loader.IsReady;

	public bool IsLoadShader => Loader.IsLoadShader;

	public bool IsLoadPack => Loader.IsLoadPack;

	public ILoader Loader { get; private set; }

	public Action<string> OnMessage { get; set; }

	public BundleManager()
	{
		Loader = SingletonUpdater<LoaderPlaying>.instance;
	}

	public void Load()
	{
		if (Loader != null)
		{
			Loader.Load();
		}
	}

	public BundleInfoContainer GetInfoContainer()
	{
		return Loader.InfoContainer;
	}

	public IBundleLoad LockBundle(string bundleFileName)
	{
		return Loader.LockBundle(bundleFileName);
	}

	public static void ReleaseBundle(IBundleLoad bundleLoad, bool destroyImmediately)
	{
		if (SingletonAuto<BundleManager>.isValidInstance)
		{
			SingletonAuto<BundleManager>.instance.Loader.ReleaseBundle(bundleLoad, destroyImmediately);
		}
	}

	public static void DestroyUnused(bool unusedAll)
	{
		if (SingletonAuto<BundleManager>.isValidInstance)
		{
			SingletonAuto<BundleManager>.instance.Loader.DestroyUnused(unusedAll);
		}
	}

	public string GetSummary()
	{
		if (Loader == null)
		{
			return "(empty)";
		}
		return Loader.GetSummary();
	}
}
