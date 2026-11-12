using NGame2.N3D;
using NGame2.NBattleView;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NRender;
using NGameAsset;
using UnityEngine;

namespace NGame2.NNodeTree;

public class NullSceneContainer : ISceneContainer
{
	public static NullSceneContainer Instance = new NullSceneContainer();

	ICamera ISceneContainer.GetCamera()
	{
		return null;
	}

	RenderFacade ISceneContainer.GetRenderFacade(int uid)
	{
		return null;
	}

	Creature ISceneContainer.GetBattleCreature(int uid)
	{
		return null;
	}

	int ISceneContainer.GetCreatureCount()
	{
		return 0;
	}

	Creature ISceneContainer.GetCreatureByIndex(int index)
	{
		return null;
	}

	int ISceneContainer.SpawnRenderFacade(StringKey key, RenderFacadeBuilder renderFacadeBuilder)
	{
		return 0;
	}

	void ISceneContainer.SpawnAssetEffect(IAsyncLoad<IEffect> effectAsync)
	{
		EffectContainer.ReleaseEffect(effectAsync, detachParent: true, immediatelyRelease: true);
	}

	void ISceneContainer.ReleaseAssetEffect(string code)
	{
	}

	void ISceneContainer.LoadBattleField(string code, string prefix, Vector3 position, Quaternion rotation, float scale, float backgroundAngle)
	{
	}
}
