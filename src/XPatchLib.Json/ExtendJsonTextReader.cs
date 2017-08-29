// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
#if NETSTANDARD_1_0 || NETSTANDARD_1_1
using XmlConvert=XPatchLib.XmlConvert;
#else
using XmlConvert = System.Xml.XmlConvert;
#endif
using Newtonsoft.Json;

namespace XPatchLib
{
    internal class ExtendJsonTextReader : Newtonsoft.Json.JsonTextReader
    {
        /// <summary>
        ///     Path/Value字典。用来记录不同层次节点的名称。
        /// </summary>
        private readonly Dictionary<string, string> _nameDic = new Dictionary<string, string>();

        /// <summary>
        ///     预读队列
        /// </summary>
        private readonly Queue<ExtendJsonToken> _nodeQueue = new Queue<ExtendJsonToken>();

        public JsonSerializeSetting setting;

        public string ExtendValue;

        public string Name;

        public ExtendJsonTextReader(TextReader input, JsonSerializeSetting setting) : base(input)
        {
            this.setting = setting;
        }

        public NodeType NodeType
        {
            get
            {
                switch (TokenType)
                {
                    case JsonToken.StartObject:
                    case JsonToken.StartArray:
                    case JsonToken.PropertyName:
                    case JsonToken.String:
                    case JsonToken.Boolean:
                    case JsonToken.Bytes:
                    case JsonToken.Date:
                    case JsonToken.Float:
                    case JsonToken.Integer:
                        return NodeType.Element;
                    case JsonToken.EndObject:
                    case JsonToken.EndArray:
                        return NodeType.EndElement;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public ReadState ReadState
        {
            get
            {
                switch (CurrentState)
                {
                    case State.Start:
                        return ReadState.Initial;
                    case State.Finished:
                        return ReadState.EndOfFile;
                    case State.Closed:
                        return ReadState.Closed;
                    case State.Error:
                        return ReadState.Error;
                    default:
                        return ReadState.Interactive;
                }
            }
        }

        /// <summary>
        ///     根据当前_reader.Depth设置_name属性。
        /// </summary>
        private void SetNameByDepth(ExtendJsonToken token)
        {
            SetName(_nameDic[token.Path]);
            if (string.IsNullOrEmpty(Name))
                throw new NotImplementedException();
        }

        private void SetName(string name)
        {
            Name = Equals(name, setting.SAIN) ? typeof(String).Name : name;
        }

        /// <summary>
        ///     按照Depth属性记录每一层的名称
        /// </summary>
        private void RecordName(ExtendJsonToken token)
        {
            bool needRecord = false;
            string name = string.Empty;
            if (!_nameDic.ContainsKey(token.Path))
                if (token.TokenType == JsonToken.StartObject || token.TokenType == JsonToken.StartArray)
                {
                    if (Value == null)
                        needRecord = true;
                    if (token.TokenType == JsonToken.StartObject && token.Path.IndexOf('[') >= 0)
                        name = _nameDic[token.Path.Substring(0, token.Path.IndexOf('['))];
                }
                //当当前读取到的节点时PropertyName时，按照当前节点的Value值记录
                else if (token.TokenType == JsonToken.PropertyName)
                {
                    name = token.Value;
                    needRecord = true;
                }
            if (needRecord)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("name");
                _nameDic.Add(token.Path, name);
            }
        }

        private void ResetName()
        {
            Name = null;
        }

        private void ResetValue()
        {
            ExtendValue = null;
        }

        public bool Read()
        {
            if (CurrentState == State.Start)
                base.Read();

            ResetName();
            ResetValue();

            ExtendJsonToken token = ReadCore();

            if (token == null) return false;
            //如果当前读取到的节点是PropertyName，说明有对应的Value，需要再读一次
            if (token.TokenType == JsonToken.PropertyName)
            {
                SetName(token.Value);
                ExtendJsonToken nextToken = ReadCore();
                if (nextToken == null) return false;
                if (nextToken.TokenType == JsonToken.StartArray)
                    ReadCore();
                token = nextToken;
            }
            else if (token.TokenType == JsonToken.StartObject)
            {
                //如果是ObjectStart，那么需要预读3个节点，看是否为单独节点
                //Exp:{"String":"Value"}

                ExtendJsonToken[] tokens = new ExtendJsonToken[3];
                for (int i = 0; i < 3; i++)
                    tokens[i] = PreRead(i);
                if (tokens[0].TokenType == JsonToken.PropertyName && (tokens[1].TokenType == JsonToken.String
                                                                      || tokens[1].TokenType == JsonToken.Boolean
                                                                      || tokens[1].TokenType == JsonToken.Bytes
                                                                      || tokens[1].TokenType == JsonToken.Date
                                                                      || tokens[1].TokenType == JsonToken.Float
                                                                      || tokens[1].TokenType == JsonToken.Integer) &&
                    tokens[2].TokenType == JsonToken.EndObject)
                {
                    SetName(tokens[0].Value);
                    SetValue(tokens[1]);
                    RemovePreRead(3);
                    return true;
                }
                //设置名称
                SetNameByDepth(token);
            }
            else if (token.TokenType == JsonToken.EndArray)
            {
                //设置名称
                SetNameByDepth(token);
            }
            else
            {
                //设置名称
                SetNameByDepth(token);
            }
            //读取完成后设置值
            SetValue(token);

            return true;
        }

        private void SetValue(ExtendJsonToken token)
        {
            if (token.TokenType == JsonToken.String
                || token.TokenType == JsonToken.Boolean
                || token.TokenType == JsonToken.Bytes
                || token.TokenType == JsonToken.Date
                || token.TokenType == JsonToken.Float
                || token.TokenType == JsonToken.Integer)
                ExtendValue = token.Value;
            else
                ExtendValue = string.Empty;
        }

        private ExtendJsonToken ReadCore()
        {
            if (_nodeQueue.Count > 0)
                return _nodeQueue.Dequeue();
            if (base.Read())
            {
                if (ReadState == ReadState.EndOfFile)
                    return null;
                ExtendJsonToken result = new ExtendJsonToken(this, setting);
                if (result != null)
                    RecordName(result);
                return result;
            }
            return null;
        }

        /// <summary>
        ///     移除预读数据
        /// </summary>
        /// <param name="count"></param>
        public void RemovePreRead(int count)
        {
            for (int i = 0; i < count; i++)
                _nodeQueue.Dequeue();
        }

        /// <summary>
        ///     预先读取
        /// </summary>
        /// <param name="index">从预读取序列中读取第几号元素（从0开始）</param>
        /// <param name="skipQueue">是否跳过现有缓存，继续往下读取</param>
        /// <returns>返回读取到的<see cref="ExtendJsonToken" />实例。</returns>
        public ExtendJsonToken PreRead(int index = 0, bool skipQueue = false)
        {
            if (_nodeQueue.Count > 0)
                if (index == 0)
                    return _nodeQueue.Peek();
                else if (index < _nodeQueue.Count)
                    return _nodeQueue.ToArray()[index];
            if (base.Read())
            {
                ExtendJsonToken result = new ExtendJsonToken(this, setting);
                _nodeQueue.Enqueue(result);
                if (result != null)
                    RecordName(result);
                return result;
            }
            return null;
        }

    }

}