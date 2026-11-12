using NGame2.NRender;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3D;

public abstract class OnFieldDisplayBase : LinkFactoryComponent
{
	public Transform BaseTransform;

	public UIWidget Root;

	private Transform _followTm;

	private RenderFacade _renderFacade;

	private Vector3 _offset;

	public void InitPosition(Camera cam, Transform followTm, Vector3 offset, bool followCreature)
	{
		_followTm = (followCreature ? _followTm : null);
		_offset = offset;
		if (followTm != null)
		{
			base.transform.position = followTm.position + _offset;
		}
		else
		{
			base.transform.position = _offset;
		}
		if (cam != null)
		{
			base.transform.rotation = cam.transform.rotation;
		}
	}

	public void Translate(Vector3 delta)
	{
		_offset += delta;
		if (_renderFacade != null)
		{
			base.transform.position = _renderFacade.transform.position + _offset;
		}
		else
		{
			base.transform.Translate(delta);
		}
	}
}
