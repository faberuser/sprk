using UnityEngine;

namespace NVespa.NGui;

public class RichText
{
	public string m_text;

	public RichText(string i_text)
	{
		if (String_Extension_Method.IsNullOrEmpty(i_text))
		{
			m_text = string.Empty;
		}
		else
		{
			m_text = i_text;
		}
	}

	public RichText Bold()
	{
		m_text = "<b>" + m_text + "</b>";
		return this;
	}

	public RichText Italics()
	{
		m_text = "<i>" + m_text + "</i>";
		return this;
	}

	public RichText Size(int i_size)
	{
		m_text = $"<size={i_size}>{m_text}</size>";
		return this;
	}

	public RichText Color(Color i_color)
	{
		Color32 color = i_color;
		string arg = $"#{color.r:x2}{color.g:x2}{color.b:x2}ff";
		m_text = $"<color={arg}>{m_text}</color>";
		return this;
	}
}
