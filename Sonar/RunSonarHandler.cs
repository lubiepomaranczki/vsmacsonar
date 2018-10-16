using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using System;
using System.Diagnostics;
using MonoDevelop.Projects;

namespace SonarRunner
{
    class RunSonarHandler : CommandHandler
    {
        protected override void Run()
        {
            var pathToSonar = "Downloads/sonar-scanner-msbuild-4.4.1.1530-net46";
            var projectName = ProjectOperations.CurrentSelectedSolution.Name;

            if (!ProjectIsNotBuildingOrRunning())
            {
                return;
            }

            ExecuteBashCommand($"mono ~/{pathToSonar}/SonarScanner.MSBuild.exe begin /k:${projectName}");
            ExecuteBashCommand("MSbuild /t:rebuild");
            ExecuteBashCommand($"mono ~/{pathToSonar}/SonarScanner.MSBuild.exe end");

            //IdeApp.Workbench.StatusBar.BeginProgress("Deleting /bin & /obj directories");

            //var solutionItems = ProjectOperations.CurrentSelectedSolution.Items;

            //foreach (var item in solutionItems)
            //{
            //    var binPath = item.BaseDirectory.FullPath + "/bin";

            //    if (FileService.IsValidPath(binPath) && FileService.IsDirectory(binPath))
            //    {
            //        FileService.DeleteDirectory(binPath);
            //    }

            //    var objPath = item.BaseDirectory.FullPath + "/obj";

            //    if (FileService.IsValidPath(objPath) && FileService.IsDirectory(objPath))
            //    {
            //        FileService.DeleteDirectory(objPath);
            //    }
            //}

            //IdeApp.Workbench.StatusBar.EndProgress();
            //IdeApp.Workbench.StatusBar.ShowMessage("Deleted /bin & /obj directories successfully");

        }

        protected override void Update(CommandInfo info)
        {

        }

        protected ProjectOperations ProjectOperations => IdeApp.ProjectOperations;

        // Shoud be enabled only when the workspace is opened
        protected bool IsWorkspaceOpen() => IdeApp.Workspace.IsOpen;

        protected bool ProjectIsNotBuildingOrRunning()
        {
            var isBuild = ProjectOperations.IsBuilding(ProjectOperations.CurrentSelectedSolution);
            var isRun = ProjectOperations.IsRunning(ProjectOperations.CurrentSelectedSolution);

            return !isBuild && !isRun && IdeApp.ProjectOperations.CurrentBuildOperation.IsCompleted
                         && IdeApp.ProjectOperations.CurrentRunOperation.IsCompleted;
        }

        string ExecuteBashCommand(string command)
        {
            // according to: https://stackoverflow.com/a/15262019/637142
            // thans to this we will pass everything as one command
            command = command.Replace("\"", "\"\"");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            return proc.StandardOutput.ReadToEnd();
        }
    }
}
