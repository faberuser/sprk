using NGame2.NRender;
using NGame2.NUI.NGame3DWidget;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI;

public class UIFlagWidget : UIWidgetExtraRenderQ
{
	[SerializeField]
	private Transform _pivotTm;

	private bool _loadComplete;

	private RenderQueueVisitor _renderQueueVisitor = new RenderQueueVisitor();

	private ILocalTimer _localTimer;

	public ILocalTimer LocalTimer
	{
		get
		{
			if (_localTimer == null)
			{
				return UnityLocalTimer.Default;
			}
			return _localTimer;
		}
		set
		{
			_localTimer = value;
		}
	}

	public Transform PivotTm
	{
		get
		{
			if (!(_pivotTm == null))
			{
				return _pivotTm;
			}
			return base.transform;
		}
	}

	public GuildFlagFacade GuildFlagFacade { get; private set; }

	public int RenderLayer
	{
		get
		{
			if (panel != null)
			{
				return panel.gameObject.layer;
			}
			return base.gameObject.layer;
		}
	}

	public void Open(byte flagIndex, byte guildLogo, byte guildLogoBg)
	{
		Close();
		string flagCode = Vespa_Generic_IList_Extension_Method.GetLast(GameRoot.Instance.GuildArenaSeasonRewardDataContainer.GetAllData()).FlagCode;
		GuildArenaSeasonRewardData data = GameRoot.Instance.GuildArenaSeasonRewardDataContainer.GetData(flagIndex);
		if (data != null)
		{
			flagCode = data.FlagCode;
		}
		string guildLogoIconPath = UIHelper.GetGuildLogoIconPath(guildLogo);
		string guildLogo3dBgIconPath = UIHelper.GetGuildLogo3dBgIconPath(guildLogoBg);
		GuildFlagFacade = GuildFlagFacade.CreateFlag(flagCode, guildLogoIconPath, guildLogo3dBgIconPath, base.gameObject.layer);
		GuildFlagFacade.transform.parent = base.transform;
		GuildFlagFacade.transform.localPosition = Vector3.zero;
		GuildFlagFacade.transform.localRotation = Quaternion.Euler(Vector3.zero);
		GuildFlagFacade.transform.localScale = Vector3.one;
	}

	public void ChangeLogo(byte guildLogo)
	{
		string guildLogoIconPath = UIHelper.GetGuildLogoIconPath(guildLogo);
		if (GuildFlagFacade != null)
		{
			GuildFlagFacade.ChangeLogo(guildLogoIconPath);
		}
	}

	public void ChangeBG(byte bgIndex)
	{
		string guildLogo3dBgIconPath = UIHelper.GetGuildLogo3dBgIconPath(bgIndex);
		if (GuildFlagFacade != null)
		{
			GuildFlagFacade.ChangeBg(guildLogo3dBgIconPath);
		}
	}

	public void Play(string phaseName = "Put")
	{
		if (!(GuildFlagFacade == null))
		{
			GuildFlagFacade.Play(phaseName);
		}
	}

	public void Close()
	{
		if (GuildFlagFacade != null)
		{
			GuildFlagFacade.ReleaseSelf();
			GuildFlagFacade = null;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	private void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			ReleaseAllContents();
		}
	}

	private void ReleaseAllContents()
	{
		Close();
	}

	public void SetAlpha(float alpha)
	{
		this.alpha = alpha;
	}

	public void FadeIn(float duration)
	{
		SetAlpha(0f);
		TweenAlpha.Begin(base.gameObject, duration, 1f);
	}

	public void FadeOut(float duration)
	{
		SetAlpha(1f);
		TweenAlpha.Begin(base.gameObject, duration, 0f);
	}

	public override int UpdateExtraRenderQueue(UIPanel panel, int renderQueue, bool isExplicit)
	{
		if (GuildFlagFacade == null || GuildFlagFacade.Effect == null)
		{
			return renderQueue;
		}
		GuildFlagFacade.Effect.GetRenderGroupModifier().SetExtraAlpha(panel.alpha * alpha);
		GuildFlagFacade.Effect.Visit(_renderQueueVisitor);
		return _renderQueueVisitor.UpdateExtraRenderQueue(renderQueue, isExplicit);
	}
}
