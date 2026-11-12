using System.Collections.Generic;
using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

namespace NVespa;

internal class TrailProxy
{
	private GameObject _trailGo;

	private List<TrailRenderer_Base> _trailList = new List<TrailRenderer_Base>();

	private bool _renderEnabled = true;

	public bool RenderEnabled
	{
		get
		{
			return _renderEnabled;
		}
		set
		{
			if (_renderEnabled != value)
			{
				_renderEnabled = value;
			}
		}
	}

	public bool IsPlaying { get; private set; }

	public bool Open(GameObject trailGo)
	{
		_trailGo = trailGo;
		TrailRenderer_Base[] componentsInChildren = _trailGo.GetComponentsInChildren<TrailRenderer_Base>(includeInactive: true);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(componentsInChildren))
		{
			return false;
		}
		foreach (TrailRenderer_Base trailRenderer_Base in componentsInChildren)
		{
			trailRenderer_Base.Emit = false;
			_trailList.Add(trailRenderer_Base);
		}
		IsPlaying = false;
		return true;
	}

	public void Close()
	{
		if (_trailGo != null)
		{
			for (int i = 0; i < _trailList.Count; i++)
			{
				_trailList[i].ClearSystem(emitState: false);
			}
			_trailList.Clear();
			_trailGo = null;
		}
	}

	public void ChainUpdate(ILocalTimer timer, float deltaTime)
	{
		if (_trailGo != null)
		{
			for (int i = 0; i < _trailList.Count; i++)
			{
				_trailList[i].ChainUpdate(timer, deltaTime);
			}
		}
	}

	public void Play()
	{
		IsPlaying = true;
		if (!(_trailGo == null))
		{
			for (int i = 0; i < _trailList.Count; i++)
			{
				_trailList[i].Emit = true;
			}
		}
	}

	public void Stop()
	{
		IsPlaying = false;
		if (!(_trailGo == null))
		{
			for (int i = 0; i < _trailList.Count; i++)
			{
				_trailList[i].Emit = false;
			}
		}
	}

	public void Clear()
	{
		if (!(_trailGo == null))
		{
			for (int i = 0; i < _trailList.Count; i++)
			{
				_trailList[i].ClearSystem(_trailList[i].Emit);
			}
		}
	}

	public bool IsEmpty()
	{
		if (_trailGo == null)
		{
			return true;
		}
		for (int i = 0; i < _trailList.Count; i++)
		{
			if (_trailList[i].NumSegments() > 0)
			{
				return false;
			}
		}
		return true;
	}

	public void Visit(IVisitor visitor)
	{
	}
}
