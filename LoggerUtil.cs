﻿using Serilog;
using Serilog.Context;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR
{
    public static class LoggerUtil
    {
        private static readonly Util util = new Util();
        private static readonly ILogger _log = LoggerService.Instance._log;
        public static void LogError(Exception ex, string message, params object[] properties)
        {
            if (_log == null)
            {
                return;
            }

            if (ex is System.Net.Sockets.SocketException socketEx)
            {
                if (socketEx.ErrorCode == 10048 || socketEx.ErrorCode == 10060 || socketEx.ErrorCode == 10049)
                {
                    // Optionally, you can log this event in a different way or simply ignore it
                    // For example:
                    _log.Error($"Ignored SocketException(Bluetooth Connection) ({socketEx.ErrorCode}): {ex}", message, properties);
                    return;
                }
                
            }

            _log.Error(ex, message, properties);
            util.sendLogTelegramBy(ex, message, properties);
        }
        public static void LogPrivateMethod(string methodName)
        {
            if (_log == null)
            {
                return;
            }
            _log.Information(methodName);
        }

        public static void LogWarning(string message)
        {
            if (_log == null)
            {
                return;
            }
            _log.Warning(message);
        }

        public static void LogNetwork(string message)
        {
            if (_log == null)
            {
                return;
            }
            _log.Warning(message);
            util.sendLogTelegramNetworkError(message);

        }
    }
}
