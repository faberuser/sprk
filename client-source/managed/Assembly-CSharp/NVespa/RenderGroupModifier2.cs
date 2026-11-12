using System;
using System.Collections.Generic;
using NShader;
using NShader.NUtil;
using NVespa.NRenderGroupModifier;
using UnityEngine;

namespace NVespa;

public class RenderGroupModifier2 : SmallPooling<RenderGroupModifier2>, ISmallPoolingItem, IGenericVisitor<RenderProxy>, IVisitor
{
	[Flags]
	private enum EDirty
	{
		PropertyBlock_Dirty = 1,
		PropertyBlock_Reset = 2,
		ExtraAlpha_Dirty = 4,
		MaterialKeyword_Dirty = 8,
		ColorMix_Dirty = 0x10
	}

	private class ReplaceTexData
	{
		public Texture Tex;

		public object Caller;

		public int TexPriority;
	}

	private EDirty _dirty;

	private bool _useSoloTimeColor;

	private Vector4 _origColorMix = Vector4.zero;

	private Vector4 _colorMix = Vector4.zero;

	private float _extraAlphaFinal = 1f;

	private List<ReplaceTexData> _replaceTexList = new List<ReplaceTexData>();

	private MatrixDic _matrixDic = new MatrixDic();

	private IncreaseDic<int, float> _floatDic = new IncreaseDic<int, float>();

	private ExtraAlpha _extraAlpha = new ExtraAlpha();

	private TimerEventColorMix _soloTimeColor = new TimerEventColorMix();

	private IncreaseDic<string, bool> _materialKeywordDic = new IncreaseDic<string, bool>();

	public MaterialPropertyBlock PropertyBlock { get; private set; }

	public float ExtraAlphaFinal => _extraAlphaFinal;

	void ISmallPoolingItem.OnCreateNew()
	{
		PropertyBlock = new MaterialPropertyBlock();
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		_dirty = (EDirty)0;
		_useSoloTimeColor = false;
		_origColorMix = Vector4.zero;
		_colorMix = Vector4.zero;
		_extraAlphaFinal = 1f;
		_replaceTexList.Clear();
		_matrixDic.Clear();
		_floatDic.Clear();
		_extraAlpha.Reset();
		_soloTimeColor.Reset();
		_materialKeywordDic.Clear();
		PropertyBlock.Clear();
	}

	public void Open(bool useSoloTimeColor, bool isBattleField = false)
	{
		_useSoloTimeColor = useSoloTimeColor;
		_extraAlpha.Reset();
		_soloTimeColor.IsBattleField = isBattleField;
	}

	public void Close()
	{
		PropertyBlock.Clear();
	}

	public bool BeginFlush(bool forcePropertyBlockDirty = false)
	{
		if (forcePropertyBlockDirty)
		{
			_dirty |= EDirty.PropertyBlock_Dirty;
		}
		if (_dirty == (EDirty)0)
		{
			return false;
		}
		if ((_dirty & EDirty.PropertyBlock_Reset) != 0)
		{
			ResetBlock();
		}
		return true;
	}

	public void EndFlush()
	{
		_dirty = (EDirty)0;
	}

	private bool IsDirty(EDirty dirtyFlag)
	{
		return (_dirty & dirtyFlag) != 0;
	}

