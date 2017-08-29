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

        internal class ExtendJsonToken
        {
            internal ExtendJsonToken(ExtendJsonTextReader reader, ISerializeSetting setting)
            {
                //if(reader.ReadState!=ReadState.Interactive)
                //    throw new ArgumentOutOfRangeException(nameof(reader));

                if (reader.Value == null)
                    Value = string.Empty;
                else if (reader.Value is DateTime)
                    Value = XmlConvert.ToString((DateTime) reader.Value, Convert(setting.Mode));
                else
                    Value = reader.Value.ToString();
                Depth = reader.Depth;
                Path = reader.Path;
                TokenType = reader.TokenType;
            }

            internal string Value { get; }

            internal int Depth { get; }

            internal string Path { get; }

            internal JsonToken TokenType { get; }

            private static XmlDateTimeSerializationMode Convert(DateTimeSerializationMode pMode)
            {
                switch (pMode)
                {
                    case DateTimeSerializationMode.Local:
                        return XmlDateTimeSerializationMode.Local;
                    case DateTimeSerializationMode.RoundtripKind:
                        return XmlDateTimeSerializationMode.RoundtripKind;
                    case DateTimeSerializationMode.Unspecified:
                        return XmlDateTimeSerializationMode.Unspecified;
                    case DateTimeSerializationMode.Utc:
                        return XmlDateTimeSerializationMode.Utc;
                }
                throw new NotImplementedException();
            }
        }
    }

