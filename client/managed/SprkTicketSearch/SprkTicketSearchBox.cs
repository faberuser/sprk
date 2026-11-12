using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class SprkTicketSearchBox : MonoBehaviour
{
	public const string NodeName = "SprkTicketSearch";

	public UIInput input;

	public UILabel label;

	public GameObject clear;

	public GameObject content;

	public Action rebuild;

	public Func<bool> visible;

	public string[] terms = new string[0];

	public string text = "";

	public bool fuzzy;

	public Func<IEnumerable<string>> pool;

	public UISprite styleSource;

	private float pending = -1f;

	private bool silent;

	public readonly Dictionary<Transform, SprkTicketSearch.Geometry> original = new Dictionary<Transform, SprkTicketSearch.Geometry>();

	public bool Active => terms.Length != 0;

	public bool Accepts(string haystack)
	{
		if (terms.Length != 0)
		{
			if (!fuzzy)
			{
				return SprkTicketSearch.Matches(terms, haystack);
			}
			return SprkTicketSearch.FuzzyMatches(terms, haystack);
		}
		return true;
	}

	public void OnChanged()
	{
		if (!silent && !(input == null))
		{
			string text = input.value ?? "";
			if (!(text == this.text))
			{
				this.text = text;
				terms = SprkTicketSearch.Terms(text);
				RefreshClear();
				pending = Time.unscaledTime + 0.18f;
			}
		}
	}

	public void OnSubmitted()
	{
		if (!silent)
		{
			OnChanged();
			if (pending >= 0f)
			{
				pending = -1f;
				Apply();
			}
		}
	}

	public void Clear()
	{
		ResetSilently();
		Apply();
	}

	public void ResetSilently()
	{
		pending = -1f;
		text = "";
		terms = new string[0];
		fuzzy = false;
		if (input != null)
		{
			silent = true;
			try
			{
				input.value = "";
			}
			finally
			{
				silent = false;
			}
		}
		RefreshClear();
	}

	private void RefreshClear()
	{
		if (clear != null)
		{
			clear.SetActive(text.Length > 0);
		}
	}

	public void Apply()
	{
		pending = -1f;
		fuzzy = false;
		if (terms.Length != 0 && pool != null)
		{
			try
			{
				fuzzy = !pool().Any((string text) => SprkTicketSearch.Matches(terms, text));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (rebuild == null)
		{
			return;
		}
		try
		{
			rebuild();
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	private void MatchStyle()
	{
		if (styleSource == null || styleSource.atlas == null || input == null)
		{
			return;
		}
		UISprite component = input.GetComponent<UISprite>();
		if (!(component == null) && (!(component.atlas == styleSource.atlas) || !(component.spriteName == styleSource.spriteName) || component.height != styleSource.height))
		{
			component.atlas = styleSource.atlas;
			component.spriteName = styleSource.spriteName;
			component.type = styleSource.type;
			component.height = styleSource.height;
			BoxCollider component2 = input.GetComponent<BoxCollider>();
			if (component2 != null)
			{
				component2.size = new Vector3(component2.size.x, styleSource.height, 1f);
			}
		}
	}

	private void Update()
	{
		MatchStyle();
		if (content != null && visible != null)
		{
			bool flag = visible();
			if (content.activeSelf != flag)
			{
				content.SetActive(flag);
			}
		}
		if (pending >= 0f && Time.unscaledTime >= pending)
		{
			Apply();
		}
	}
}
