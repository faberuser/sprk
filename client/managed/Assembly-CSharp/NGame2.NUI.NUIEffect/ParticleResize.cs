using System;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NUIEffect;

public class ParticleResize : MonoBehaviour, IInspectorGuiDrawer
{
	public bool UseCustomUIPanel;

	[ExecuteInEditMode]
	private void Start()
	{
		if (!UseCustomUIPanel)
		{
			Resize();
		}
	}

	public void Resize(UIPanel customPanel)
	{
		if (!(customPanel == null))
		{
			CalculateParticleSize(customPanel);
		}
	}

	private void Resize()
	{
		UIPanel basePanel = GetBasePanel();
		if (basePanel == null)
		{
			VespaDebug.LogError($"Cannot find UI panel from heirarchy.");
		}
		else
		{
			CalculateParticleSize(basePanel);
		}
	}

	private UIPanel GetBasePanel()
	{
		UIPanel uIPanel = base.gameObject.GetComponent<UIPanel>();
		if (uIPanel == null)
		{
			uIPanel = base.gameObject.GetComponentInParent<UIPanel>();
		}
		if (uIPanel == null)
		{
			UIPanel[] componentsInChildren = base.gameObject.GetComponentsInChildren<UIPanel>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				uIPanel = componentsInChildren[0];
			}
		}
		return uIPanel;
	}

	protected void CalculateParticleSize(UIPanel basePanel)
	{
		try
		{
			int num = 1920;
			int num2 = 1080;
			float num3 = basePanel.finalClipRegion.w / basePanel.finalClipRegion.z * (float)num;
			float num4 = (float)num2 / num3;
			if (num3 <= (float)num2)
			{
				num4 = 1f;
				return;
			}
			ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
			foreach (ParticleSystem obj in componentsInChildren)
			{
				Vector3 localScale = obj.gameObject.transform.localScale;
				localScale *= num4;
				obj.gameObject.transform.localScale = localScale;
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Particle Resize : " + ex.ToString());
		}
	}
}
