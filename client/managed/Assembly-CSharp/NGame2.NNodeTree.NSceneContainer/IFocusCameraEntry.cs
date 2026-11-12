using UnityEngine;

namespace NGame2.NNodeTree.NSceneContainer;

public interface IFocusCameraEntry
{
	float Weight { get; }

	bool Released { get; }

	void UpdateCamera(ref Vector3 targetPos, ref Quaternion cameraRot, ref float cameraDistance, ref float fieldOfView);

	void ReleaseFocus();
}
