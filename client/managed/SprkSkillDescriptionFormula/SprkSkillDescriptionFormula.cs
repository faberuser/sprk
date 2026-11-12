using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using LitJson;
using NGame2.NAccount;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NTypeValue.NOperator.NData.NOperationData;
using NGame2.NUI;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NOperationRuntime;
using NShared.NUtil;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkSkillDescriptionFormula : MonoBehaviour
{
	public sealed class Result
	{
		public string Text;

		public int Formulas;

		public readonly HashSet<string> Unsupported = new HashSet<string>();

		public readonly Dictionary<string, string> Legends = new Dictionary<string, string>();
	}

	private sealed class Expression : IFormattable
	{
		public string Text;

		public decimal Value;

		public bool Symbolic;

		public int Priority = 3;

		public string Key;

		public List<Expression> Addends;

		public Expression(string text, decimal value, bool symbolic)
		{
			Text = text;
			Value = value;
			Symbolic = symbolic;
			Key = (symbolic ? "symbol:" : "constant:") + text + ":" + Number(value);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			if (!Symbolic)
			{
				return Value.ToString(format, provider);
			}
			return Text;
		}

		public override string ToString()
		{
			return Text;
		}
	}

	private sealed class Context
	{
		public HeroInfo Hero;

		public StatData Stat;

		public StatController FormulaStats;

		public OperationInfo Operation;

		public bool RuntimeValues;

		public int Level;

		public SkillDescVariable Variable;

		public Result Result = new Result();

		public Func<string, string> Localize;
	}

	private const string Preference = "Sprk.SkillDescription.Formula";

	private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

	private HeroSkillDesc card;

	private ArchiveSkillDesc archive;

	private UIButton button;

	private UILabel modeLabel;

	private UIWidget descriptionBounds;

	private int skillIndex;

	private static CalcDamage.FuncLinker formulaLinker;

	public static Func<string, string> WordProvider;

	private MonoBehaviour Owner
	{
		get
		{
			if (!(card != null))
			{
				return archive;
			}
			return card;
		}
	}

	private HeroInfo Hero
	{
		get
		{
			if (!(card != null))
			{
				return archive.HeroInfo;
			}
			return card.HeroInfo;
		}
	}

	private UILabel Description
	{
		get
		{
			if (!(card != null))
			{
				return archive.LabelDesc;
			}
			return card.Label_SkillDesc;
		}
	}

	private UILabel Title
	{
		get
		{
			if (!(card != null))
			{
				return archive.LabelName;
			}
			return card.Label_SkillLevel;
		}
	}

	private UIButton LinkedButton
	{
		get
		{
			if (!(card != null))
			{
				return archive.Button_LinkedSkill;
			}
			return card.Button_LinkedSkill;
		}
	}

	private UIScrollView Scroll
	{
		get
		{
			if (!(card != null))
			{
				return archive.LabelDesc.GetComponentInParent<UIScrollView>();
			}
			return card.ScrollView_SkillDesc;
		}
	}

	public static bool Formula
	{
		get
		{
			return SprkTestPrefs.GetInt("Sprk.SkillDescription.Formula", 0) != 0;
		}
		set
		{
			SprkTestPrefs.SetInt("Sprk.SkillDescription.Formula", value ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	public static void Setup(HeroSkillDesc owner, Helper.SkillDetailInfo info)
	{
		SprkSkillDescriptionFormula sprkSkillDescriptionFormula = owner.GetComponent<SprkSkillDescriptionFormula>();
		if (sprkSkillDescriptionFormula == null)
		{
			sprkSkillDescriptionFormula = owner.gameObject.AddComponent<SprkSkillDescriptionFormula>();
		}
		sprkSkillDescriptionFormula.card = owner;
		sprkSkillDescriptionFormula.skillIndex = info.BasicInfo.TableIndex;
		try
		{
			sprkSkillDescriptionFormula.EnsureButton();
			sprkSkillDescriptionFormula.Display();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK formula card: " + ex.Message);
		}
	}

	public static void SetupArchive(ArchiveSkillDesc owner)
	{
		if (owner.HeroInfo == null)
		{
			return;
		}
		int linkIndex = (int)typeof(ArchiveSkillDesc).GetField("_linkedIndex", Private).GetValue(owner);
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(owner.HeroInfo, owner.SkillSlotIndex, linkIndex);
		if (skillDetailInfo.BasicInfo.TableData == null)
		{
			return;
		}
		SprkSkillDescriptionFormula sprkSkillDescriptionFormula = owner.GetComponent<SprkSkillDescriptionFormula>();
		if (sprkSkillDescriptionFormula == null)
		{
			sprkSkillDescriptionFormula = owner.gameObject.AddComponent<SprkSkillDescriptionFormula>();
		}
		sprkSkillDescriptionFormula.archive = owner;
		sprkSkillDescriptionFormula.skillIndex = skillDetailInfo.BasicInfo.TableIndex;
		try
		{
			sprkSkillDescriptionFormula.EnsureButton();
			sprkSkillDescriptionFormula.Display();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK Index formula card: " + ex.Message);
		}
	}

	private static Transform Header(UILabel title)
	{
		Transform parent = title.transform.parent;
		while (parent != null)
		{
			UIWidget component = parent.GetComponent<UIWidget>();
			if (component != null && component.width > 200 && component.height >= 40 && component.height <= 150)
			{
				return parent;
			}
			parent = parent.parent;
		}
		return title.transform.parent;
	}

	private void EnsureButton()
	{
		if (button != null)
		{
			return;
		}
		UIButton linkedButton = LinkedButton;
		if (linkedButton == null)
		{
			return;
		}
		if (archive != null)
		{
			UIButton uIButton = archive.GetComponentsInChildren<UIButton>(includeInactive: true).FirstOrDefault((UIButton b) => b.name == "Button_Close");
			if (uIButton != null)
			{
				Transform parent = linkedButton.transform.parent;
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(parent, uIButton.transform, considerInactive: true);
				UIWidget component = linkedButton.GetComponent<UIWidget>();
				float num = ((component == null) ? 27f : ((float)component.width * 0.5f));
				float x = parent.InverseTransformPoint(Header(Title).position).x;
				UnityEngine.Vector3 localPosition = linkedButton.transform.localPosition;
				localPosition.x = bounds.min.x - num - 10f;
				linkedButton.transform.localPosition = localPosition;
				UIRect component2 = linkedButton.GetComponent<UIRect>();
				if (component2 != null)
				{
					component2.leftAnchor.target = null;
					component2.rightAnchor.target = null;
					component2.topAnchor.target = null;
					component2.bottomAnchor.target = null;
				}
				Title.width = Mathf.Min(Title.width, Mathf.FloorToInt(2f * (localPosition.x - x - num - 10f)));
				Title.overflowMethod = UILabel.Overflow.ShrinkContent;
				Title.maxLineCount = 1;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(linkedButton.gameObject, linkedButton.transform.parent, worldPositionStays: false);
		gameObject.name = "SPRK Formula Toggle";
		button = gameObject.GetComponent<UIButton>();
		MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			switch (monoBehaviour.GetType().Name)
			{
			case "UIEvent":
			case "UIButtonMessage":
			case "UIEventListener":
			case "UIPlayAnimation":
			case "UIPlayTween":
				monoBehaviour.enabled = false;
				UnityEngine.Object.Destroy(monoBehaviour);
				break;
			}
		}
		button.onClick.Clear();
		EventDelegate.Add(button.onClick, Toggle);
		UILabel title = Title;
		Transform parent2 = linkedButton.transform.parent;
		UnityEngine.Vector3 vector = parent2.InverseTransformPoint(Header(title).position);
		UnityEngine.Vector3 localPosition2 = linkedButton.transform.localPosition;
		localPosition2.x = 2f * vector.x - localPosition2.x;
		if (archive != null)
		{
			UIButton uIButton2 = archive.GetComponentsInChildren<UIButton>(includeInactive: true).FirstOrDefault((UIButton b) => b.name == "Button_Close");
			if (uIButton2 != null)
			{
				UnityEngine.Vector3 vector2 = parent2.InverseTransformPoint(uIButton2.transform.position);
				localPosition2.x = 2f * vector.x - vector2.x;
				localPosition2.y = vector2.y;
			}
		}
		gameObject.transform.localPosition = localPosition2;
		UIRect[] componentsInChildren2 = gameObject.GetComponentsInChildren<UIRect>(includeInactive: true);
		foreach (UIRect obj in componentsInChildren2)
		{
			obj.leftAnchor.target = null;
			obj.rightAnchor.target = null;
			obj.topAnchor.target = null;
			obj.bottomAnchor.target = null;
		}
		GameObject gameObject2 = new GameObject("Label_Mode");
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		modeLabel = gameObject2.AddComponent<UILabel>();
		modeLabel.bitmapFont = title.bitmapFont;
		modeLabel.trueTypeFont = title.trueTypeFont;
		modeLabel.fontSize = Mathf.Max(14, Mathf.RoundToInt((float)title.fontSize * 0.68f));
		modeLabel.color = title.color;
		modeLabel.effectStyle = title.effectStyle;
		modeLabel.effectColor = title.effectColor;
		modeLabel.width = 52;
		modeLabel.height = 40;
		modeLabel.depth = linkedButton.GetComponentsInChildren<UIWidget>(includeInactive: true).Max((UIWidget w) => w.depth) + 2;
		modeLabel.overflowMethod = UILabel.Overflow.ShrinkContent;
		UISprite[] componentsInChildren3 = gameObject.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite uISprite in componentsInChildren3)
		{
			if (uISprite.type != UIBasicSprite.Type.Sliced)
			{
				uISprite.enabled = false;
			}
		}
		UISprite uISprite2 = Owner.GetComponentsInChildren<UISprite>(includeInactive: true).FirstOrDefault((UISprite s) => s.name == "Button_Learn" && s.type == UIBasicSprite.Type.Sliced);
		UISprite component3 = gameObject.GetComponent<UISprite>();
		if (component3 != null)
		{
			component3.atlas = ((uISprite2 == null) ? null : uISprite2.atlas);
			component3.type = UIBasicSprite.Type.Sliced;
			component3.spriteName = "Mission_Button_Normal";
			component3.enabled = true;
			button.normalSprite = "Mission_Button_Normal";
			button.hoverSprite = "Mission_Button_Normal";
			button.pressedSprite = "Mission_Button_Normal";
			button.disabledSprite = "Mission_Button_Disable";
			UISpriteAtlasAssigner uISpriteAtlasAssigner = gameObject.GetComponent<UISpriteAtlasAssigner>();
			if (uISpriteAtlasAssigner == null)
			{
				uISpriteAtlasAssigner = gameObject.AddComponent<UISpriteAtlasAssigner>();
			}
			if (uISpriteAtlasAssigner != null)
			{
				uISpriteAtlasAssigner.AtlasName = "Common2";
				uISpriteAtlasAssigner.SpriteName = "Mission_Button_Normal";
				uISpriteAtlasAssigner.Target = component3;
				if (component3.atlas == null)
				{
					uISpriteAtlasAssigner.Assign();
				}
			}
		}
		gameObject.AddComponent<SprkSkillFormulaButtonHelp>();
		gameObject.SetActive(value: true);
	}

	public void Toggle()
	{
		Formula = !Formula;
		SprkSkillDescriptionFormula[] array = Resources.FindObjectsOfTypeAll<SprkSkillDescriptionFormula>();
		foreach (SprkSkillDescriptionFormula sprkSkillDescriptionFormula in array)
		{
			if (sprkSkillDescriptionFormula.Owner != null && sprkSkillDescriptionFormula.Owner.gameObject.activeInHierarchy)
			{
				sprkSkillDescriptionFormula.Display();
			}
		}
	}

	private void Display()
	{
		if (modeLabel != null)
		{
			modeLabel.text = (Formula ? "fx" : "123");
		}
		if (button != null)
		{
			button.gameObject.SetActive(value: true);
		}
		string text = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(skillIndex, Hero);
		if (Formula)
		{
			try
			{
				text = Render(skillIndex, Hero, null).Text;
			}
			catch (Exception)
			{
				text = text + "\n\n" + Words("Formula view unavailable for this skill.");
			}
		}
		UIHelper.SetLabelText(Description, text);
		LayoutDescription();
	}

	internal static UnityEngine.Vector3 TextStart(UILabel label, Transform relativeTo)
	{
		RenderingList<UnityEngine.Vector3> renderingList = new RenderingList<UnityEngine.Vector3>();
		label.OnFill(renderingList, new RenderingList<Vector2>(), new RenderingList<Color32>());
		if (renderingList.size < 4)
		{
			return relativeTo.InverseTransformPoint(label.worldCorners[1]);
		}
		float num = float.MaxValue;
		float num2 = float.MinValue;
		for (int i = 0; i < 4; i++)
		{
			UnityEngine.Vector3 vector = relativeTo.InverseTransformPoint(label.transform.TransformPoint(renderingList[i]));
			num = Mathf.Min(num, vector.x);
			num2 = Mathf.Max(num2, vector.y);
		}
		return new UnityEngine.Vector3(num, num2, 0f);
	}

	private void LayoutDescription()
	{
		UIScrollView scroll = Scroll;
		if (scroll == null)
		{
			return;
		}
		UISprite uISprite = Owner.GetComponentsInChildren<UISprite>(includeInactive: true).FirstOrDefault((UISprite s) => s.name == "Sprite_DescBack");
		if (uISprite == null)
		{
			return;
		}
		UIPanel component = scroll.GetComponent<UIPanel>();
		scroll.disableDragIfFits = false;
		scroll.restrictWithinPanel = true;
		scroll.iOSDragEmulation = true;
		scroll.dragEffect = UIScrollView.DragEffect.MomentumAndSpring;
		scroll.InvalidateBounds();
		scroll.ResetPosition();
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(scroll.transform, uISprite.transform, considerInactive: true);
		UnityEngine.Vector3 vector = TextStart(Description, scroll.transform);
		float num = vector.x - bounds.min.x;
		UnityEngine.Vector3 vector2 = scroll.transform.TransformVector(new UnityEngine.Vector3(0f, bounds.max.y - num - vector.y, 0f));
		Description.topAnchor.target = null;
		Description.bottomAnchor.target = null;
		Description.transform.position += vector2;
		if (descriptionBounds == null)
		{
			GameObject gameObject = new GameObject("SPRK Description Bounds");
			gameObject.transform.SetParent(scroll.transform, worldPositionStays: false);
			descriptionBounds = gameObject.AddComponent<UIWidget>();
			descriptionBounds.pivot = UIWidget.Pivot.Center;
		}
		Vector4 finalClipRegion = component.finalClipRegion;
		float num2 = ((component.clipping == UIDrawCall.Clipping.SoftClip) ? component.clipSoftness.y : 0f);
		descriptionBounds.width = Mathf.RoundToInt(finalClipRegion.z);
		descriptionBounds.height = Mathf.RoundToInt(finalClipRegion.w - 2f * num2);
		descriptionBounds.transform.localPosition = new UnityEngine.Vector3(finalClipRegion.x, finalClipRegion.y, 0f);
		if (archive != null)
		{
			UIWidget uIWidget = scroll.GetComponentsInChildren<UIWidget>(includeInactive: true).FirstOrDefault((UIWidget w) => w.name == "DragScrollView");
			if (uIWidget != null)
			{
				uIWidget.topAnchor.target = null;
				uIWidget.bottomAnchor.target = null;
				uIWidget.height = descriptionBounds.height;
				uIWidget.transform.position = descriptionBounds.transform.position;
				NGUITools.UpdateWidgetCollider(uIWidget.gameObject);
			}
		}
		scroll.InvalidateBounds();
		scroll.ResetPosition();
	}

	private static string Number(decimal n)
	{
		return n.ToString("0.############################", CultureInfo.InvariantCulture);
	}

	private static Expression Constant(decimal n)
	{
		return new Expression(Number(n), n, symbolic: false);
	}

	private static Expression Symbol(string s, decimal n)
	{
		return new Expression(s, n, symbolic: true);
	}

	private static Expression Binary(Expression a, Expression b, string op)
	{
		if (op == "/" && b.Value == 0m)
		{
			return a;
		}
		decimal n = op switch
		{
			"*" => a.Value * b.Value, 
			"-" => a.Value - b.Value, 
			"+" => a.Value + b.Value, 
			_ => a.Value / b.Value, 
		};
		if (!a.Symbolic && !b.Symbolic)
		{
			return Constant(n);
		}
		if (op == "+" && b.Value == 0m && !b.Symbolic)
		{
			return a;
		}
		if (op == "+" && a.Value == 0m && !a.Symbolic)
		{
			return b;
		}
		if ((op == "*" || op == "/") && b.Value == 1m && !b.Symbolic)
		{
			return a;
		}
		int num = ((op == "+" || op == "-") ? 1 : 2);
		string text = ((a.Priority < num) ? ("(" + a.Text + ")") : a.Text);
		string text2 = ((b.Priority < num || ((op == "-" || op == "/") && b.Priority == num)) ? ("(" + b.Text + ")") : b.Text);
		Expression expression = Symbol(text + " " + op + " " + text2, n);
		expression.Priority = num;
		expression.Key = op + "(" + a.Key + "," + b.Key + ")";
		return expression;
	}

	private static Expression Sum(IEnumerable<Expression> expressions)
	{
		List<Expression> list = new List<Expression>();
		foreach (Expression expression2 in expressions)
		{
			foreach (Expression item in expression2.Addends ?? new List<Expression> { expression2 })
			{
				if (item.Symbolic || item.Value != 0m)
				{
					list.Add(item);
				}
			}
		}
		if (list.Count == 0)
		{
			return Constant(0m);
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		List<Expression> list2 = new List<Expression>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (Expression item2 in list)
		{
			string key = item2.Key + "|value:" + Number(item2.Value);
			if (dictionary.ContainsKey(key))
			{
				dictionary[key]++;
				continue;
			}
			dictionary.Add(key, 1);
			list2.Add(item2);
		}
		Expression expression = Constant(0m);
		foreach (Expression item3 in list2)
		{
			int num = dictionary[item3.Key + "|value:" + Number(item3.Value)];
			expression = Binary(expression, (num == 1) ? item3 : Binary(Constant(num), item3, "*"), "+");
		}
		expression.Addends = list;
		return expression;
	}

	private static Expression Truncate(Expression a)
	{
		if (!a.Symbolic)
		{
			return Constant(decimal.Truncate(a.Value));
		}
		return new Expression(a.Text, decimal.Truncate(a.Value), symbolic: true)
		{
			Priority = a.Priority,
			Key = "truncate(" + a.Key + ")"
		};
	}

	private static Expression Rounded(Expression a, string operation, int digits)
	{
		decimal num = ((operation == "Ceiling") ? decimal.Ceiling(a.Value) : decimal.Round(a.Value, digits));
		if (!a.Symbolic)
		{
			return Constant(num);
		}
		Expression expression = new Expression(a.Text, num, symbolic: true);
		expression.Priority = a.Priority;
		expression.Key = operation + ":" + digits + "(" + a.Key + ")";
		return expression;
	}

	internal static void VerifyRepeatedTermGrouping()
	{
		Expression expression = Binary(Symbol("ATK", 100m), Constant(10m), "+");
		Expression expression2 = Binary(Symbol("HP", 100m), Constant(10m), "+");
		Expression expression3 = Sum(new Expression[4] { expression, expression, expression, expression });
		if (expression3.Text != "4 * (ATK + 10)" || expression3.Value != 440m)
		{
			throw new Exception("Repeated hit grouping failed");
		}
		Expression expression4 = Sum(new Expression[4] { expression, expression2, expression, expression2 });
		if (expression4.Text != "2 * (ATK + 10) + 2 * (HP + 10)" || expression4.Value != 440m)
		{
			throw new Exception("Mixed hit grouping failed");
		}
		Expression expression5 = Sum(new Expression[2]
		{
			Sum(new Expression[2] { expression, expression }),
			expression
		});
		if (expression5.Text != "3 * (ATK + 10)" || expression5.Value != 330m)
		{
			throw new Exception("Nested grouping failed");
		}
		Expression expression6 = Truncate(Symbol("x", 1.25m));
		Expression expression7 = Symbol("x", 1m);
		if (Sum(new Expression[2] { expression6, expression7 }).Text != "x + x")
		{
			throw new Exception("Different rounding boundaries were grouped");
		}
		Expression expression8 = Rounded(Binary(Symbol("ATK", 100.45m), Constant(10m), "+"), "Round", 0);
		if (expression8.Text != "ATK + 10" || expression8.Value != 110m || Binary(expression8, Constant(2m), "*").Text != "(ATK + 10) * 2")
		{
			throw new Exception("Hidden rounding lost value or precedence");
		}
		Expression expression9 = Rounded(Symbol("x", 1.25m), "Round", 1);
		Expression expression10 = Rounded(Symbol("x", 1.25m), "Ceiling", 0);
		if (expression9.Text != "x" || expression9.Value != 1.2m || expression10.Text != "x" || expression10.Value != 2m)
		{
			throw new Exception("Hidden round/ceiling value mismatch");
		}
		if (Sum(new Expression[2]
		{
			Rounded(Symbol("x", 1m), "Round", 0),
			Rounded(Symbol("x", 1m), "Round", 1)
		}).Text != "x + x")
		{
			throw new Exception("Different rounding precision was grouped");
		}
		if (Sum(new Expression[2]
		{
			expression10,
			Symbol("x", 2m)
		}).Text != "x + x")
		{
			throw new Exception("Ceiling boundary was grouped with an unrounded value");
		}
		if (Sum(new Expression[2] { expression, expression2 }).Text.Contains("2 *"))
		{
			throw new Exception("Different symbols were grouped by their current values");
		}
	}

	private static int Int(JsonData j, string name)
	{
		return Convert.ToInt32(j[name].ToString(), CultureInfo.InvariantCulture);
	}

	private static string Str(JsonData j, string name)
	{
		return j[name].ToString();
	}

	private static string Words(string text)
	{
		if (WordProvider != null)
		{
			return WordProvider(text);
		}
		return SprkSkillFormulaWords.Get(text, SprkSkillFormulaWords.Locale);
	}

	private static Expression LevelFactor(int type, Context c)
	{
		SkillLevelFactorDataContainer skillLevelFactorDataContainer = GameRoot.Instance.SkillLevelFactorDataContainer;
		int levelFactor = skillLevelFactorDataContainer.GetLevelFactor(type, c.Level);
		if (type == 9999)
		{
			return Symbol(Words("Skill Level"), c.Level);
		}
		int levelFactor2 = skillLevelFactorDataContainer.GetLevelFactor(type, 1);
		int num = skillLevelFactorDataContainer.GetLevelFactor(type, 2) - levelFactor2;
		bool flag = true;
		for (int i = 1; i <= 100; i++)
		{
			if (skillLevelFactorDataContainer.GetLevelFactor(type, i) != levelFactor2 + (i - 1) * num)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return Binary(Constant((decimal)(levelFactor2 - num) / 1000m), Binary(Constant((decimal)num / 1000m), Symbol(Words("Skill Level"), c.Level), "*"), "+");
		}
		string text = Words("Level Scaling") + ((type == 5) ? "" : (" " + type));
		c.Result.Legends[text + "(" + c.Level + ")"] = Number((decimal)levelFactor / 1000m);
		return Symbol(text + "(" + Words("Skill Level") + ")", (decimal)levelFactor / 1000m);
	}

	private static Expression LevelValue(int[] factors, int at, Context c, bool awakening)
	{
		if (factors == null || at + 1 >= factors.Length || factors[at + 1] == 0)
		{
			return Constant(0m);
		}
		Expression expression = Truncate(Binary(Constant(factors[at + 1]), LevelFactor(factors[at], c), "*"));
		if (awakening)
		{
			HeroInfo heroInfo = (c.RuntimeValues ? c.Operation.FiredCreature.CreateInfo.HeroInfo : c.Hero);
			int creatureIndex = (c.RuntimeValues ? c.Operation.FiredCreature.CreatureIndex : heroInfo.HeroIndex);
			decimal n = (decimal)GameRoot.Instance.CreatureStatDataContainer.GetLevelStatFactor(creatureIndex, heroInfo.Star, heroInfo.Transcended) / 1000m;
			c.Result.Legends[Words("Awakening Coefficient")] = Number(n);
			expression = Truncate(Binary(expression, Symbol(Words("Awakening Coefficient"), n), "*"));
		}
		return expression;
	}

	private static Expression OperationValue(OperationData op, int index, Context c)
	{
		string[] array = (c.RuntimeValues ? c.Operation.GetValues() : op.Values);
		string text = ((array != null && index < array.Length) ? array[index] : "0");
		Expression a;
		if (c.RuntimeValues && text != null && text.StartsWith("$"))
		{
			a = OperationExpression(text, c);
		}
		else
		{
			int result = 0;
			int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
			a = Constant(result);
		}
		return Binary(a, LevelValue(op.ValueFactors, index * 2, c, awakening: true), "+");
	}

	private static Expression OperationExpression(string raw, Context c)
	{
		OperationValueExpression operationValueExpression = OperationValueExpression.Create(raw);
		if (operationValueExpression == null)
		{
			throw new NotSupportedException("Missing operation expression");
		}
		string[] array = OperationValueExpression.GetMatches(raw)[0].Groups["ARGUMENT"].Value.Split(',', ';');
		Stack<Expression> stack = new Stack<Expression>();
		foreach (TokenInfo rpnToken in operationValueExpression.RpnTokens)
		{
			if (rpnToken is OperatorInfo)
			{
				Expression expression = stack.Pop();
				Expression a = ((stack.Count > 0) ? stack.Pop() : Constant(0m));
				string token = rpnToken.Token;
				if (token == "/" && expression.Value == 0m)
				{
					stack.Push(Constant(0m));
					continue;
				}
				if (new string[4] { "+", "-", "*", "/" }.Contains(token))
				{
					stack.Push((token == "/") ? Truncate(Binary(a, expression, token)) : Binary(a, expression, token));
					continue;
				}
				throw new NotSupportedException("Operation expression operator " + token);
			}
			if (rpnToken is ConstantInfo constantInfo)
			{
				stack.Push(Constant(constantInfo.Value));
				continue;
			}
			string text = rpnToken.Token;
			if (text.StartsWith("$") && int.TryParse(text.Substring(1), out var result) && result > 0 && result <= array.Length)
			{
				text = array[result - 1].Trim();
			}
			if (long.TryParse(text, out var result2))
			{
				stack.Push(Constant(result2));
				continue;
			}
			decimal n = operationValueExpression.VariableFunc(c.Operation, rpnToken.Token);
			if (text == "$SkillLevel")
			{
				stack.Push(Symbol(Words("Skill Level"), n));
				continue;
			}
			if (text == "$Fc.DistanceMm")
			{
				c.Result.Legends[Words("Distance")] = Words("caster-to-target distance in game units (preview: 0)");
				stack.Push(Symbol(Words("Distance"), n));
				continue;
			}
			string text2 = text.Split('(')[0];
			StatType readingStatType = OperationValueReaderFactory.GetReadingStatType(text2);
			if (readingStatType != StatType.None)
			{
				bool num = text2.StartsWith("$Fc.", StringComparison.OrdinalIgnoreCase);
				Expression expression2 = Symbol(string.Concat(str1: readingStatType switch
				{
					StatType.MagicalDefense => "M.DEF", 
					StatType.PhysicalDefense => "P.DEF", 
					StatType.MaxHp => Words("Max HP"), 
					StatType.MagicalAttack => "M.ATK", 
					StatType.PhysicalAttack => "P.ATK", 
					_ => readingStatType.ToString(), 
				}, str0: num ? "" : (Words("Target") + " ")), n);
				expression2.Key = "reader:" + text + ":" + Number(n);
				stack.Push(expression2);
				continue;
			}
			throw new NotSupportedException("Operation expression reader " + text);
		}
		if (stack.Count != 1)
		{
			throw new NotSupportedException("Operation expression shape");
		}
		return stack.Pop();
	}

	private static Expression Timing(OperationData op, bool tick, Context c)
	{
		return Binary(Constant(tick ? op.TickTimeMs : op.DurationMs), LevelValue(tick ? op.TickTimeFactors : op.DurationFactors, 0, c, awakening: false), "+");
	}

	private static Expression Damage(OperationData op, SkillData skill, Context c, bool dot)
	{
		if (formulaLinker == null)
		{
			formulaLinker = new CalcDamage.FuncLinker((IOperationRuntime value) => value is GetDamage || value is GetHeal || value is GetDot || value is GetHot);
		}
		CalcDamage.FuncLinker funcLinker = formulaLinker;
		funcLinker.InitCreature(GameRoot.Instance.CreatureDataContainer.GetData(c.Hero.HeroIndex), c.Hero.Star, c.Stat);
		BaseOperationRuntime baseOperationRuntime = (BaseOperationRuntime)funcLinker.InitRuntime(skill, c.Level, op);
		if (baseOperationRuntime == null)
		{
			return Constant(0m);
		}
		c.Operation = baseOperationRuntime.OperationInfo;
		c.RuntimeValues = true;
		c.FormulaStats = c.Operation.FiredCreature.StatController;
		string text = op.Type.ToString();
		bool flag = text == "GetHealR" || text == "GetHotR";
		Expression expression6;
		if (flag || new string[7] { "GetDamageR", "GetStateDamageR", "GetPhysicalDamageR", "GetMagicalDamageR", "GetDotR", "GetPhysicalDotR", "GetMagicalDotR" }.Contains(text))
		{
			StatType statType = ((flag || text.Contains("Magical")) ? StatType.MagicalAttack : (text.Contains("Physical") ? StatType.PhysicalAttack : ((skill.AttrType == SkillAttrType.Physical) ? StatType.PhysicalAttack : StatType.MagicalAttack)));
			Expression expression = Symbol((statType == StatType.PhysicalAttack) ? "P.ATK" : "M.ATK", (c.FormulaStats == null) ? c.Stat.GetStat(statType) : c.FormulaStats.GetFinalStat(statType));
			Expression expression2 = OperationValue(op, 0, c);
			if (text == "GetStateDamageR")
			{
				c.Result.Legends[Words("Stacks")] = Words("applicable effect stacks on the target (preview: 0)");
				Expression expression3 = Symbol(Words("Stacks"), 0m);
				expression3.Key = "stacks:" + c.Operation.GetValueString(3) + ":" + c.Operation.GetValueBool(5);
				expression2 = Binary(expression2, Binary(OperationValue(op, 4, c), expression3, "*"), "+");
			}
			Expression expression4 = Binary(expression2, Constant(10m), "/");
			Expression expression5 = Symbol((expression4.Symbolic ? ("(" + expression4.Text + ")") : expression4.Text) + "% " + Words("of") + " " + expression.Text, System.Math.Max(decimal.Truncate(expression.Value * expression2.Value / 1000m), 0m));
			expression5.Priority = 2;
			expression5.Key = "attack(" + expression4.Key + "," + expression.Key + ")";
			expression6 = Binary(OperationValue(op, 1, c), expression5, "+");
		}
		else
		{
			if (!new string[6] { "GetDamage", "GetPhysicalDamage", "GetMagicalDamage", "GetHeal", "GetDot", "GetHot" }.Contains(text))
			{
				throw new NotSupportedException("Operation " + text);
			}
			expression6 = OperationValue(op, 0, c);
		}
		if (dot)
		{
			Expression b = Truncate(Binary(Timing(op, tick: false, c), Timing(op, tick: true, c), "/"));
			expression6 = Binary(expression6, b, "*");
		}
		if (!dot && baseOperationRuntime is GetDamage)
		{
			((GetDamage)baseOperationRuntime).GetInstantDamageInfo(out var amount, out var _);
			if (expression6.Value != (decimal)amount)
			{
				throw new InvalidOperationException("Operation parity " + skill.Index + "/" + op.OperationNo + " " + text + " values=" + string.Join(",", op.Values) + " runtime=" + string.Join(",", c.Operation.GetValues()) + " native1=" + c.Operation.GetValueInt(1) + " symbolic1=" + OperationValue(op, 1, c).Value + " expected=" + amount + " got=" + expression6.Value);
			}
		}
		return expression6;
	}

	private static Expression Numeric(ITypeValueBase node, Context c)
	{
		decimal num = Convert.ToDecimal(node.GetObjectValue(c.Variable), CultureInfo.InvariantCulture);
		try
		{
			JsonData jsonData = node.MakeBodyJson();
			string valuePath = node.ValuePath;
			Expression expression = null;
			c.RuntimeValues = false;
			switch (valuePath)
			{
			case "Data/OperationData/CalcDamage":
			case "Data/OperationData/CalcDot":
			{
				SkillData data = GameRoot.Instance.SkillDataContainer.GetData(Int(jsonData, "SkillIndex"));
				List<Expression> list2 = new List<Expression>();
				JsonData jsonData2 = jsonData["OperationNoList"];
				for (int k = 0; k < jsonData2.Count; k++)
				{
					OperationData operationData2 = data?.GetOperationDataUnsafe(Convert.ToInt32(jsonData2.GetFromArray(k).ToString()));
					if (operationData2 != null && operationData2.Type != OperationType.None)
					{
						list2.Add(Damage(operationData2, data, c, valuePath.EndsWith("CalcDot")));
					}
				}
				expression = Sum(list2);
				break;
			}
			case "Data/ValueFromOperationData":
			{
				OperationData operationData = GameRoot.Instance.SkillDataContainer.GetData(Int(jsonData, "SkillIndex"))?.GetOperationDataUnsafe(Int(jsonData, "OperationNo"));
				string text3 = Str(jsonData, "ValueType");
				expression = ((operationData == null) ? Constant(0m) : (text3 switch
				{
					"TickTime" => Timing(operationData, tick: true, c), 
					"Duration" => Timing(operationData, tick: false, c), 
					"Value" => OperationValue(operationData, Int(jsonData, "ValueIndex"), c), 
					_ => Constant(num), 
				}));
				break;
			}
			default:
				if (valuePath.StartsWith("Decimal/Calculate/"))
				{
					ArgumentContainer argumentContainer = node.GetArgumentContainer();
					string text = valuePath.Substring(valuePath.LastIndexOf('/') + 1);
					Expression expression2 = Numeric(argumentContainer.GetTypeValueBase(0), c);
					if (text == "Add")
					{
						List<Expression> list = new List<Expression> { expression2 };
						for (int i = 1; i < argumentContainer.HolderList.Count; i++)
						{
							list.Add(Numeric(argumentContainer.GetTypeValueBase(i), c));
						}
						expression = Sum(list);
					}
					else if (new string[3] { "Sub", "Multiply", "Divide" }.Contains(text))
					{
						expression = expression2;
						for (int j = 1; j < argumentContainer.HolderList.Count; j++)
						{
							expression = Binary(expression, Numeric(argumentContainer.GetTypeValueBase(j), c), text switch
							{
								"Multiply" => "*", 
								"Sub" => "-", 
								"Add" => "+", 
								_ => "/", 
							});
						}
					}
					else if (text == "Round" || text == "Ceiling")
					{
						int digits = ((argumentContainer.HolderList.Count > 1) ? ((int)Numeric(argumentContainer.GetTypeValueBase(1), c).Value) : 0);
						expression = Rounded(expression2, text, digits);
					}
				}
				else if (valuePath == "Data/ValueFromStatData")
				{
					string text2 = Str(jsonData, "StatType");
					expression = Symbol(text2 switch
					{
						"MaxHp" => Words("Max HP"), 
						"MagicalAttack" => "M.ATK", 
						"PhysicalAttack" => "P.ATK", 
						_ => text2, 
					}, num);
				}
				else if (valuePath == "Decimal/Convert/ToFloat")
				{
					expression = Numeric(node.GetArgumentContainer().GetTypeValueBase(0), c);
				}
				else if (valuePath.EndsWith("/Constant") || valuePath == "Data/ValueFromSkillData" || valuePath == "Data/SkillData/OperationFireCount")
				{
					expression = Constant(num);
				}
				break;
			}
			if (expression == null)
			{
				throw new NotSupportedException(valuePath);
			}
			if (expression.Value != num)
			{
				throw new InvalidOperationException("Native parity: " + valuePath + " expected " + num + " got " + expression.Value);
			}
			if (expression.Symbolic)
			{
				c.Result.Formulas++;
			}
			return expression;
		}
		catch (Exception ex)
		{
			c.Result.Unsupported.Add(node.ValuePath + ": " + ex.Message);
			return Constant(num);
		}
	}

	private static string Text(ITypeValueBase node, Context c)
	{
		if (node.ValuePath != "String/Localize")
		{
			return Convert.ToString(node.GetObjectValue(c.Variable), CultureInfo.CurrentCulture);
		}
		string text = Str(node.MakeBodyJson()["LocalizeLink"], "Key");
		ArgumentContainer argumentContainer = node.GetArgumentContainer();
		List<object> list = new List<object>();
		if (argumentContainer != null && argumentContainer.HolderList != null)
		{
			foreach (ArgumentHolder holder in argumentContainer.HolderList)
			{
				ITypeValueBase value = holder.Value;
				list.Add((value.GetValueType() == typeof(string)) ? ((object)Text(value, c)) : ((object)Numeric(value, c)));
			}
		}
		if (c.Localize == null)
		{
			return LocalizationManager.GetStringDirect(text, list.ToArray());
		}
		string text2 = c.Localize(text);
		for (int i = 0; i < list.Count; i++)
		{
			text2 = text2.Replace("{" + (i + 1) + "}", Convert.ToString(list[i], CultureInfo.CurrentCulture));
		}
		for (int j = 0; j < 10; j++)
		{
			if (!text2.Contains("{@"))
			{
				break;
			}
			text2 = Regex.Replace(text2, "\\{@([^}]+)\\}", (Match m) => c.Localize(m.Groups[1].Value));
		}
		return text2;
	}

	public static Result Render(int index, HeroInfo hero, Func<string, string> localize)
	{
		SkillDescContainer instance = SingletonAuto<SkillDescContainer>.instance;
		SkillDesc skillDesc = instance.SkillDescDic.GETVALUE(new SkillDescContainer.DescKey(index, hero.CostumeIndex), null) ?? instance.SkillDescDic.GETVALUE(new SkillDescContainer.DescKey(index, 0), null);
		if (skillDesc == null)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(index);
			if (data == null)
			{
				throw new NotSupportedException("Missing skill");
			}
			if (localize == null)
			{
				return new Result
				{
					Text = data.Description
				};
			}
			string text = localize("SKILL_DESC_" + index);
			if (data.DescParam != null)
			{
				for (int i = 0; i < data.DescParam.Length; i++)
				{
					text = text.Replace("{" + (i + 1) + "}", data.DescParam[i]);
				}
			}
			for (int j = 0; j < 10; j++)
			{
				if (!text.Contains("{@"))
				{
					break;
				}
				text = Regex.Replace(text, "\\{@([^}]+)\\}", (Match m) => localize(m.Groups[1].Value));
			}
			return new Result
			{
				Text = text
			};
		}
		SkillDescVariable skillDescVariable = SmallPooling<SkillDescVariable>.CreateInstance();
		try
		{
			CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(hero.HeroIndex);
			int skillLevelBySkillIndex = hero.GetSkillLevelBySkillIndex(data2, index);
			StatData statData = ((hero is CachedHeroInfo) ? SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat((CachedHeroInfo)hero) : SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(hero));
			skillDescVariable.Open(statData, hero.HeroIndex, hero.Star, hero.Transcended, index, skillLevelBySkillIndex);
			Context context = new Context
			{
				Hero = hero,
				Stat = statData,
				Level = skillLevelBySkillIndex + StatCalculator.GetEquipOptionAddedSkillLevel(statData, data2.GetSkillSlotByIndex(index, includeLinkedSkill: true)),
				Variable = skillDescVariable,
				Localize = localize
			};
			ArgumentSingleGeneric<string> argumentSingleGeneric = (ArgumentSingleGeneric<string>)typeof(SkillDesc).GetField("_skillDesc", Private).GetValue(skillDesc);
			context.Result.Text = Text(argumentSingleGeneric.GetTypeValue(), context);
			if (context.Result.Unsupported.Count > 0)
			{
				Result result = context.Result;
				result.Text = result.Text + "\n\n" + Words("Some values remain calculated.");
			}
			return context.Result;
		}
		finally
		{
			SmallPooling<SkillDescVariable>.ReleaseInstance(skillDescVariable);
		}
	}
}