	void IGenericVisitor<RenderProxy>.Visit(RenderProxy proxy)
	{
		if (proxy.renderer == null)
		{
			return;
		}
		if (IsDirty(EDirty.PropertyBlock_Dirty | EDirty.PropertyBlock_Reset))
		{
			proxy.renderer.SetPropertyBlock(PropertyBlock);
		}
		if (IsDirty(EDirty.ColorMix_Dirty))
		{
			Material[] materials = proxy.materials;
			int num = __Extension_Method_ICollection__.COUNT(materials);
			for (int i = 0; i < num; i++)
			{
				if (!(materials[i] == null))
				{
					materials[i].SetVector(Define.COLORMIX_ID, _colorMix);
				}
			}
		}
		if (IsDirty(EDirty.MaterialKeyword_Dirty))
		{
			for (int j = 0; j < _materialKeywordDic.Count; j++)
			{
				KeyValuePair<string, bool> keyValuePairByIndex = _materialKeywordDic.GetKeyValuePairByIndex(j);
				string key = keyValuePairByIndex.Key;
				if (keyValuePairByIndex.Value)
				{
					proxy.EnableMaterialKeyword(key);
				}
				else
				{
					proxy.DisableMaterialKeyword(key);
				}
			}
		}
		if (!IsDirty(EDirty.ExtraAlpha_Dirty))
		{
			return;
		}
		bool flag = !Mathf.Approximately(ExtraAlphaFinal, 1f);
		Material[] materials2 = proxy.materials;
		int num2 = __Extension_Method_ICollection__.COUNT(materials2);
		if (proxy.IncludeAlphaTest)
		{
			if (flag)
			{
				Material[] arraySortByRenderQ = GetArraySortByRenderQ(proxy.materials);
				int num3 = 0;
				int num4 = 0;
				for (int k = 0; k < num2; k++)
				{
					Material material = arraySortByRenderQ[k];
					if (!(material == null))
					{
						material.SetInt(Define.BLENDMODESRC_ID, 5);
						material.SetInt(Define.BLENDMODEDST_ID, 10);
						if (material.renderQueue < 3000)
						{
							int num5 = ((num3 == 0 || num3 >= material.renderQueue) ? num4 : (++num4));
							num3 = material.renderQueue;
							material.renderQueue = 3000 + num5;
						}
					}
				}
			}
			else
			{
				Material[] sharedMaterials = proxy.SharedMaterials;
				for (int l = 0; l < num2; l++)
				{
					Material material2 = materials2[l];
					Material material3 = sharedMaterials[l];
					if (!(material2 == null) && !(material3 == null))
					{
						if (material2.HasProperty(Define.BLENDMODESRC_ID) && material3.HasProperty(Define.BLENDMODESRC_ID))
						{
							material2.SetInt(Define.BLENDMODESRC_ID, material3.GetInt(Define.BLENDMODESRC_ID));
						}
						if (material2.HasProperty(Define.BLENDMODEDST_ID) && material3.HasProperty(Define.BLENDMODEDST_ID))
						{
							material2.SetInt(Define.BLENDMODEDST_ID, material3.GetInt(Define.BLENDMODEDST_ID));
						}
						material2.renderQueue = material3.renderQueue;
					}
				}
			}
		}
		for (int m = 0; m < num2; m++)
		{
			Material material4 = materials2[m];
			if (!(material4 == null))
			{
				material4.SetFloat(Define.EXTRAALPHA_ID, ExtraAlphaFinal);
			}
		}
	}

	public Material[] GetArraySortByRenderQ(Material[] materials)
	{
		int num = __Extension_Method_ICollection__.COUNT(materials);
		if (num < 2)
		{
			return materials;
		}
		Material[] array = new Material[num];
		Array.Copy(materials, array, num);
		Array.Sort(array, MaterialCompareTo);
		return array;
	}

	private int MaterialCompareTo(Material l, Material r)
	{
		if (l == null && r == null)
		{
			return 0;
		}
		if (l == null)
		{
			return -1;
		}
		if (r == null)
		{
			return 1;
		}
		return l.renderQueue.CompareTo(r.renderQueue);
	}

	public void CopyTo(RenderGroupModifier2 modifier, bool ignoreReplaceTex = false)
	{
		for (int i = 0; i < _materialKeywordDic.Count; i++)
		{
			KeyValuePair<string, bool> keyValuePairByIndex = _materialKeywordDic.GetKeyValuePairByIndex(i);
			modifier.SetMaterialKeyword(keyValuePairByIndex.Key, keyValuePairByIndex.Value);
		}
		modifier.SetExtraAlpha(_extraAlpha.Value);
		if (!ignoreReplaceTex)
		{
			modifier.RemoveReplaceTextureAll();
			for (int j = 0; j < _replaceTexList.Count; j++)
			{
				ReplaceTexData replaceTexData = _replaceTexList[j];
				modifier.ReplaceTexture(replaceTexData.Tex, replaceTexData.Caller, replaceTexData.TexPriority);
			}
		}
		for (int k = 0; k < _matrixDic.Count; k++)
		{
			modifier.SetMatrixProperty(_matrixDic.GetKeyByIndex(k), _matrixDic.GetValueByIndex(k));
		}
		for (int l = 0; l < _floatDic.Count; l++)
		{
			modifier.SetFloatProperty(_floatDic.GetKeyByIndex(l), _floatDic.GetValueByIndex(l));
		}
		modifier.ColorMix(_origColorMix);
	}

	public void SetGroundClip(bool enable)
	{
		SetMaterialKeyword(Keyword.GROUNDCLIP_OFF, !enable);
		SetMaterialKeyword(Keyword.GROUNDCLIP_ON, enable);
		SetMaterialKeyword(Keyword.SKIP_GROUNDCLIP_OFF, enable);
		SetMaterialKeyword(Keyword.SKIP_GROUNDCLIP_ON, !enable);
	}

