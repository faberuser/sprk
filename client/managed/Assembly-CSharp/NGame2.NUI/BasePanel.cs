using NVespa.NNGUIExtension.NAnimation;
using UnityEngine;

namespace NGame2.NUI;

public class BasePanel : IPanel, IPanelDrawInspector, IPanelAnimation
{
	public GameObject PanelRoot;

	public virtual UIComponetAnimationPlayData ShowHideAnimationPlayData => null;

	public virtual bool IsOpen()
	{
		if (PanelRoot == null)
		{
			return false;
		}
		return PanelRoot.activeSelf;
	}

	public virtual void Show()
	{
		OnOffRoot(isOn: true);
	}

	public virtual void Hide()
	{
		OnOffRoot(isOn: false);
	}

	public virtual void Close()
	{
	}

	private void OnOffRoot(bool isOn)
	{
		UIHelper.EnableGameObject(PanelRoot, isOn);
	}

	public virtual void DrawInspactor()
	{
	}
}
