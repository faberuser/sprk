using UnityEngine;

namespace NGame2.NNodeTree.NSceneContainer;

public interface IEventCameraEntry
{
	int EventPriority { get; }

	float BattleFieldAlpha { get; }

	float UiAlpha { get; }

	float UiOn3dAlpha { get; }

	float ShakeWeight { get; }

	bool FadeInEnd { get; }

	void UpdateCamera(ref Vector3 targetPos, ref Quaternion cameraRot, ref float cameraDistance, ref float fieldOfView);
}
