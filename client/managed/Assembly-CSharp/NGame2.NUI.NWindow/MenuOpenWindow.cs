using System;
using System.Collections;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MenuOpenWindow : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tutorial/MenuOpenWindow";

	private Action _onClose;

	public UILabel TextLabel;

	public UIWidget WidgetConfirm;

	public UIGame3DWidget BackgroundEffect;

	public Transform LabelStartPosition;

	public Transform LabelEndPosition;

	public float initUIDuration;

	public AnimationCurve SwipeCurve;

	private Coroutine _initUI;

	public void Open(ERightMenuButton type, Action onOk)
	{
		base.Open();
		_onClose = onOk;
		BackgroundEffect.Stop(immediatelyClear: true);
		BackgroundEffect.Play(null);
		UIHelper.EnableGameObject(TextLabel, enable: false);
		UIHelper.EnableGameObject(WidgetConfirm, enable: false);
		BottomButtonData data = GameRoot.Instance.BottomButtonDataContainer.GetData(type);
		string text = string.Empty;
		if (data != null)
		{
			text = LocalizationManager.GetStringDirect(data.LabelKey);
		}
		string stringDirect = LocalizationManager.GetStringDirect("OPEN_FUNCTION_MESSAGE", text);
		UIHelper.SetLabelText(TextLabel, stringDirect);
		if (_initUI != null)
		{
			VespaCoroutine.StopScene(_initUI);
			_initUI = null;
		}
		_initUI = VespaCoroutine.StartScene(coInitUI(null));
	}

	public override void Reset()
	{
		base.Reset();
		if (_initUI != null)
		{
			VespaCoroutine.StopScene(_initUI);
			_initUI = null;
		}
		BackgroundEffect.Stop(immediatelyClear: true);
		_onClose = null;
	}

	public void OnClickConfirm()
	{
		Action onClose = _onClose;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onClose);
	}

	public override void OnSystemBackButton()
	{
		OnClickConfirm();
	}

	private IEnumerator coInitUI(Action onComplete)
	{
		yield return new WaitForSeconds(0.6f);
		float elapsedTime = -0.03f;
		yield return null;
		Vector3 _startPos = LabelStartPosition.localPosition;
		Vector3 _endPos = LabelEndPosition.localPosition;
		UIHelper.EnableGameObject(TextLabel, enable: true);
		UIHelper.EnableGameObject(WidgetConfirm, enable: true);
		WidgetConfirm.alpha = 0f;
		TextLabel.transform.localPosition = _startPos;
		do
		{
			yield return null;
			elapsedTime += GlobalTimer.deltaTime;
			float num = SwipeCurve.Evaluate(Mathf.Clamp01(elapsedTime / initUIDuration));
			TextLabel.transform.localPosition = Vector3.Lerp(_startPos, _endPos, num);
			WidgetConfirm.alpha = num;
		}
		while (!(elapsedTime > initUIDuration));
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}
}
