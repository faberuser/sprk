using UnityEngine;

/// <summary>
/// Draws one name tag after a name label: a rounded pill tinted with the tag's
/// background colour, the icon (scaled to the text height) and the short text.
///
/// The name label itself is padded with trailing spaces as wide as the badge,
/// and the badge is drawn over them. The label's printed size therefore
/// includes the badge: right-aligned and centred names and anything anchored
/// after the name make room for it, and a fixed-width label that shrinks long
/// names (chat) shrinks name and badge together.
///
/// Sizes are measured once per name at the label's natural font size with a
/// free-sizing label, so shrinking never feeds back into the padding; the
/// badge is then scaled by however much the label actually shrank.
/// Created and fed by <see cref="SprkNameTag.OnText"/>; widgets are made
/// lazily and hidden whenever the label shows no tagged name.
/// </summary>
public sealed class SprkNameTagBadge : MonoBehaviour
{
	public UILabel Label;

	public long TagId;

	/// <summary>The name as shown, without markers or padding.</summary>
	public string Base;

	/// <summary>Spaces currently appended to <see cref="Base"/> in the label.</summary>
	public string Padding = string.Empty;

	private GameObject root;

	private UITexture background;

	private UITexture icon;

	private UILabel text;

	/// <summary>Free-sizing label in the name's font, used only to measure.</summary>
	private UILabel ruler;

	private int revision = -1;

	private long shownTag;

	// Natural (unshrunk) metrics of the current name, badge and space.
	private string measuredBase;

	private int measuredRevision = -1;

	private int measuredFontSize;

	private float lineHeight;

	private float spaceWidth;

	private int badgeWidth;

	private int badgeHeight;

	/// <summary>The badge's root while built (probes inspect its widgets).</summary>
	public GameObject Root => root;

	/// <summary>Natural width the padding adds to the label (times <see cref="Scale"/> when shrunk).</summary>
	public float PaddingWidth => spaceWidth * (float)Padding.Length;

	/// <summary>How much the label shrank its text (1: natural size).</summary>
	public float Scale { get; private set; } = 1f;

	/// <summary>
	/// Shows <paramref name="tag"/> after <paramref name="name"/> (0: none) and
	/// returns the label text to store: the name plus the current padding.
	/// </summary>
	internal string Show(long tag, string name)
	{
		if (tag <= 0)
		{
			TagId = 0L;
			Base = null;
			Padding = string.Empty;
			if (root != null && root.activeSelf)
			{
				root.SetActive(value: false);
			}
			return name;
		}
		// Same tag on a recycled list item keeps its padding until measured again (no flicker).
		if (tag != TagId)
		{
			Padding = string.Empty;
		}
		TagId = tag;
		Base = name;
		return name + Padding;
	}

	private void LateUpdate()
	{
		Refresh();
	}

	/// <summary>Shows, hides, pads and lays out the badge for the label's current state.</summary>
	public void Refresh()
	{
		if (Label == null || TagId <= 0)
		{
			return;
		}
		SprkNameTag.Tag tag = SprkNameTag.Get(TagId);
		if (tag == null || !Label.enabled || string.IsNullOrEmpty(Base))
		{
			if (root != null && root.activeSelf)
			{
				root.SetActive(value: false);
			}
			// Deleted tag: the name loses its room too.
			if (tag == null && Padding.Length > 0 && Base != null && SprkNameTag.Known(TagId))
			{
				Padding = string.Empty;
				Write(Base);
			}
			return;
		}
		if (root == null)
		{
			Build();
		}
		if (!root.activeSelf)
		{
			root.SetActive(value: true);
		}
		if (revision != SprkNameTag.Revision || shownTag != TagId)
		{
			revision = SprkNameTag.Revision;
			shownTag = TagId;
			text.text = tag.Label;
			text.color = tag.TextColor;
			background.color = tag.BackgroundColor;
			icon.mainTexture = tag.Icon;
			icon.enabled = tag.Icon != null;
			measuredRevision = -1;
		}
		Measure(tag);
		if (spaceWidth > 0.5f)
		{
			int count = Mathf.CeilToInt(((float)badgeWidth + Gap()) / spaceWidth);
			if (count != Padding.Length || Label.text != Base + Padding)
			{
				Padding = new string(' ', count);
				Write(Base + Padding);
			}
		}
		Layout(tag);
	}

	private float Gap()
	{
		return Mathf.Max(4f, (float)badgeHeight * 0.3f);
	}

	private void Write(string value)
	{
		SprkNameTag.Writing = true;
		try
		{
			Label.text = value;
		}
		finally
		{
			SprkNameTag.Writing = false;
		}
	}

	private void Build()
	{
		root = new GameObject("SprkNameTag");
		root.layer = Label.gameObject.layer;
		root.transform.SetParent(Label.transform, worldPositionStays: false);
		background = Widget<UITexture>("Pill");
		background.shader = Shader.Find("Unlit/Transparent Colored");
		background.mainTexture = SprkNameTag.Pill;
		background.type = UIBasicSprite.Type.Sliced;
		background.border = new Vector4(SprkNameTag.PillBorder, SprkNameTag.PillBorder, SprkNameTag.PillBorder, SprkNameTag.PillBorder);
		icon = Widget<UITexture>("Icon");
		icon.shader = Shader.Find("Unlit/Transparent Colored");
		text = MakeLabel("Text");
		text.fontStyle = FontStyle.Bold;
		ruler = MakeLabel("Ruler");
		ruler.alpha = 0f;
		ruler.enabled = false;
	}