#if (NETSTANDARD_1_0 || NETSTANDARD_1_1)

    internal enum XmlDateTimeSerializationMode
    {
        Local = 0,
        RoundtripKind = 3,
        Unspecified = 2,
        Utc = 1
    }

    internal static class XmlConvert
    {
        internal static TimeSpan ToTimeSpan(string s)
        {
            return System.Xml.XmlConvert.ToTimeSpan(s);
        }

        internal static DateTimeOffset ToDateTimeOffset(string s)
        {
            return System.Xml.XmlConvert.ToDateTimeOffset(s);
        }

        internal static string ToString(bool value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(byte value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(char value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(decimal value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(double value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(Guid value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(short value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(int value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(long value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(sbyte value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(float value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(ushort value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(uint value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(ulong value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(TimeSpan value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }

        internal static string ToString(DateTimeOffset value)
        {
            return System.Xml.XmlConvert.ToString(value);
        }


        internal static string ToString(DateTime value, XmlDateTimeSerializationMode dateTimeOption)
        {
            switch (dateTimeOption)
            {
                case XmlDateTimeSerializationMode.Local:
                    value = SwitchToLocalTime(value);
                    break;

                case XmlDateTimeSerializationMode.Utc:
                    value = SwitchToUtcTime(value);
                    break;

                case XmlDateTimeSerializationMode.Unspecified:
                    value = new DateTime(value.Ticks, DateTimeKind.Unspecified);
                    break;

                case XmlDateTimeSerializationMode.RoundtripKind:
                    break;

                default:
                    throw new ArgumentException(
                        //TODO:
                        string.Format(
                            "The '{0}' value for the 'dateTimeOption' parameter is not an allowed value for the 'XmlDateTimeSerializationMode' enumeration.",
                            "dateTimeOption"));
            }
            XsdDateTime xsdDateTime = new XsdDateTime(value, XsdDateTimeFlags.DateTime);
            return xsdDateTime.ToString();
        }

        internal static DateTime ToDateTime(string s, XmlDateTimeSerializationMode dateTimeOption)
        {
            XsdDateTime xsdDateTime = new XsdDateTime(s, XsdDateTimeFlags.AllXsd);
            DateTime dt = xsdDateTime;

            switch (dateTimeOption)
            {
                case XmlDateTimeSerializationMode.Local:
                    dt = SwitchToLocalTime(dt);
                    break;

                case XmlDateTimeSerializationMode.Utc:
                    dt = SwitchToUtcTime(dt);
                    break;

                case XmlDateTimeSerializationMode.Unspecified:
                    dt = new DateTime(dt.Ticks, DateTimeKind.Unspecified);
                    break;

                case XmlDateTimeSerializationMode.RoundtripKind:
                    break;

                default:
                    throw new ArgumentException(
                        //TODO:
                        string.Format(
                            "The '{0}' value for the 'dateTimeOption' parameter is not an allowed value for the 'XmlDateTimeSerializationMode' enumeration.",
                            "dateTimeOption"));
            }
            return dt;
        }

        private static DateTime SwitchToLocalTime(DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Local:
                    return value;

                case DateTimeKind.Unspecified:
                    return new DateTime(value.Ticks, DateTimeKind.Local);

                case DateTimeKind.Utc:
                    return value.ToLocalTime();
            }
            return value;
        }

        private static DateTime SwitchToUtcTime(DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Utc:
                    return value;

                case DateTimeKind.Unspecified:
                    return new DateTime(value.Ticks, DateTimeKind.Utc);

                case DateTimeKind.Local:
                    return value.ToUniversalTime();
            }
            return value;
        }
    }

    internal static class Bits
    {
        private static readonly uint MASK_0101010101010101 = 0x55555555;
        private static readonly uint MASK_0011001100110011 = 0x33333333;
        private static readonly uint MASK_0000111100001111 = 0x0f0f0f0f;
        private static readonly uint MASK_0000000011111111 = 0x00ff00ff;
        private static readonly uint MASK_1111111111111111 = 0x0000ffff;

        /// <summary>
        ///     Returns the number of 1 bits in an unsigned integer.  Counts bits by divide-and-conquer method,
        ///     first computing 16 2-bit counts, then 8 4-bit counts, then 4 8-bit counts, then 2 16-bit counts,
        ///     and finally 1 32-bit count.
        /// </summary>
        internal static int Count(uint num)
        {
            num = (num & MASK_0101010101010101) + ((num >> 1) & MASK_0101010101010101);
            num = (num & MASK_0011001100110011) + ((num >> 2) & MASK_0011001100110011);
            num = (num & MASK_0000111100001111) + ((num >> 4) & MASK_0000111100001111);
            num = (num & MASK_0000000011111111) + ((num >> 8) & MASK_0000000011111111);
            num = (num & MASK_1111111111111111) + (num >> 16);

            return (int) num;
        }

        /// <summary>
        ///     Compute the 1-based position of the least sigificant bit that is set, and return it (return 0 if no bits are set).
        ///     (e.g. 0x1001100 will return 3, since the 3rd bit is set).
        /// </summary>
        internal static int LeastPosition(uint num)
        {
            if (num == 0) return 0;
            return Count(num ^ (num - 1));
        }
    }

    #region 从XsdDataTime.cs复制

    internal struct XsdDateTime
    {
        private DateTime dt;
        private uint extra;

        private const uint TypeMask = 0xFF000000;
        private const uint KindMask = 0x00FF0000;
        private const uint ZoneHourMask = 0x0000FF00;
        private const uint ZoneMinuteMask = 0x000000FF;
        private const int TypeShift = 24;
        private const int KindShift = 16;
        private const int ZoneHourShift = 8;


        private static readonly int Lzyyyy = "yyyy".Length;
        private static readonly int Lzyyyy_ = "yyyy-".Length;
        private static readonly int Lzyyyy_MM = "yyyy-MM".Length;
        private static readonly int Lzyyyy_MM_ = "yyyy-MM-".Length;
        private static readonly int Lzyyyy_MM_dd = "yyyy-MM-dd".Length;
        private static readonly int Lzyyyy_MM_ddT = "yyyy-MM-ddT".Length;
        private static readonly int LzHH = "HH".Length;
        private static readonly int LzHH_ = "HH:".Length;
        private static readonly int LzHH_mm = "HH:mm".Length;
        private static readonly int LzHH_mm_ = "HH:mm:".Length;
        private static readonly int LzHH_mm_ss = "HH:mm:ss".Length;
        private static readonly int Lz_ = "-".Length;
        private static readonly int Lz_zz = "-zz".Length;
        private static readonly int Lz_zz_ = "-zz:".Length;
        private static readonly int Lz_zz_zz = "-zz:zz".Length;
        private static readonly int Lz__ = "--".Length;
        private static readonly int Lz__mm = "--MM".Length;
        private static readonly int Lz__mm_ = "--MM-".Length;
        private static readonly int Lz__mm__ = "--MM--".Length;
        private static readonly int Lz__mm_dd = "--MM-dd".Length;
        private static readonly int Lz___ = "---".Length;
        private static readonly int Lz___dd = "---dd".Length;

        // Maximum number of fraction digits;
        private const short maxFractionDigits = 7;

        public XsdDateTime(string text, XsdDateTimeFlags kinds) : this()
        {
            Parser parser = new Parser();
            if (!parser.Parse(text, kinds))
                //TODO:
                throw new FormatException(string.Format("The string '{0}' is not a valid {1} value.", text, kinds));
            InitiateXsdDateTime(parser);
        }

        private void InitiateXsdDateTime(Parser parser)
        {
            dt = new DateTime(parser.year, parser.month, parser.day, parser.hour, parser.minute, parser.second);
            if (parser.fraction != 0)
                dt = dt.AddTicks(parser.fraction);
            extra = (uint) (((int) parser.typeCode << TypeShift) | ((int) parser.kind << KindShift) |
                            (parser.zoneHour << ZoneHourShift) | parser.zoneMinute);
        }

        public XsdDateTime(DateTime dateTime, XsdDateTimeFlags kinds)
        {
            dt = dateTime;

            DateTimeTypeCode code = (DateTimeTypeCode) (Bits.LeastPosition((uint) kinds) - 1);
            int zoneHour = 0;
            int zoneMinute = 0;
            XsdDateTimeKind kind;

            switch (dateTime.Kind)
            {
                case DateTimeKind.Unspecified:
                    kind = XsdDateTimeKind.Unspecified;
                    break;
                case DateTimeKind.Utc:
                    kind = XsdDateTimeKind.Zulu;
                    break;

                default:
                {
                    TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(dateTime);

                    if (utcOffset.Ticks < 0)
                    {
                        kind = XsdDateTimeKind.LocalWestOfZulu;
                        zoneHour = -utcOffset.Hours;
                        zoneMinute = -utcOffset.Minutes;
                    }
                    else
                    {
                        kind = XsdDateTimeKind.LocalEastOfZulu;
                        zoneHour = utcOffset.Hours;
                        zoneMinute = utcOffset.Minutes;
                    }
                    break;
                }
            }

            extra = (uint) (((int) code << TypeShift) | ((int) kind << KindShift) | (zoneHour << ZoneHourShift) |
                            zoneMinute);
        }

        private DateTimeTypeCode InternalTypeCode
        {
            get { return (DateTimeTypeCode) ((extra & TypeMask) >> TypeShift); }
        }

        private XsdDateTimeKind InternalKind
        {
            get { return (XsdDateTimeKind) ((extra & KindMask) >> KindShift); }
        }

        public static implicit operator DateTime(XsdDateTime xdt)
        {
            DateTime result;
            switch (xdt.InternalTypeCode)
            {
                case DateTimeTypeCode.GMonth:
                case DateTimeTypeCode.GDay:
                    result = new DateTime(DateTime.Now.Year, xdt.Month, xdt.Day);
                    break;
                case DateTimeTypeCode.Time:
                    //back to DateTime.Now 
                    DateTime currentDateTime = DateTime.Now;
                    TimeSpan addDiff = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day) -
                                       new DateTime(xdt.Year, xdt.Month, xdt.Day);
                    result = xdt.dt.Add(addDiff);
                    break;
                default:
                    result = xdt.dt;
                    break;
            }

            long ticks;
            switch (xdt.InternalKind)
            {
                case XsdDateTimeKind.Zulu:
                    // set it to UTC
                    result = new DateTime(result.Ticks, DateTimeKind.Utc);
                    break;
                case XsdDateTimeKind.LocalEastOfZulu:
                    // Adjust to UTC and then convert to local in the current time zone
                    ticks = result.Ticks - new TimeSpan(xdt.ZoneHour, xdt.ZoneMinute, 0).Ticks;
                    if (ticks < DateTime.MinValue.Ticks)
                    {
                        // Underflow. Return the DateTime as local time directly
                        ticks += TimeZoneInfo.Local.GetUtcOffset(result).Ticks;
                        if (ticks < DateTime.MinValue.Ticks)
                            ticks = DateTime.MinValue.Ticks;
                        return new DateTime(ticks, DateTimeKind.Local);
                    }
                    result = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
                    break;
                case XsdDateTimeKind.LocalWestOfZulu:
                    // Adjust to UTC and then convert to local in the current time zone
                    ticks = result.Ticks + new TimeSpan(xdt.ZoneHour, xdt.ZoneMinute, 0).Ticks;
                    if (ticks > DateTime.MaxValue.Ticks)
                    {
                        // Overflow. Return the DateTime as local time directly
                        ticks += TimeZoneInfo.Local.GetUtcOffset(result).Ticks;
                        if (ticks > DateTime.MaxValue.Ticks)
                            ticks = DateTime.MaxValue.Ticks;
                        return new DateTime(ticks, DateTimeKind.Local);
                    }
                    result = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
                    break;
                default:
                    break;
            }
            return result;
        }


        // Serialize year, month and day
        private void PrintDate(StringBuilder sb)
        {
            char[] text = new char[Lzyyyy_MM_dd];
            IntToCharArray(text, 0, Year, 4);
            text[Lzyyyy] = '-';
            ShortToCharArray(text, Lzyyyy_, Month);
            text[Lzyyyy_MM] = '-';
            ShortToCharArray(text, Lzyyyy_MM_, Day);
            sb.Append(text);
        }

        // Serialize hour, minute, second and fraction
        private void PrintTime(StringBuilder sb)
        {
            char[] text = new char[LzHH_mm_ss];
            ShortToCharArray(text, 0, Hour);
            text[LzHH] = ':';
            ShortToCharArray(text, LzHH_, Minute);
            text[LzHH_mm] = ':';
            ShortToCharArray(text, LzHH_mm_, Second);
            sb.Append(text);
            int fraction = Fraction;
            if (fraction != 0)
            {
                int fractionDigits = maxFractionDigits;
                while (fraction % 10 == 0)
                {
                    fractionDigits--;
                    fraction /= 10;
                }
                text = new char[fractionDigits + 1];
                text[0] = '.';
                IntToCharArray(text, 1, fraction, fractionDigits);
                sb.Append(text);
            }
        }

        // Serialize time zone
        private void PrintZone(StringBuilder sb)
        {
            char[] text;
            switch (InternalKind)
            {
                case XsdDateTimeKind.Zulu:
                    sb.Append('Z');
                    break;
                case XsdDateTimeKind.LocalWestOfZulu:
                    text = new char[Lz_zz_zz];
                    text[0] = '-';
                    ShortToCharArray(text, Lz_, ZoneHour);
                    text[Lz_zz] = ':';
                    ShortToCharArray(text, Lz_zz_, ZoneMinute);
                    sb.Append(text);
                    break;
                case XsdDateTimeKind.LocalEastOfZulu:
                    text = new char[Lz_zz_zz];
                    text[0] = '+';
                    ShortToCharArray(text, Lz_, ZoneHour);
                    text[Lz_zz] = ':';
                    ShortToCharArray(text, Lz_zz_, ZoneMinute);
                    sb.Append(text);
                    break;
                default:
                    // do nothing
                    break;
            }
        }

        /// <summary>
        ///     Serialization to a string
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(64);
            char[] text;
            switch (InternalTypeCode)
            {
                case DateTimeTypeCode.DateTime:
                    PrintDate(sb);
                    sb.Append('T');
                    PrintTime(sb);
                    break;
                case DateTimeTypeCode.Time:
                    PrintTime(sb);
                    break;
                case DateTimeTypeCode.Date:
                    PrintDate(sb);
                    break;
                case DateTimeTypeCode.GYearMonth:
                    text = new char[Lzyyyy_MM];
                    IntToCharArray(text, 0, Year, 4);
                    text[Lzyyyy] = '-';
                    ShortToCharArray(text, Lzyyyy_, Month);
                    sb.Append(text);
                    break;
                case DateTimeTypeCode.GYear:
                    text = new char[Lzyyyy];
                    IntToCharArray(text, 0, Year, 4);
                    sb.Append(text);
                    break;
                case DateTimeTypeCode.GMonthDay:
                    text = new char[Lz__mm_dd];
                    text[0] = '-';
                    text[Lz_] = '-';
                    ShortToCharArray(text, Lz__, Month);
                    text[Lz__mm] = '-';
                    ShortToCharArray(text, Lz__mm_, Day);
                    sb.Append(text);
                    break;
                case DateTimeTypeCode.GDay:
                    text = new char[Lz___dd];
                    text[0] = '-';
                    text[Lz_] = '-';
                    text[Lz__] = '-';
                    ShortToCharArray(text, Lz___, Day);
                    sb.Append(text);
                    break;
                case DateTimeTypeCode.GMonth:
                    text = new char[Lz__mm__];
                    text[0] = '-';
                    text[Lz_] = '-';
                    ShortToCharArray(text, Lz__, Month);
                    text[Lz__mm] = '-';
                    text[Lz__mm_] = '-';
                    sb.Append(text);
                    break;
            }
            PrintZone(sb);
            return sb.ToString();
        }

        // Serialize integer into character array starting with index [start]. 
        // Number of digits is set by [digits]
        private void IntToCharArray(char[] text, int start, int value, int digits)
        {
            while (digits-- != 0)
            {
                text[start + digits] = (char) (value % 10 + '0');
                value /= 10;
            }
        }

        // Serialize two digit integer into character array starting with index [start].
        private void ShortToCharArray(char[] text, int start, int value)
        {
            text[start] = (char) (value / 10 + '0');
            text[start + 1] = (char) (value % 10 + '0');
        }

        /// <summary>
        ///     Returns the year part of XsdDateTime
        ///     The returned value is integer between 1 and 9999
        /// </summary>
        public int Year
        {
            get { return dt.Year; }
        }

        /// <summary>
        ///     Returns the month part of XsdDateTime
        ///     The returned value is integer between 1 and 12
        /// </summary>
        public int Month
        {
            get { return dt.Month; }
        }

        /// <summary>
        ///     Returns the day of the month part of XsdDateTime
        ///     The returned value is integer between 1 and 31
        /// </summary>
        public int Day
        {
            get { return dt.Day; }
        }

        /// <summary>
        ///     Returns the hour part of XsdDateTime
        ///     The returned value is integer between 0 and 23
        /// </summary>
        public int Hour
        {
            get { return dt.Hour; }
        }

        /// <summary>
        ///     Returns the minute part of XsdDateTime
        ///     The returned value is integer between 0 and 60
        /// </summary>
        public int Minute
        {
            get { return dt.Minute; }
        }

        /// <summary>
        ///     Returns the second part of XsdDateTime
        ///     The returned value is integer between 0 and 60
        /// </summary>
        public int Second
        {
            get { return dt.Second; }
        }

        /// <summary>
        ///     Returns number of ticks in the fraction of the second
        ///     The returned value is integer between 0 and 9999999
        /// </summary>
        public int Fraction
        {
            get
            {
                return (int) (dt.Ticks - new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second).Ticks);
            }
        }

        /// <summary>
        ///     Returns the hour part of the time zone
        ///     The returned value is integer between -13 and 13
        /// </summary>
        public int ZoneHour
        {
            get
            {
                uint result = (extra & ZoneHourMask) >> ZoneHourShift;
                return (int) result;
            }
        }

        /// <summary>
        ///     Returns the minute part of the time zone
        ///     The returned value is integer between 0 and 60
        /// </summary>
        public int ZoneMinute
        {
            get
            {
                uint result = extra & ZoneMinuteMask;
                return (int) result;
            }
        }

        private enum DateTimeTypeCode
        {
            DateTime,
            Time,
            Date,
            GYearMonth,
            GYear,
            GMonthDay,
            GDay,
            GMonth,
#if !SILVERLIGHT // XDR is not supported in Silverlight
            XdrDateTime,
#endif
        }

        // Internal representation of DateTimeKind
        private enum XsdDateTimeKind
        {
            Unspecified,
            Zulu,
            LocalWestOfZulu, // GMT-1..14, N..Y
            LocalEastOfZulu // GMT+1..14, A..M
        }


        private struct Parser
        {
            public DateTimeTypeCode typeCode;
            public int year;
            public int month;
            public int day;
            public int hour;
            public int minute;
            public int second;
            public int fraction;
            public XsdDateTimeKind kind;
            public int zoneHour;
            public int zoneMinute;

            private string text;
            private int length;


            private const int leapYear = 1904;
            private const int firstMonth = 1;
            private const int firstDay = 1;

            private static bool Test(XsdDateTimeFlags left, XsdDateTimeFlags right)
            {
                return (left & right) != 0;
            }


            private bool ParseChar(int start, char ch)
            {
                return start < length && text[start] == ch;
            }

            private bool Parse4Dig(int start, ref int num)
            {
                if (start + 3 < length)
                {
                    int d4 = text[start] - '0';
                    int d3 = text[start + 1] - '0';
                    int d2 = text[start + 2] - '0';
                    int d1 = text[start + 3] - '0';
                    if (0 <= d4 && d4 < 10 &&
                        0 <= d3 && d3 < 10 &&
                        0 <= d2 && d2 < 10 &&
                        0 <= d1 && d1 < 10
                    )
                    {
                        num = ((d4 * 10 + d3) * 10 + d2) * 10 + d1;
                        return true;
                    }
                }
                return false;
            }

            private bool Parse2Dig(int start, ref int num)
            {
                if (start + 1 < length)
                {
                    int d2 = text[start] - '0';
                    int d1 = text[start + 1] - '0';
                    if (0 <= d2 && d2 < 10 &&
                        0 <= d1 && d1 < 10
                    )
                    {
                        num = d2 * 10 + d1;
                        return true;
                    }
                }
                return false;
            }

            private bool ParseDate(int start)
            {
                return
                    Parse4Dig(start, ref year) && 1 <= year &&
                    ParseChar(start + Lzyyyy, '-') &&
                    Parse2Dig(start + Lzyyyy_, ref month) && 1 <= month && month <= 12 &&
                    ParseChar(start + Lzyyyy_MM, '-') &&
                    Parse2Dig(start + Lzyyyy_MM_, ref day) && 1 <= day && day <= DateTime.DaysInMonth(year, month);
            }

            private bool ParseTimeAndZoneAndWhitespace(int start)
            {
                if (ParseTime(ref start))
                    if (ParseZoneAndWhitespace(start))
                        return true;
                return false;
            }

            private bool ParseTime(ref int start)
            {
                if (
                    Parse2Dig(start, ref hour) && hour < 24 &&
                    ParseChar(start + LzHH, ':') &&
                    Parse2Dig(start + LzHH_, ref minute) && minute < 60 &&
                    ParseChar(start + LzHH_mm, ':') &&
                    Parse2Dig(start + LzHH_mm_, ref second) && second < 60
                )
                {
                    start += LzHH_mm_ss;
                    if (ParseChar(start, '.'))
                    {
                        // Parse factional part of seconds
                        // We allow any number of digits, but keep only first 7
                        fraction = 0;
                        int fractionDigits = 0;
                        int round = 0;
                        while (++start < length)
                        {
                            int d = text[start] - '0';
                            if (9u < (uint) d)
                                break;
                            if (fractionDigits < maxFractionDigits)
                            {
                                fraction = fraction * 10 + d;
                            }
                            else if (fractionDigits == maxFractionDigits)
                            {
                                if (5 < d)
                                    round = 1;
                                else if (d == 5)
                                    round = -1;
                            }
                            else if (round < 0 && d != 0)
                            {
                                round = 1;
                            }
                            fractionDigits++;
                        }
                        if (fractionDigits < maxFractionDigits)
                        {
                            if (fractionDigits == 0)
                                return false; // cannot end with .
                            fraction *= Power10[maxFractionDigits - fractionDigits];
                        }
                        else
                        {
                            if (round < 0)
                                round = fraction & 1;
                            fraction += round;
                        }
                    }
                    return true;
                }
                // cleanup - conflict with gYear
                hour = 0;
                return false;
            }

            private static readonly int[] Power10 = new int[maxFractionDigits]
                {-1, 10, 100, 1000, 10000, 100000, 1000000};

            private bool ParseZoneAndWhitespace(int start)
            {
                if (start < length)
                {
                    char ch = text[start];
                    if (ch == 'Z' || ch == 'z')
                    {
                        kind = XsdDateTimeKind.Zulu;
                        start++;
                    }
                    else if (start + 5 < length)
                    {
                        if (
                            Parse2Dig(start + Lz_, ref zoneHour) && zoneHour <= 99 &&
                            ParseChar(start + Lz_zz, ':') &&
                            Parse2Dig(start + Lz_zz_, ref zoneMinute) && zoneMinute <= 99
                        )
                            if (ch == '-')
                            {
                                kind = XsdDateTimeKind.LocalWestOfZulu;
                                start += Lz_zz_zz;
                            }
                            else if (ch == '+')
                            {
                                kind = XsdDateTimeKind.LocalEastOfZulu;
                                start += Lz_zz_zz;
                            }
                    }
                }
                while (start < length && char.IsWhiteSpace(text[start]))
                    start++;
                return start == length;
            }

            private bool ParseTimeAndWhitespace(int start)
            {
                if (ParseTime(ref start))
                {
                    while (start < length)
                        //&& char.IsWhiteSpace(text[start])) {
                        start++;
                    return start == length;
                }
                return false;
            }

            public bool Parse(string text, XsdDateTimeFlags kinds)
            {
                this.text = text;
                length = text.Length;

                // Skip leading withitespace
                int start = 0;
                while (start < length && char.IsWhiteSpace(text[start]))
                    start++;
                // Choose format starting from the most common and trying not to reparse the same thing too many times

#if !SILVERLIGHT // XDR is not supported in Silverlight
                if (Test(kinds,
                    XsdDateTimeFlags.DateTime | XsdDateTimeFlags.Date | XsdDateTimeFlags.XdrDateTime |
                    XsdDateTimeFlags.XdrDateTimeNoTz))
                {
#else
                if (Test(kinds, XsdDateTimeFlags.DateTime | XsdDateTimeFlags.Date)) {
#endif
                    if (ParseDate(start))
                    {
                        if (Test(kinds, XsdDateTimeFlags.DateTime))
                            if (ParseChar(start + Lzyyyy_MM_dd, 'T') &&
                                ParseTimeAndZoneAndWhitespace(start + Lzyyyy_MM_ddT))
                            {
                                typeCode = DateTimeTypeCode.DateTime;
                                return true;
                            }
                        if (Test(kinds, XsdDateTimeFlags.Date))
                            if (ParseZoneAndWhitespace(start + Lzyyyy_MM_dd))
                            {
                                typeCode = DateTimeTypeCode.Date;
                                return true;
                            }
#if !SILVERLIGHT // XDR is not supported in Silverlight
                        if (Test(kinds, XsdDateTimeFlags.XdrDateTime))
                            if (ParseZoneAndWhitespace(start + Lzyyyy_MM_dd) || ParseChar(start + Lzyyyy_MM_dd, 'T') &&
                                ParseTimeAndZoneAndWhitespace(start + Lzyyyy_MM_ddT))
                            {
                                typeCode = DateTimeTypeCode.XdrDateTime;
                                return true;
                            }
                        if (Test(kinds, XsdDateTimeFlags.XdrDateTimeNoTz))
                            if (ParseChar(start + Lzyyyy_MM_dd, 'T'))
                            {
                                if (ParseTimeAndWhitespace(start + Lzyyyy_MM_ddT))
                                {
                                    typeCode = DateTimeTypeCode.XdrDateTime;
                                    return true;
                                }
                            }
                            else
                            {
                                typeCode = DateTimeTypeCode.XdrDateTime;
                                return true;
                            }
#endif
                    }
                }

                if (Test(kinds, XsdDateTimeFlags.Time))
                    if (ParseTimeAndZoneAndWhitespace(start))
                    {
                        //Equivalent to NoCurrentDateDefault on DateTimeStyles while parsing xs:time
                        year = leapYear;
                        month = firstMonth;
                        day = firstDay;
                        typeCode = DateTimeTypeCode.Time;
                        return true;
                    }

#if !SILVERLIGHT // XDR is not supported in Silverlight
                if (Test(kinds, XsdDateTimeFlags.XdrTimeNoTz))
                    if (ParseTimeAndWhitespace(start))
                    {
                        //Equivalent to NoCurrentDateDefault on DateTimeStyles while parsing xs:time
                        year = leapYear;
                        month = firstMonth;
                        day = firstDay;
                        typeCode = DateTimeTypeCode.Time;
                        return true;
                    }
#endif

                if (Test(kinds, XsdDateTimeFlags.GYearMonth | XsdDateTimeFlags.GYear))
                    if (Parse4Dig(start, ref year) && 1 <= year)
                    {
                        if (Test(kinds, XsdDateTimeFlags.GYearMonth))
                            if (
                                ParseChar(start + Lzyyyy, '-') &&
                                Parse2Dig(start + Lzyyyy_, ref month) && 1 <= month && month <= 12 &&
                                ParseZoneAndWhitespace(start + Lzyyyy_MM)
                            )
                            {
                                day = firstDay;
                                typeCode = DateTimeTypeCode.GYearMonth;
                                return true;
                            }
                        if (Test(kinds, XsdDateTimeFlags.GYear))
                            if (ParseZoneAndWhitespace(start + Lzyyyy))
                            {
                                month = firstMonth;
                                day = firstDay;
                                typeCode = DateTimeTypeCode.GYear;
                                return true;
                            }
                    }
                if (Test(kinds, XsdDateTimeFlags.GMonthDay | XsdDateTimeFlags.GMonth))
                    if (
                        ParseChar(start, '-') &&
                        ParseChar(start + Lz_, '-') &&
                        Parse2Dig(start + Lz__, ref month) && 1 <= month && month <= 12
                    )
                    {
                        if (Test(kinds, XsdDateTimeFlags.GMonthDay) && ParseChar(start + Lz__mm, '-'))
                            if (
                                Parse2Dig(start + Lz__mm_, ref day) && 1 <= day &&
                                day <= DateTime.DaysInMonth(leapYear, month) &&
                                ParseZoneAndWhitespace(start + Lz__mm_dd)
                            )
                            {
                                year = leapYear;
                                typeCode = DateTimeTypeCode.GMonthDay;
                                return true;
                            }
                        if (Test(kinds, XsdDateTimeFlags.GMonth))
                            if (ParseZoneAndWhitespace(start + Lz__mm) || ParseChar(start + Lz__mm, '-') &&
                                ParseChar(start + Lz__mm_, '-') && ParseZoneAndWhitespace(start + Lz__mm__))
                            {
                                year = leapYear;
                                day = firstDay;
                                typeCode = DateTimeTypeCode.GMonth;
                                return true;
                            }
                    }
                if (Test(kinds, XsdDateTimeFlags.GDay))
                    if (
                        ParseChar(start, '-') &&
                        ParseChar(start + Lz_, '-') &&
                        ParseChar(start + Lz__, '-') &&
                        Parse2Dig(start + Lz___, ref day) && 1 <= day &&
                        day <= DateTime.DaysInMonth(leapYear, firstMonth) &&
                        ParseZoneAndWhitespace(start + Lz___dd)
                    )
                    {
                        year = leapYear;
                        month = firstMonth;
                        typeCode = DateTimeTypeCode.GDay;
                        return true;
                    }
                return false;
            }
        }
    }

    [Flags]
    internal enum XsdDateTimeFlags
    {
        DateTime = 0x01,
        Time = 0x02,
        Date = 0x04,
        GYearMonth = 0x08,
        GYear = 0x10,
        GMonthDay = 0x20,
        GDay = 0x40,
        GMonth = 0x80,
#if !SILVERLIGHT // XDR is not supported in Silverlight
        XdrDateTimeNoTz = 0x100,
        XdrDateTime = 0x200,
        XdrTimeNoTz = 0x400, //XDRTime with tz is the same as xsd:time  
#endif
        AllXsd = 0xFF //All still does not include the XDR formats
    }

    #endregion 从XsdDataTime.cs复制

#endif
}