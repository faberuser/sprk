using System;
using NGame2.NRender;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI;

[Serializable]
public class Monster3DWidget
{
	public UICreatureWidget CreatureWidget;

	public Transform TransCreatureRoot;

	public Vector3 ShadowDirection;

	private WeakRef<RenderFacade> _weakRenderFacade;

	private CreatureData _creatureData;

	private RenderFacade _renderFacade
	{
		get
		{
			if (_weakRenderFacade == null)
			{
				return null;
			}
			if (_weakRenderFacade.Value == null)
			{
				return null;
			}
			return _weakRenderFacade.Value;
		}
	}

	public void Setup(CreatureData creatureData)
	{
		_creatureData = creatureData;
		CreateCreatureWidget();
	}

	public void ChangeCreatureWidgetTransform(Vector3 localPosition, float scale)
	{
		if (TransCreatureRoot != null)
		{
			TransCreatureRoot.localPosition = localPosition;
			TransCreatureRoot.localScale = Vector3.one * scale;
		}
	}

	public void CreateCreatureWidget()
	{
		if (!(CreatureWidget == null))
		{
			CreatureWidget.Open(_creatureData, 0);
			if (CreatureWidget.RenderFacade != null)
			{
				_weakRenderFacade = new WeakRef<RenderFacade>(CreatureWidget.RenderFacade);
				CreatureWidget.RenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
			}
			if (GameRoot.Instance.ClientCreatureDataContainer.GetData(_creatureData.CodeName) != null)
			{
				int num = 20;
				CreatureWidget.PivotTm.localScale = Vector3.one * num;
			}
		}
	}

	public void SetAnimation(string animationKey)
	{
		if (!(CreatureWidget == null))
		{
			CreatureWidget.SetNamedAction(animationKey);
		}
	}

	public void Close()
	{
		CreatureWidget.Close();
		_weakRenderFacade = null;
	}
}
