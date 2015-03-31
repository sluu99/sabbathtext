namespace System
{
    /// <summary>
    /// This class starts a new fake clock scope
    /// </summary>
    public class FakeClockScope : IDisposable
    {
        /// <summary>
        /// Creates a new instance of the scope and switches to fake clock mode
        /// </summary>
        public FakeClockScope()
        {
            Clock.ResetClock();
            Clock.UseFakeClock();
        }
        
        /// <summary>
        /// Resets the clock to use system clock
        /// </summary>
        public void Dispose()
        {
            Clock.ResetClock();
            Clock.UseSystemClock();
        }
    }
}
