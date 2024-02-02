namespace OpenSilverApplication1

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Windows
open System.Windows.Controls

type MainPage() as this =
    inherit MainPageXaml()

    // For code examples, refer to the OpenSilver Showcase app at: https://opensilver.net/gallery/
    do
        this.InitializeComponent()
        // Enter construction logic here...
        let btn = new Button(HorizontalAlignment = HorizontalAlignment.Center,
                             VerticalAlignment = VerticalAlignment.Center)
        btn.Content <- "Click me"
        btn.Click.Add(fun _ ->
                      btn.Content <- "The button has been clicked!")
        this.Content <- btn