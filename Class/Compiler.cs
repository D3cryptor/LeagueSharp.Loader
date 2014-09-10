#region

using System;
using System.IO;
using LeagueSharp.Loader.Data;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;

#endregion

/*
    Copyright (C) 2014 LeagueSharp

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

/*
    Copyright (C) 2014 Nikita Bernthaler

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace LeagueSharp.Loader.Class
{
    internal class Compiler
    {
        public static bool Compile(Project project, string logfile, Log log)
        {
            try
            {
                if (project != null)
                {
                    var doLog = false;
                    if (!string.IsNullOrWhiteSpace(logfile))
                    {
                        var logDir = Path.GetDirectoryName(logfile);
                        if (!string.IsNullOrWhiteSpace(logDir))
                        {
                            doLog = true;
                            if (!Directory.Exists(logDir))
                            {
                                Directory.CreateDirectory(logDir);
                            }
                            var fileLogger = new FileLogger { Parameters = @"logfile=" + logfile, ShowSummary = true };
                            ProjectCollection.GlobalProjectCollection.RegisterLogger(fileLogger);
                        }
                    }

                    var result = project.Build();
                    ProjectCollection.GlobalProjectCollection.UnregisterAllLoggers();
                    Utility.Log(
                        result ? LogStatus.Ok : LogStatus.Error, "Compiler",
                        result ? string.Format("Compile - {0}", project.FullPath) : string.Format("Compile - Check ./logs/ for details - {0}", project.FullPath), log);

                    if (!result && doLog && File.Exists(logfile))
                    {
                        var pathDir = Path.GetDirectoryName(logfile);
                        if (!string.IsNullOrWhiteSpace(pathDir))
                        {
                            File.Move(
                                logfile, Path.Combine(Directories.LogsDir, ("Error - " + Path.GetFileName(logfile))));
                        }
                    }
                    else if (result && File.Exists(logfile))
                    {
                        File.Delete(logfile);
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                Utility.Log(LogStatus.Error, "Compiler", ex.Message, log);
            }
            return false;
        }

        public static string GetOutputFilePath(Project project)
        {
            if (project != null)
            {
                var extension = project.GetPropertyValue("OutputType").ToLower() == "exe"
                    ? ".exe"
                    : (project.GetPropertyValue("OutputType").ToLower() == "library" ? ".dll" : string.Empty);
                var pathDir = Path.GetDirectoryName(project.FullPath);
                if (!string.IsNullOrWhiteSpace(extension) && !string.IsNullOrWhiteSpace(pathDir))
                {
                    return Path.Combine(pathDir, project.GetPropertyValue("OutputPath")) +
                           (project.GetPropertyValue("AssemblyName") + extension);
                }
            }
            return string.Empty;
        }
    }
}