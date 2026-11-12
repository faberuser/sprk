using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LitJson;

public class JsonWriter
{
	private static NumberFormatInfo number_format;

	private LitJson.WriterContext context;

	private Stack<LitJson.WriterContext> ctx_stack;

	private bool has_reached_end;

	private char[] hex_seq;

	private int indentation;

	private int indent_value;

	private StringBuilder inst_string_builder;

	private bool pretty_print;

	private bool validate;

	private TextWriter writer;

	public int IndentValue
	{
		get
		{
			return indent_value;
		}
		set
		{
			indentation = indentation / indent_value * value;
			indent_value = value;
		}
	}

	public bool PrettyPrint
	{
		get
		{
			return pretty_print;
		}
		set
		{
			pretty_print = value;
		}
	}

	public TextWriter TextWriter => writer;

	public bool Validate
	{
		get
		{
			return validate;
		}
		set
		{
			validate = value;
		}
	}

	static JsonWriter()
	{
		number_format = NumberFormatInfo.InvariantInfo;
	}

	public JsonWriter()
	{
		inst_string_builder = new StringBuilder();
		writer = new StringWriter(inst_string_builder);
		Init();
	}

	public JsonWriter(StringBuilder sb)
		: this(new StringWriter(sb))
	{
	}

	public JsonWriter(TextWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		this.writer = writer;
		Init();
	}

	private bool DoValidation(LitJson.Condition cond)
	{
		if (!context.ExpectingValue)
		{
			context.Count++;
		}
		if (!validate)
		{
			return true;
		}
		if (has_reached_end)
		{
			return false;
		}
		switch (cond)
		{
		case LitJson.Condition.InArray:
			if (!context.InArray)
			{
				return false;
			}
			break;
		case LitJson.Condition.InObject:
			if (!context.InObject || context.ExpectingValue)
			{
				return false;
			}
			break;
		case LitJson.Condition.NotAProperty:
			if (context.InObject && !context.ExpectingValue)
			{
				return false;
			}
			break;
		case LitJson.Condition.Property:
			if (!context.InObject || context.ExpectingValue)
			{
				return false;
			}
			break;
		case LitJson.Condition.Value:
			if (!context.InArray && (!context.InObject || !context.ExpectingValue))
			{
				return false;
			}
			break;
		}
		return true;
	}

	private void Init()
	{
		has_reached_end = false;
		hex_seq = new char[4];
		indentation = 0;
		indent_value = 4;
		pretty_print = false;
		validate = true;
		ctx_stack = new Stack<LitJson.WriterContext>();
		context = new LitJson.WriterContext();
		ctx_stack.Push(context);
	}

	private static void IntToHex(int n, char[] hex)
	{
		for (int i = 0; i < 4; i++)
		{
			int num = n % 16;
			if (num < 10)
			{
				hex[3 - i] = (char)(48 + num);
			}
			else
			{
				hex[3 - i] = (char)(65 + (num - 10));
			}
			n >>= 4;
		}
	}

	private void Indent()
	{
		if (pretty_print)
		{
			indentation += indent_value;
		}
	}

	private void Put(string str)
	{
		if (pretty_print && !context.ExpectingValue)
		{
			for (int i = 0; i < indentation; i++)
			{
				writer.Write(' ');
			}
		}
		writer.Write(str);
	}

	private void PutNewline()
	{
		PutNewline(add_comma: true);
	}

	private void PutForceNewline()
	{
		PutNewline(add_comma: true, i_forceNewLine: true);
	}

	private void PutNewline(bool add_comma, bool i_forceNewLine = false)
	{
		if (add_comma && !context.ExpectingValue && context.Count > 1)
		{
			writer.Write(',');
		}
		if (!pretty_print)
		{
			return;
		}
		if (i_forceNewLine)
		{
			writer.Write('\n');
			if (context.ExpectingValue)
			{
				for (int i = 0; i < indentation; i++)
				{
					writer.Write(' ');
				}
			}
		}
		else if (!context.ExpectingValue)
		{
			writer.Write('\n');
		}
	}

	private void PutString(string str)
	{
		Put(string.Empty);
		writer.Write('"');
		int length = str.Length;
		for (int i = 0; i < length; i++)
		{
			switch (str[i])
			{
			case '\n':
				writer.Write("\\n");
				continue;
			case '\r':
				writer.Write("\\r");
				continue;
			case '\t':
				writer.Write("\\t");
				continue;
			case '"':
			case '\\':
				writer.Write('\\');
				writer.Write(str[i]);
				continue;
			case '\f':
				writer.Write("\\f");
				continue;
			case '\b':
				writer.Write("\\b");
				continue;
			}
			if (str[i] >= ' ' && str[i] <= '~')
			{
				writer.Write(str[i]);
				continue;
			}
			if (char.GetUnicodeCategory(str[i]) == UnicodeCategory.OtherLetter)
			{
				writer.Write(str[i]);
				continue;
			}
			IntToHex(str[i], hex_seq);
			writer.Write("\\u");
			writer.Write(hex_seq);
		}
		writer.Write('"');
	}

