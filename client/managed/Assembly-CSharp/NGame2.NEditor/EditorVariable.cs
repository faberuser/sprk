using NGame2.NEditor.NViewer;

namespace NGame2.NEditor;

public class EditorVariable<T>
{
	protected string _name;

	protected string _tStr;

	public EditorVariable(string name)
	{
		_name = name;
	}

	public EditorVariable(string name, T value)
	{
		_name = name;
		if (value != null)
		{
			_tStr = value.ToString();
		}
	}

	public T Edit(T value)
	{
		DrawHelper.FlexibleSpace();
		if (!String_Extension_Method.IsNullOrEmpty(_name))
		{
			DrawHelper.LabelAutoSize(_name);
		}
		DrawHelper.LabelAutoSize((value != null) ? value.ToString() : " ");
		_tStr = DrawHelper.TextFieldLarge(_tStr);
		if (DrawHelper.ButtonNormal("적용하기"))
		{
			value = ViewerHelper.GenericConver(_tStr, value);
		}
		return value;
	}
}
