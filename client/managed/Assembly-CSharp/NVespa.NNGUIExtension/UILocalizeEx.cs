using NVespa.NGlobal;
using NVespa.NUtil;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/UI/Localize Ex (Vespa)")]
public class UILocalizeEx : MonoBehaviour, IInspectorGuiDrawer
{
	public bool IsLocalizeInput;

	[HideInInspector]
	public LocalizeLinkData LocalizeLink = new LocalizeLinkData();

	[HideInInspector]
	public LocalizeLinkData IPhoneLocalizeLink = new LocalizeLinkData();

	private bool mStarted;

	private object[] _localizeArguments;

	public LocalizeLinkData PlatformLink => LocalizeLink;

	public void SetLocalizeArg(params object[] args)
	{
		if (Application.isPlaying)
		{
			_localizeArguments = args;
			if (mStarted)
			{
				OnLocalize();
			}
		}
	}

	private void Reset()
	{
		_localizeArguments = null;
	}

	public void OnReloadLocalization()
	{
		if (mStarted)
		{
			OnLocalize();
		}
	}

	private void OnEnable()
	{
		if (mStarted)
		{
			OnLocalize();
		}
	}

	private void Start()
	{
		mStarted = true;
		OnLocalize();
	}

	private void OnLocalize()
	{
		SingletonAuto<LocalizationManager>.CreateInstance();
		bool flag = PlatformLink.IsValid();
		if (!flag && String_Extension_Method.IsNullOrEmpty(PlatformLink.Key))
		{
			if (Application.isPlaying)
			{
				SetLabelEmptyKey();
			}
			return;
		}
		string text = ((!flag) ? ("@" + PlatformLink.Key) : ((__Extension_Method_ICollection__.COUNT(_localizeArguments) <= 0) ? PlatformLink.GetLocalize() : PlatformLink.GetLocalize(_localizeArguments)));
		UIWidget component = GetComponent<UIWidget>();
		UILabel uILabel = component as UILabel;
		if (uILabel != null)
		{
			UIInput uIInput = null;
			if (IsLocalizeInput)
			{
				uIInput = NGUITools.FindInParents<UIInput>(uILabel.gameObject);
			}
			if (uIInput != null && uIInput.label == uILabel)
			{
				uIInput.defaultText = text;
			}
			else
			{
				uILabel.text = text;
			}
			return;
		}
		UISprite uISprite = component as UISprite;
		if (uISprite != null && flag)
		{
			UIButton uIButton = NGUITools.FindInParents<UIButton>(uISprite.gameObject);
			if (uIButton != null && uIButton.tweenTarget == uISprite.gameObject)
			{
				uIButton.normalSprite = text;
			}
			uISprite.spriteName = text;
			uISprite.MakePixelPerfect();
		}
	}

	private void SetLabelEmptyKey()
	{
		UILabel uILabel = GetComponent<UIWidget>() as UILabel;
		if (uILabel != null)
		{
			uILabel.text = "[ff0000ff]** 로컬라이즈 키가 존재하지 않습니다 **[ff0000ff]";
		}
	}
}
