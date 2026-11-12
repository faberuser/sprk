using System.Collections.Generic;
using NGame2.NBattleView;
using NGame2.NBattleView.NUIOn3d2;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.N3D;

public class EventCamera
{
	public class EventValue
	{
		public float BattleFieldAlpha = 1f;

		public float UiAlpha = 1f;

		public float UiOn3dAlpha = 1f;

		public TeamType ApplyTeamType = TeamType.All;

		public float ShakeWeight = 1f;

		public void Reset()
		{
			UiAlpha = 1f;
			UiOn3dAlpha = 1f;
			BattleFieldAlpha = 1f;
			ShakeWeight = 1f;
			ApplyTeamType = TeamType.All;
		}
	}

	public class AlphaBlendValue
	{
		public AlphaToggleEntry ToggleEntry;

		public bool Released;

		public float Frame;

		private int _frameCount;

		public void Update(EventValue eventValue)
		{
			if (_frameCount != UnitySystemTimer.frameCount)
			{
				_frameCount = UnitySystemTimer.frameCount;
				float num = GlobalTimer.deltaTime / Vespa_Float_Extension_Method.NoneZero(ToggleEntry.TimeLength);
				Frame = Mathf.Clamp01(Frame + (Released ? (0f - num) : num));
				float b = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(ToggleEntry.AlphaCurve, Frame, Frame);
				if (ToggleEntry.ApplyBattleField)
				{
					eventValue.BattleFieldAlpha = Mathf.Min(eventValue.BattleFieldAlpha, b);
				}
				if (ToggleEntry.ApplyUi)
				{
					eventValue.UiAlpha = Mathf.Min(eventValue.UiAlpha, b);
				}
				if (ToggleEntry.ApplyUiOn3d)
				{
					eventValue.UiOn3dAlpha = Mathf.Min(eventValue.UiOn3dAlpha, b);
				}
				if (ToggleEntry.ApplyTeamType != TeamType.All)
				{
					eventValue.ApplyTeamType = ToggleEntry.ApplyTeamType;
				}
			}
		}
	}

	public class AlphaToggleValue2
	{
		private bool _isShow = true;

		private AnimationCurve _blendCurve = new AnimationCurve();

		private float _timeLength;

		private float _frame = 1f;

		private float _beginAlpha = 1f;

		private int _frameCount;

		public float FinalAlpha { get; private set; }

		public AlphaToggleValue2()
		{
			FinalAlpha = 1f;
			_frameCount = -1;
		}

		public void Apply(EBoolCheck show, AnimationCurve curve, float time)
		{
			if (show != EBoolCheck.DontCare)
			{
				_blendCurve = curve;
				_timeLength = time;
				_isShow = show == EBoolCheck.True;
				_frame = 0f;
				_beginAlpha = FinalAlpha;
			}
		}

