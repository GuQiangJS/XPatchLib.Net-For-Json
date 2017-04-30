// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace XPatchLib.Json
{
    /// <summary>
    ///     表示提供快速、非缓存、只进方法的写入器，该方法生成包含 JSON 数据的流或文件。
    /// </summary>
    /// <seealso cref="ITextWriter" />
    public class JsonTextWriter : ITextWriter
    {
        private JsonWriter _writer;

        /// <summary>
        ///     以指定的 <paramref name="pWriter" /> 实例创建 <see cref="JsonTextWriter" /> 类型实例。
        /// </summary>
        /// <param name="pWriter">指定的 Json 编写器。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="pWriter" /> 为 <b>null</b> 时。</exception>
        /// <remarks>
        ///     <para>
        ///         默认在字符串与 System.DateTime 之间转换时，转换时应保留时区信息。
        ///     </para>
        ///     <para> 默认不序列化默认值。 </para>
        /// </remarks>
        public JsonTextWriter(JsonWriter pWriter)
        {
            if (pWriter == null)
                throw new ArgumentNullException("pWriter");
            _writer = pWriter;
            IgnoreAttributeType = typeof(JsonIgnoreAttribute);
            Setting = new JsonSerializeSetting();
        }

        /// <summary>
        ///     执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                _writer.Flush();
            }
            catch (ObjectDisposedException)
            {
            }
            //if (disposing)
            //    ((IDisposable) Writer)?.Dispose();
        }

        /// <summary>写入文档开始标记。</summary>
        public void WriteStartDocument()
        {
        }

        /// <summary>写入文档结束标记。</summary>
        public void WriteEndDocument()
        {
        }

        /// <summary>将缓冲区中的所有内容刷新到基础流，并同时刷新基础流。</summary>
        public void Flush()
        {
            _writer.Flush();
        }

        /// <summary>写入对象开始标记。</summary>
        /// <param name="pName">对象名称。</param>
        public void WriteStartObject(string pName)
        {
            if (_writer.WriteState == WriteState.Start)
            {
                _writer.WriteStartObject();
#if DEBUG
                Debug.WriteLine(string.Format("WriteStartObject '{0}'.", pName));
#endif
            }
        }

        /// <summary>写入对象结束标记。</summary>
        public void WriteEndObject()
        {
            if (_writer.WriteState == WriteState.Start)
            {
                _writer.WriteEndObject();
#if DEBUG
                Debug.WriteLine("WriteEndObject.");
#endif
            }
        }

        /// <summary>写入特性。</summary>
        /// <param name="pName">特性名称。</param>
        /// <param name="pValue">特性值。</param>
        public void WriteAttribute(string pName, string pValue)
        {
            WriteProperty(pName, pValue);
        }

        /// <summary>写入属性。</summary>
        /// <param name="pName">属性名称。</param>
        /// <param name="pValue">属性值。</param>
        public void WriteProperty(string pName, string pValue)
        {
            if (_writer.WriteState == WriteState.Property)
            {
                _writer.WriteValue(";");
            }
            _writer.WritePropertyName(pName);
            _writer.WriteValue(pValue);
#if DEBUG
            Debug.WriteLine("WriteProperty '{0}'='{1}'.", pName, pValue);
#endif
        }

        /// <summary>写入属性开始标记。</summary>
        /// <param name="pName">属性名称。</param>
        public void WriteStartProperty(string pName)
        {
            _writer.WritePropertyName(pName);
#if DEBUG
            Debug.WriteLine(string.Format("WriteStartProperty '{0}'.", pName));
#endif
        }

        /// <summary>写入属性结束标记。</summary>
        public void WriteEndProperty()
        {
        }

        /// <summary>
        ///     写入列表类型对象开始标记。
        ///     <param name="pName">列表类型对象实例名称。</param>
        /// </summary>
        public void WriteStartArray(string pName)
        {
            _writer.WriteStartArray();
            _writer.WritePropertyName(pName);
#if DEBUG
            Debug.WriteLine(string.Format("WriteStartArray '{0}'.", pName));
#endif
        }

        /// <summary>写入列表对象结束标记。</summary>
        public void WriteEndArray()
        {
            _writer.WriteEndArray();
#if DEBUG
            Debug.WriteLine("WriteEndArray.");
#endif
        }

        /// <summary>写入文本。</summary>
        /// <param name="pValue">待写入的文本。</param>
        public void WriteValue(string pValue)
        {
            _writer.WriteValue(pValue);
#if DEBUG
            Debug.WriteLine(string.Format("WriteValue '{0}'.", pValue));
#endif
        }

        /// <summary>
        ///     获取指示 <see cref="T:XPatchLib.Serializer" /> 方法
        ///     <see cref="M:XPatchLib.Serializer.Divide(XPatchLib.ITextWriter,System.Object,System.Object)" />
        ///     进行序列化的公共字段或公共读/写属性值。
        /// </summary>
        /// <remarks>
        ///     用于控制如何 <see cref="T:XPatchLib.Serializer" /> 方法
        ///     <see cref="M:XPatchLib.Serializer.Divide(XPatchLib.ITextWriter,System.Object,System.Object)" /> 序列化对象。
        /// </remarks>
        /// <seealso cref="P:XPatchLib.XmlTextWriter.IgnoreAttributeType" />
        /// <value>默认返回 <see cref="JsonIgnoreAttribute" /></value>
        public Type IgnoreAttributeType { get; }

        /// <summary>获取或设置写入器设置。</summary>
        /// <value>默认值为 <see cref="JsonSerializeSetting" /></value>
        public ISerializeSetting Setting { get; set; }
    }
}