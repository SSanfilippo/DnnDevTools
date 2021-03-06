﻿using System;
using System.Linq;
using System.Threading;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;
using Microsoft.AspNet.SignalR;
using weweave.DnnDevTools.Dto;
using weweave.DnnDevTools.Service;
using weweave.DnnDevTools.Util;

namespace weweave.DnnDevTools.SignalR
{
    internal class DnnEventWatcher
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MailPickupFolderWatcher));

        private IServiceLocator _serviceLocator;

        private IServiceLocator ServiceLocator => _serviceLocator ?? (_serviceLocator = new ServiceLocator());

        private static DnnEventWatcher _instance;
        internal static DnnEventWatcher Instance => _instance ?? (_instance = new DnnEventWatcher());

        private DnnEventWatcher() { }

        internal void Run()
        {
            Logger.Debug("Initializing DNN event trace");

            var lastLog = DateTime.Now;

            new Thread(() =>
            {
                var totalRecords = 0;
                while (true)
                {
                    Thread.Sleep(1000);

                    // Do nothing if module has been uninstalled
                    if (!ModuleUtil.IsInstalled()) return;

                    // Skip if DnnDevTools or EnableEventCatch is not enabled
                    if (!ServiceLocator.ConfigService.GetEnable() || !ServiceLocator.ConfigService.GetEnableDnnEventTrace())
                        return;

                    // Get last 100 log entries
                    var logs = LogController.Instance.GetLogs(Null.NullInteger, Null.NullString, 100, 0, ref totalRecords);

                    var newLogs = logs.Where(e => e.LogCreateDate > lastLog).ToList();
                    lastLog = logs.Max(e => e.LogCreateDate);

                    // ReSharper disable once LoopCanBePartlyConvertedToQuery
                    foreach (var newLog in newLogs)
                    {
                        // Send DNN event notification to clients
                        var eventNotification = new DnnEvent(newLog);
                        GlobalHost.ConnectionManager.GetHubContext<DnnDevToolsNotificationHub>().Clients.All.OnEvent(eventNotification);
                    }
                }

            }).Start();
        }


    }
}
