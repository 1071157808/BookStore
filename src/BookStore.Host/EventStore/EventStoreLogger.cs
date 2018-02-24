using System;
using EventStore.ClientAPI;
using Serilog;
using ILogger = EventStore.ClientAPI.ILogger;

namespace BookStore.Host.EventStore
{
    public class EventStoreLogger : ILogger
    {
        private readonly Serilog.ILogger _log;

        public EventStoreLogger()
        {
            _log = Log.ForContext<IEventStoreConnection>();
        }
        
        public void Error(string format, params object[] args)
        {
            _log.Error(format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            _log.Error(ex, format, args);
        }

        public void Info(string format, params object[] args)
        {
            _log.Information(format, args);
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            _log.Information(ex, format, args);
        }

        public void Debug(string format, params object[] args)
        {
            _log.Debug(format, args);
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            _log.Debug(ex, format, args);
        }
    }
}