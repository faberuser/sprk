using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class SystemButtonPvpGiveUp : BaseWindow
{
	public interface Listener
	{
		int GetElapsedTime();

		bool IsBattleEnabled();

		void OnPvpGiveUp();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/SystemButtonPvpGiveUp";

	public Transform Root;

	private int _defaultDelaySec = 10;

	private int _giveUpAcitiveDelay;

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
	}

	public override void Reset()
	{
		base.Reset();
		UIHelper.EnableGameObject(Root, enable: false);
		_giveUpAcitiveDelay = 0;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_listener.IsBattleEnabled())
		{
			if (_giveUpAcitiveDelay == 0)
			{
				_giveUpAcitiveDelay = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchGiveUpDelay", _defaultDelaySec);
			}
			else if (_giveUpAcitiveDelay < _listener.GetElapsedTime())
			{
				UIHelper.EnableGameObject(Root, enable: true);
			}
		}
	}

	public void OnClickGiveUp()
	{
		if (_listener != null)
		{
			_listener.OnPvpGiveUp();
		}
	}
}
