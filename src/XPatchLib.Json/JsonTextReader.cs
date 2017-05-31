// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Lifetime;
using System.Xml;
using Newtonsoft.Json;

namespace XPatchLib.Json {

    /// <summary>
    ///     表示提供对 JSON 数据进行快速、非缓存、只进访问的读取器。
    /// </summary>
    /// <seealso cref="XPatchLib.ITextReader" />
    public class JsonTextReader : ITextReader {

        private JsonReader _reader;

        private readonly Stack<JsonToken> _readerState = new Stack<JsonToken>();

        /// <summary>
        ///     以指定的 <paramref name="pReader" /> 实例创建 <see cref="JsonTextReader" /> 类型实例。
        /// </summary>
        /// <param name="pReader">指定的 Json 读取器。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="pReader" /> 为 <b>null</b> 时。</exception>
        /// <remarks>
        ///     <para>
        ///         默认在字符串与 System.DateTime 之间转换时，转换时应保留时区信息。
        ///     </para>
        ///     <para> 默认不序列化默认值。 </para>
        /// </remarks>
        public JsonTextReader(JsonReader pReader)
        {
            if (pReader == null)
                throw new ArgumentNullException("pReader");
            _reader = pReader;
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
                _reader.Close();
            }
            catch (ObjectDisposedException)
            {
            }
            //if (disposing)
            //    ((IDisposable) Writer)?.Dispose();
        }

        /// <summary>从流中读取下一个节点。</summary>
        public bool Read() {
            bool result = _reader.Read();

            if (_reader.TokenType == JsonToken.EndObject) {
                JsonToken lastToken = _readerState.Pop();
                while (lastToken != JsonToken.StartObject) {
                    lastToken = _readerState.Pop();
                }
            }
            else if (_reader.TokenType == JsonToken.EndArray) {
                JsonToken lastToken = _readerState.Pop();
                while (lastToken != JsonToken.StartArray) {
                    lastToken = _readerState.Pop();
                }
            }
            else {
                _readerState.Push(_reader.TokenType);
            }

            SetNodeType();

            return result;
        }

        private void SetNodeType() {
            JsonToken lastToken = GetLastToken();
            switch (lastToken) {
                case JsonToken.StartObject:
                case JsonToken.StartArray:
                    NodeType = NodeType.Element;
                    break;
                case JsonToken.EndArray:
                case JsonToken.EndObject:
                    NodeType = NodeType.EndElement;
                    break;
                case JsonToken.PropertyName:
                    NodeType = IsPropertyName ? NodeType.Element : NodeType.Attribute;
                    break;
                case JsonToken.String:
                case JsonToken.Date:
                    NodeType = NodeType.Element;
                    break;
            }
        }

        JsonToken GetLastToken() {
            if (_readerState.Count > 0) {
                return _readerState.Peek();
            }
            return JsonToken.None;
        }

        bool IsAttribute {
            get {
                JsonToken lastToken = GetLastToken();
                if (lastToken == JsonToken.PropertyName && Name.StartsWith("@")) {
                    return true;
                }
                return false;
            }
        }

        private bool IsPropertyName {
            get {
                JsonToken lastToken = GetLastToken();
                if (lastToken == JsonToken.PropertyName && !Name.StartsWith("@")) {
                    return true;
                }
                return false;
            }
        }

        /// <summary>将元素或文本节点的内容当做字符串读取。</summary>
        public string ReadString() {
            while (true) {
                if (NodeType == NodeType.Element && !IsPropertyName && !IsAttribute) {
                    break;
                }
                Read();
            }
            return Value;
        }

        /// <summary>移动到下一个属性。</summary>
        /// <returns>如果存在下一个属性，则为 <c>true</c>；如果没有其他属性，则为 <c>false</c>。</returns>
        public bool MoveToNextAttribute() {
            return false;
        }

        /// <summary>移动到包含当前属性节点的元素。</summary>
        /// <returns>如果读取器定位在属性上，则为 <c>true</c>（读取器移动到拥有该属性的元素）；如果读取器不是定位在属性上，则为 <c>false</c>（读取器的位置不改变）。</returns>
        public bool MoveToElement()
        {
            bool result = false;
            while (result=Read() && _reader.TokenType != JsonToken.PropertyName) { }
            return result;
        }

        /// <summary>获取或设置读取器设置。</summary>
        public ISerializeSetting Setting { get; set; }

        /// <summary>获取读取器的状态。</summary>
        public ReadState ReadState {
            get {

                if (_reader.TokenType == JsonToken.None) {
                    return ReadState.Initial;
                }
                else {
                    return ReadState.Interactive;
                }
            }
        }

        /// <summary>获取一个值，该值指示此读取器是否定位在流的结尾。</summary>
        public bool EOF { get { return _reader.TokenType == JsonToken.None; } }

        /// <summary>获取当前节点的限定名。</summary>
        public string Name { get { return _reader.Value != null ? _reader.Value.ToString() : string.Empty; } }

        XmlDateTimeSerializationMode DataTimeMode {
            get {
                switch (Setting.Mode) {
                    case DateTimeSerializationMode.Local:
                        return XmlDateTimeSerializationMode.Local;
                    case DateTimeSerializationMode.RoundtripKind:
                        return XmlDateTimeSerializationMode.RoundtripKind;
                    case DateTimeSerializationMode.Unspecified:
                        return XmlDateTimeSerializationMode.Unspecified;
                    case DateTimeSerializationMode.Utc:
                        return XmlDateTimeSerializationMode.Utc;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>获取当前节点的文本值。</summary>
        public string Value {
            get {
                if (_reader.Value != null) {
                    if (_reader.ValueType == typeof(DateTime)) {
                        return XmlConvert.ToString((DateTime) _reader.Value, DataTimeMode);
                    }
                    else {
                        return _reader.Value.ToString();
                    }
                }
                return string.Empty;
            }
        }

        /// <summary>获取当前节点上的属性数。</summary>
        public int AttributeCount { get; private set; }

        /// <summary>获取当前节点的类型。</summary>
        public NodeType NodeType { get; private set; }
    }
}