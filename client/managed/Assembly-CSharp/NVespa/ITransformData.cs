using UnityEngine;

namespace NVespa;

public interface ITransformData
{
	Vector3 Position { get; }

	Quaternion Rotation { get; }

	float Scale { get; }
}
