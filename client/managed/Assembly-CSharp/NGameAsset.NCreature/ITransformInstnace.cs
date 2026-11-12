using UnityEngine;

namespace NGameAsset.NCreature;

public interface ITransformInstnace
{
	void SetPosition(Vector3 position);

	void SetRotation(Vector3 angle);

	void SetScale(float scale);

	Vector3 GetPosition();

	Vector3 GetRotation();

	float GetScale();
}
