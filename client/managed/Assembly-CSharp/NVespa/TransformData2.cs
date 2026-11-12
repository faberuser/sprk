using UnityEngine;

namespace NVespa;

public class TransformData2 : ITransformData
{
	public Vector3 Position = Vector3.zero;

	public Quaternion Rotation = Quaternion.identity;

	public float Scale = 1f;

	Vector3 ITransformData.Position => Position;

	Quaternion ITransformData.Rotation => Rotation;

	float ITransformData.Scale => Scale;
}
