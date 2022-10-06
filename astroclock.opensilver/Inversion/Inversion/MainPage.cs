using System;
using System.Windows.Controls;

namespace Inversion
{
  public abstract class MainPage : Page
  {
    public MainPage()
    {
      System.Windows.Application.LoadComponent(
        this,
        new Uri("/Inversion;component/MainPage.xaml", UriKind.Relative)
      );
    }
  }
}