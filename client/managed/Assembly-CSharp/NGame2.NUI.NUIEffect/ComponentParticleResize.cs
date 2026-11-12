using NGame2.NUI.NWindow;
using NVespa;

namespace NGame2.NUI.NUIEffect;

public class ComponentParticleResize : ParticleResize
{
	private void Start()
	{
		UIPanel basePanelFromBaseWindow = GetBasePanelFromBaseWindow();
		if (basePanelFromBaseWindow == null)
		{
			VespaDebug.LogError($"Cannot find UI panel from heirarchy.");
		}
		else
		{
			CalculateParticleSize(basePanelFromBaseWindow);
		}
	}

	private UIPanel GetBasePanelFromBaseWindow()
	{
		BaseWindow componentInParent = GetComponentInParent<BaseWindow>();
		if (componentInParent == null)
		{
			return null;
		}
		UIPanel uIPanel = componentInParent.GetComponent<UIPanel>();
		if (uIPanel == null)
		{
			uIPanel = componentInParent.GetComponentInParent<UIPanel>();
		}
		if (uIPanel == null)
		{
			UIPanel[] componentsInChildren = componentInParent.GetComponentsInChildren<UIPanel>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				uIPanel = componentsInChildren[0];
			}
		}
		return uIPanel;
	}
}
