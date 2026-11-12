using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class DeckStatus : BaseWindow
{
	private interface IDeckStatus
	{
		bool IsValid();

		int GetCurrent();

		int GetTotal();
	}

	private class EclipseDeckStatus : IDeckStatus
	{
		private UserEclipseManager EclipseManager => GlobalAutoBehaviour<AccountManager>.instance.EclipseManager;

		public int GetCurrent()
		{
			if (!IsValid())
			{
				return 0;
			}
			return EclipseManager.GetEclipseInfo().DeckIndex;
		}

		public int GetTotal()
		{
			if (!IsValid())
			{
				return 0;
			}
			int savedDeckInfoCount = EclipseManager.GetSavedDeckInfoCount();
			if (savedDeckInfoCount != 0)
			{
				return savedDeckInfoCount;
			}
			if (EclipseManager.GetEclipseInfo().LastDeckIndex == 0)
			{
				return 0;
			}
			return EclipseManager.GetEclipseInfo().LastDeckIndex - 1;
		}

		public bool IsValid()
		{
			if (EclipseManager == null)
			{
				return false;
			}
			if (EclipseManager.GetEclipseInfo() == null)
			{
				return false;
			}
			return true;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/DeckStatus";

	public UILabel UILabel;

	private float _changeDelaySeconds;

	private float _changeTime;

	private IDeckStatus _deckStatus;

	public int CurrentDeckIndex { get; private set; }

	public void Open(BattleType battleType)
	{
		base.Open();
		if (!InitStatus(battleType))
		{
			Close();
			return;
		}
		_changeDelaySeconds = (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("DeckStatusChangeDelayMs", 3000) / 1000f;
		SetCurrentDeckIndex();
	}

	public override void Reset()
	{
		base.Reset();
		CurrentDeckIndex = 0;
		_changeTime = 0f;
		_deckStatus = null;
	}

	private bool InitStatus(BattleType battleType)
	{
		if (battleType == BattleType.Eclipse)
		{
			_deckStatus = new EclipseDeckStatus();
		}
		return _deckStatus != null;
	}

	private bool IsValidInfo()
	{
		return _deckStatus.IsValid();
	}

	private int GetCurrent()
	{
		return _deckStatus.GetCurrent();
	}

	private int GetTotal()
	{
		return _deckStatus.GetTotal();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!IsValidInfo())
		{
			Close();
		}
		else if (CurrentDeckIndex != GetCurrent())
		{
			if (_changeTime == 0f)
			{
				_changeTime = Time.time + _changeDelaySeconds;
			}
			else if (!(_changeTime > Time.time))
			{
				_changeTime = 0f;
				SetCurrentDeckIndex();
			}
		}
	}

	private void SetCurrentDeckIndex()
	{
		CurrentDeckIndex = GetCurrent();
		UIHelper.SetLabelText(UILabel, $"{CurrentDeckIndex}/{GetTotal()}");
	}
}