		public void Update()
		{
			if (_frameCount != UnitySystemTimer.frameCount)
			{
				_frameCount = UnitySystemTimer.frameCount;
				float num = GlobalTimer.deltaTime / Vespa_Float_Extension_Method.NoneZero(_timeLength);
				_frame = Mathf.Clamp01(_frame + num);
				float b = (_isShow ? 1f : 0f);
				float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_blendCurve, _frame, _frame);
				FinalAlpha = Mathf.Lerp(_beginAlpha, b, t);
			}
		}
	}

	public const float CameraTargetHeight = 1f;

	public EventValue Value = new EventValue();

	private List<IEventCameraEntry> _entryList = new List<IEventCameraEntry>();

	private List<IFocusCameraEntry> _focusEntryList = new List<IFocusCameraEntry>();

	private List<AlphaBlendValue> _alphaBlendList = new List<AlphaBlendValue>();

	private AlphaToggleValue2 _battleFieldToggle = new AlphaToggleValue2();

	private AlphaToggleValue2 _uiToggle = new AlphaToggleValue2();

	private AlphaToggleValue2 _uiOn3dToggle = new AlphaToggleValue2();

	public void SetAlphaToggleEntry(AlphaToggleEntry entry)
	{
		for (int i = 0; i < _alphaBlendList.Count; i++)
		{
			_alphaBlendList[i].Released = true;
		}
		if (entry != null)
		{
			AlphaBlendValue alphaBlendValue = new AlphaBlendValue();
			alphaBlendValue.ToggleEntry = entry;
			_alphaBlendList.Add(alphaBlendValue);
		}
	}

	public void SetAlphaToggleEntry2(AlphaToggleEntry2 entry)
	{
		_battleFieldToggle.Apply(entry.ShowBattleField, entry.BlendCurve, entry.TimeLength);
		_uiToggle.Apply(entry.ShowUi, entry.BlendCurve, entry.TimeLength);
		_uiOn3dToggle.Apply(entry.ShowUiOn3d, entry.BlendCurve, entry.TimeLength);
	}

	public void SetFocusEntry(IFocusCameraEntry entry)
	{
		for (int i = 0; i < _focusEntryList.Count; i++)
		{
			_focusEntryList[i].ReleaseFocus();
		}
		if (entry != null)
		{
			_focusEntryList.Add(entry);
		}
	}

	public void SetEntry(IEventCameraEntry entry)
	{
		for (int i = 0; i < _entryList.Count; i++)
		{
			if (_entryList[i].EventPriority > entry.EventPriority)
			{
				_entryList.Insert(i, entry);
				return;
			}
		}
		_entryList.Add(entry);
	}

	public void ReleaseEntry(IEventCameraEntry entry = null)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_entryList) && entry != null && _entryList.Remove(entry) && _entryList.Count <= 0)
		{
			Value.Reset();
			NotifyExternal();
		}
	}

	public void ChainUpdate(ref Vector3 cameraPos, ref Quaternion cameraRot, ref float cameraFov)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_entryList) || !__Extension_Method_ICollection__.IsNullOrEmpty(_focusEntryList) || !__Extension_Method_ICollection__.IsNullOrEmpty(_alphaBlendList))
		{
			Value.Reset();
			UpdateEntry(ref cameraPos, ref cameraRot, ref cameraFov);
			NotifyExternal();
		}
	}

	private void UpdateEntry(ref Vector3 cameraPos, ref Quaternion cameraRot, ref float cameraFov)
	{
		for (int num = _entryList.Count - 1; num >= 1; num--)
		{
			if (_entryList[num].FadeInEnd)
			{
				_entryList.RemoveRange(0, num);
				break;
			}
		}
		for (int i = 0; i < _focusEntryList.Count; i++)
		{
			IFocusCameraEntry focusCameraEntry = _focusEntryList[i];
			if (focusCameraEntry.Released && focusCameraEntry.Weight < Mathf.Epsilon)
			{
				_focusEntryList.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < _alphaBlendList.Count; j++)
		{
			AlphaBlendValue alphaBlendValue = _alphaBlendList[j];
			if (alphaBlendValue.Released && alphaBlendValue.Frame < Mathf.Epsilon)
			{
				_alphaBlendList.RemoveAt(j);
				j--;
			}
		}
		_battleFieldToggle.Update();
		_uiToggle.Update();
		_uiOn3dToggle.Update();
		Vector3 vector = cameraRot * Vector3.forward;
		float cameraDistance = ((!(Mathf.Abs(vector.y) > 0.01f)) ? Mathf.Clamp(Vespa_Unity_Vector3_Extension_Method.ReplaceX(cameraPos, 0f).magnitude, 1f, 20f) : ((1f - cameraPos.y) / vector.y));
		Vector3 targetPos = cameraPos + vector * cameraDistance;
		for (int k = 0; k < _entryList.Count; k++)
		{
			IEventCameraEntry eventCameraEntry = _entryList[k];
			eventCameraEntry.UpdateCamera(ref targetPos, ref cameraRot, ref cameraDistance, ref cameraFov);
			Value.UiAlpha = Mathf.Min(Value.UiAlpha, eventCameraEntry.UiAlpha);
			Value.UiOn3dAlpha = Mathf.Min(Value.UiOn3dAlpha, eventCameraEntry.UiOn3dAlpha);
			Value.BattleFieldAlpha = Mathf.Min(Value.BattleFieldAlpha, eventCameraEntry.BattleFieldAlpha);
			Value.ShakeWeight = Mathf.Min(Value.ShakeWeight, eventCameraEntry.ShakeWeight);
		}
		for (int l = 0; l < _focusEntryList.Count; l++)
		{
			_focusEntryList[l].UpdateCamera(ref targetPos, ref cameraRot, ref cameraDistance, ref cameraFov);
		}
		for (int m = 0; m < _alphaBlendList.Count; m++)
		{
			_alphaBlendList[m].Update(Value);
		}
		Value.BattleFieldAlpha = Mathf.Min(Value.BattleFieldAlpha, _battleFieldToggle.FinalAlpha);
		Value.UiAlpha = Mathf.Min(Value.UiAlpha, _uiToggle.FinalAlpha);
		Value.UiOn3dAlpha = Mathf.Min(Value.UiOn3dAlpha, _uiOn3dToggle.FinalAlpha);
		cameraPos = targetPos - cameraRot * Vector3.forward * cameraDistance;
	}

	private void NotifyExternal()
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			BattleViewGenericSingleton<CreatureManager>.instance.OnEventCamera(Value);
		}
		if (Manager.IsValidInstance)
		{
			Manager.instance.OnEventCamera(Value);
		}
		if (BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
		{
			BattleViewGenericSingleton<FieldMapManager2>.instance.OnEventCamera(Value);
		}
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			SceneAutoBehaviour<BattleManagement>.instance.OnEventCamera(Value);
		}
		if (SceneAutoBehaviour<ChattingManagement>.isValidInstance)
		{
			SceneAutoBehaviour<ChattingManagement>.instance.OnEventCamera(Value);
		}
	}
}
