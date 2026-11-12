using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NUtil;

[ExecuteInEditMode]
public class TargetCamera : SceneBehaviour<TargetCamera>
{
	public enum ECalculate
	{
		Disable,
		Enable,
		Reverse
	}

	public Camera m_camera;

	public GameObject m_target;

	private void Start()
	{
	}

	private void Update()
	{
	}
}
