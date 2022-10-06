using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Inversion
{
  public abstract class App : Application
  {
    public App()
    {
      System.Windows.Application.LoadComponent(
        this,
        new Uri("/Inversion;component/App.xaml", System.UriKind.Relative)
      );
    }
  }
}