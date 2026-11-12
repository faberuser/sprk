using System;
using NGame2.NRender;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NCinema;

public class RenderFacadeInteractionProvider : IInteractionDataProvider
{
	private Action _onClick;

	private string _code;

	private RenderFacade _renderFacade;

	public string Code => _code;

	public Vector3 BoxColliderCenter
	{
		get
		{
			if (_renderFacade == null)
			{
				return Vector3.zero;
			}
			return _renderFacade.ShapeBounds.center;
		}
	}

	public Vector3 BoxColliderSize
	{
		get
		{
			if (_renderFacade == null)
			{
				return Vector3.zero;
			}
			return _renderFacade.ShapeBounds.size;
		}
	}

	public Action OnClick => _onClick;

	public RenderFacadeInteractionProvider(string code, Action onClick)
	{
		_code = code;
		_onClick = onClick;
	}

	public void SetRenderfacade(RenderFacade renderFacade)
	{
		_renderFacade = renderFacade;
	}
}
