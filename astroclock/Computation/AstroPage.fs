namespace Astroclock
open System
open System.ComponentModel
open System.Globalization
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes

open Computation

type AstroPage = class
    inherit System.Windows.Controls.Page

    val mutable page : string
    val mutable query : System.Collections.Generic.IDictionary<string,string>
    val animate : BackgroundWorker
    val mutable culture : CultureInfo
    val mutable hms : string
    val mutable date : string
    val mutable sunup : string
    val mutable sundown : string
    val mutable latitude : float
    val mutable longitude : float
    val compute : BackgroundWorker
    val mutable sun : Plot
    val mutable planets : list<Plot>
    val mutable moon : Plot
    val planetNames : list<string>
    val mutable phase : float

    member this.EverySecond() =
        System.Threading.Thread.Sleep(1000)
        this.animate.ReportProgress(0)
        this.EverySecond()

    member this.UpdateSky () =
      let display = {Latitude = this.latitude * 1.0<deg>;
                     Longitude = this.longitude * 1.0<deg>;
                     CentreX = 120.0;
                     CentreY = 120.0;
                     Radius = 100.0;}
      try
        this.sunup <- sunrise true display
        this.sundown <- sunrise false display
        this.sun <- plotSun display
        this.planets <- getPP display
        this.moon <- getMoon display
        this.phase <- getMoonPhase display
      with x ->
        this.sunup <- x.Message
      ()

    member this.EveryMinute() =
        System.Threading.Thread.Sleep(60*1000)
        this.UpdateSky ()
        this.EveryMinute()

    member this.MoveHand name angle =
      let line = this.FindName(name) :?> Line
      let tg = line.RenderTransform :?> TransformGroup
      let rotate = tg.Children.[0] :?> RotateTransform
      rotate.Angle <- angle

    member this.SetText name value =
      let block = this.FindName(name) :?> TextBlock
      block.Text <- value
      block

    member this.SetPlace name where =
      let item = this.FindName(name) :?> Path
      (item.Data :?> EllipseGeometry).Center <- Point(where.X, where.Y)
      item.Visibility <- select where.Visible Visibility.Visible Visibility.Collapsed

    member this.DrawMoon () =
      let item = this.FindName("moon") :?> Path
      let where = this.moon
      let figure = (item.Data :?> PathGeometry).Figures.[0]
      let rh = (figure.Segments.[0] :?> ArcSegment)
      let lh = (figure.Segments.[1] :?> ArcSegment)
      item.Visibility <- select where.Visible Visibility.Visible Visibility.Collapsed

      figure.StartPoint <- Point(where.X, where.Y - 5.0)
      rh.Point <- Point(where.X, where.Y + 5.0)
      lh.Point <- figure.StartPoint

      let tmp = 1.0 - (abs this.phase)
      match this.phase with
      | x when x > 0.5 -> // waxing crescent
        rh.Size <- new Size(5.0, 5.0)
        lh.Size <- new Size(10.0 * (this.phase - 0.5), 5.0)
        lh.SweepDirection <- SweepDirection.Counterclockwise
      | x when x >= 0.0 -> // waxing gibbous
        rh.Size <- new Size(5.0, 5.0)
        lh.Size <- new Size(10.0 * (0.5 - this.phase), 5.0)
      | x when x > -0.5 -> // waning gibbous
        lh.Size <- new Size(5.0, 5.0)
        rh.Size <- new Size(10.0 * (this.phase + 0.5), 5.0)
      | _ -> // waning cresent
        lh.Size <- new Size(5.0, 5.0)
        rh.Size <- new Size(10.0 * (-(0.5 + this.phase)), 5.0)
        rh.SweepDirection <- SweepDirection.Counterclockwise

    member this.UpdateTick() =
        let now = System.DateTime.Now
        this.SetText "hms" (now.ToString(this.hms, this.culture))   |> ignore
        this.SetText "day" (now.ToString("dddd", this.culture))     |> ignore
        this.SetText "date" (now.ToString(this.date, this.culture)) |> ignore
        try
          this.MoveHand "second" (6.0 * float(now.Second))
          this.MoveHand "minute" (float(6*now.Minute)+(0.1*float(now.Second)))
          this.MoveHand "hour" (float(30*now.Hour)+(float(now.Minute)/2.0)+(float(now.Second)/300.0))
          this.SetText "sunup" (String.Format("{0}-{1}", this.sunup, this.sundown)) |> ignore
          this.SetPlace "sun" this.sun
          List.iter2 this.SetPlace this.planetNames this.planets
          this.DrawMoon ()
        with x -> // debug technique
          (this.SetText "sunup" x.Message).FontSize <- 8.0

    member this.Latitude
      with get() = this.latitude
      and set l = this.latitude <- l

    member this.Longitude
      with get() = this.longitude
      and set l = this.longitude <- l

    member this.LatitudeCaption
      with get() =
        (select (this.Latitude < 0.0)
          (String.Format("Latitude {0:F2}S", -this.Latitude))
          (String.Format("Latitude {0:F2}N", this.Latitude)))

    member this.LongitudeCaption
      with get() =
        (select (this.Longitude < 0.0)
          (String.Format("Longitude {0:F2}W", -this.Longitude))
          (String.Format("Longitude {0:F2}E", this.Longitude)))

    member this.GetParam<'T> (name:string) (transform:(string->'T)) (fallback:'T) : ('T * bool)  =
       try
          let value = this.query.[name]
          ((transform value), true)
       with _ ->
          (fallback, false)

    member this.ToggleUI state =
      let expander = this.FindName("expander1") :?> UIElement
      let button = this.FindName("button1") :?> UIElement
      match state with
      | true ->
        expander.Visibility <- Visibility.Collapsed
        button.Visibility <- Visibility.Visible
      | _ ->
        expander.Visibility <- Visibility.Visible
        button.Visibility <- Visibility.Collapsed
      ()

    member this.Button1Click() =
      let button = this.FindName("button1") :?> UIElement
      let configured = (button.Visibility = Visibility.Visible)
      this.ToggleUI (not configured)

    member this.UpdateURL () =
      let hyperlink = this.FindName("hyperlink") :?> HyperlinkButton
      hyperlink.NavigateUri <- System.Uri(String.Format("{0}?lat={1}&long={2}",
                                           this.page, this.Latitude, this.Longitude))

    member this.LatValueChanged() =
      let lat = this.FindName("slider1") :?> Slider
      this.Latitude <- lat.Value
      this.SetText "label1" this.LatitudeCaption |> ignore
      this.UpdateSky ()
      this.UpdateURL ()

    member this.LongValueChanged() =
      let lat = this.FindName("slider2") :?> Slider
      this.Longitude <- lat.Value
      this.SetText "label2" this.LongitudeCaption |> ignore
      this.UpdateSky ()
      this.UpdateURL ()

    member this.Begin() =
       this.XamlSourcePath <- @"OpenSilverApplication1\astroclock.xaml"
       this.query <- System.Windows.Browser.HtmlPage.Document.QueryString

       this.page <- System.Windows.Browser.HtmlPage.Document.DocumentUri.GetComponents(
                          System.UriComponents.SchemeAndServer ||| System.UriComponents.Path,
                          System.UriFormat.Unescaped).Split( [|'?'|]).[0]

       (this.SetText "label1" this.LatitudeCaption).DataContext <- this.LatitudeCaption
       (this.SetText "label2" this.LongitudeCaption).DataContext <- this.LongitudeCaption

       (this.FindName("button1") :?> Button).Click.Add(fun _ -> this.Button1Click())

       this.culture <- fst (this.GetParam "locale" (fun x -> new CultureInfo(x)) CultureInfo.CurrentCulture)
       this.hms <- fst ( this.GetParam "hms" id this.culture.DateTimeFormat.LongTimePattern  ) // HH:mm:ss
       this.date <- fst ( this.GetParam "date" id this.culture.DateTimeFormat.ShortDatePattern ) // d-MMM-yyyy

       (this.FindName("slider1") :?> Slider).ValueChanged.Add(fun _ -> this.LatValueChanged())
       let islat = this.GetParam "lat" (fun x -> Double.Parse(x)) this.latitude
       this.latitude <- fst(islat)
       (this.FindName("slider1") :?> Slider).Value <- this.latitude

       (this.FindName("slider2") :?> Slider).ValueChanged.Add(fun _ -> this.LongValueChanged())
       let islong = this.GetParam "long" (fun x -> Double.Parse(x)) this.longitude
       this.longitude <- fst(islong)
       (this.FindName("slider2") :?> Slider).Value <- this.longitude

       let version = System.Reflection.Assembly.GetExecutingAssembly().FullName.ToString()
       let bits = version.Split([|','|])
       let legend = String.Format("Astroclock {0}", bits.[1])
       this.SetText "legend" legend |> ignore

       this.ToggleUI ( snd(islat) && snd(islong) )

       this.compute.DoWork.Add(fun _ -> this.EveryMinute())
       this.compute.RunWorkerAsync()
       this.UpdateSky ()

       this.animate.WorkerReportsProgress <- true
       this.animate.DoWork.Add(fun _ -> this.EverySecond())
       this.animate.ProgressChanged.Add(fun _ -> this.UpdateTick())
       this.animate.RunWorkerAsync()
       this.UpdateTick ()

    new () = {page = String.Empty;
              query = null;
              animate = new BackgroundWorker();
              culture = null
              hms = "HH:mm:ss";
              date = "d-MMM-yyyy";
              sunup = String.Empty;
              sundown = String.Empty;
              latitude = 52.0;
              longitude = 0.0;
              compute = new BackgroundWorker();
              sun = {X=60.0; Y=60.0; Visible=false;}
              planets = [];
              moon = {X=60.0; Y=60.0; Visible=true;}
              planetNames = ["mercury"; "venus"; "mars"; "jupiter"; "saturn"];
              phase = 0.5
              }
end

//type MyApp = class
//    inherit Application

//    new () as this = {} then
//        this.Startup.Add(fun _ ->  this.RootVisual <- new Page())

//end