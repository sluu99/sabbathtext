namespace SabbathText.Runner
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// The main program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The application entry
        /// </summary>
        /// <param name="args">The application argument</param>
        public static void Main(string[] args)
        {
            GoodieBag.Initialize(RunnerEnvironmentSettings.Create());
            AccountRunner runner = new AccountRunner();
            new WorkerProgram("Runner", runner).Run();
        }
    }
}
