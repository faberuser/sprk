using NGameAsset.NBattleFieldMap2.NEffectFx;
using NVespa;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class EffectFxInstance
{
	public interface IDisplayEffect
	{
		void Close();

		void Play();

		void Stop();

		void Update(Vector3 cameraPos);

		void Apply(RenderGroupModifier2 modifier);

		IAsyncLoad<IEffect> GetEffectAsync();
	}

	public enum EState
	{
		Wait,
		Play,
		End
	}

	public EState State;

	private IDisplayEffect _displayEffect;

	public EffectFxData Data { get; private set; }

	public BattleFieldMapInstance Owner { get; private set; }

	public EffectFxInstance(BattleFieldMapInstance owner)
	{
		Owner = owner;
	}

	public bool Open(EffectFxData data)
	{
		Data = data;
		if (Data.DisplayType2 == EffectFxData.EDisplayType2.Normal)
		{
			_displayEffect = new DisplayNormal(this);
		}
		else if (Data.DisplayType2 == EffectFxData.EDisplayType2.Screen)
		{
			_displayEffect = new DisplayScreen(this);
		}
		else if (Data.DisplayType2 == EffectFxData.EDisplayType2.AttachToModel)
		{
			_displayEffect = new DisplayAttachToModel(this);
		}
		return true;
	}

	public void Close()
	{
		if (_displayEffect != null)
		{
			_displayEffect.Close();
			_displayEffect = null;
		}
	}

	public void ChainUpdate(Camera cam)
	{
		if (Data.TriggerType == EffectFxData.ETriggerType.Area)
		{
			UpdateTriggerArea(cam);
		}
		if (State == EState.Play)
		{
			GameObject instanceGo = Owner.InstanceGo;
			if (instanceGo != null && _displayEffect != null)
			{
				Vector3 cameraPos = instanceGo.transform.worldToLocalMatrix.MultiplyPoint(cam.transform.position);
				_displayEffect.Update(cameraPos);
			}
		}
	}

	public void OnEvent(EBattleFieldEventType eventType, int eventValue)
	{
		if (Data.TriggerType != EffectFxData.ETriggerType.Event)
		{
			return;
		}
		if (State == EState.Wait)
		{
			if (Data.IsPlayEvent(eventType, eventValue))
			{
				if (_displayEffect != null)
				{
					_displayEffect.Play();
				}
				State = EState.Play;
			}
		}
		else if (State == EState.Play)
		{
			if (Data.IsStopEvent(eventType, eventValue))
			{
				if (_displayEffect != null)
				{
					_displayEffect.Stop();
				}
				State = (Data.PlayOnce ? EState.End : EState.Wait);
			}
		}
		else if (State != EState.End)
		{
			State = EState.End;
		}
	}

	private void UpdateTriggerArea(Camera cam)
	{
		GameObject instanceGo = Owner.InstanceGo;
		if (instanceGo == null)
		{
			return;
		}
		if (State == EState.Wait)
		{
			if (Data.CheckEnableArea(cam, instanceGo) == EffectFxData.ECheckResult.Enable)
			{
				if (_displayEffect != null)
				{
					_displayEffect.Play();
				}
				State = EState.Play;
			}
		}
		else if (State == EState.Play)
		{
			if (Data.CheckEnableArea(cam, instanceGo) == EffectFxData.ECheckResult.Disable)
			{
				if (_displayEffect != null)
				{
					_displayEffect.Stop();
				}
				State = (Data.PlayOnce ? EState.End : EState.Wait);
			}
		}
		else if (State != EState.End)
		{
			State = EState.End;
		}
	}

	public void Apply(RenderGroupModifier2 modifier)
	{
		if (_displayEffect != null)
		{
			_displayEffect.Apply(modifier);
		}
	}
}
