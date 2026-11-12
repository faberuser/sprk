using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NSceneContainer;

public interface ICamera
{
	Camera GetUnityCamera();

	ShakeMaker GetCameraShake();

	void SetAlphaToggleEntry(AlphaToggleEntry entry);

	void SetAlphaToggleEntry2(AlphaToggleEntry2 entry);

	void SetFocusEntry(IFocusCameraEntry entry);

	void SetEventEntry(IEventCameraEntry entry);

	void ReleaseEventEntry(IEventCameraEntry entry);
}
