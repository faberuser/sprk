using System.Collections;
using NGame2.NBattleView;
using NGame2.NScene.NPreloader;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BossNameTag : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BossNameTag";

	[SerializeField]
	private UIGame3DWidget _bossEffectWidget;

	[SerializeField]
	private UIGame3DWidget _eliteEffectWidget;

	[SerializeField]
	private UIWidget _labelRoot;

	[SerializeField]
	private UILabel _prefixLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private float _effectTimeLength = 3f;

	[SerializeField]
	private AnimationCurve _alphaCurve = new AnimationCurve();

	private bool _isBossTag;

	private Creature _bossCreature;

	private Coroutine _openCo;

	public override void ReserveCache(UiPreloader preloader)
	{
		if (_bossEffectWidget != null)
		{
			_bossEffectWidget.ReserveCache(preloader);
		}
		if (_eliteEffectWidget != null)
		{
			_eliteEffectWidget.ReserveCache(preloader);
		}
		base.ReserveCache(preloader);
	}

	public void Open(Creature bossCreature, bool isBossTag)
	{
		Open(delegate
		{
			Init(bossCreature, isBossTag);
		}, null);
	}

	public void Init(Creature bossCreature, bool isBossTag)
	{
		_isBossTag = isBossTag;
		_bossCreature = bossCreature;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureNameByBossNameTag(bossCreature.CreatureTableData, bossCreature.CreateInfo.VisualInfo, LocalizationManager.GetStringDirect, out var creatureName, out var creatureNamePrefix);
		UIHelper.SetLabelText(_prefixLabel, creatureNamePrefix);
		UIHelper.SetLabelText(_nameLabel, creatureName);
		_openCo = VespaCoroutine.Start(this, coOpen());
	}

	public override void Reset()
	{
		base.Reset();
		if (_openCo != null)
		{
			VespaCoroutine.StopAll(this);
			_openCo = null;
		}
		if (_bossEffectWidget != null)
		{
			_bossEffectWidget.Stop(immediatelyClear: true);
		}
		if (_eliteEffectWidget != null)
		{
			_eliteEffectWidget.Stop(immediatelyClear: true);
		}
		UIHelper.SetWidgetAlpha(_labelRoot, 0f);
	}

	private IEnumerator coOpen()
	{
		if (_isBossTag)
		{
			_bossEffectWidget.Play(null);
		}
		else
		{
			_eliteEffectWidget.Play(null);
		}
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += UnitySystemTimer.deltaTime;
			float i_frame = Mathf.Clamp01(elapsedTime / Vespa_Float_Extension_Method.NoneZero(_effectTimeLength));
			float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_alphaCurve, i_frame, 1f);
			UIHelper.SetWidgetAlpha(_labelRoot, alpha);
		}
		while (!(elapsedTime >= _effectTimeLength));
		Close();
	}
}
