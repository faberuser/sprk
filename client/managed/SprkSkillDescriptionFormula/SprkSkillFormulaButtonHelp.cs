using NGame2.NUI.NWindow;
using NGame2.NUtil;
using UnityEngine;

public sealed class SprkSkillFormulaButtonHelp : MonoBehaviour
{
	private void OnTooltip(bool show)
	{
		UIWidget component = GetComponent<UIWidget>();
		if (!(component == null))
		{
			if (!show)
			{
				GameOptionTooltip.Hide(component);
				return;
			}
			string value = SprkSkillFormulaWords.Get(SprkSkillDescriptionFormula.Formula ? "Show calculated values" : "Show formulas", SprkSkillFormulaWords.Locale);
			GameOptionTooltip.Open(component, new TooltipTextInfo
			{
				Name = new StringProxy
				{
					Value = value
				},
				Desc = new StringProxy
				{
					Value = value
				}
			});
		}
	}

	private void OnDisable()
	{
		UIWidget component = GetComponent<UIWidget>();
		if (component != null)
		{
			GameOptionTooltip.Hide(component);
		}
	}
}
