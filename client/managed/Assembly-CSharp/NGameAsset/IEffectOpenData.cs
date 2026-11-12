using NGameAsset.NEffect;
using NGameAsset.NSound;
using UnityEngine;

namespace NGameAsset;

public interface IEffectOpenData
{
	string GetCode();

	BaseData GetBaseData();

	int GetModelCount();

	IModelData GetModelData(int index);

	IAssetFactory<GameObject> GetModelFactory(int index);

	int GetParticleCount();

	IParticleData GetParticleData(int index);

	IAssetFactory<GameObject> GetParticleFactory(int index);

	int GetTrailCount();

	ITrailData GetTrailData(int index);

	IAssetFactory<GameObject> GetTrailFactory(int index);

	int GetSoundFxCount();

	SoundFxData GetSoundFxData(int index);
}
