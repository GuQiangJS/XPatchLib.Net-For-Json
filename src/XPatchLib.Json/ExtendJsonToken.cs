// Copyright © 2013-2017 - GuQiang
// Licensed under the LGPL-3.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
#if NETSTANDARD_1_0 || NETSTANDARD_1_1
using XmlConvert = XPatchLib.XmlConvert;
#else
using XmlConvert = System.Xml.XmlConvert;
using System.Xml;
#endif
using Newtonsoft.Json;

namespace XPatchLib
{
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