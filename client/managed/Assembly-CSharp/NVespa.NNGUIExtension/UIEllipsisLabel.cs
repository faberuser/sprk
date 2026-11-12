using NGame2.NUI;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[RequireComponent(typeof(UILabel))]
public class UIEllipsisLabel : MonoBehaviour
{
	public int DotCount = 3;

	private UILabel _thisLabel;

	private string _text;

	private void Awake()
	{
		_thisLabel = GetComponent<UILabel>();
	}

	private void Update()
	{
		if (_thisLabel.overflowMethod != UILabel.Overflow.ClampContent)
		{
			_thisLabel.overflowMethod = UILabel.Overflow.ClampContent;
			return;
		}
		string text = _thisLabel.text;
		if (!string.Equals(text, _text))
		{
			_text = UIHelper.GetFitEllipsisString(text, _thisLabel, DotCount);
			_thisLabel.text = _text;
		}
	}
}
