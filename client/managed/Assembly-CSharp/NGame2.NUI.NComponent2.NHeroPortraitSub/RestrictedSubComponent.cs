using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class RestrictedSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public const int DEFAULT_VALUE_FONT_SIZE = 25;

	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/RestrictedSubComponent";

	public static PrefabAuto<RestrictedSubComponent> PrefabAuto = new PrefabAuto<RestrictedSubComponent>(PrefabPath);

	public static Color DefaultColorLabel = Color.white;

	public static Color DefaultColorGradientBottom = new Color(0.7f, 0.7f, 0.7f, 1f);

	public UIWidget TextRoot;

	public UIWidget SpriteDiabled;

	public UIWidget TextRootSmall;

	public UIWidget SpriteDiabledSmall;

	public UILabel[] LabelTextList;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.CustomLabel));
		Reset();
	}

	public void SetLarge()
	{
		SetLargetInternal(active: true);
		SetSmallInternal(active: false);
	}

	public void SetSmall()
	{
		SetLargetInternal(active: false);
		SetSmallInternal(active: true);
	}

	public void SetDiabledSpriteLarge(bool active)
	{
		UIHelper.EnableGameObject(SpriteDiabled, active);
	}

	public void SetDiabledSpriteSmall(bool active)
	{
		UIHelper.EnableGameObject(SpriteDiabledSmall, active);
	}

	private void SetLargetInternal(bool active)
	{
		UIHelper.EnableGameObject(TextRoot, active);
		UIHelper.EnableGameObject(SpriteDiabled, active);
	}

	private void SetSmallInternal(bool active)
	{
		UIHelper.EnableGameObject(TextRootSmall, active);
		UIHelper.EnableGameObject(SpriteDiabledSmall, active);
	}

	public void SetLabelText(string text)
	{
		int i = 0;
		for (int num = LabelTextList.Length; i < num; i++)
		{
			UIHelper.SetLabelText(LabelTextList[i], text);
		}
	}

	public void SetLabelColor(Color color)
	{
		int i = 0;
		for (int num = LabelTextList.Length; i < num; i++)
		{
			UIHelper.SetLabelColor(LabelTextList[i], color);
		}
	}

	public void SetFontSize(int size)
	{
		int i = 0;
		for (int num = LabelTextList.Length; i < num; i++)
		{
			UILabel uILabel = LabelTextList[i];
			if (!(uILabel == null))
			{
				uILabel.fontSize = size;
			}
		}
	}

	public void SetGradientActivation(bool isActive)
	{
		int i = 0;
		for (int num = LabelTextList.Length; i < num; i++)
		{
			UILabel uILabel = LabelTextList[i];
			if (!(uILabel == null))
			{
				uILabel.applyGradient = isActive;
			}
		}
	}

	public void SetGradientColor(Color top, Color bottom)
	{
		int i = 0;
		for (int num = LabelTextList.Length; i < num; i++)
		{
			UILabel uILabel = LabelTextList[i];
			if (!(uILabel == null))
			{
				uILabel.gradientTop = top;
				uILabel.gradientBottom = bottom;
			}
		}
	}

	private void Reset()
	{
		SetLargetInternal(active: false);
		SetSmallInternal(active: false);
		int i = 0;
		for (int num = LabelTextList.Length; i < num; i++)
		{
			UILabel uILabel = LabelTextList[i];
			if (!(uILabel == null))
			{
				UIHelper.SetLabelColor(uILabel, DefaultColorLabel);
				uILabel.fontSize = 25;
				uILabel.applyGradient = false;
				uILabel.gradientTop = DefaultColorLabel;
				uILabel.gradientBottom = DefaultColorGradientBottom;
			}
		}
	}
}
