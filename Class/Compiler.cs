using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Loader.Class;
using LeagueSharp.Loader.Data;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;

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

namespace LeagueSharp.Loader
{
    internal class Compiler
    {
        public static bool Compile(Project project, string logfile, Log log)
        {
            try
            {
                if (project != null)
                {
                    bool doLog = false;
                    if (!string.IsNullOrWhiteSpace(logfile))
                    {
                        string logDir = Path.GetDirectoryName(logfile);
                        if (!string.IsNullOrWhiteSpace(logDir))
                        {
                            doLog = true;
                            if (!Directory.Exists(logDir))
                            {
                                Directory.CreateDirectory(logDir);
                            }
                            var fileLogger = new FileLogger
                            {
                                Parameters = @"logfile=" + logfile,
                                ShowSummary = true
                            };
                            ProjectCollection.GlobalProjectCollection.RegisterLogger(fileLogger);
                        }
                    }
                    bool result = project.Build();
                    ProjectCollection.GlobalProjectCollection.UnregisterAllLoggers();
                    Utility.Log(result ? LogStatus.Ok : LogStatus.Error, "Compiler",
                        string.Format("Compile - Check ./logs/ for details - {0}", project.FullPath), log);

                    if (!result && doLog && File.Exists(logfile))
                    {
                        string pathDir = Path.GetDirectoryName(logfile);
                        if (!string.IsNullOrWhiteSpace(pathDir))
                        {
                            File.Move(logfile, Path.Combine(Directories.LogsDir, ("Error - " + Path.GetFileName(logfile))));
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
                string extension = project.GetPropertyValue("OutputType").ToLower() == "exe"
                    ? ".exe"
                    : (project.GetPropertyValue("OutputType").ToLower() == "library" ? ".dll" : string.Empty);
                string pathDir = Path.GetDirectoryName(project.FullPath);
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
