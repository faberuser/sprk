using UnityEngine;

namespace NVespa.NGui;

public static class PredefineStyle
{
	public const string TextAreaName = "TextArea";

	public static GUIStyle SmallTitleLabel { get; private set; }

	public static GUIStyle ASTextArea { get; private set; }

	public static GUIStyle WordWrapLabel { get; private set; }

	public static GUIStyle RichTextLabel { get; private set; }

	public static GUIStyle RedColorLabel { get; private set; }

	public static GUIStyle Tooltip { get; private set; }

	public static GUIStyle dockareaStandalone { get; private set; }

	public static GUIStyle flowOverlayBox { get; private set; }

	static PredefineStyle()
	{
		SmallTitleLabel = new GUIStyle(GUI.skin.label);
		SmallTitleLabel.fontStyle = FontStyle.Bold;
		SmallTitleLabel.fontSize = 15;
		SmallTitleLabel.alignment = TextAnchor.MiddleCenter;
		ASTextArea = new GUIStyle("TextArea");
		ASTextArea.stretchWidth = false;
		ASTextArea.stretchHeight = false;
		WordWrapLabel = VespaGuiStyleBuilder_Extension_Method.Builder(GUI.skin.label).WordWrap().Style;
		RichTextLabel = VespaGuiStyleBuilder_Extension_Method.Builder(GUI.skin.label).RichText().Style;
		RedColorLabel = VespaGuiStyleBuilder_Extension_Method.Builder(GUI.skin.label).Color(Color.red).Style;
		Tooltip = new GUIStyle("Tooltip");
		dockareaStandalone = new GUIStyle("dockareaStandalone");
		flowOverlayBox = new GUIStyle("flow overlay box");
	}
}
