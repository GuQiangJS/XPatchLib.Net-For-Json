// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XPatchLib
{
    /// <summary>
    ///     表示提供快速、非缓存、只进方法的写入器，该方法生成包含 JSON 数据的流或文件。
    /// </summary>
    /// <seealso cref="XPatchLib.ITextWriter" />
    public class JsonTextWriter : ITextWriter
    {
        private Encoding _encoding;
        private Formatting _formatting;
        private int _indentation;
        private char _indentChar;
        private bool _indented;
        private char _quoteChar;

        private JsonSerializeSetting _setting = new JsonSerializeSetting();
        private Newtonsoft.Json.JsonTextWriter _textWriter;

        /// <summary>
        ///     创建 <see cref="XmlTextWriter" /> 类型实例。
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         默认在字符串与 System.DateTime 之间转换时，转换时应保留时区信息。
        ///     </para>
        ///     <para> 默认不序列化默认值。 </para>
        ///     <para> 默认 <see cref="Formatting.Indented" />。 </para>
        /// </remarks>
        internal JsonTextWriter()
        {
            _formatting = Formatting.Indented;
            _indented = true;
            _indentation = 2;
            _indentChar = ' ';
            _quoteChar = '"';
            _encoding = new UTF8Encoding(false);

#if (NET || NETSTANDARD_2_0_UP)
            IgnoreAttributeType = typeof(JsonIgnoreAttribute);
#endif
        }

        /// <summary>
        ///     使用指定的流和编码方式创建 <see cref="XmlTextWriter" /> 类的实例。
        /// </summary>
        /// <param name="pStream">要写入的流。</param>
        /// <param name="pEncoding">要生成的编码方式。如果编码方式为 空引用，则它以 UTF-8 的形式写出流。</param>
        /// <remarks>
        ///     <para>
        ///         默认在字符串与 System.DateTime 之间转换时，转换时应保留时区信息。
        ///     </para>
        ///     <para> 默认不序列化默认值。 </para>
        ///     <para> 默认 <see cref="Formatting.Indented" />。 </para>
        /// </remarks>
        public JsonTextWriter(Stream pStream, Encoding pEncoding) : this()
        {
            _encoding = pEncoding;
            StreamWriter w = pEncoding != null ? new StreamWriter(pStream, pEncoding) : new StreamWriter(pStream);
            w.AutoFlush = false;
            CreateJsonWriter(w);
        }

#if NET || NETSTANDARD_1_3_UP
        /// <summary>
        ///     使用指定的文件创建 <see cref="XmlTextWriter" /> 类的实例。
        /// </summary>
        /// <param name="pFilename">要写入的文件名。如果该文件存在，它将截断该文件并用新内容对其进行改写。</param>
        /// <param name="pEncoding">要生成的编码方式。如果编码方式为 空引用，则它以 UTF-8 的形式写出流。</param>
        public JsonTextWriter(String pFilename, Encoding pEncoding)
            : this(new FileStream(pFilename, FileMode.Create,
                FileAccess.Write, FileShare.Read), pEncoding)
        {
        }
#endif

        /// <summary>
        ///     以指定的 <paramref name="pWriter" /> 实例创建 <see cref="XmlTextWriter" /> 类型实例。
        /// </summary>
        /// <param name="pWriter">指定的有序字符系列的编写器。</param>
        /// <remarks>
        ///     <para>
        ///         默认在字符串与 System.DateTime 之间转换时，转换时应保留时区信息。
        ///     </para>
        ///     <para> 默认不序列化默认值。 </para>
        ///     <para> 默认 <see cref="Formatting.Indented" />。 </para>
        /// </remarks>
        public JsonTextWriter(TextWriter pWriter) : this()
        {
            CreateJsonWriter(pWriter);
            _encoding = pWriter.Encoding;
        }

        /// <summary>执行与释放或重置非托管资源相关的应用程序定义的任务。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>写入文档开始标记。</summary>
        public void WriteStartDocument()
        {
            _textWriter.WriteStartObject();
        }

        /// <summary>写入文档结束标记。</summary>
        public void WriteEndDocument()
        {
            //_textWriter.WriteEndObject();
            _textWriter.WriteEnd();
        }

        /// <summary>将缓冲区中的所有内容刷新到基础流，并同时刷新基础流。</summary>
        public void Flush()
        {
            _textWriter.Flush();
        }

        /// <summary>写入对象开始标记。</summary>
        /// <param name="pName">对象名称。</param>
        public void WriteStartObject(string pName)
        {
            _textWriter.WritePropertyName(pName);
            _textWriter.WriteStartObject();
        }

        /// <summary>写入对象结束标记。</summary>
        public void WriteEndObject()
        {
            _textWriter.WriteEndObject();
        }

        /// <summary>写入特性。</summary>
        /// <param name="pName">特性名称。</param>
        /// <param name="pValue">特性值。</param>
        public void WriteAttribute(string pName, string pValue)
        {
            WriteProperty(string.Concat(_setting.AFC, pName), pValue);
        }

        /// <summary>写入属性。</summary>
        /// <param name="pName">属性名称。</param>
        /// <param name="pValue">属性值。</param>
        public void WriteProperty(string pName, string pValue)
        {
            _textWriter.WritePropertyName(pName);
            _textWriter.WriteValue(pValue);
        }

        /// <summary>写入属性开始标记。</summary>
        /// <param name="pName">属性名称。</param>
        public void WriteStartProperty(string pName)
        {
            if (_textWriter.WriteState != WriteState.Object)
                _textWriter.WriteStartObject();
            _textWriter.WritePropertyName(pName);
        }

        /// <summary>写入属性结束标记。</summary>
        public void WriteEndProperty()
        {
        }

        /// <summary>写入列表类型对象开始标记。</summary>
        /// <param name="pName">列表类型对象实例名称。</param>
        public void WriteStartArray(string pName)
        {
            if (_textWriter.WriteState != WriteState.Object)
                _textWriter.WriteStartObject();
            _textWriter.WritePropertyName(pName);
            _textWriter.WriteStartObject();
        }

        /// <summary>写入列表元素对象开始标记。</summary>
        /// <param name="pName">列表元素对象实例名称。</param>
        public void WriteStartArrayItem(string pName)
        {
            if (_textWriter.WriteState != WriteState.Array)
            {
                _textWriter.WritePropertyName(pName);
                _textWriter.WriteStartArray();
            }
            _textWriter.WriteStartObject();
        }

        /// <summary>写入列表元素结束标记。</summary>
        public void WriteEndArrayItem()
        {
            _textWriter.WriteEndObject();
        }

        /// <summary>写入列表对象结束标记。</summary>
        public void WriteEndArray()
        {
            _textWriter.WriteEndArray();
            _textWriter.WriteEndObject();
        }

        /// <summary>写入文本。</summary>
        /// <param name="pValue">待写入的文本。</param>
        public void WriteValue(string pValue)
        {
            if (_textWriter.WriteState != WriteState.Property)
                _textWriter.WritePropertyName(_setting.SAIN);
            _textWriter.WriteValue(pValue);
        }

        /// <summary>
        ///     获取或设置指示 <see cref="T:XPatchLib.Serializer" /> 方法
        ///     <see cref="M:XPatchLib.Serializer.Divide(XPatchLib.ITextWriter,System.Object,System.Object)" />
        ///     进行序列化的公共字段或公共读/写属性值。
        /// </summary>
        /// <remarks>
        ///     用于控制如何 <see cref="T:XPatchLib.Serializer" /> 方法
        ///     <see cref="M:XPatchLib.Serializer.Divide(XPatchLib.ITextWriter,System.Object,System.Object)" /> 序列化对象。
        /// </remarks>
        /// <seealso cref="P:Newtonsoft.Json.JsonIgnoreAttribute" />
        public Type IgnoreAttributeType { get; set; }

        /// <summary>获取或设置写入器设置。</summary>
        /// <exception cref="ArgumentOutOfRangeException">当赋值不是<see cref="JsonSerializeSetting" />类型实例时。</exception>
        public ISerializeSetting Setting
        {
            get { return _setting; }
            set
            {
                if (value != null)
                {
                    _setting = value as JsonSerializeSetting;
                    if (_setting == null)
                        throw new ArgumentOutOfRangeException(nameof(Setting));
                }
            }
        }

        /// <summary>指示如何对输出进行格式设置。</summary>
        /// <value><see cref="P:XPatchLib.ITextWriter.Formatting" /> 值之一。默认值为 <c>Formatting.Indented</c>（缩进显示）。</value>
        /// <remarks>
        ///     如果设置了 <c>Formatting.Indented</c> 选项，则使用 <see cref="P:XPatchLib.ITextWriter.Indentation" /> 和
        ///     <see cref="P:XPatchLib.ITextWriter.IndentChar" /> 属性对子元素进行缩进。
        /// </remarks>
        public Formatting Formatting
        {
            get { return _formatting; }
            set
            {
                _formatting = value;
                _indented = value == Formatting.Indented;
                _textWriter.Formatting = _formatting == Formatting.Indented
                    ? Newtonsoft.Json.Formatting.Indented
                    : Newtonsoft.Json.Formatting.None;
            }
        }

        /// <summary>
        ///     获取或设置当 <see cref="P:XPatchLib.ITextWriter.Formatting" /> 设置为 <c>Formatting.Indented</c> 时将为层次结构中的每个级别书写多少
        ///     <see cref="P:XPatchLib.ITextWriter.IndentChar" />。
        /// </summary>
        /// <value>每个级别的 <see cref="P:XPatchLib.ITextWriter.IndentChar" /> 的数目。默认值为 2。</value>
        public int Indentation
        {
            get { return _indentation; }
            set
            {
                if (value < 0)
                {
                    //TODO
                    //throw new ArgumentException(ResourceHelper.GetResourceString(LocalizationRes.Exp_String_InvalidIndentation));
                }
                _indentation = value;
                _textWriter.Indentation = _indentation;
            }
        }

        /// <summary>
        ///     获取或设置当 <see cref="P:XPatchLib.ITextWriter.Formatting" /> 设置为 <c>Formatting.Indented</c> 时哪个字符用于缩进。
        /// </summary>
        /// <value>用于缩进的字符。默认为空格。</value>
        public char IndentChar
        {
            get { return _indentChar; }
            set
            {
                if (value != _indentChar)
                    _indentChar = value;
                _textWriter.IndentChar = _indentChar;
            }
        }

        /// <summary>获取或设置哪个字符用于将属性值引起来。</summary>
        /// <value>用于将属性值引起来的字符。这必须是单引号 (') 或双引号 (")。默认为双引号。</value>
        public char QuoteChar
        {
            get { return _quoteChar; }
            set
            {
                if (value != '"' && value != '\'')
                {
                    //TODO
                    //throw new ArgumentException(ResourceHelper.GetResourceString(LocalizationRes.Exp_String_InvalidQuote));
                }
                _quoteChar = value;
                _textWriter.QuoteChar = _quoteChar;
            }
        }

        private void CreateJsonWriter(TextWriter writer)
        {
            _textWriter = new Newtonsoft.Json.JsonTextWriter(writer);
            _textWriter.IndentChar = _indentChar;
            _textWriter.Indentation = _indentation;
            _textWriter.QuoteChar = _quoteChar;
            _textWriter.Formatting = _formatting == Formatting.Indented
                ? Newtonsoft.Json.Formatting.Indented
                : Newtonsoft.Json.Formatting.None;
        }

        /// <summary>
        ///     执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                Flush();
            }
            catch
            {
                // never fail
            }
            finally
            {
#if NET || NETSTANDARD_2_0_UP
                _textWriter.Close();
#endif
            }
        }
    }
}