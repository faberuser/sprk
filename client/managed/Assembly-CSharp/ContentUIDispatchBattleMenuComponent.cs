using NGame2.NUI;
using NVespa.NPrefab2;
using UnityEngine;

public class ContentUIDispatchBattleMenuComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClick(ContentUIDispatchBattleMenuComponent comp);
	}

	public UIButton ButtonOpen;

	public UIGame3DWidget UIEffect;

	public UISprite SpriteNew;

	public Transform EffectFinishTranform;

	private IListener _listener;

	public void Open(IListener listener)
	{
		_listener = listener;
	}

	public void SetEffectPlay(bool play)
	{
		if (UIEffect == null)
		{
			return;
		}
		if (play)
		{
			if (!UIEffect.IsPlaying)
			{
				UIEffect.Play(null);
			}
		}
		else
		{
			UIEffect.Stop(immediatelyClear: true);
		}
	}

	public void EnableEffectFinish(bool enable)
	{
		if (!(EffectFinishTranform == null))
		{
			UIHelper.EnableGameObject(EffectFinishTranform, enable);
		}
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
