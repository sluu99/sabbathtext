namespace SabbathText.CompensationAgent
{
    using SabbathText.Compensation.V1;
    using SabbathText.V1;

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
            GoodieBag.Initialize(EnvironmentSettings.Create());
            GoodieBag bag = GoodieBag.Create();
            CheckpointWorker worker = new CheckpointWorker(
                bag.CompensationClient,
                bag.Settings.CheckpointWorkerIdleDelay,
                new OperationCheckpointHandler());

            new WorkerProgram("Compensation Agent", worker).Run();
        }
    }
}
