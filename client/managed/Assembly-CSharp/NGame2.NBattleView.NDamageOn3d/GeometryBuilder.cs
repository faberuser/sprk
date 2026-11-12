using System.Collections.Generic;
using NGameAsset;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NDamageOn3d;

public class GeometryBuilder : SceneAutoBehaviour<GeometryBuilder>
{
	public enum ECachedSprite
	{
		Char_Plus,
		Icon_Block,
		Text_Block,
		Text_Critical,
		Text_Miss,
		Text_Immune_Physical,
		Text_Immune_Magical,
		Text_Resist,
		__count__
	}

	public class GeometryData
	{
		public Vector3 Position;

		public float Scale = 1f;

		public Vector2 LayoutOffset;

		public float CreateTime;

		public float ExtraValue;

		public float Width;

		public float Height;

		public RenderingList<Vector2> layouts = new RenderingList<Vector2>();

		public RenderingList<Vector2> uvs = new RenderingList<Vector2>();

		public RenderingList<Color32> cols = new RenderingList<Color32>();

		public float LayoutWidth => Width * Scale * RenderInstance.DisplayScale;

		public float LayoutHeight => Height * Scale * RenderInstance.DisplayScale;

		public void Reset()
		{
			Position = Vector3.zero;
			LayoutOffset = Vector2.zero;
			Scale = 1f;
			CreateTime = 0f;
			ExtraValue = 0f;
			Width = 0f;
			Height = 0f;
			layouts.Clear();
			uvs.Clear();
			cols.Clear();
		}
	}

	private const long KILLO = 1000L;

	private const long MILLION = 1000000L;

	private const long BILLION = 1000000000L;

	private const long TRILLION = 1000000000000L;

	private const long KILLO_HIGHER = 10000L;

	private const long MILLION_HIGHER = 10000000L;

	private const long BILLION_HIGHER = 10000000000L;

	private const long TRILLION_HIGHER = 10000000000000L;

	private const long KILLO_LOWER = 100L;

	private const long MILLION_LOWER = 100000L;

	private const long BILLION_LOWER = 100000000L;

	private const long TRILLION_LOWER = 100000000000L;

	private const int KILLO_PLACE_VALUE = 3;

	private const int MILLION_PLACE_VALUE = 6;

	private const int BILLION_PLACE_VALUE = 9;

	private const int TRILLION_PLACE_VALUE = 12;

	private bool _isReady;

	private INguiAtlas _atlasAsyncLoad;

	private UISpriteData[] _numberSprites = new UISpriteData[10];

	private UISpriteData _dotSprite;

	private UISpriteData[] _unitSprites = new UISpriteData[4];

	private Dictionary<string, UISpriteData> _spriteDict = new Dictionary<string, UISpriteData>(15);

	private readonly string[] _damageUnitKeywords = new string[4] { "Killo", "Million", "Billion", "Trillion" };

	private UISpriteData[] _cachedSprites = new UISpriteData[8];

	private Queue<GeometryData> _unusedQueue = new Queue<GeometryData>();

	private int _generateCount;

	private UISpriteData[] _generateSprites = new UISpriteData[100];

	public UIAtlas NguiAtlas
	{
		get
		{
			if (!_atlasAsyncLoad.ISVALID())
			{
				return null;
			}
			return _atlasAsyncLoad.Value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (SystemOptionData.ShowFontNew)
		{
			_atlasAsyncLoad = SingletonAuto<NguiAtlasContainer>.instance.Lock("Battle.Damage2");
		}
		else
		{
			_atlasAsyncLoad = SingletonAuto<NguiAtlasContainer>.instance.Lock("Battle.Damage");
		}
		AsyncManager.SingleEntry singleEntry = new AsyncManager.SingleEntry();
		singleEntry.AsyncLoad = _atlasAsyncLoad;
		singleEntry.OnComplete = OnComplete;
		SingletonUpdater<AsyncManager>.instance.Add(singleEntry);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_atlasAsyncLoad != null)
		{
			NguiAtlasContainer.ReleaseAtlas(_atlasAsyncLoad, destroyImmediately: false);
			_atlasAsyncLoad = null;
		}
		SetSpritesNull(_numberSprites);
		SetSpritesNull(_generateSprites);
		_isReady = false;
	}

