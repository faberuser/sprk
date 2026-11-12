using System;
using System.Collections.Generic;
using NGame2.NUI;
using NNguiExtension;
using UnityEngine;

namespace NGame2.NUI3.NEmbed;

public class Embed_EquipStarBox_Comp : MonoBehaviour
{
	[Serializable]
	public class Case5StarUi
	{
		private enum ESelectType
		{
			Blank,
			Normal,
			TechnoMagic
		}

		public UIWidget Root;

		public List<UISpriteSelector> ToggleStarList;

		public void InitUi(int star, bool isTechnoMagic)
		{
			UIHelper.EnableGameObject(Root, enable: true);
			star = Mathf.Min(star, 5);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleStarList); i++)
			{
				UISpriteSelector uISpriteSelector = ToggleStarList[i];
				if (i + 1 > star)
				{
					uISpriteSelector.SelectEnum(ESelectType.Blank);
				}
				else if (isTechnoMagic)
				{
					uISpriteSelector.SelectEnum(ESelectType.TechnoMagic);
				}
				else
				{
					uISpriteSelector.SelectEnum(ESelectType.Normal);
				}
			}
		}

		public void ResetUi()
		{
			UIHelper.EnableGameObject(Root, enable: false);
		}
	}

	[Serializable]
	public class CaseLimitBreakUi
	{
		public UIWidget Root;

		public UITable Table_Layout;

		public UILabel Label_StarCount;

		public void InitUi(int star)
		{
			UIHelper.EnableGameObject(Root, enable: true);
			UIHelper.SetLabelText(Label_StarCount, star.ToString());
			if (Table_Layout != null)
			{
				Table_Layout.Reposition();
			}
		}

		public void ResetUi()
		{
			UIHelper.EnableGameObject(Root, enable: false);
		}
	}

	public Case5StarUi CaseNormal;

	public CaseLimitBreakUi CaseLimitBreak;

	public void InitUi(int star, bool isTechnoMagic)
	{
		Reset();
		if (star <= 5)
		{
			CaseNormal.InitUi(star, isTechnoMagic);
		}
		else
		{
			CaseLimitBreak.InitUi(star);
		}
	}

	public void Reset()
	{
		CaseNormal.ResetUi();
		CaseLimitBreak.ResetUi();
	}
}
