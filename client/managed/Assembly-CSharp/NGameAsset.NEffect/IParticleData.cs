using NVespa.NRenderProxy;

namespace NGameAsset.NEffect;

public interface IParticleData
{
	bool IsDistanceEmit { get; }

	string GetName();

	CommonData GetCommonData();

	int GetSequenceCount();

	ParticleSequenceData GetSequenceData(int index);

	IMaterialFactory GetMaterialFactory();
}
