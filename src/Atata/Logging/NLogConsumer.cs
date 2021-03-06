﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atata
{
    public class NLogConsumer : ILogConsumer
    {
        private readonly Type logEventInfoType;
        private readonly Dictionary<LogLevel, dynamic> logLevelsMap = new Dictionary<LogLevel, dynamic>();

        private string loggerName;
        private dynamic logger;

        public NLogConsumer()
            : this(null)
        {
        }

        public NLogConsumer(string loggerName)
        {
            LoggerName = loggerName;

            logEventInfoType = Type.GetType("NLog.LogEventInfo,NLog", true);

            InitLogLevelsMap();
        }

        public string LoggerName
        {
            get
            {
                return loggerName;
            }
            set
            {
                loggerName = value;
                InitLogger(value);
            }
        }

        private void InitLogLevelsMap()
        {
            Type logLevelType = Type.GetType("NLog.LogLevel,NLog", true);

            PropertyInfo allLevelsProperty = logLevelType.GetPropertyWithThrowOnError("AllLoggingLevels");
            IEnumerable allLevels = (IEnumerable)allLevelsProperty.GetStaticValue();

            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {
                logLevelsMap[level] = allLevels.
                    Cast<dynamic>().
                    First(x => x.Name == Enum.GetName(typeof(LogLevel), level));
            }
        }

        private void InitLogger(string loggerName)
        {
            Type logManagerType = Type.GetType("NLog.LogManager,NLog", true);

            logger = loggerName != null
                ? logManagerType.GetMethodWithThrowOnError("GetLogger", typeof(string)).InvokeStatic(loggerName)
                : logManagerType.GetMethodWithThrowOnError("GetCurrentClassLogger").InvokeStatic();

            if (logger == null)
                throw new InvalidOperationException("Failed to create NLog logger.");
        }

        public void Log(LogEventInfo eventInfo)
        {
            dynamic otherEventInfo = ActivatorEx.CreateInstance(logEventInfoType);

            otherEventInfo.TimeStamp = eventInfo.Timestamp;
            otherEventInfo.Level = logLevelsMap[eventInfo.Level];
            otherEventInfo.Message = eventInfo.Message;
            otherEventInfo.Exception = eventInfo.Exception;

            var properties = (IDictionary<object, object>)otherEventInfo.Properties;

            properties["build-start"] = eventInfo.BuildStart;
            properties["test-name"] = eventInfo.TestName;
            properties["test-start"] = eventInfo.TestStart;
            properties["driver-alias"] = eventInfo.DriverAlias;

            logger.Log(otherEventInfo);
        }
    }
}
