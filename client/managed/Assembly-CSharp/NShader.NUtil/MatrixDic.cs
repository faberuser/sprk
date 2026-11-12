using NVespa;
using UnityEngine;

namespace NShader.NUtil;

public class MatrixDic
{
	private struct ReserveMatrix
	{
		public bool Use;

		public int KeywordId;

		public Matrix4x4 Value;
	}

	private ReserveMatrix _ShaderTransform_PreTransform;

	private ReserveMatrix _ShaderTransform_PostTransform;

	private ReserveMatrix _ShaderTransform_NormalTransform;

	private IncreaseDic<int, Matrix4x4> _matrixDic;

	public int Count { get; private set; }

	public MatrixDic()
	{
		_ShaderTransform_PreTransform.KeywordId = Define._ShaderTransform_PreTransform;
		_ShaderTransform_PostTransform.KeywordId = Define._ShaderTransform_PostTransform;
		_ShaderTransform_NormalTransform.KeywordId = Define._ShaderTransform_NormalTransform;
	}

	public void Clear()
	{
		_ShaderTransform_PreTransform.Use = false;
		_ShaderTransform_PostTransform.Use = false;
		_ShaderTransform_NormalTransform.Use = false;
		if (_matrixDic != null)
		{
			_matrixDic.Clear();
		}
		Count = 0;
	}

	public int GetKeyByIndex(int index)
	{
		if (index < __Extension_Method_ICollection__.COUNT(_matrixDic))
		{
			return _matrixDic.GetKeyByIndex(index);
		}
		index -= __Extension_Method_ICollection__.COUNT(_matrixDic);
		if (_ShaderTransform_PreTransform.Use)
		{
			if (index == 0)
			{
				return _ShaderTransform_PreTransform.KeywordId;
			}
			index--;
		}
		if (_ShaderTransform_PostTransform.Use)
		{
			if (index == 0)
			{
				return _ShaderTransform_PostTransform.KeywordId;
			}
			index--;
		}
		if (_ShaderTransform_NormalTransform.Use)
		{
			if (index == 0)
			{
				return _ShaderTransform_NormalTransform.KeywordId;
			}
			index--;
		}
		return 0;
	}

	public Matrix4x4 GetValueByIndex(int index)
	{
		if (index < __Extension_Method_ICollection__.COUNT(_matrixDic))
		{
			return _matrixDic.GetValueByIndex(index);
		}
		index -= __Extension_Method_ICollection__.COUNT(_matrixDic);
		if (_ShaderTransform_PreTransform.Use)
		{
			if (index == 0)
			{
				return _ShaderTransform_PreTransform.Value;
			}
			index--;
		}
		if (_ShaderTransform_PostTransform.Use)
		{
			if (index == 0)
			{
				return _ShaderTransform_PostTransform.Value;
			}
			index--;
		}
		if (_ShaderTransform_NormalTransform.Use)
		{
			if (index == 0)
			{
				return _ShaderTransform_NormalTransform.Value;
			}
			index--;
		}
		return Matrix4x4.identity;
	}

	public void Add(int key, Matrix4x4 value)
	{
		if (_ShaderTransform_PreTransform.KeywordId == key)
		{
			_ShaderTransform_PreTransform.Value = value;
			_ShaderTransform_PreTransform.Use = true;
		}
		else if (_ShaderTransform_PostTransform.KeywordId == key)
		{
			_ShaderTransform_PostTransform.Value = value;
			_ShaderTransform_PostTransform.Use = true;
		}
		else if (_ShaderTransform_NormalTransform.KeywordId == key)
		{
			_ShaderTransform_NormalTransform.Value = value;
			_ShaderTransform_NormalTransform.Use = true;
		}
		else if (_matrixDic == null)
		{
			_matrixDic = new IncreaseDic<int, Matrix4x4>();
			_matrixDic.Add(key, value);
		}
		else
		{
			_matrixDic.AddAndOverride(key, value);
		}
		Count = __Extension_Method_ICollection__.COUNT(_matrixDic);
		if (_ShaderTransform_PreTransform.Use)
		{
			Count++;
		}
		if (_ShaderTransform_PostTransform.Use)
		{
			Count++;
		}
		if (_ShaderTransform_NormalTransform.Use)
		{
			Count++;
		}
	}
}
