using OpenSilver.Simulator;
using System;

namespace astroclock.opensilver3._0.Simulator
{
  internal static class Startup
  {
    [STAThread]
    private static int Main(string[] args)
    {
      return SimulatorLauncher.Start(typeof(Astroclock.App));
    }
  }
}