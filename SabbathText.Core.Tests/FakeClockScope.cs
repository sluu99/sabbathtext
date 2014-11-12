using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Tests
{
    public class FakeClockScope : IDisposable
    {
        public FakeClockScope()
        {
            Clock.ResetClock();
            Clock.UseFakeClock();
        }

        public void Dispose()
        {
            Clock.ResetClock();
            Clock.UseSystemClock();
        }
    }
}
