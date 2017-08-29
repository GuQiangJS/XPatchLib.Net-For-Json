// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Newtonsoft.Json;

namespace XPatchLib
{
    /// <summary>
    ///     表示提供对 JSON 数据进行快速、非缓存、只进访问的读取器。
    /// </summary>
    /// <seealso cref="XPatchLib.ITextReader" />
    public class JsonTextReader : ITextReader
    {
        private const int AttrDefLen = 4;
        private readonly ExtendJsonTextReader _reader;
        private JsonSerializeSetting _setting = new JsonSerializeSetting();

        /// <summary>
        ///     初始化 <see cref="XmlTextReader" /> 的新实例。
        /// </summary>
        protected JsonTextReader()
        {
        }

        /// <summary>
        ///     使用指定的 <see cref="TextReader" /> 初始化 <see cref="XmlTextReader" /> 的新实例。
        /// </summary>
        /// <param name="input">包含要读取的 XML 数据的 TextReader。</param>
        public JsonTextReader(TextReader input) : this()
        {
            _reader = new ExtendJsonTextReader(input, _setting);
            InitReader();
        }

        /// <summary>获取当前节点的文本值。</summary>
        /// <returns>返回当前节点的文本值。</returns>
        public string GetValue()
        {
            return _reader.ExtendValue;
        }

        /// <summary>获取当前节点的特性名称与值的键值对数组。</summary>
        /// <returns>返回当前节点的特性名称与值的键值对数组。</returns>
        public string[,] GetAttributes()
        {
            string[,] result = new string[AttrDefLen, 2];
            int index = 0;

            while (true)
            {
                //开始预读一个节点
                ExtendJsonToken token = _reader.PreRead(0);
                if (token != null && token.TokenType == JsonToken.PropertyName)
                    if (token.Value[0] == _setting.AFC)
                    {
                        //当节点是PropertyName时，继续读下一个节点，如果是String，则表示是一组键值对
                        ExtendJsonToken nextToken = _reader.PreRead(1);
                        if (nextToken != null && nextToken.TokenType == JsonToken.String)
                        {
                            int len = result.GetLength(0);
                            if (len == index + 1)
                            {
                                string[,] newResult = new string[len * 2, 2];
                                Array.Copy(result, newResult, len);
                                result = newResult;
                            }

                            result[index, 0] = token.Value.Substring(1);
                            result[index, 1] = nextToken.Value;
                            _reader.RemovePreRead(2);
                            index++;
                            continue;
                        }
                    }
                    else if (string.Equals(token.Value, _setting.SAIN))
                    {
                        //当节点是PropertyName时，继续读下一个节点，如果是 _setting.SAIN(Exp:#text)，则表示是一组键值对
                        ExtendJsonToken nextToken = _reader.PreRead(1);
                        if (nextToken != null && nextToken.TokenType == JsonToken.String)
                        {
                            _reader.ExtendValue = nextToken.Value;
                            _reader.RemovePreRead(2);
                        }
                    }

                break;
            }

            return result;
        }

        /// <summary>从流中读取下一个节点。</summary>
        /// <returns>如果成功读取了下一个节点，则为 <c>true</c>；如果没有其他节点可读取，则为 <c>false</c>。</returns>
        public bool Read()
        {
            return _reader.Read();
        }

        /// <summary>获取或设置读取器设置。</summary>
        /// <exception cref="ArgumentOutOfRangeException">设置实例的类型不是<see cref="JsonSerializeSetting" />类型时。</exception>
        public ISerializeSetting Setting
        {
            get { return _setting; }
            set
            {
                JsonSerializeSetting s = value as JsonSerializeSetting;
                if (s == null)
                    throw new ArgumentOutOfRangeException();
                _setting = s;
                _reader.setting = _setting;
                UpdateReader(_setting);
            }
        }

        private void UpdateReader(JsonSerializeSetting setting)
        {
            switch (setting.Mode)
            {
                case DateTimeSerializationMode.Local:
                    _reader.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    break;
                case DateTimeSerializationMode.Unspecified:
                    _reader.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                    break;
                case DateTimeSerializationMode.Utc:
                    _reader.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    break;
                case DateTimeSerializationMode.RoundtripKind:
                    _reader.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
                    break;
            }
            _reader.FloatParseHandling = FloatParseHandling.Decimal;
            _reader.DateParseHandling = DateParseHandling.DateTime;
        }

        /// <summary>获取读取器的状态。</summary>
        public ReadState ReadState
        {
            get { return _reader.ReadState; }
        }

        /// <summary>获取一个值，该值指示此读取器是否定位在流的结尾。</summary>
        public bool EOF
        {
            get { return _reader.ReadState == ReadState.EndOfFile; }
        }

        /// <summary>获取当前节点的限定名。</summary>
        public string Name
        {
            get { return _reader.Name; }
        }

        /// <summary>获取当前节点的类型。</summary>
        public NodeType NodeType
        {
            get { return _reader.NodeType; }
        }

        /// <summary>
        ///     初始化 _reader
        /// </summary>
        private void InitReader()
        {
            UpdateReader(_setting);
        }

        #region Dispose

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     释放 <see cref="JsonTextReader" /> 类的当前实例所使用的所有资源。
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            //the boolean flag may be used by subclasses to differentiate between disposing and finalizing
            //if (disposing && readState != ReadState.Closed)
            if (disposing)
                _reader.Close();
        }

        #endregion
    }
}