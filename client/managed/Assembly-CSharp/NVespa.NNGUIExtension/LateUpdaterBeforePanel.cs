using UnityEngine;

namespace NVespa.NNGUIExtension;

public class LateUpdaterBeforePanel : MonoBehaviour
{
	public interface IReceiver
	{
		void OnUpdate();
	}

	private void Awake()
	{
		if (!UIPanel.lateUpdateBeforePanelList.Contains(base.gameObject))
		{
			UIPanel.lateUpdateBeforePanelList.Add(base.gameObject);
		}
	}

	private void OnEnable()
	{
		if (!UIPanel.lateUpdateBeforePanelList.Contains(base.gameObject))
		{
			UIPanel.lateUpdateBeforePanelList.Add(base.gameObject);
		}
	}

	private void OnDisable()
	{
		UIPanel.lateUpdateBeforePanelList.Remove(base.gameObject);
	}

	private void OnDestroy()
	{
		UIPanel.lateUpdateBeforePanelList.Remove(base.gameObject);
	}
}
