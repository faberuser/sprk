using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NUtil;

public class TexturePacker
{
	public class PackData
	{
		public Texture2D TargetTex;

		public Texture2D[] SrcTextures;

		public int Size;

		public int Padding;

		public int MaxSize;
	}

	public int binSize;

	public List<Rect> usedRectangles = new List<Rect>();

	public List<Rect> freeRectangles = new List<Rect>();

	public static Rect[] PackTextures(PackData pack)
	{
		if (pack.Size > pack.MaxSize)
		{
			return null;
		}
		TexturePacker texturePacker = new TexturePacker(pack.Size);
		Rect[] array = new Rect[pack.SrcTextures.Length];
		for (int i = 0; i < pack.SrcTextures.Length; i++)
		{
			Texture2D texture2D = pack.SrcTextures[i];
			if ((bool)texture2D)
			{
				Rect rect = default(Rect);
				rect = texturePacker.Insert(texture2D.width + pack.Padding, texture2D.height + pack.Padding);
				if (rect.width == 0f || rect.height == 0f)
				{
					pack.Size *= 2;
					return PackTextures(pack);
				}
				array[i] = rect;
			}
		}
		pack.TargetTex.Reinitialize(pack.Size, pack.Size);
		pack.TargetTex.SetPixels(new Color[pack.Size * pack.Size]);
		Rect[] array2 = new Rect[pack.SrcTextures.Length];
		int b = 0;
		int b2 = 0;
		for (int j = 0; j < pack.SrcTextures.Length; j++)
		{
			Texture2D texture2D2 = pack.SrcTextures[j];
			if (texture2D2 == null)
			{
				continue;
			}
			Color[] pixels = texture2D2.GetPixels();
			Rect rect2 = array[j];
			for (int k = 0; k < (int)rect2.height; k++)
			{
				b = Mathf.Max(k, b);
				for (int l = 0; l < (int)rect2.width; l++)
				{
					b2 = Mathf.Max(l, b2);
					int num = k - pack.Padding / 2;
					int num2 = l - pack.Padding / 2;
					bool flag = false;
					int num5;
					int num6;
					if (rect2.width != (float)(texture2D2.width + pack.Padding))
					{
						int num3 = texture2D2.height - (num + 1);
						int num4 = num2;
						if (num3 >= 0 && num3 < texture2D2.height)
						{
							num5 = num3;
						}
						else
						{
							flag = true;
							num5 = Math.ClampOver(num3, 0, texture2D2.height);
						}
						if (num4 >= 0 && num4 < texture2D2.width)
						{
							num6 = num4;
						}
						else
						{
							flag = true;
							num6 = Math.ClampOver(num4, 0, texture2D2.width);
						}
					}
					else
					{
						if (num2 >= 0 && num2 < texture2D2.width)
						{
							num5 = num2;
						}
						else
						{
							flag = true;
							num5 = Math.ClampOver(num2, 0, texture2D2.width);
						}
						if (num >= 0 && num < texture2D2.height)
						{
							num6 = num;
						}
						else
						{
							flag = true;
							num6 = Math.ClampOver(num, 0, texture2D2.height);
						}
					}
					Color color = pixels[num6 * texture2D2.width + num5];
					int x = Math.ClampOver(num2 + (int)rect2.x, 0, pack.TargetTex.width);
					int y = Math.ClampOver(num + (int)rect2.y, 0, pack.TargetTex.height);
					pack.TargetTex.SetPixel(x, y, color);
				}
			}
			rect2.x /= pack.Size;
			rect2.y /= pack.Size;
			rect2.width = (rect2.width - (float)pack.Padding) / (float)pack.Size;
			rect2.height = (rect2.height - (float)pack.Padding) / (float)pack.Size;
			array2[j] = rect2;
		}
		pack.TargetTex.Apply();
		return array2;
	}

	public TexturePacker(int size)
	{
		Init(size);
	}

	public void Init(int size)
	{
		binSize = size;
		Rect item = new Rect
		{
			x = 0f,
			y = 0f,
			width = size,
			height = size
		};
		usedRectangles.Clear();
		freeRectangles.Clear();
		freeRectangles.Add(item);
	}

	public Rect Insert(int width, int height)
	{
		Rect usedNode = FindPositionForNewNodeBestAreaFit(width, height);
		if (usedNode.height == 0f)
		{
			return usedNode;
		}
		int num = freeRectangles.Count;
		for (int i = 0; i < num; i++)
		{
			if (SplitFreeNode(freeRectangles[i], ref usedNode))
			{
				freeRectangles.RemoveAt(i);
				i--;
				num--;
			}
		}
		PruneFreeList();
		usedRectangles.Add(usedNode);
		return usedNode;
	}

	private Rect FindPositionForNewNodeBestAreaFit(int width, int height)
	{
		Rect result = default(Rect);
		int num = int.MaxValue;
		int num2 = 0;
		for (int i = 0; i < freeRectangles.Count; i++)
		{
			int num3 = (int)freeRectangles[i].width * (int)freeRectangles[i].height - width * height;
			if (freeRectangles[i].width >= (float)width && freeRectangles[i].height >= (float)height)
			{
				int a = Mathf.Abs((int)freeRectangles[i].width - width);
				int b = Mathf.Abs((int)freeRectangles[i].height - height);
				int num4 = Mathf.Min(a, b);
				if (num3 < num || (num3 == num && num4 < num2))
				{
					result.x = freeRectangles[i].x;
					result.y = freeRectangles[i].y;
					result.width = width;
					result.height = height;
					num2 = num4;
					num = num3;
				}
			}
		}
		return result;
	}

	private bool SplitFreeNode(Rect freeNode, ref Rect usedNode)
	{
		if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x || usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y)
		{
			return false;
		}
		if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x)
		{
			if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height)
			{
				Rect item = freeNode;
				item.height = usedNode.y - item.y;
				freeRectangles.Add(item);
			}
			if (usedNode.y + usedNode.height < freeNode.y + freeNode.height)
			{
				Rect item2 = freeNode;
				item2.y = usedNode.y + usedNode.height;
				item2.height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height);
				freeRectangles.Add(item2);
			}
		}
		if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y)
		{
			if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width)
			{
				Rect item3 = freeNode;
				item3.width = usedNode.x - item3.x;
				freeRectangles.Add(item3);
			}
			if (usedNode.x + usedNode.width < freeNode.x + freeNode.width)
			{
				Rect item4 = freeNode;
				item4.x = usedNode.x + usedNode.width;
				item4.width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width);
				freeRectangles.Add(item4);
			}
		}
		return true;
	}

	private void PruneFreeList()
	{
		for (int i = 0; i < freeRectangles.Count; i++)
		{
			for (int j = i + 1; j < freeRectangles.Count; j++)
			{
				if (IsContainedIn(freeRectangles[i], freeRectangles[j]))
				{
					freeRectangles.RemoveAt(i);
					i--;
					break;
				}
				if (IsContainedIn(freeRectangles[j], freeRectangles[i]))
				{
					freeRectangles.RemoveAt(j);
					j--;
				}
			}
		}
	}

	private bool IsContainedIn(Rect a, Rect b)
	{
		if (a.x >= b.x && a.y >= b.y && a.x + a.width <= b.x + b.width)
		{
			return a.y + a.height <= b.y + b.height;
		}
		return false;
	}
}
