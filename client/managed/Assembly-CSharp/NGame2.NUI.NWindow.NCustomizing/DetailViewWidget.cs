using System;
using NGame2.NAccount;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NCustomizing;

public class DetailViewWidget : MonoBehaviour
{
	public enum Mode
	{
		Position,
		Rotation,
		Scale
	}

	public interface IListener
	{
		Vector3 GetPostionVariation();

		Vector3 GetRotationVariation();

		float GetScaleVariation();

		void SetCostumeTransform(PositionInfo positionInfo);

		void OnBuyCostume(Action onComplete);

		HeroInfo GetHeroInfo();

		AccessoryCostumeData GetAccessoryCostumeData();

		void CloseDetaliView();
	}

	public Vector3 CameraOffset;

	public UILabel LabelX;

	public UILabel LabelY;

	public UILabel LabelZ;

	public UIToggle UITogglePosition;

	public UIToggle UIToggleRotation;

	public UIToggle UIToggleScale;

	public Transform RootSliderHorizontal;

	public Transform RootSliderVertical;

	public Transform RootSliderForward;

	public UISlider SliderHorizontal;

	public UISlider SliderVertical;

	public UISlider SliderForward;

	public UIButton ButtonPurchase;

	private Vector3 _positionMin = Vector3.zero;

	private Vector3 _positionMax = Vector3.zero;

	private Vector3 _rotationMin = Vector3.zero;

	private Vector3 _rotationMax = Vector3.zero;

	private const float ScaleDefault = 1f;

	private float _scaleMin = 1f;

	private float _scaleMax = 1f;

	private const string LocalPostionX = "CONTENT_CUSTOMIZE_DETAIL_MOVE_HORIZONTAL";

	private const string LocalPostionY = "CONTENT_CUSTOMIZE_DETAIL_MOVE_VERTICAL";

	private const string LocalPostionZ = "CONTENT_CUSTOMIZE_DETAIL_MOVE_FORWARD";

	private const string LocalRotaionX = "CONTENT_CUSTOMIZE_DETAIL_ROTATION_HORIZONTAL";

	private const string LocalRotaionY = "CONTENT_CUSTOMIZE_DETAIL_ROTATION_VERTICAL";

	private const string LocalRotaionZ = "CONTENT_CUSTOMIZE_DETAIL_ROTATION_FORWARD";

	private const string LocalScaleX = "CONTENT_CUSTOMIZE_DETAIL_SIZE";

	private Vector3 _position;

	private Vector3 _rotation;

	private float _scale = 1f;

	private IListener _listener;

	private ITransformInstnace _transfomInstance;

	private bool _inProgressToggleSetting;

	public Mode CurrentMode { get; private set; }

