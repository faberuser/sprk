using NGame2.NFunctionality;
using NNguiExtension;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class SystemButtonSpeed : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/SystemButtonSpeed";

	public UISpriteSelector SpeedSpriteSelector;

	[SerializeField]
	private UIButton _buttonSpeed;

	[SerializeField]
	private UIGame3DWidget _effectSpeed;

	private SystemButtonAuto.ESpeedType _applySpeedType;

	private LocalAccountEnum<SystemButtonAuto.ESpeedType> _speedType = new LocalAccountEnum<SystemButtonAuto.ESpeedType>(SystemButtonAuto.ESpeedType.Normal, typeof(SystemButtonSpeed).FullName + "_speedType");

	public SystemButtonAuto.ESpeedType SpeedType => _speedType.Value;

	public bool FastSpeed
	{
		get
		{
			if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
			{
				return false;
			}
			if (GameOptionData.EditorToggleFastSpeed)
			{
				return true;
			}
			return _speedType.Value == SystemButtonAuto.ESpeedType.Fast;
		}
	}

	public override void Open()
	{
		Open(delegate
		{
			ApplySpeedType(SystemButtonAuto.ESpeedType.Normal, force: true);
		}, null);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		SystemButtonAuto.ESpeedType speedType = _speedType.Value;
		if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
		{
			speedType = SystemButtonAuto.ESpeedType.Normal;
		}
		ApplySpeedType(speedType, force: false);
	}

	public void OnClickFastSpeed()
	{
		if (_speedType.Value == SystemButtonAuto.ESpeedType.Normal)
		{
			_speedType.Value = SystemButtonAuto.ESpeedType.Fast;
		}
		else if (_speedType.Value == SystemButtonAuto.ESpeedType.Fast)
		{
			_speedType.Value = SystemButtonAuto.ESpeedType.Normal;
		}
		else
		{
			_speedType.Value = SystemButtonAuto.ESpeedType.Normal;
		}
	}

	private void ApplySpeedType(SystemButtonAuto.ESpeedType speedType, bool force)
	{
		if (!force && _applySpeedType == speedType)
		{
			return;
		}
		_applySpeedType = speedType;
		if (SpeedSpriteSelector != null)
		{
			SpeedSpriteSelector.SelectEnum(_applySpeedType);
		}
		if (!(_effectSpeed != null))
		{
			return;
		}
		_effectSpeed.ReserveContents();
		if (_applySpeedType != SystemButtonAuto.ESpeedType.Normal)
		{
			if (!_effectSpeed.IsPlaying)
			{
				_effectSpeed.Play(null);
			}
		}
		else if (_effectSpeed.IsPlaying)
		{
			_effectSpeed.Stop(immediatelyClear: true);
		}
	}
}
