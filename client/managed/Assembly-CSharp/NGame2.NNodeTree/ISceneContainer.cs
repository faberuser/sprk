using NGame2.N3D;
using NGame2.NBattleView;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NRender;
using NGameAsset;
using UnityEngine;

namespace NGame2.NNodeTree;

public interface ISceneContainer
{
	ICamera GetCamera();

	RenderFacade GetRenderFacade(int uid);

	Creature GetBattleCreature(int uid);

	int GetCreatureCount();

	Creature GetCreatureByIndex(int index);

	int SpawnRenderFacade(StringKey key, RenderFacadeBuilder renderFacadeBuilder);

	void SpawnAssetEffect(IAsyncLoad<IEffect> effectAsync);

	void ReleaseAssetEffect(string code);

	void LoadBattleField(string code, string prefix, Vector3 position, Quaternion rotation, float scale, float backgroundAngle);
}
