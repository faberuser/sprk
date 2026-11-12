using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BossIntroWindow : BaseWindow
{
	public interface IListener
	{
		void OnClickSkip();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BossIntroWindow";

	public AnimationCurve AlphaCurve;

	public UIButton SkipButton;

	private IListener _listener;

	public void Open(IListener listener)
	{
		base.Open();
		_listener = listener;
	}

	public override void SetEventHide(float alpha)
	{
		base.SetEventHide(Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(AlphaCurve, alpha, 1f - alpha));
	}

	public void OnClickSkip()
	{
		if (_listener != null)
		{
			_listener.OnClickSkip();
			Close();
		}
	}
}
