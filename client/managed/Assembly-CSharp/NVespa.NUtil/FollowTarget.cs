using UnityEngine;

namespace NVespa.NUtil;

public class FollowTarget : MonoBehaviour
{
	public Transform m_target;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (!(m_target == null))
		{
			base.transform.position = m_target.position;
		}
	}
}
