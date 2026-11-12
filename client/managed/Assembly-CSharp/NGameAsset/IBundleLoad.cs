using System;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset;

public interface IBundleLoad : IAsyncLoadBase
{
	string BundleFileName { get; }

	ILockCounter Lock { get; }

	void StartLoad();

	UnityEngine.Object Load(string assetName, Type type);

	IRequestWrapper LoadAsync(string assetName, Type type);

	IRequestWrapper LoadMainAsset();

	UnityEngine.Object[] LoadAll();

	void Close(bool unloadAllLoadedObjects);

	bool Exists();

	bool IsDownloaded();

	void StartDownload();
}
