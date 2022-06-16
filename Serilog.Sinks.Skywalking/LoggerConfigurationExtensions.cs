﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;
using SkyApm.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.Skywalking
{
    public static class LoggerConfigurationExtensions
    {
        static LoggerConfigurationExtensions() { }

        public static LoggerConfiguration Skywalking(
            this LoggerSinkConfiguration loggerConfiguration, IServiceProvider serviceCollection, int batchSizeLimit, int period)
        {
            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit,
                Period = TimeSpan.FromSeconds(period)
            };

            var batchingSink = new PeriodicBatchingSink(new SkywalkingSink(serviceCollection, null), batchingOptions);

            return loggerConfiguration
                .Sink(batchingSink);
        }
    }
}