	public void SetMaterialKeyword(string keyword, bool isEnable)
	{
		if (!_materialKeywordDic.TryGetValue(keyword, out var o_value) || o_value != isEnable)
		{
			_materialKeywordDic.AddAndOverride(keyword, isEnable);
			_dirty |= EDirty.MaterialKeyword_Dirty;
		}
	}

	public void SetExtraAlpha(float alpha)
	{
		_extraAlpha.Value = alpha;
	}

	public void ReplaceTexture(Texture tex, object caller, int texPriority)
	{
		if (!(tex == null))
		{
			_replaceTexList.Add(new ReplaceTexData
			{
				Tex = tex,
				Caller = caller,
				TexPriority = texPriority
			});
			UpdateMainTexture();
		}
	}

	public void RemoveReplaceTexture(object caller)
	{
		for (int i = 0; i < _replaceTexList.Count; i++)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(_replaceTexList[i].Caller, caller))
			{
				_replaceTexList.RemoveAt(i);
				UpdateMainTexture();
				break;
			}
		}
	}

	public void RemoveReplaceTextureAll()
	{
		if (_replaceTexList.Count != 0)
		{
			_replaceTexList.Clear();
			UpdateMainTexture();
		}
	}

	private void UpdateMainTexture()
	{
		_replaceTexList.Sort((ReplaceTexData lhs, ReplaceTexData rhs) => rhs.TexPriority.CompareTo(lhs.TexPriority));
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_replaceTexList))
		{
			_dirty |= EDirty.PropertyBlock_Reset;
			return;
		}
		PropertyBlock.SetTexture(Define.MAINTEX_ID, _replaceTexList[0].Tex);
		_dirty |= EDirty.PropertyBlock_Dirty;
	}

	public void SetMatrixProperty(int keywordId, Matrix4x4 matrix)
	{
		_matrixDic.Add(keywordId, matrix);
		PropertyBlock.SetMatrix(keywordId, matrix);
		_dirty |= EDirty.PropertyBlock_Dirty;
	}

	public void SetFloatProperty(int keywordId, float value)
	{
		if (!_floatDic.TryGetValue(keywordId, out var o_value) || !Mathf.Approximately(o_value, value))
		{
			_floatDic.Add(keywordId, value);
			PropertyBlock.SetFloat(keywordId, value);
			_dirty |= EDirty.PropertyBlock_Dirty;
		}
	}

	public Vector4 GetColorMix()
	{
		return _origColorMix;
	}

	public void ColorMix(Vector4 color)
	{
		if (!(Vector4.Distance(_origColorMix, color) < Mathf.Epsilon))
		{
			_origColorMix = color;
		}
	}

	public void ChainUpdate(ILocalTimer localTimer, bool isBattleField = false)
	{
		UpdateExtraAlpha(localTimer, isBattleField);
		UpdateColorMix(localTimer);
	}

	private void UpdateExtraAlpha(ILocalTimer localTimer, bool isBattleField = false)
	{
		float num = _extraAlpha.Value;
		if (_useSoloTimeColor)
		{
			_extraAlpha.Update(localTimer, isBattleField);
			num = _extraAlpha.Final;
		}
		if (!Math.Approximately(_extraAlphaFinal, num))
		{
			_extraAlphaFinal = num;
			_dirty |= EDirty.ExtraAlpha_Dirty;
		}
	}

	private void UpdateColorMix(ILocalTimer localTimer)
	{
		Vector4 vector = _origColorMix;
		if (_useSoloTimeColor)
		{
			vector = _soloTimeColor.UpdateAndCalc(localTimer, vector);
		}
		if (!(Vector4.Distance(_colorMix, vector) < Mathf.Epsilon))
		{
			_colorMix = vector;
			PropertyBlock.SetVector(Define.COLORMIX_ID, _colorMix);
			_dirty |= EDirty.PropertyBlock_Dirty;
			_dirty |= EDirty.ColorMix_Dirty;
		}
	}

	private void ResetBlock()
	{
		PropertyBlock.Clear();
		PropertyBlock.SetVector(Define.COLORMIX_ID, _colorMix);
		for (int i = 0; i < _matrixDic.Count; i++)
		{
			PropertyBlock.SetMatrix(_matrixDic.GetKeyByIndex(i), _matrixDic.GetValueByIndex(i));
		}
		for (int j = 0; j < _floatDic.Count; j++)
		{
			KeyValuePair<int, float> keyValuePairByIndex = _floatDic.GetKeyValuePairByIndex(j);
			PropertyBlock.SetFloat(keyValuePairByIndex.Key, keyValuePairByIndex.Value);
		}
		if (_replaceTexList.Count > 0)
		{
			PropertyBlock.SetTexture(Define.MAINTEX_ID, _replaceTexList[0].Tex);
		}
	}
}
