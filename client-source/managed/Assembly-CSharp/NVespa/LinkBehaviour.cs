using UnityEngine;

namespace NVespa;

public class LinkBehaviour : MonoBehaviour
{
	public LinkBehaviour m_linkedParent;

	private int __link_frameCount;

	private void LateUpdate()
	{
	}

	private void internalLinkUpdate()
	{
		if (__link_frameCount != Time.renderedFrameCount)
		{
			__link_frameCount = Time.renderedFrameCount;
			if ((bool)m_linkedParent)
			{
				m_linkedParent.internalLinkUpdate();
			}
			LinkUpdate();
		}
	}

	public virtual void LinkUpdate()
	{
	}
}
