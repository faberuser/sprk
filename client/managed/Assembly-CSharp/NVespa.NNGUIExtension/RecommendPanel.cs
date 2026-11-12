using System;

namespace NVespa.NNGUIExtension;

public class RecommendPanel : IDisposable
{
	private UIPanel _origPanel;

	public RecommendPanel(UIPanel panel)
	{
		_origPanel = UIPanel.RecommendedPanel;
		UIPanel.RecommendedPanel = panel;
	}

	public virtual void Dispose()
	{
		UIPanel.RecommendedPanel = _origPanel;
		_origPanel = null;
	}
}
