using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NCutScene;

public class CutSceneForeground : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/CutScene/CutSceneForeground";

	[SerializeField]
	private GameObject TextPrefab;

	[SerializeField]
	private Transform RootTr;

	[SerializeField]
	private UIPanel _rootPanel;

	[SerializeField]
	private UITexture _baseColorTex;

	[SerializeField]
	private UIWidget _decorateWidget;

	private string _lastDecorateName = string.Empty;

	private string _curDecorateName = string.Empty;

	private List<CutSceneForegroundTextComponent> TextComponentList = new List<CutSceneForegroundTextComponent>();

	private List<CutSceneForegroundComponent> ComponentList = new List<CutSceneForegroundComponent>();

	public Color BaseColor
	{
		get
		{
			return _baseColorTex.color;
		}
		set
		{
			_baseColorTex.color = value;
		}
	}

	public void Open(Color color, string decorateName, float blendTime)
	{
		base.Open();
		_baseColorTex.color = color;
		_lastDecorateName = _curDecorateName;
		_curDecorateName = decorateName;
		InitDescroateWidget();
		VespaCoroutine.StopAll(this);
		UIHelper.EnableGameObject(_decorateWidget, enable: true);
		if (blendTime < Mathf.Epsilon)
		{
			CompleteDecorateBlend();
		}
		else
		{
			VespaCoroutine.Start(this, coBlendDecorate(blendTime));
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_lastDecorateName = string.Empty;
		_curDecorateName = string.Empty;
		CompleteDecorateBlend();
		ClearComponentList();
	}

	public void ClearList()
	{
		foreach (CutSceneForegroundTextComponent textComponent in TextComponentList)
		{
			VespaObject.DestroyGo(textComponent);
		}
		TextComponentList.Clear();
	}

	public void ClearComponentList()
	{
		foreach (CutSceneForegroundComponent component in ComponentList)
		{
			CutSceneForegroundComponent comp = component;
			FactoryBase.SafeReleaseComponent(ref comp);
		}
		ComponentList.Clear();
	}

	public override void Reset()
	{
		base.Reset();
		ClearList();
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(_rootPanel, show);
	}

	private void InitDescroateWidget()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(ComponentList))
		{
			return;
		}
		GameRoot.Instance.CutSceneForegroundDataContainer.ForEachData(delegate(CutSceneForegroundData data)
		{
			CutSceneForegroundComponent cutSceneForegroundComponent = CutSceneForegroundComponent.PrefabAuto.Factory.Create(_decorateWidget.transform);
			if (cutSceneForegroundComponent != null)
			{
				cutSceneForegroundComponent.Open(RootTr, data.DecorateWidget, data.AssetPath);
				ComponentList.Add(cutSceneForegroundComponent);
			}
		});
	}

	private IEnumerator coBlendDecorate(float blendTime)
	{
		if (_decorateWidget == null)
		{
			yield break;
		}
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += (float)UnitySystemTimer.unscaledDeltaTick * 0.001f;
			float num = Mathf.Clamp01(elapsedTime / Vespa_Float_Extension_Method.NoneZero(blendTime));
			for (int i = 0; i < _decorateWidget.transform.childCount; i++)
			{
				Transform child = _decorateWidget.transform.GetChild(i);
				if (!(child == null))
				{
					UIWidget component = child.gameObject.GetComponent<UIWidget>();
					if (Vespa_Unity_Object_Extension_Method.NAME(child) == _curDecorateName)
					{
						UIHelper.EnableGameObject(child.gameObject, enable: true);
						UIHelper.SetWidgetAlpha(component, num);
					}
					else if (Vespa_Unity_Object_Extension_Method.NAME(child) == _lastDecorateName && component != null)
					{
						UIHelper.EnableGameObject(child.gameObject, enable: true);
						UIHelper.SetWidgetAlpha(child.gameObject.GetComponent<UIWidget>(), Mathf.Min(component.alpha, 1f - num));
					}
					else
					{
						UIHelper.EnableGameObject(child.gameObject, enable: false);
					}
				}
			}
		}
		while (!(elapsedTime >= blendTime));
		CompleteDecorateBlend();
	}

	private void CompleteDecorateBlend()
	{
		if (_decorateWidget == null)
		{
			return;
		}
		for (int i = 0; i < _decorateWidget.transform.childCount; i++)
		{
			Transform child = _decorateWidget.transform.GetChild(i);
			if (!(child == null))
			{
				bool flag = Vespa_Unity_Object_Extension_Method.NAME(child) == _curDecorateName;
				UIHelper.EnableGameObject(child.gameObject, flag);
				if (flag)
				{
					UIHelper.SetWidgetAlpha(child.gameObject.GetComponent<UIWidget>(), 1f);
				}
			}
		}
	}

	public CutSceneForegroundTextComponent AddTextComponent()
	{
		GameObject obj = NGUITools.AddChild(WindowRoot.gameObject, TextPrefab);
		obj.SetActive(value: true);
		CutSceneForegroundTextComponent component = obj.GetComponent<CutSceneForegroundTextComponent>();
		TextComponentList.Add(component);
		return component;
	}

	public CutSceneForegroundTextComponent GetTextComponent(int index)
	{
		if (TextComponentList.Count <= index)
		{
			return null;
		}
		return TextComponentList[index];
	}
}
