using NVespa;

namespace NGameAsset.NCreature;

public interface IWeaponEffectData
{
	string Code { get; }

	string TargetPivot { get; }

	TransformData ExtraTransform { get; }
}
