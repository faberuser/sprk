using UnityEngine;

namespace NVespa.NGui;

public class StyleBuilder
{
	public GUIStyle Style;

	public StyleBuilder(GUIStyle i_base)
	{
		Style = new GUIStyle(i_base);
	}

	public StyleBuilder WordWrap(bool i_wrap)
	{
		Style.wordWrap = i_wrap;
		return this;
	}

	public StyleBuilder WordWrap()
	{
		return WordWrap(i_wrap: true);
	}

	public StyleBuilder Color(Color i_color)
	{
		Style.normal.textColor = i_color;
		return this;
	}

	public StyleBuilder Alignment(TextAnchor i_anchor)
	{
		Style.alignment = i_anchor;
		return this;
	}

	public StyleBuilder FontSize(int i_size)
	{
		Style.fontSize = i_size;
		return this;
	}

	public StyleBuilder FontStyle(FontStyle i_fontStyle)
	{
		Style.fontStyle = i_fontStyle;
		return this;
	}

	public StyleBuilder FontBold()
	{
		Style.fontStyle = UnityEngine.FontStyle.Bold;
		return this;
	}

	public StyleBuilder TextClip()
	{
		Style.clipping = TextClipping.Clip;
		return this;
	}

	public StyleBuilder TextOverflow()
	{
		Style.clipping = TextClipping.Overflow;
		return this;
	}

	public StyleBuilder RichText(bool i_richText)
	{
		Style.richText = i_richText;
		return this;
	}

	public StyleBuilder RichText()
	{
		return RichText(i_richText: true);
	}
}
