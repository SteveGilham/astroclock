using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace astroclock
{
  public partial class MainPage : Page
  {
    private Astroclock.AstroPage carry;

    public MainPage()
    {
      this.InitializeComponent();

      var temp = new Astroclock.AstroPage(this);
      temp.Begin();
      carry = temp;
    }
  }
}