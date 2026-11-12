using UnityEngine;

namespace NVespa.NUtil;

public class AllowScreenRotation : MonoBehaviour
{
	public bool m_landscape;

	public bool m_portrait;

	private void Start()
	{
		if (m_landscape)
		{
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
		}
		if (m_portrait)
		{
			Screen.autorotateToPortrait = true;
			Screen.autorotateToPortraitUpsideDown = true;
		}
	}
}
