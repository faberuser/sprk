using System;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.PlayRecord.DisplayValue;

public class TextValueComponent : MonoBehaviour, IDisplayValueComponent
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/DisplayValue/TextValueComponent";

	public static PrefabAuto<TextValueComponent> PrefabAuto = new PrefabAuto<TextValueComponent>("Prefab2/Component/PlayRecord/DisplayValue/TextValueComponent");

	public UILabel ValueLabel;

	private Func<string, string> _textConverter;

	private string _defaultText;

	void IDisplayValueComponent.Open(PlayRecordData playRecordData, string recordValue)
	{
		string text = recordValue;
		if (text == null || string.Equals(text, "null", StringComparison.CurrentCultureIgnoreCase))
		{
			text = _defaultText;
		}
		if (playRecordData.AppendComma)
		{
			int num = Vespa_String_Extension_Method.TOINT(text, 0);
			text = Vespa_Int_Extension_Method.ToCommaString(num);
		}
		if (_textConverter != null)
		{
			text = Vespa_System_Action_Extension_Method.fire(_textConverter, text);
		}
		UIHelper.SetLabelText(ValueLabel, text);
	}

	public void SetTextConverter<T>(Func<string, string> textConverter, T defaultValue)
	{
		_textConverter = textConverter;
		_defaultText = ((defaultValue == null) ? "-" : defaultValue.ToString());
	}
}