	private T Widget<T>(string name) where T : UIWidget
	{
		GameObject gameObject = new GameObject(name);
		gameObject.layer = root.layer;
		gameObject.transform.SetParent(root.transform, worldPositionStays: false);
		T val = gameObject.AddComponent<T>();
		// NGUI keeps a widget in place when its pivot changes by moving the
		// transform; every widget is positioned explicitly in Layout.
		val.pivot = UIWidget.Pivot.Left;
		return val;
	}

	private UILabel MakeLabel(string name)
	{
		UILabel val = Widget<UILabel>(name);
		val.bitmapFont = Label.bitmapFont;
		val.trueTypeFont = Label.trueTypeFont;
		val.overflowMethod = UILabel.Overflow.ResizeFreely;
		val.alignment = NGUIText.Alignment.Left;
		val.effectStyle = UILabel.Effect.None;
		val.supportEncoding = false;
		return val;
	}

	/// <summary>Natural sizes of the name's line, a space and the badge.</summary>
	private void Measure(SprkNameTag.Tag tag)
	{
		if (measuredBase == Base && measuredRevision == revision && measuredFontSize == Label.fontSize)
		{
			return;
		}
		measuredBase = Base;
		measuredRevision = revision;
		measuredFontSize = Label.fontSize;
		ruler.fontSize = Label.fontSize;
		ruler.fontStyle = Label.fontStyle;
		ruler.spacingX = Label.spacingX;
		ruler.supportEncoding = Label.supportEncoding;
		ruler.text = "|" + Base + "|";
		Vector2 printedSize = ruler.printedSize;
		ruler.text = "|" + Base + "        |";
		spaceWidth = Mathf.Max(0f, (ruler.printedSize.x - printedSize.x) / 8f);
		lineHeight = Mathf.Max(1f, Mathf.Min(printedSize.y, (float)Label.fontSize * 1.4f));
		badgeHeight = Mathf.Max(8, Mathf.RoundToInt(lineHeight * 0.9f));
		int num = Mathf.Max(2, Mathf.RoundToInt((float)badgeHeight * 0.3f));
		int num2 = (tag.Icon != null) ? Mathf.RoundToInt((float)badgeHeight * 0.72f) : 0;
		text.fontSize = Mathf.Max(6, Mathf.RoundToInt((float)badgeHeight * 0.7f));
		float x = text.printedSize.x;
		int num3 = num + ((num2 > 0) ? (num2 + Mathf.RoundToInt((float)num * 0.6f)) : 0);
		badgeWidth = Mathf.Max(Mathf.RoundToInt((float)num3 + x + (float)num), SprkNameTag.PillBorder * 2);
		background.width = badgeWidth;
		background.height = Mathf.Max(badgeHeight, SprkNameTag.PillBorder * 2 - 2);
		background.cachedTransform.localPosition = Vector3.zero;
		if (num2 > 0)
		{
			float num4 = (float)tag.Icon.width / (float)Mathf.Max(1, tag.Icon.height);
			icon.height = num2;
			icon.width = Mathf.Max(1, Mathf.RoundToInt((float)num2 * num4));
			if (icon.width > num2)
			{
				icon.width = num2;
				icon.height = Mathf.Max(1, Mathf.RoundToInt((float)num2 / num4));
			}
			icon.cachedTransform.localPosition = new Vector3(num + (num2 - icon.width) / 2, 0f, 0f);
		}
		text.cachedTransform.localPosition = new Vector3(num3, 0f, 0f);
	}

	private void Layout(SprkNameTag.Tag tag)
	{
		Vector2 printedSize = Label.printedSize;
		if (printedSize.x <= 0f || printedSize.y <= 0f)
		{
			root.SetActive(value: false);
			return;
		}
		// A shrink-to-fit label printed everything smaller: scale the badge alike.
		Scale = Mathf.Clamp(printedSize.y / lineHeight, 0.05f, 1f);
		root.transform.localScale = new Vector3(Scale, Scale, 1f);
		int depth = Label.depth;
		background.depth = depth;
		icon.depth = depth + 1;
		text.depth = depth + 1;
		// Where the label prints (see UILabel.ApplyOffset and NGUIText alignment).
		Vector2 pivotOffset = Label.pivotOffset;
		float num = Label.width;
		float num2 = Label.height;
		float num3 = (0f - pivotOffset.x) * num;
		NGUIText.Alignment alignment = Label.alignment;
		if (alignment == NGUIText.Alignment.Automatic)
		{
			alignment = ((pivotOffset.x < 0.25f) ? NGUIText.Alignment.Left : ((pivotOffset.x > 0.75f) ? NGUIText.Alignment.Right : NGUIText.Alignment.Center));
		}
		float num4 = alignment switch
		{
			NGUIText.Alignment.Center => num3 + (num - printedSize.x) * 0.5f,
			NGUIText.Alignment.Right => num3 + num - printedSize.x,
			_ => num3,
		};
		float num5 = Mathf.Lerp(num2, 0f, pivotOffset.y) + Mathf.Lerp(printedSize.y - num2, 0f, pivotOffset.y);
		float y = num5 - lineHeight * Scale * 0.5f;
		// With padding the badge ends where the printed text (name + spaces) ends;
		// without (a font with no space width) it simply follows the name.
		float x = ((spaceWidth > 0.5f) ? (num4 + printedSize.x - (float)badgeWidth * Scale) : (num4 + printedSize.x + Gap()));
		root.transform.localPosition = new Vector3(Mathf.Round(x), Mathf.Round(y), 0f);
		float alpha = Label.alpha;
		background.alpha = alpha * tag.BackgroundColor.a;
		icon.alpha = alpha;
		text.alpha = alpha * tag.TextColor.a;
	}

	private void OnDestroy()
	{
		if (root != null)
		{
			Object.Destroy(root);
		}
	}
}
