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
using SkyApm.Common;
using SkyApm.Config;

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
            var tags = new Dictionary<string, object>
            {
                { "Level", logEvent.Level.ToString() }
            };
            var message = string.Empty;
            if (_formatter != null)
            {
                using var render = new StringWriter(CultureInfo.InvariantCulture);
                _formatter.Format(logEvent, render);
                message = render.ToString();
            }
            else
            {
                message = logEvent.RenderMessage();
                foreach (var prop in logEvent.Properties)
                {
                    tags.Add($"fields.{prop.Key}", prop.Value);
                }
                if (logEvent.Exception != null)
                {
                    message += "\r\n" + (logEvent.Exception.HasInnerExceptions() ? logEvent.Exception.ToDemystifiedString(10) : logEvent.Exception.ToString());
                }
            }

            var segmentContext = _entrySegmentContextAccessor.Context;
            var logContext = new LogRequest
            {
                Message = message ?? string.Empty,
                Tags = tags,
                SegmentReference = segmentContext == null
                    ? null
                    : new LogSegmentReference
                    {
                        TraceId = segmentContext.TraceId,
                        SegmentId = segmentContext.SegmentId
                    }
            };

            _skyApmLogDispatcher.Dispatch(logContext);
        }
    }
}
