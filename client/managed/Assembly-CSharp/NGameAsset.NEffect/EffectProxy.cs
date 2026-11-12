using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class EffectProxy : IEffect
{
	private bool _isClosed;

	private IAsyncLoad<IEffect> _asyncLoad;

	private bool _setDummyParent;

	private Vector3 _dummyPosition;

	private Quaternion _dummyRotation;

	private Vector3 _dummyScale;

	private string _dummyName;

	private ICreature _parent;

	private Transform _parentTm;

	private Tuple<ShaderTransformData, Transform> _renderModeTuple;

	private RenderGroupModifier2 _renderGroupModifier;

	private bool _grayscaleEnabled;

	private bool _isVisible = true;

	private ILocalTimer _localTimer;

	private string _phaseName = string.Empty;

	private float _elapsedTime;

	private float _deltaTime
	{
		get
		{
			if (_localTimer != null)
			{
				return (float)_localTimer.deltaTick * 0.001f;
			}
			return MainTimer.deltaTime;
		}
	}

	public EffectProxy(IAsyncLoad<IEffect> asyncLoad)
	{
		_asyncLoad = asyncLoad;
	}

	public void ApplyToReal(IEffect effect)
	{
		if (!_isClosed)
		{
			if (_setDummyParent)
			{
				effect.SetDummyParent(_dummyPosition, _dummyRotation, _dummyScale, _dummyName);
			}
			else if (_parent != null)
			{
				effect.SetParent(_parent);
			}
			else if (_parentTm != null)
			{
				effect.SetParent(_parentTm);
			}
			if (_renderModeTuple != null)
			{
				effect.SetShaderTransform(_renderModeTuple.Item1, _renderModeTuple.Item2);
			}
			if (_grayscaleEnabled)
			{
				effect.SetGrayscale(_grayscaleEnabled);
			}
			effect.SetVisible(_isVisible);
			if (_renderGroupModifier != null)
			{
				RenderGroupModifier2 renderGroupModifier = effect.GetRenderGroupModifier();
				_renderGroupModifier.CopyTo(renderGroupModifier);
			}
			if (_localTimer != null)
			{
				effect.SetTimer(_localTimer);
			}
			if (!String_Extension_Method.IsNullOrEmpty(_phaseName))
			{
				effect.PlayPhase(_phaseName);
			}
			if (_elapsedTime > Mathf.Epsilon)
			{
				effect.Update(_elapsedTime);
			}
		}
	}

	void IEffect.Close(bool detachParent)
	{
		_isClosed = true;
		_asyncLoad = null;
		if (_renderGroupModifier != null)
		{
			SmallPooling<RenderGroupModifier2>.ReleaseInstance(_renderGroupModifier);
			_renderGroupModifier = null;
		}
	}

	bool IEffect.IsEmpty()
	{
		return true;
	}

	void IEffect.Release()
	{
	}

	RenderGroupModifier2 IEffect.GetRenderGroupModifier()
	{
		if (_renderGroupModifier == null)
		{
			_renderGroupModifier = SmallPooling<RenderGroupModifier2>.CreateInstance();
			_renderGroupModifier.Open(useSoloTimeColor: true);
		}
		return _renderGroupModifier;
	}

	void IEffect.PlayPhase(string phaseName)
	{
		if (!String_Extension_Method.IsNullOrEmpty(phaseName))
		{
			_phaseName = phaseName;
			_elapsedTime = 0f;
		}
	}

	void IEffect.SetVisible(bool isVisible)
	{
		_isVisible = isVisible;
	}

	void IEffect.Visit(IVisitor visitor)
	{
	}

	void IEffect.SetDummyParent(Vector3 position, Quaternion rotation, Vector3 scale, string dummyName = "")
	{
		_parent = null;
		_parentTm = null;
		_setDummyParent = true;
		_dummyPosition = position;
		_dummyRotation = rotation;
		_dummyScale = scale;
		_dummyName = dummyName;
	}

	void IEffect.SetParent(ICreature parent)
	{
		if (!_isClosed)
		{
			_setDummyParent = false;
			_parent = parent;
			_parentTm = null;
		}
	}

	void IEffect.SetParent(Transform parentTm)
	{
		if (!_isClosed)
		{
			_setDummyParent = false;
			_parent = null;
			_parentTm = parentTm;
		}
	}

	void IEffect.SetShaderTransform(ShaderTransformData shaderTransform, Transform pivotTm)
	{
		_renderModeTuple = Tuple.Create(shaderTransform, pivotTm);
	}

	void IEffect.SetGrayscale(bool isEnable)
	{
		_grayscaleEnabled = isEnable;
	}

	void IEffect.SetTimer(ILocalTimer localTimer)
	{
		_localTimer = localTimer;
	}

	void IEffect.Update()
	{
		_elapsedTime += _deltaTime;
	}

	void IEffect.Update(float elapsedTime)
	{
		_elapsedTime += elapsedTime;
	}

	bool IEffect.IsEnd()
	{
		if (_isClosed)
		{
			return true;
		}
		if (_asyncLoad == null)
		{
			return true;
		}
		if (!_asyncLoad.IsDone)
		{
			return false;
		}
		if (!_asyncLoad.IsValid)
		{
			return true;
		}
		return false;
	}

	Transform IEffect.FindTransform(string name)
	{
		return null;
	}
}
