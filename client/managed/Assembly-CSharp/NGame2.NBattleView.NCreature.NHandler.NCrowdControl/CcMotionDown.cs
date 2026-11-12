using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NSound;
using NShared;
using NVespa;

namespace NGame2.NBattleView.NCreature.NHandler.NCrowdControl;

public class CcMotionDown : CrowdControl.ICcMotion
{
	private CrowdControl _crowdControl;

	private CrowdControl.CcDownDuration _downDuration;

	private int _actionUid;

	public CcMotionDown(CrowdControl crowdControl)
	{
		_crowdControl = crowdControl;
		ActionDown();
	}

	public void Refresh()
	{
		ActionDown();
	}

	void CrowdControl.ICcMotion.ChainUpdate()
	{
		if (_actionUid == 0)
		{
			return;
		}
		NGame2.NRender.NRenderFacade.AnimationData.RuntimeData runtime = _crowdControl.Owner.RenderFacade.AnimationData.Runtime;
		if (runtime == null)
		{
			return;
		}
		if (runtime.Stock.AnimationKey.MotionType == DefineCreature.EMotion.Down)
		{
			if (runtime.IsMotionEnd)
			{
				CrowdControl.CcDownDuration maxRemainedDown = GetMaxRemainedDown();
				float num = ((maxRemainedDown != null) ? ((float)(maxRemainedDown.DurationTick - maxRemainedDown.ElapsedTick) * 0.001f) : 0f);
				if (num < GameSetting.data.BattleView.CcDown_GetupDuration)
				{
					ActionGetup(maxRemainedDown);
				}
			}
		}
		else if (runtime.Stock.AnimationKey.MotionType == DefineCreature.EMotion.Getup)
		{
			CrowdControl.CcDownDuration maxRemainedDown2 = GetMaxRemainedDown();
			if (_downDuration != maxRemainedDown2 && maxRemainedDown2 != null)
			{
				_crowdControl.Owner.RenderFacade.AnimationData.SetRuntimeTimer(maxRemainedDown2.LocalTimer);
			}
		}
	}

	void CrowdControl.ICcMotion.ReleaseCc()
	{
	}

	private CrowdControl.CcDownDuration GetMaxRemainedDown()
	{
		float num = 0f;
		CrowdControl.CcDownDuration result = null;
		for (int i = 0; i < _crowdControl.DownDurationList.Count; i++)
		{
			CrowdControl.CcDownDuration ccDownDuration = _crowdControl.DownDurationList[i];
			float num2 = (float)(ccDownDuration.DurationTick - ccDownDuration.ElapsedTick) * 0.001f;
			if (num < num2)
			{
				num = num2;
				result = ccDownDuration;
			}
		}
		return result;
	}

	private void ActionDown()
	{
		ILocalTimer localTimer = null;
		for (int i = 0; i < _crowdControl.DownDurationList.Count; i++)
		{
			CrowdControl.CcDownDuration ccDownDuration = _crowdControl.DownDurationList[i];
			if (ccDownDuration.IsSoloOperation)
			{
				localTimer = ccDownDuration.LocalTimer;
			}
		}
		NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
		stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
		stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Down);
		stockData.MotionEnd = DefineCreature.EMotionEnd.Infinite;
		_crowdControl.Owner.Action.Motion(stockData, localTimer);
		_actionUid = _crowdControl.Owner.RenderFacade.AnimationData.Runtime.Uid;
		_downDuration = null;
		Creature owner = _crowdControl.Owner;
		if (owner.CreateInfo != null && owner.CreateInfo.HeroInfo != null)
		{
			CachedHeroInfo heroInfo = owner.CreateInfo.HeroInfo;
			SoundStock stock = new SoundStock
			{
				LocalTimer = localTimer
			};
			string prefix = "DOWN";
			string codeName = heroInfo.CreatureData.CodeName;
			string costumeVoicePostfix = SoundHelper.GetCostumeVoicePostfix(heroInfo);
			string soundCode = SoundHelper.GetSoundCode(prefix, codeName, costumeVoicePostfix);
			stock.Code = soundCode;
			bool enable = !SingletonAuto<SoundContainer>.instance.TryPlayOnce(stock, 10f);
			owner.RenderFacade.EnableAnimationSound(enable);
		}
	}

	private void ActionGetup(CrowdControl.CcDownDuration downDuration)
	{
		_downDuration = downDuration;
		ILocalTimer localTimer = ((_downDuration == null) ? null : _downDuration.LocalTimer);
		NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
		stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
		stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Getup);
		stockData.MotionEnd = DefineCreature.EMotionEnd.End;
		_crowdControl.Owner.Action.Motion(stockData, localTimer);
		_actionUid = _crowdControl.Owner.RenderFacade.AnimationData.Runtime.Uid;
	}
}
