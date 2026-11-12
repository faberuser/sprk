using UnityEngine;

namespace NVespa.NNGUIExtension;

public class LateUpdaterAfterPanel : MonoBehaviour
{
	public interface IReceiver
	{
		void OnUpdate();
	}

	private void Awake()
	{
		if (!UIPanel.lateUpdateAfterPanelList.Contains(base.gameObject))
		{
			UIPanel.lateUpdateAfterPanelList.Add(base.gameObject);
		}
	}

	private void OnEnable()
	{
		if (!UIPanel.lateUpdateAfterPanelList.Contains(base.gameObject))
		{
			UIPanel.lateUpdateAfterPanelList.Add(base.gameObject);
		}
	}

	private void OnDisable()
	{
		UIPanel.lateUpdateAfterPanelList.Remove(base.gameObject);
	}

	private void OnDestroy()
	{
		UIPanel.lateUpdateAfterPanelList.Remove(base.gameObject);
	}
}
