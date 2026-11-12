using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[RequireComponent(typeof(UIInput))]
public class UIAndroidKeyboard : MonoBehaviour
{
	public EventDelegate onSubmit;

	private UIInput _thisInput;

	public static void DrawGui()
	{
	}

	private void Awake()
	{
		_thisInput = GetComponent<UIInput>();
		if (_thisInput != null && onSubmit != null && onSubmit.isValid && !_thisInput.onSubmit.Contains(onSubmit))
		{
			_thisInput.onReturnKey = UIInput.OnReturnKey.Submit;
			_thisInput.onSubmit.Add(onSubmit);
		}
	}

	private void OnClick()
	{
	}
}
