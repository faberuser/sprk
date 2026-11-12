using System;
using NGameAsset;

namespace NGame2.NScene;

public interface IResourcePreloader
{
	object ExtraData { get; }

	EAsyncPreloadType CreaturePreloadType { get; }

	EAsyncPreloadType EffectPreloadType { get; }

	EAsyncPreloadType SoundPreloadType { get; }

	void ExecuteAction(EAsyncPreloadType preloadType, Action action);

	void AddAsyncLoad(IAsyncLoadBase asyncLoad);

	void ReserveAsyncLoad(IAsyncLoadBase asyncLoad);

	void AddNodeTree(string packPathName, long treeUid);

	void AddAction(Action action, string desc);

	void AddCompleteFunc(Func<bool> func, string desc);

	void ReleaseAsyncLoadAll();
}