	private static void SetSpritesNull(UISpriteData[] sprites)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(sprites); i++)
		{
			sprites[i] = null;
		}
	}

	public static void ReleaseGeometryList(List<GeometryData> geometryList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(geometryList); i++)
		{
			ReleaseGeometry(geometryList[i]);
		}
		geometryList.Clear();
	}

	public static void ReleaseGeometry(GeometryData geometry)
	{
		geometry.Reset();
		if (SceneAutoBehaviour<GeometryBuilder>.isValidInstance)
		{
			SceneAutoBehaviour<GeometryBuilder>.instance._unusedQueue.Enqueue(geometry);
		}
	}

	public GeometryData CreateImage(ECachedSprite cachedSprite, Color gradianTop, Color gradianBottom, float scale)
	{
		if (!_isReady)
		{
			return null;
		}
		UISpriteData uISpriteData = _cachedSprites[(int)cachedSprite];
		if (uISpriteData == null)
		{
			return null;
		}
		_generateCount = 1;
		_generateSprites[0] = uISpriteData;
		return GenerateGeometry(_generateCount, _generateSprites, gradianTop, gradianBottom, 0f, scale);
	}

	public GeometryData CreateImage(string image, Color gradianTop, Color gradianBottom, float scale)
	{
		if (!_isReady)
		{
			return null;
		}
		if (String_Extension_Method.IsNullOrEmpty(image))
		{
			return null;
		}
		UISpriteData sprite = _atlasAsyncLoad.Value.GetSprite(image);
		if (sprite == null)
		{
			return null;
		}
		_generateCount = 1;
		_generateSprites[0] = sprite;
		return GenerateGeometry(_generateCount, _generateSprites, gradianTop, gradianBottom, 0f, scale);
	}

	public GeometryData CreateNumber(long number, Color gradianTop, Color gradianBottom, float scale)
	{
		if (!_isReady)
		{
			return null;
		}
		if (number == 0L)
		{
			_generateCount = 1;
			_generateSprites[0] = _numberSprites[0];
		}
		else
		{
			UISpriteData uISpriteData = null;
			if (SystemOptionData.ShowDamageUnit)
			{
				if (number >= 10000000000000L)
				{
					number /= 100000000000L;
					uISpriteData = _unitSprites[3];
				}
				else if (number >= 10000000000L)
				{
					number /= 100000000;
					uISpriteData = _unitSprites[2];
				}
				else if (number >= 10000000)
				{
					number /= 100000;
					uISpriteData = _unitSprites[1];
				}
				else if (number >= 10000)
				{
					number /= 100;
					uISpriteData = _unitSprites[0];
				}
			}
			_generateCount = 0;
			for (int i = 0; i < _generateSprites.Length; i++)
			{
				if (number <= 0)
				{
					break;
				}
				long num = number % 10;
				_generateSprites[_generateCount] = _numberSprites[num];
				_generateCount++;
				number /= 10;
			}
			for (int j = 0; j < _generateCount / 2; j++)
			{
				UISpriteData uISpriteData2 = _generateSprites[j];
				_generateSprites[j] = _generateSprites[_generateCount - j - 1];
				_generateSprites[_generateCount - j - 1] = uISpriteData2;
			}
			if (uISpriteData != null)
			{
				if (_generateCount > 3)
				{
					_generateCount--;
					_generateSprites[_generateCount] = null;
				}
				else
				{
					_generateCount--;
					UISpriteData uISpriteData3 = _generateSprites[_generateCount];
					_generateSprites[_generateCount] = _dotSprite;
					_generateCount++;
					_generateSprites[_generateCount] = uISpriteData3;
					_generateCount++;
				}
				_generateSprites[_generateCount] = uISpriteData;
				_generateCount++;
			}
		}
		return GenerateGeometry(_generateCount, _generateSprites, gradianTop, gradianBottom, 0f, scale);
	}

	private GeometryData GenerateGeometry(int count, UISpriteData[] sprites, Color gradianTop, Color gradianBottom, float spacingX, float scale)
	{
		Texture texture = NguiAtlas.texture;
		GeometryData geometryData = CreateGeometry();
		geometryData.Width = 0f;
		geometryData.Height = 0f;
		for (int i = 0; i < count; i++)
		{
			UISpriteData uISpriteData = sprites[i];
			int num = uISpriteData.Width + uISpriteData.PaddingLeft + uISpriteData.PaddingRight;
			int num2 = uISpriteData.Height + uISpriteData.PaddingTop + uISpriteData.PaddingBottom;
			geometryData.Width += num;
			if (geometryData.Height < (float)num2)
			{
				geometryData.Height = num2;
			}
		}
		geometryData.Width += spacingX * (float)(count - 1);
		int num3 = geometryData.layouts.Count;
		int num4 = geometryData.uvs.Count;
		int num5 = geometryData.cols.Count;
		geometryData.layouts.AddEmpty(count * 4);
		geometryData.uvs.AddEmpty(count * 4);
		geometryData.cols.AddEmpty(count * 4);
		Vector2[] buffer = geometryData.layouts.buffer;
		Vector2[] buffer2 = geometryData.uvs.buffer;
		Color32[] buffer3 = geometryData.cols.buffer;
		float num6 = (0f - geometryData.Width) * 0.5f;
		for (int j = 0; j < count; j++)
		{
			UISpriteData uISpriteData2 = sprites[j];
			float num7 = num6 + (float)uISpriteData2.PaddingLeft;
			float x = num7 + (float)uISpriteData2.Width;
			float num8 = (float)(uISpriteData2.Height + uISpriteData2.PaddingTop + uISpriteData2.PaddingBottom) * 0.5f - (float)uISpriteData2.PaddingTop;
			float y = num8 - (float)uISpriteData2.Height;
			buffer[num3] = new Vector3(num7, y);
			buffer[num3 + 1] = new Vector3(num7, num8);
			buffer[num3 + 2] = new Vector3(x, num8);
			buffer[num3 + 3] = new Vector3(x, y);
			Rect rect = new Rect(uISpriteData2.X, uISpriteData2.Y, uISpriteData2.Width, uISpriteData2.Height);
			rect = NGUIMath.ConvertToTexCoords(rect, texture.width, texture.height);
			buffer2[num4] = new Vector2(rect.xMin, rect.yMin);
			buffer2[num4 + 1] = new Vector2(rect.xMin, rect.yMax);
			buffer2[num4 + 2] = new Vector2(rect.xMax, rect.yMax);
			buffer2[num4 + 3] = new Vector2(rect.xMax, rect.yMin);
			buffer3[num5] = gradianBottom;
			buffer3[num5 + 1] = gradianTop;
			buffer3[num5 + 2] = gradianTop;
			buffer3[num5 + 3] = gradianBottom;
			num6 += (float)(uISpriteData2.Width + uISpriteData2.PaddingLeft + uISpriteData2.PaddingRight) + spacingX;
			num3 += 4;
			num4 += 4;
			num5 += 4;
		}
		geometryData.CreateTime = GlobalTimer.time;
		geometryData.Scale = scale;
		return geometryData;
	}

	private void OnComplete()
	{
		if (!_atlasAsyncLoad.ISVALID())
		{
			return;
		}
		UIAtlas value = _atlasAsyncLoad.Value;
		if (!(value == null) && !(value.texture == null))
		{
			for (int i = 0; i < 10; i++)
			{
				string key = i.ToString();
				_numberSprites[i] = value.GetSprite(key);
				_spriteDict.Add(key, _numberSprites[i]);
			}
			for (int j = 0; j < _damageUnitKeywords.Length; j++)
			{
				_unitSprites[j] = value.GetSprite(_damageUnitKeywords[j]);
				_spriteDict.Add(_damageUnitKeywords[j], _unitSprites[j]);
			}
			_dotSprite = value.GetSprite("Dot");
			_spriteDict.Add("Dot", _dotSprite);
			for (int k = 0; k < 8; k++)
			{
				ECachedSprite eCachedSprite = (ECachedSprite)k;
				_cachedSprites[k] = value.GetSprite(eCachedSprite.ToString());
			}
			_isReady = true;
		}
	}

	private GeometryData CreateGeometry()
	{
		if (_unusedQueue.Count > 0)
		{
			return _unusedQueue.Dequeue();
		}
		return new GeometryData();
	}
}
