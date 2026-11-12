using System;
using NGame2.NRender;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class NpcTouchBehaviour : MonoBehaviour, UICamera.IOnClickListener
{
	private RenderFacade _renderFacade;

	private BoxCollider _boxCollider;

	private Action _clickAction;

	public void Open(RenderFacade renderFacade, Action clickAction)
	{
		_renderFacade = renderFacade;
		_boxCollider = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<BoxCollider>(base.gameObject);
		_boxCollider.center = _renderFacade.ShapeBounds.center;
		_boxCollider.size = _renderFacade.ShapeBounds.size;
		_clickAction = clickAction;
		OnOffBoxCollider(isOn: true);
	}

	public void OnOffBoxCollider(bool isOn)
	{
		if (_boxCollider != null)
		{
			_boxCollider.enabled = isOn;
		}
	}

	public void OnClick()
	{
		Vespa_System_Action_Extension_Method.fire(_clickAction);
	}
}
