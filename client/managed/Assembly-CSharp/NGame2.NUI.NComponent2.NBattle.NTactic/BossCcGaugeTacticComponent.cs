using NGame2.NBattleField;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle.NTactic;

public class BossCcGaugeTacticComponent : TacticComponent, InitOnCreate
{
	[Header("[Cc Gauge]")]
	public Transform Root;

	public UIClipSprite GaugeSprite;

	protected float _nextUpdateTime;

	protected IBattleTactic _tactic;

	protected int _bossUid;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public override void Open(CampaignTacticData tacticData)
	{
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			_tactic = battleInstance.Snapshot.TacticManager.Tactic;
		});
		_nextUpdateTime = 0f;
		_bossUid = GetBossUid();
	}

	private void Update()
	{
		if (!(Time.time < _nextUpdateTime))
		{
			if (!(_tactic is BossCcGauge bossCcGauge))
			{
				UIHelper.EnableGameObject(Root, enable: false);
				_nextUpdateTime = Time.time + 0.5f;
			}
			else if (_bossUid == 0)
			{
				_bossUid = GetBossUid();
				UIHelper.EnableGameObject(Root, enable: false);
				_nextUpdateTime = Time.time + 0.5f;
			}
			else
			{
				UpdateCcGauge(bossCcGauge.CurGauge, bossCcGauge.MaxGauge);
				_nextUpdateTime = 0f;
			}
		}
	}

	protected void UpdateCcGauge(int curValue, int maxValue)
	{
		if (!(GaugeSprite == null))
		{
			float b = ((maxValue > 0) ? ((float)curValue / (float)maxValue) : 0f);
			float fillAmount = Mathf.Lerp(GaugeSprite.fillAmount, b, Time.deltaTime * 10f);
			UIHelper.EnableGameObject(Root, enable: true);
			UIHelper.SetSpriteFillAmount(GaugeSprite, fillAmount);
		}
	}

	protected int GetBossUid()
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return 0;
		}
		int result = 0;
		int creatureCount = SingletonAuto<BattleFacade>.instance.GetCreatureCount();
		for (int i = 0; i < creatureCount; i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex != null && creatureCacheByIndex.CreateInfo != null && creatureCacheByIndex.CreateInfo.IsBoss)
			{
				result = creatureCacheByIndex.Uid;
				break;
			}
		}
		return result;
	}
}