	private void Unindent()
	{
		if (pretty_print)
		{
			indentation -= indent_value;
		}
	}

	public override string ToString()
	{
		if (inst_string_builder == null)
		{
			return string.Empty;
		}
		return inst_string_builder.ToString();
	}

	public void Reset()
	{
		has_reached_end = false;
		ctx_stack.Clear();
		context = new LitJson.WriterContext();
		ctx_stack.Push(context);
		if (inst_string_builder != null)
		{
			inst_string_builder.Remove(0, inst_string_builder.Length);
		}
	}

	public bool Write(bool boolean)
	{
		if (!DoValidation(LitJson.Condition.Value))
		{
			return false;
		}
		PutNewline();
		Put(boolean ? "true" : "false");
		context.ExpectingValue = false;
		return true;
	}

	public bool Write(decimal number)
	{
		if (!DoValidation(LitJson.Condition.Value))
		{
			return false;
		}
		PutNewline();
		Put(Convert.ToString(number, number_format));
		context.ExpectingValue = false;
		return true;
	}

	public bool Write(double number)
	{
		if (!DoValidation(LitJson.Condition.Value))
		{
			return false;
		}
		PutNewline();
		string text = Convert.ToString(number, number_format);
		Put(text);
		if (text.IndexOf('.') == -1 && text.IndexOf('E') == -1)
		{
			writer.Write(".0");
		}
		context.ExpectingValue = false;
		return true;
	}

	public bool Write(long number)
	{
		if (!DoValidation(LitJson.Condition.Value))
		{
			return false;
		}
		PutNewline();
		Put(Convert.ToString(number, number_format));
		context.ExpectingValue = false;
		return true;
	}

	public bool Write(string str)
	{
		if (!DoValidation(LitJson.Condition.Value))
		{
			return false;
		}
		PutNewline();
		if (str == null)
		{
			Put("null");
		}
		else
		{
			PutString(str);
		}
		context.ExpectingValue = false;
		return true;
	}

	public bool WriteArrayEnd()
	{
		if (!DoValidation(LitJson.Condition.InArray))
		{
			return false;
		}
		PutNewline(add_comma: false);
		ctx_stack.Pop();
		if (ctx_stack.Count == 1)
		{
			has_reached_end = true;
		}
		else
		{
			context = ctx_stack.Peek();
			context.ExpectingValue = false;
		}
		Unindent();
		Put("]");
		return true;
	}

	public bool WriteArrayStart()
	{
		if (!DoValidation(LitJson.Condition.NotAProperty))
		{
			return false;
		}
		PutForceNewline();
		Put("[");
		context = new LitJson.WriterContext();
		context.InArray = true;
		ctx_stack.Push(context);
		Indent();
		return true;
	}

	public bool WriteObjectEnd()
	{
		if (!DoValidation(LitJson.Condition.InObject))
		{
			return false;
		}
		PutNewline(add_comma: false);
		ctx_stack.Pop();
		if (ctx_stack.Count == 1)
		{
			has_reached_end = true;
		}
		else
		{
			context = ctx_stack.Peek();
			context.ExpectingValue = false;
		}
		Unindent();
		Put("}");
		return true;
	}

	public bool WriteObjectStart()
	{
		if (!DoValidation(LitJson.Condition.NotAProperty))
		{
			return false;
		}
		PutForceNewline();
		Put("{");
		context = new LitJson.WriterContext();
		context.InObject = true;
		ctx_stack.Push(context);
		Indent();
		return true;
	}

	public bool WritePropertyName(string property_name)
	{
		if (!DoValidation(LitJson.Condition.Property))
		{
			return false;
		}
		PutNewline();
		PutString(property_name);
		if (pretty_print)
		{
			if (property_name.Length > context.Padding)
			{
				context.Padding = property_name.Length;
			}
			for (int num = context.Padding - property_name.Length; num >= 0; num--)
			{
				writer.Write(' ');
			}
			writer.Write(": ");
		}
		else
		{
			writer.Write(':');
		}
		context.ExpectingValue = true;
		return true;
	}
}
