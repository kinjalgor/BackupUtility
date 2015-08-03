﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using BackupLibrary;
using System.Reflection;

namespace SqlBackUpper
{
    class Program
    {
        static void Main(string[] args)
        {
            Display.SetWindowStyle(ProcessWindowStyle.Hidden);

            // init config file (and current log file)
            Config config = new Config(args);
            if (!config.IsSetConfig)
            {
                Log.Add("Config file is not set. Exit..");
                return;
            }

            ProcessWindowStyle windowStyle = config.WindowStyle;
            Display.SetWindowStyle(windowStyle);

            // connecting to the server & execute sql query
            List<Connection> connections = config.Connections;
            if (connections != null)
            {
                if (config.UniteSameInst)
                {
                    // use grouping by server
                    List<ConnectionGroup> groups = ConnectionGroup.SpritToGroups(connections);
                    foreach(ConnectionGroup group in groups)
                    {
                        Log.AddLine();
                        SqlQuery.ExecuteGroup(group, config.ConnectionGroupMask, config.Timeout, config.SqlQueryMask, config.BackupNameMask);
                        foreach (Connection conn in group.Connections)
                        {
                            FileSystem.DeleteOldFiles(conn.BackupPath, conn.MaxBackups, conn.BaseName + "*");
                        }
                    }
                }
                else
                {
                    // execute all queries successively
                    foreach (Connection conn in connections)
                    {
                        Log.AddLine();
                        string backupName = conn.BaseName + String.Format(config.BackupNameMask, DateTime.Now);
                        SqlQuery.Execute(conn, config.ConnectionMask, config.Timeout, config.SqlQueryMask, backupName);
                        FileSystem.DeleteOldFiles(conn.BackupPath, conn.MaxBackups, conn.BaseName + "*");
                    }
                }
            }
            else Log.Add("Error in config ([Connections] == null");

            // delete extra log files
            FileSystem.DeleteOldFiles(config.LogPath, config.MaxLogs, "*.log");
            
            Log.Add("Finish");
            //
            if ((windowStyle == ProcessWindowStyle.Normal || windowStyle == ProcessWindowStyle.Maximized) && config.ReadKeyInFinish)
            {
                Console.WriteLine("Press any key to exit..");
                Console.ReadKey();
            }
        }
    }
}
