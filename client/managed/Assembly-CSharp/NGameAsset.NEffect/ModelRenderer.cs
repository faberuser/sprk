using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class ModelRenderer
{
	public const float MinAlpha = 0.0019607844f;

	public Color OriginalColor = Color.white;

	private RendererExtraData.MaterialProxy _materialProxy;

	private bool _renderEnabled;

	public bool Enabled
	{
		get
		{
			return _renderEnabled;
		}
		set
		{
			_renderEnabled = value;
			RefreshRendererEnable();
		}
	}

	public RendererExtraData Extra { get; private set; }

	public RenderProxy Proxy { get; private set; }

	public bool Open(Renderer component, RendererExtraData extra)
	{
		if (component == null)
		{
			return false;
		}
		Proxy = RenderProxy.CreateProxy(component, extra.MaterialFactory);
		if (Proxy == null)
		{
			return false;
		}
		Extra = extra;
		if (Proxy.sharedMaterial != null)
		{
			OriginalColor = Proxy.sharedMaterial.color;
			_materialProxy = new RendererExtraData.MaterialProxy(Proxy);
		}
		_renderEnabled = !(Proxy.renderer == null) && Proxy.renderer.enabled;
		RefreshRendererEnable();
		return true;
	}

	private void RefreshRendererEnable()
	{
		if (!(Proxy.renderer == null))
		{
			bool flag = _renderEnabled;
			if (_materialProxy != null && _materialProxy.IsValid)
			{
				flag = flag && _materialProxy.color.a > 0.0019607844f;
			}
			Proxy.renderer.enabled = flag;
		}
	}

	public void UpdateMaterial(float frame, float extraAlpha)
	{
		if (_materialProxy != null && _materialProxy.IsValid)
		{
			Extra.UpdateMaterial(frame, _materialProxy, OriginalColor, extraAlpha);
			RefreshRendererEnable();
		}
	}

	public void Close()
	{
		if (Proxy != null)
		{
			Proxy.ReleaseProxy();
			Proxy = null;
		}
	}
}
