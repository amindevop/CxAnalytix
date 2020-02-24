﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;


[assembly: InternalsVisibleTo("CxRestClient_Tests")]
namespace CxRestClient
{
    internal class JsonUtils
    {
        public static bool MoveToNextProperty(JTokenReader reader)
        {
            while (reader.Read())
            {
                if (reader.CurrentToken.Type == JTokenType.Property)
                    return true;
            }

            return false;
        }

        public static bool MoveToNextProperty(JTokenReader reader, String named)
        {
            while (MoveToNextProperty(reader))
            {
                if (((JProperty)reader.CurrentToken).Name.CompareTo(named) == 0)
                    return true;
            }

            return false;
        }


        public static DateTime LocalEpochTimeToDateTime (long epochTime)
        {
            return EpochTimeToDateTime(epochTime, DateTimeKind.Local);
        }

        public static DateTime LocalEpochTimeToDateTime(JToken token)
        {
            return LocalEpochTimeToDateTime(token.Value<long>());
        }

        public static DateTime EpochTimeToDateTime (long epochTime, DateTimeKind valueKind)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, valueKind);
            return epoch.AddSeconds(epochTime);
        }

        public static DateTime UtcEpochTimeToDateTime(long epochTime)
        {
            return EpochTimeToDateTime(epochTime, DateTimeKind.Utc);
        }

        public static DateTime UtcEpochTimeToDateTime(JToken token)
        {
            return UtcEpochTimeToDateTime(token.Value<long>());
        }

    }
}