	private float SliderAddValue => (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("CustomizingDetailViewSliderAddValue", 100) / 1000f;

	private float PositionLimitMin => -1f;

	private float PositionLimitMax => 1f;

	private float RotationLimitMin => -179f;

	private float RotationLimitMax => 179f;

	private float ScaleLimitMin => 0.1f;

	private float ScaleLimitMax => 2f;

	public Vector3 Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position.x = Mathf.Clamp(value.x, PositionLimitMin, PositionLimitMax);
			_position.y = Mathf.Clamp(value.y, PositionLimitMin, PositionLimitMax);
			_position.z = Mathf.Clamp(value.z, PositionLimitMin, PositionLimitMax);
		}
	}

	public Vector3 Rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rotation.x = Mathf.Clamp(value.x, RotationLimitMin, RotationLimitMax);
			_rotation.y = Mathf.Clamp(value.y, RotationLimitMin, RotationLimitMax);
			_rotation.z = Mathf.Clamp(value.z, RotationLimitMin, RotationLimitMax);
		}
	}

	public float Scale
	{
		get
		{
			return _scale;
		}
		set
		{
			_scale = Mathf.Clamp(value, ScaleLimitMin, ScaleLimitMax);
		}
	}

	public void Close()
	{
		SetCostumeTransform();
		Reset();
		base.gameObject.SetActive(value: false);
	}

	private void SetCostumeTransform()
	{
		Position = _transfomInstance.GetPosition();
		Rotation = _transfomInstance.GetRotation();
		Scale = _transfomInstance.GetScale();
		PositionInfo positionInfo = new PositionInfo();
		positionInfo.SetPosition(Position);
		positionInfo.SetRotation(Rotation);
		positionInfo.SetScale(Scale);
		positionInfo.Round();
		_listener.SetCostumeTransform(positionInfo);
	}

	private void Reset()
	{
		_position = Vector3.zero;
		_rotation = Vector3.zero;
		_scale = 1f;
	}

	public bool Open(IListener listener, ITransformInstnace transfomInstance)
	{
		Reset();
		if (listener == null)
		{
			VespaDebug.LogError("DetailViewWidger Open Fail (listener is null)");
			return false;
		}
		if (transfomInstance == null)
		{
			VespaDebug.LogError("DetailViewWidger Open Fail (transfomInstance is null)");
			return false;
		}
		base.gameObject.SetActive(value: true);
		_listener = listener;
		_transfomInstance = transfomInstance;
		CurrentMode = Mode.Position;
		SetToggle();
		SetVariation();
		SetSliderValue();
		SetSliderLabel();
		SetButton();
		SetSliderDisable();
		return true;
	}

	private void SetToggle()
	{
		UITogglePosition.Set(CurrentMode == Mode.Position);
		UIToggleRotation.Set(CurrentMode == Mode.Rotation);
		UIToggleRotation.Set(CurrentMode == Mode.Scale);
	}

	private void PrintVariation()
	{
		_ = $"pMin ({_positionMin.ToString()}) pMax ({_positionMax.ToString()}) rMin ({_rotationMin.ToString()}) rMax ({_rotationMax.ToString()}) sMin ({_scaleMin}) sMax({_scaleMax})";
	}

	private void SetVariation()
	{
		_positionMin = -1f * _listener.GetPostionVariation();
		_positionMax = _listener.GetPostionVariation();
		_rotationMin = -1f * _listener.GetRotationVariation();
		_rotationMax = _listener.GetRotationVariation();
		_scaleMin = 1f - _listener.GetScaleVariation();
		_scaleMax = 1f + _listener.GetScaleVariation();
		if (_scaleMin < 0f)
		{
			VespaDebug.LogError("_scaleMin < 0");
		}
	}

	public void OnChangeValue()
	{
		if (!_inProgressToggleSetting && _listener != null)
		{
			if (CurrentMode == Mode.Position)
			{
				Position = GetSliderValueToPosition();
				_transfomInstance.SetPosition(Position);
			}
			else if (CurrentMode == Mode.Rotation)
			{
				Rotation = GetSliderValueToRotation();
				_transfomInstance.SetRotation(Rotation);
			}
			else if (CurrentMode == Mode.Scale)
			{
				Scale = GetSliderValueToScale();
				_transfomInstance.SetScale(Scale);
			}
		}
	}

	public void OnChangeToggle()
	{
		Mode mode = CurrentMode;
		if (UITogglePosition.value)
		{
			mode = Mode.Position;
		}
		else if (UIToggleRotation.value)
		{
			mode = Mode.Rotation;
		}
		else if (UIToggleScale.value)
		{
			mode = Mode.Scale;
		}
		if (CurrentMode != mode)
		{
			ChangeMode(mode);
		}
	}

	private void ChangeMode(Mode mode)
	{
		CurrentMode = mode;
		SetSliderDisable();
		SetSliderValue();
		SetSliderLabel();
	}

	private void SetSliderLabel()
	{
		Func<string, string> i_func = (string key) => LocalizationManager.GetStringDirect(key);
		if (CurrentMode == Mode.Position)
		{
			UIHelper.SetLabelText(LabelX, Vespa_System_Action_Extension_Method.fire(i_func, "CONTENT_CUSTOMIZE_DETAIL_MOVE_HORIZONTAL"));
			UIHelper.SetLabelText(LabelY, Vespa_System_Action_Extension_Method.fire(i_func, "CONTENT_CUSTOMIZE_DETAIL_MOVE_VERTICAL"));
			UIHelper.SetLabelText(LabelZ, Vespa_System_Action_Extension_Method.fire(i_func, "CONTENT_CUSTOMIZE_DETAIL_MOVE_FORWARD"));
		}
		else if (CurrentMode == Mode.Rotation)
		{
			UIHelper.SetLabelText(LabelX, Vespa_System_Action_Extension_Method.fire(i_func, "CONTENT_CUSTOMIZE_DETAIL_ROTATION_HORIZONTAL"));
			UIHelper.SetLabelText(LabelY, Vespa_System_Action_Extension_Method.fire(i_func, "CONTENT_CUSTOMIZE_DETAIL_ROTATION_VERTICAL"));
			UIHelper.SetLabelText(LabelZ, Vespa_System_Action_Extension_Method.fire(i_func, "CONTENT_CUSTOMIZE_DETAIL_ROTATION_FORWARD"));
		}
		else
		{
			UIHelper.SetLabelText(LabelX, Vespa_System_Action_Extension_Method.fire(i_func, "CONTENT_CUSTOMIZE_DETAIL_SIZE"));
			UIHelper.SetLabelText(LabelY, string.Empty);
			UIHelper.SetLabelText(LabelZ, string.Empty);
		}
	}

	private void SetSliderDisable()
	{
		UIHelper.EnableGameObject(RootSliderVertical, CurrentMode != Mode.Scale);
		UIHelper.EnableGameObject(RootSliderForward, CurrentMode != Mode.Scale);
	}

	private void SetButton()
	{
		HeroInfo heroInfo = _listener.GetHeroInfo();
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroInfo.HeroIndex);
		AccessoryCostumeData accessoryCostumeData = _listener.GetAccessoryCostumeData();
		bool flag = accessoryCostumeData?.IsBuy ?? false;
		bool flag2 = accessoryCostumeData != null && GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(heroInfo.HeroIndex, accessoryCostumeData.Index)) != null;
		UIHelper.EnableButton(ButtonPurchase, (flag || flag2) && cachedHeroInfo != null);
	}

	private void SetSliderValue()
	{
		_inProgressToggleSetting = true;
		if (CurrentMode == Mode.Position)
		{
			Vector3 position = _transfomInstance.GetPosition();
			SliderHorizontal.SetSlider(_positionMin.x, _positionMax.x, position.x);
			SliderVertical.SetSlider(_positionMin.y, _positionMax.y, position.y);
			SliderForward.SetSlider(_positionMin.z, _positionMax.z, position.z);
		}
		else if (CurrentMode == Mode.Rotation)
		{
			Vector3 rotation = _transfomInstance.GetRotation();
			rotation.x = ((rotation.x > 180f) ? (rotation.x - 360f) : rotation.x);
			rotation.y = ((rotation.y > 180f) ? (rotation.y - 360f) : rotation.y);
			rotation.z = ((rotation.z > 180f) ? (rotation.z - 360f) : rotation.z);
			SliderHorizontal.SetSlider(_rotationMin.x, _rotationMax.x, rotation.x);
			SliderVertical.SetSlider(_rotationMin.y, _rotationMax.y, rotation.y);
			SliderForward.SetSlider(_rotationMin.z, _rotationMax.z, rotation.z);
		}
		else if (CurrentMode == Mode.Scale)
		{
			float scale = _transfomInstance.GetScale();
			SliderHorizontal.SetSlider(_scaleMin, _scaleMax, scale);
			SliderVertical.SetSlider(1f, 1f, 1f);
			SliderForward.SetSlider(1f, 1f, 1f);
		}
		_inProgressToggleSetting = false;
	}

	private Vector3 GetSliderValueToPosition()
	{
		float value = SliderHorizontal.value;
		float x = Mathf.Lerp(_positionMin.x, _positionMax.x, value);
		float y = Mathf.Lerp(t: SliderVertical.value, a: _positionMin.y, b: _positionMax.y);
		float z = Mathf.Lerp(t: SliderForward.value, a: _positionMin.z, b: _positionMax.z);
		return new Vector3(x, y, z);
	}

	private Vector3 GetSliderValueToRotation()
	{
		float value = SliderHorizontal.value;
		float x = Mathf.Lerp(_rotationMin.x, _rotationMax.x, value);
		float y = Mathf.Lerp(t: SliderVertical.value, a: _rotationMin.y, b: _rotationMax.y);
		float z = Mathf.Lerp(t: SliderForward.value, a: _rotationMin.z, b: _rotationMax.z);
		return new Vector3(x, y, z);
	}

	private float GetSliderValueToScale()
	{
		float value = SliderHorizontal.value;
		return Mathf.Lerp(_scaleMin, _scaleMax, value);
	}

	public void OnClickConfirm()
	{
		if (_listener != null)
		{
			SetCostumeTransform();
			_listener.OnBuyCostume(delegate
			{
				_listener.CloseDetaliView();
			});
		}
	}

	public void OnClickReset()
	{
		_transfomInstance.SetPosition(Vector3.zero);
		_transfomInstance.SetRotation(Vector3.zero);
		_transfomInstance.SetScale(1f);
		SetSliderValue();
	}

	private void AddSliderValue(UISlider slider, float addValue)
	{
		if (!(slider == null))
		{
			slider.value += addValue;
		}
	}

	public void OnClickXLeft()
	{
		AddSliderValue(SliderHorizontal, 0f - SliderAddValue);
	}

	public void OnClickXRight()
	{
		AddSliderValue(SliderHorizontal, SliderAddValue);
	}

	public void OnClickYLeft()
	{
		AddSliderValue(SliderVertical, 0f - SliderAddValue);
	}

	public void OnClickYRight()
	{
		AddSliderValue(SliderVertical, SliderAddValue);
	}

	public void OnClickZLeft()
	{
		AddSliderValue(SliderForward, 0f - SliderAddValue);
	}

	public void OnClickZRight()
	{
		AddSliderValue(SliderForward, SliderAddValue);
	}
}
