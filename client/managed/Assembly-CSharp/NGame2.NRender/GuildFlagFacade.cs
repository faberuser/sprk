using NGameAsset;
using NGameAsset.NBattleFieldMap2;
using NVespa;
using UnityEngine;

namespace NGame2.NRender;

public class GuildFlagFacade : MonoBehaviour, IInspectorGuiDrawer
{
	private IAsyncLoad<IEffect> _guildFlagAsync;

	private AssetTexture _markAssetTexture;

	private AssetTexture _markBgAssetTexture;

	private bool _initialized;

	private bool IsPlaying;

	public IEffect Effect
	{
		get
		{
			if (_guildFlagAsync == null)
			{
				return null;
			}
			return _guildFlagAsync.Value;
		}
	}

	public static GuildFlagFacade CreateFlag(string flagCode, string markCode, string markBgCode, int layer)
	{
		GuildFlagFacade guildFlagFacade = new GameObject($"GuildFlag_{markCode}_{markBgCode}").AddComponent<GuildFlagFacade>();
		guildFlagFacade.Open(flagCode, markCode, markBgCode, layer);
		return guildFlagFacade;
	}

	public void Open(string flagCode, string markCode, string markBgCode, int layer)
	{
		_guildFlagAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(flagCode, layer);
		base.gameObject.layer = layer;
		_guildFlagAsync.Value.SetParent(base.transform);
		_markAssetTexture = new AssetTexture();
		_markAssetTexture.Open(markCode);
		_markBgAssetTexture = new AssetTexture();
		_markBgAssetTexture.Open(markBgCode);
		base.gameObject.SetActive(value: false);
	}

	public void ChangeLogo(string markCode)
	{
		if (_markAssetTexture != null)
		{
			_markAssetTexture.Open(markCode);
		}
	}

	public void ChangeBg(string markBgCode)
	{
		if (_markBgAssetTexture != null)
		{
			_markBgAssetTexture.Open(markBgCode);
		}
	}

	public void Play(string phaseName = "Put")
	{
		if (Effect != null)
		{
			_guildFlagAsync.Value.PlayPhase(phaseName);
			base.gameObject.SetActive(value: true);
			IsPlaying = true;
		}
	}

	public void Stop()
	{
		if (Effect != null)
		{
			IsPlaying = false;
		}
	}

	public bool ISDONE()
	{
		if (_guildFlagAsync.ISDONE() && _markAssetTexture.ISDONE())
		{
			return _markBgAssetTexture.ISDONE();
		}
		return false;
	}

	public void Close()
	{
		if (_guildFlagAsync != null)
		{
			EffectContainer.ReleaseEffect(_guildFlagAsync, detachParent: true, immediatelyRelease: true);
			_guildFlagAsync = null;
		}
		if (_markAssetTexture != null)
		{
			_markAssetTexture.CloseAndRelease();
			_markAssetTexture = null;
		}
		if (_markBgAssetTexture != null)
		{
			_markBgAssetTexture.CloseAndRelease();
			_markBgAssetTexture = null;
		}
	}

	public void ApplyGuildFlag(MapBaseData.GuildFlagPivot guildFlagPivot)
	{
		base.transform.localPosition = guildFlagPivot.Position;
		base.transform.localRotation = guildFlagPivot.Rotation;
		base.transform.localScale = guildFlagPivot.Scale;
	}

	private void Update()
	{
		if (_guildFlagAsync.ISDONE() && _markAssetTexture.ISDONE() && _markBgAssetTexture.ISDONE())
		{
			_initialized = true;
			OnCompleteAsset();
		}
		if (_guildFlagAsync != null && IsPlaying && base.gameObject.activeSelf)
		{
			_guildFlagAsync.Value.Update();
		}
	}

	private void OnCompleteAsset()
	{
		if (!_guildFlagAsync.ISVALID())
		{
			VespaDebug.LogError("_guildFlagAsync is invalid");
			return;
		}
		if (!_markAssetTexture.ISVALID())
		{
			VespaDebug.LogError("Guild Mark Asset is invalid");
		}
		if (!_markBgAssetTexture.ISVALID())
		{
			VespaDebug.LogError("Guild Mark Bg Asset is invalid");
		}
		NGame2.NRender.ReplaceTexVisitor visitor = new NGame2.NRender.ReplaceTexVisitor(_markAssetTexture, _markBgAssetTexture);
		_guildFlagAsync.Value.Visit(visitor);
	}
}
