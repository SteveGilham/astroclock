using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace astroclock
{
  public partial class MainPage : Page
  {
    private Astroclock.AstroPage carry;
    private DispatcherTimer _dispatcherTimer;

    public MainPage()
    {
      this.InitializeComponent();

      var temp = new Astroclock.AstroPage(this);
      temp.Begin();
      carry = temp;

      //We create a new instance of DispatcherTimer:
      _dispatcherTimer = new DispatcherTimer
      {
        //we set the time between each tick of the DispatcherTimer to 1 second:
        Interval = new TimeSpan(0, 0, 0, 1)
      };
      _dispatcherTimer.Tick += DispatcherTimer_Tick;
      _dispatcherTimer.Start();
    }

    private void DispatcherTimer_Tick(object sender, object e)
    {
      carry.UpdateSky();
      carry.UpdateTick();
    }
  }
}