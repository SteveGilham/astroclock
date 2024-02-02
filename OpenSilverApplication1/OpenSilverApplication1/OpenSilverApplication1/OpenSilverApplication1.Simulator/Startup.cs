using OpenSilver.Simulator;
using System;

namespace OpenSilverApplication1.Simulator
{
  internal static class Startup
  {
    [STAThread]
    static int Main(string[] args)
    {
      return SimulatorLauncher.Start(typeof(App));
    }
  }
}
