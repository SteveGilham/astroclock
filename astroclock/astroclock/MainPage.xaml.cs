using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace astroclock
{
  public partial class MainPage : Page
  {
    private object carry;

    public MainPage()
    {
      this.InitializeComponent();

      var temp = new Astroclock.AstroPage(this);
      temp.Begin();
      carry = temp;
    }
  }
}