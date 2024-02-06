using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;
using SkyApm.Tracing;
using SkyApm.Transport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using SkyApm.Tracing.Segments;
using LogEvent = Serilog.Events.LogEvent;

namespace Serilog.Sinks.Skywalking
{
    public class SkywalkingSink : ILogEventSink
    {
        ITextFormatter _formatter;

        public SkywalkingSink(IServiceProvider serviceCollection, ITextFormatter formatter)
        {
            _formatter = formatter;
            _skyApmLogDispatcher = serviceCollection.GetRequiredService<ISkyApmLogDispatcher>();
            _entrySegmentContextAccessor = serviceCollection.GetRequiredService<IEntrySegmentContextAccessor>();

        }

        ISkyApmLogDispatcher _skyApmLogDispatcher;
        IEntrySegmentContextAccessor _entrySegmentContextAccessor;

        public void Emit(LogEvent logEvent)
        {
            var logs = new Dictionary<string, object>
            {
                { "Level", logEvent.Level.ToString() }
            };
            if (_formatter != null)
            {
                using var render = new StringWriter(CultureInfo.InvariantCulture);
                _formatter.Format(logEvent, render);
                logs.Add("Message", render.ToString());
            }
            else
            {
                //等升级到2.2.0版本，就可以将这些标签，设置到Tags中 #https://github.com/SkyAPM/SkyAPM-dotnet/issues/518
                logs.Add("Message", logEvent.RenderMessage());
                foreach (var prop in logEvent.Properties)
                {
                    logs.Add($"fields.{prop.Key}", prop.Value);
                }
                if (logEvent.Exception != null)
                    logs.Add($"Exception", logEvent.Exception);
            }

            var segmentContext = _entrySegmentContextAccessor.Context;
            var logContext = new LoggerRequest
            {
                Logs = logs,
                SegmentReference = segmentContext == null
                    ? null
                    : new LoggerSegmentReference
                    {
                        TraceId = segmentContext.TraceId,
                        SegmentId = segmentContext.SegmentId
                    }
            };

            _skyApmLogDispatcher.Dispatch(logContext);
        }
    }
}
