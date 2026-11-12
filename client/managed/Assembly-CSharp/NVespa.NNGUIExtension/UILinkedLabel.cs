using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[RequireComponent(typeof(UILabel))]
public class UILinkedLabel : MonoBehaviour
{
	public UILabel Target;

	private UILabel _thisLabel;

	private void Awake()
	{
		_thisLabel = GetComponent<UILabel>();
		UpdateLinkLabel();
	}

	private void Update()
	{
		UpdateLinkLabel();
	}

	private void UpdateLinkLabel()
	{
		if (!(Target == null))
		{
			string text = _thisLabel.text;
			string text2 = Target.text;
			if (!string.Equals(text, text2))
			{
				_thisLabel.text = text2;
			}
		}
	}
}
