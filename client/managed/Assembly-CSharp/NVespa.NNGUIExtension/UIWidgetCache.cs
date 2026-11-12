using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UIWidgetCache : MonoBehaviour, IInspectorGuiDrawer
{
	private UIPanel _panel;

	public UIPanel CreatePanel()
	{
		if (_panel == null)
		{
			_panel = UIPanel.Find(base.transform, createIfMissing: true, base.gameObject.layer);
		}
		return _panel;
	}

	private void OnDisable()
	{
		_panel = null;
	}

	private void OnDestroy()
	{
		_panel = null;
	}
}
