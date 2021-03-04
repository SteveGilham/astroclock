namespace Astroclock
open System
open System.Globalization
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes
open System.Windows.Threading

open Computation
open CSHTML5.Internal

type AstroPage(dummy:int) = class
  inherit System.Windows.Controls.Page()

    member val page = String.Empty with get, set
    member val query : System.Collections.Generic.IDictionary<string,string> = null with get, set
    member val culture : CultureInfo = null with get, set
    member val hms = "HH:mm:ss" with get, set
    member val date = "d-MMM-yyyy" with get, set
    member val sunup = String.Empty with get, set
    member val sundown = String.Empty with get, set
    member val latitude = 52.0 with get, set
    member val longitude = 0.0 with get, set
    member val sun = {X=60.0; Y=60.0; Visible=false;} with get, set
    member val planets : list<Plot> = [] with get, set
    member val moon  = {X=60.0; Y=60.0; Visible=true;} with get, set
    member val planetNames = ["mercury"; "venus"; "mars"; "jupiter"; "saturn"] with get
    member val phase = 0.5 with get, set
    member val timer = DispatcherTimer() with get

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

    member this.MoveHand name angle =
      let line = this.FindName(name) :?> Line
      let rotate = line.RenderTransform :?> CompositeTransform
      rotate.Rotation <- angle

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
          this.query
          |> Option.ofObj
          |> Option.map(fun q ->
            let ok, value = q.TryGetValue(name)
            (if ok
             then transform value
             else fallback), ok)
          |>Option.defaultValue (fallback, false)
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

    member this.InitializeComponent() =
       // Wanted -- XAML to UIElement so we can put something like
       // self.Content <- LoadXAMLFromResource(...)
       // and then de-layer, like LoadComponent did
      let TextBlock_0472d9dbdef54c539467b966fe38bafd = TextBlock()
      let TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd = TextBlock()
      let TextBlock_486ee42817174faf90971786822d41b5 = TextBlock()
      let TextBlock_78eae0252b22427680509cba47b1c287 = TextBlock()
      let TextBlock_c8fbfb22281d42f89936e8eb63b730dd = TextBlock()
      let Path_6bdc3f886ca24b33ad618db3d2498060 = Path()
      let Path_aedd051fe35e466ea3cb612eea7de959 = Path()
      let Path_6d547957803f431280a93cf039aad576 = Path()
      let Path_6a4ed605ffb64e919b7ac2434392740a = Path()
      let Path_5665bc5d56774050ba9a8d88d6641d23 = Path()
      let Path_9a23d0d2e8624a4cb3cfa7276659b7df = Path()
      let Path_894e7284504c43d3a1d7482844df77f = Path()
      let Ellipse_191634773284424c92edf060a412661c = Ellipse()
      let Line_07b55205a97847268e57bb74145aa80e = Line()
      let Line_fb3bc95e98b647bea6ae4a5f5618336 = Line()
      let Line_f62bafc10bd2428db6b124730c5bd68 = Line()
      let Line_3111bd2585ed41fdabf46071774c0ef = Line()
      let Line_5bcc5b9f3acb429683fdaf56a617098c = Line()
      let Line_e1c531f3b4324e81842edde85476ecef = Line()
      let Line_e525ac7b5f76403aa895b4e74cfaa204 = Line()
      let Line_42c86cb3a7aa4d5ab2345ab6901ebba = Line()
      let Line_1c44d98d5e90452792d12bf42b71e17c = Line()
      let Line_9402226581604a909271e44e66f44e6c = Line()
      let Line_8274b4c25e614aa687fde6d2babf7a74 = Line()
      let Line_77de60c5678e4b9e8916dcde2aa365bf = Line()
      let Line_13e17309867948ebab76151e70066531 = Line()
      let Line_54fe6a97eca846f7865bf0fac5e23a47 = Line()
      let Line_073c33c5d0b74cb18a795dd9d80c1741 = Line()
      let Canvas_dd0e680653b6466babf48a3176a4babf = Canvas()
      let TextBlock_baff82024ac64fe499f12bfb43e02ede = TextBlock()
      let TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3 = TextBlock()
      let Slider_f77653af2d59408881db98942bea491c = Slider()
      let Slider_94ef856dcb2a4c80be45b6139f5145e5 = Slider()
      let Canvas_316cb7aad42e47789407fcffdd7a04a6 = Canvas()
      let HyperlinkButton_d406e374622b47ecadb424ed65015a8e = HyperlinkButton()
      let Button_d88415801d1f4a5f809c0944f85c9ccd = Button()
      let Grid_df7b4b7af1c248b38448cdb6a696d9e = Grid()
      this.RegisterName("LayoutRoot", Grid_df7b4b7af1c248b38448cdb6a696d9e)
      Grid_df7b4b7af1c248b38448cdb6a696d9e.Name <- "LayoutRoot"
      let StackPanel_a6f8a1caa8314c37b67b8a919fc985b4 = StackPanel()
      let TextBlock_9aea36c2b85447eaa82be834e5c453fc = TextBlock()
      TextBlock_9aea36c2b85447eaa82be834e5c453fc.VerticalAlignment <- VerticalAlignment.Top
      TextBlock_9aea36c2b85447eaa82be834e5c453fc.HorizontalAlignment <- HorizontalAlignment.Center
      let Span_89d3b205271e4488be92577a2212f2ae = System.Windows.Documents.Span()
      Span_89d3b205271e4488be92577a2212f2ae.FontWeight <- DotNetForHtml5.Core.TypeFromStringConverters.ConvertFromInvariantString(typeof<FontWeight>, "Bold") :?> FontWeight
      let Run_ff4c1bc9e27b48968f827a02a8a50856 = System.Windows.Documents.Run()
      Run_ff4c1bc9e27b48968f827a02a8a50856.Text <- "Astroclock Blazor WASM Applet"
      Span_89d3b205271e4488be92577a2212f2ae.Inlines.Add(Run_ff4c1bc9e27b48968f827a02a8a50856)
      TextBlock_9aea36c2b85447eaa82be834e5c453fc.Inlines.Add(Span_89d3b205271e4488be92577a2212f2ae)
      TextBlock_0472d9dbdef54c539467b966fe38bafd.Height <- 28.0
      Canvas.SetTop(TextBlock_0472d9dbdef54c539467b966fe38bafd, 0.0)
      this.RegisterName("legend", TextBlock_0472d9dbdef54c539467b966fe38bafd)
      TextBlock_0472d9dbdef54c539467b966fe38bafd.Name <- "legend"
      TextBlock_0472d9dbdef54c539467b966fe38bafd.VerticalAlignment <- VerticalAlignment.Bottom
      TextBlock_0472d9dbdef54c539467b966fe38bafd.HorizontalAlignment <- HorizontalAlignment.Center
      TextBlock_0472d9dbdef54c539467b966fe38bafd.Text <- ""
      this.RegisterName("canvas1", Canvas_dd0e680653b6466babf48a3176a4babf)
      Canvas_dd0e680653b6466babf48a3176a4babf.Name <- "canvas1"
      Canvas_dd0e680653b6466babf48a3176a4babf.Margin <- Thickness(0.0, 0.0, 0.0, 129.0)
      let black = Color.FromArgb(Byte.MaxValue, 0uy, 0uy, 0uy)
      Canvas_dd0e680653b6466babf48a3176a4babf.Background <- SolidColorBrush(black)
      Canvas_dd0e680653b6466babf48a3176a4babf.Width <- 480.0
      Canvas_dd0e680653b6466babf48a3176a4babf.Height <- 240.0
      TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd.Height <- 30.0
      TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd.FontSize <- 25.0
      Canvas.SetLeft(TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd, 240.0)
      Canvas.SetTop(TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd, 24.0)
      this.RegisterName("hms", TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd)
      TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd.Name <- "hms"
      let white = Color.FromArgb(Byte.MaxValue, Byte.MaxValue, Byte.MaxValue, Byte.MaxValue)
      TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd.Foreground <- SolidColorBrush(white)
      TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd.TextAlignment <- TextAlignment.Center
      TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd.VerticalAlignment <- VerticalAlignment.Center
      TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd.HorizontalAlignment <- HorizontalAlignment.Center
      TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd.Width <- 240.0
      TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd.Text <- "::"
      TextBlock_486ee42817174faf90971786822d41b5.Height <- 30.0
      TextBlock_486ee42817174faf90971786822d41b5.FontSize <- 25.0
      Canvas.SetLeft(TextBlock_486ee42817174faf90971786822d41b5, 240.0)
      Canvas.SetTop(TextBlock_486ee42817174faf90971786822d41b5, 78.0)
      this.RegisterName("day", TextBlock_486ee42817174faf90971786822d41b5)
      TextBlock_486ee42817174faf90971786822d41b5.Name <- "day"
      TextBlock_486ee42817174faf90971786822d41b5.Foreground <- SolidColorBrush(white)
      TextBlock_486ee42817174faf90971786822d41b5.TextAlignment <- TextAlignment.Center
      TextBlock_486ee42817174faf90971786822d41b5.VerticalAlignment <- VerticalAlignment.Center
      TextBlock_486ee42817174faf90971786822d41b5.HorizontalAlignment <- HorizontalAlignment.Center
      TextBlock_486ee42817174faf90971786822d41b5.Width <- 240.0
      TextBlock_486ee42817174faf90971786822d41b5.Text <- "::"
      TextBlock_78eae0252b22427680509cba47b1c287.Height <- 30.0
      TextBlock_78eae0252b22427680509cba47b1c287.FontSize <- 25.0
      Canvas.SetLeft(TextBlock_78eae0252b22427680509cba47b1c287, 240.0)
      Canvas.SetTop(TextBlock_78eae0252b22427680509cba47b1c287, 132.0)
      this.RegisterName("date", TextBlock_78eae0252b22427680509cba47b1c287)
      TextBlock_78eae0252b22427680509cba47b1c287.Name <- "date"
      TextBlock_78eae0252b22427680509cba47b1c287.Foreground <- SolidColorBrush(white)
      TextBlock_78eae0252b22427680509cba47b1c287.TextAlignment <- TextAlignment.Center
      TextBlock_78eae0252b22427680509cba47b1c287.VerticalAlignment <- VerticalAlignment.Center
      TextBlock_78eae0252b22427680509cba47b1c287.HorizontalAlignment <- HorizontalAlignment.Center
      TextBlock_78eae0252b22427680509cba47b1c287.Width <- 240.0
      TextBlock_78eae0252b22427680509cba47b1c287.Text <- "::"
      TextBlock_c8fbfb22281d42f89936e8eb63b730dd.Height <- 30.0
      TextBlock_c8fbfb22281d42f89936e8eb63b730dd.FontSize <- 25.0
      Canvas.SetLeft(TextBlock_c8fbfb22281d42f89936e8eb63b730dd, 240.0)
      Canvas.SetTop(TextBlock_c8fbfb22281d42f89936e8eb63b730dd, 186.0)
      this.RegisterName("sunup", TextBlock_c8fbfb22281d42f89936e8eb63b730dd)
      TextBlock_c8fbfb22281d42f89936e8eb63b730dd.Name <- "sunup"
      TextBlock_c8fbfb22281d42f89936e8eb63b730dd.Foreground <- SolidColorBrush(white)
      TextBlock_c8fbfb22281d42f89936e8eb63b730dd.TextAlignment <- TextAlignment.Center
      TextBlock_c8fbfb22281d42f89936e8eb63b730dd.VerticalAlignment <- VerticalAlignment.Center
      TextBlock_c8fbfb22281d42f89936e8eb63b730dd.HorizontalAlignment <- HorizontalAlignment.Center
      TextBlock_c8fbfb22281d42f89936e8eb63b730dd.Width <- 240.0
      TextBlock_c8fbfb22281d42f89936e8eb63b730dd.Text <- "::"
      this.RegisterName("mercury", Path_6bdc3f886ca24b33ad618db3d2498060)
      Path_6bdc3f886ca24b33ad618db3d2498060.Name <- "mercury"
      let silver = Color.FromArgb(Byte.MaxValue, 192uy, 192uy, 192uy)
      Path_6bdc3f886ca24b33ad618db3d2498060.Fill <- SolidColorBrush(silver)
      Path_6bdc3f886ca24b33ad618db3d2498060.StrokeThickness <- 1.0
      Path_6bdc3f886ca24b33ad618db3d2498060.Stroke <- SolidColorBrush(silver)
      let ToolTip_6f18f803e6824e64a623963292e01f59 = ToolTip()
      ToolTip_6f18f803e6824e64a623963292e01f59.Content <- "Mercury ?"
      ToolTipService.SetToolTip(Path_6bdc3f886ca24b33ad618db3d2498060, ToolTip_6f18f803e6824e64a623963292e01f59)
      let EllipseGeometry_f1af1f8114e5464d9efa0293aa3db4bf = EllipseGeometry()
      EllipseGeometry_f1af1f8114e5464d9efa0293aa3db4bf.Center <- Point(150.0, 150.0)
      EllipseGeometry_f1af1f8114e5464d9efa0293aa3db4bf.RadiusX <- 2.0
      EllipseGeometry_f1af1f8114e5464d9efa0293aa3db4bf.RadiusY <- 2.0
      Path_6bdc3f886ca24b33ad618db3d2498060.Data <- EllipseGeometry_f1af1f8114e5464d9efa0293aa3db4bf
      this.RegisterName("venus", Path_aedd051fe35e466ea3cb612eea7de959)
      Path_aedd051fe35e466ea3cb612eea7de959.Name <- "venus"
      Path_aedd051fe35e466ea3cb612eea7de959.Fill <- SolidColorBrush(white)
      Path_aedd051fe35e466ea3cb612eea7de959.StrokeThickness <- 1.0
      Path_aedd051fe35e466ea3cb612eea7de959.Stroke <- SolidColorBrush(white)
      let ToolTip_070aaba82f3e4676a9a82afd832e2932 = ToolTip()
      ToolTip_070aaba82f3e4676a9a82afd832e2932.Content <- "Venus ?"
      ToolTipService.SetToolTip(Path_aedd051fe35e466ea3cb612eea7de959, ToolTip_070aaba82f3e4676a9a82afd832e2932)
      let EllipseGeometry_f0e420cdc6334f8eaf34c56a712e20d6 = EllipseGeometry()
      EllipseGeometry_f0e420cdc6334f8eaf34c56a712e20d6.Center <- Point(100.0, 100.0)
      EllipseGeometry_f0e420cdc6334f8eaf34c56a712e20d6.RadiusX <- 3.0
      EllipseGeometry_f0e420cdc6334f8eaf34c56a712e20d6.RadiusY <- 3.0
      Path_aedd051fe35e466ea3cb612eea7de959.Data <- EllipseGeometry_f0e420cdc6334f8eaf34c56a712e20d6
      this.RegisterName("mars", Path_6d547957803f431280a93cf039aad576)
      Path_6d547957803f431280a93cf039aad576.Name <- "mars"
      let red = Color.FromArgb(Byte.MaxValue, Byte.MaxValue, 0uy, 0uy)
      Path_6d547957803f431280a93cf039aad576.Fill <- SolidColorBrush(red)
      Path_6d547957803f431280a93cf039aad576.StrokeThickness <- 1.0
      Path_6d547957803f431280a93cf039aad576.Stroke <- SolidColorBrush(red)
      let ToolTip_dfcadc5c978d42ccafaf7e41bc65588d = ToolTip()
      ToolTip_dfcadc5c978d42ccafaf7e41bc65588d.Content <- "Mars ?"
      ToolTipService.SetToolTip(Path_6d547957803f431280a93cf039aad576, ToolTip_dfcadc5c978d42ccafaf7e41bc65588d)
      let EllipseGeometry_fc91891c1321402597625a14fdeace60 = EllipseGeometry()
      EllipseGeometry_fc91891c1321402597625a14fdeace60.Center <- Point(90.0, 90.0)
      EllipseGeometry_fc91891c1321402597625a14fdeace60.RadiusX <- 2.0
      EllipseGeometry_fc91891c1321402597625a14fdeace60.RadiusY <- 2.0
      Path_6d547957803f431280a93cf039aad576.Data <- EllipseGeometry_fc91891c1321402597625a14fdeace60
      this.RegisterName("jupiter", Path_6a4ed605ffb64e919b7ac2434392740a)
      Path_6a4ed605ffb64e919b7ac2434392740a.Name <- "jupiter"
      Path_6a4ed605ffb64e919b7ac2434392740a.Fill <- SolidColorBrush(silver)
      Path_6a4ed605ffb64e919b7ac2434392740a.StrokeThickness <- 1.0
      Path_6a4ed605ffb64e919b7ac2434392740a.Stroke <- SolidColorBrush(silver)
      let ToolTip_6b8f26cbaefd4afeac01819c8f097a0a = ToolTip()
      ToolTip_6b8f26cbaefd4afeac01819c8f097a0a.Content <- "Jupiter  ?"
      ToolTipService.SetToolTip(Path_6a4ed605ffb64e919b7ac2434392740a, ToolTip_6b8f26cbaefd4afeac01819c8f097a0a)
      let EllipseGeometry_3d497b002297465b9d52ce9f1231dfd0 = EllipseGeometry()
      EllipseGeometry_3d497b002297465b9d52ce9f1231dfd0.Center <- Point(80.0, 80.0)
      EllipseGeometry_3d497b002297465b9d52ce9f1231dfd0.RadiusX <- 3.0
      EllipseGeometry_3d497b002297465b9d52ce9f1231dfd0.RadiusY <- 3.0
      Path_6a4ed605ffb64e919b7ac2434392740a.Data <- EllipseGeometry_3d497b002297465b9d52ce9f1231dfd0
      this.RegisterName("saturn", Path_5665bc5d56774050ba9a8d88d6641d23)
      Path_5665bc5d56774050ba9a8d88d6641d23.Name <- "saturn"
      let yellow = Color.FromArgb(Byte.MaxValue, Byte.MaxValue, Byte.MaxValue, 0uy)
      Path_5665bc5d56774050ba9a8d88d6641d23.Fill <- SolidColorBrush(yellow)
      Path_5665bc5d56774050ba9a8d88d6641d23.StrokeThickness <- 1.0
      Path_5665bc5d56774050ba9a8d88d6641d23.Stroke <- SolidColorBrush(yellow)
      let ToolTip_cdc8f5c8dfd8443da2cf80a6e99f3eef = ToolTip()
      ToolTip_cdc8f5c8dfd8443da2cf80a6e99f3eef.Content <- "Saturn ?"
      ToolTipService.SetToolTip(Path_5665bc5d56774050ba9a8d88d6641d23, ToolTip_cdc8f5c8dfd8443da2cf80a6e99f3eef)
      let EllipseGeometry_5a9c2c49869f4921b952f533c6c0217b = EllipseGeometry()
      EllipseGeometry_5a9c2c49869f4921b952f533c6c0217b.Center <- Point(70.0, 70.0)
      EllipseGeometry_5a9c2c49869f4921b952f533c6c0217b.RadiusX <- 4.0
      EllipseGeometry_5a9c2c49869f4921b952f533c6c0217b.RadiusY <- 2.0
      Path_5665bc5d56774050ba9a8d88d6641d23.Data <- EllipseGeometry_5a9c2c49869f4921b952f533c6c0217b
      this.RegisterName("moon", Path_9a23d0d2e8624a4cb3cfa7276659b7df)
      Path_9a23d0d2e8624a4cb3cfa7276659b7df.Name <- "moon"
      Path_9a23d0d2e8624a4cb3cfa7276659b7df.Fill <- SolidColorBrush(silver)
      Path_9a23d0d2e8624a4cb3cfa7276659b7df.StrokeThickness <- 0.0
      Path_9a23d0d2e8624a4cb3cfa7276659b7df.Stroke <- SolidColorBrush(silver)
      let ToolTip_1cb29257b2e14d7e8480b808daef56e3 = ToolTip()
      ToolTip_1cb29257b2e14d7e8480b808daef56e3.Content <- "The Moon ?"
      ToolTipService.SetToolTip(Path_9a23d0d2e8624a4cb3cfa7276659b7df, ToolTip_1cb29257b2e14d7e8480b808daef56e3)
      let PathGeometry_5282c5c126384ae698c6142e4135de8f = PathGeometry()
      let PathFigure_bddb93cc5db54df0ad3585144226c69c = PathFigure()
      PathFigure_bddb93cc5db54df0ad3585144226c69c.StartPoint <- Point(150.0, 145.0)
      let ArcSegment_365ad6eee3e74919b0ded0358e18d3a0 = ArcSegment()
      ArcSegment_365ad6eee3e74919b0ded0358e18d3a0.Size <- DotNetForHtml5.Core.TypeFromStringConverters.ConvertFromInvariantString(typeof<Size>, "0.5, 5") :?> Size
      ArcSegment_365ad6eee3e74919b0ded0358e18d3a0.RotationAngle <- 180.0
      ArcSegment_365ad6eee3e74919b0ded0358e18d3a0.Point <- Point(150.0, 155.0)
      ArcSegment_365ad6eee3e74919b0ded0358e18d3a0.IsLargeArc <- false
      ArcSegment_365ad6eee3e74919b0ded0358e18d3a0.SweepDirection <- SweepDirection.Clockwise
      let ArcSegment_7f5fcd3619c44a24ba2cd14be36420b6 = ArcSegment()
      ArcSegment_7f5fcd3619c44a24ba2cd14be36420b6.Size <- DotNetForHtml5.Core.TypeFromStringConverters.ConvertFromInvariantString(typeof<Size>, "2.5, 2.5") :?> Size
      ArcSegment_7f5fcd3619c44a24ba2cd14be36420b6.RotationAngle <- 180.0
      ArcSegment_7f5fcd3619c44a24ba2cd14be36420b6.Point <- Point(150.0, 145.0)
      ArcSegment_7f5fcd3619c44a24ba2cd14be36420b6.IsLargeArc <- false
      ArcSegment_7f5fcd3619c44a24ba2cd14be36420b6.SweepDirection <- SweepDirection.Clockwise
      PathFigure_bddb93cc5db54df0ad3585144226c69c.Segments.Add(ArcSegment_365ad6eee3e74919b0ded0358e18d3a0)
      PathFigure_bddb93cc5db54df0ad3585144226c69c.Segments.Add(ArcSegment_7f5fcd3619c44a24ba2cd14be36420b6)
      PathGeometry_5282c5c126384ae698c6142e4135de8f.Figures.Add(PathFigure_bddb93cc5db54df0ad3585144226c69c)
      Path_9a23d0d2e8624a4cb3cfa7276659b7df.Data <- PathGeometry_5282c5c126384ae698c6142e4135de8f
      this.RegisterName("sun", Path_894e7284504c43d3a1d7482844df77f)
      Path_894e7284504c43d3a1d7482844df77f.Name <- "sun"
      Path_894e7284504c43d3a1d7482844df77f.Fill <- SolidColorBrush(yellow)
      Path_894e7284504c43d3a1d7482844df77f.StrokeThickness <- 1.0
      Path_894e7284504c43d3a1d7482844df77f.Stroke <- SolidColorBrush(yellow)
      let ToolTip_3ab72fcbd009427883f1b40660b760c6 = ToolTip()
      ToolTip_3ab72fcbd009427883f1b40660b760c6.Content <- "The Sun ?"
      ToolTipService.SetToolTip(Path_894e7284504c43d3a1d7482844df77f, ToolTip_3ab72fcbd009427883f1b40660b760c6)
      let EllipseGeometry_8f5578bb50b94469ad3f54d18f34f75e = EllipseGeometry()
      EllipseGeometry_8f5578bb50b94469ad3f54d18f34f75e.Center <- Point(120.0, 120.0)
      EllipseGeometry_8f5578bb50b94469ad3f54d18f34f75e.RadiusX <- 5.0
      EllipseGeometry_8f5578bb50b94469ad3f54d18f34f75e.RadiusY <- 5.0
      Path_894e7284504c43d3a1d7482844df77f.Data <- EllipseGeometry_8f5578bb50b94469ad3f54d18f34f75e
      this.RegisterName("dial", Ellipse_191634773284424c92edf060a412661c)
      Ellipse_191634773284424c92edf060a412661c.Name <- "dial"
      Ellipse_191634773284424c92edf060a412661c.Height <- 240.0
      Ellipse_191634773284424c92edf060a412661c.Width <- 240.0
      Ellipse_191634773284424c92edf060a412661c.StrokeThickness <- 20.0
      Ellipse_191634773284424c92edf060a412661c.Stroke <- SolidColorBrush(black)
      this.RegisterName("t1", Line_07b55205a97847268e57bb74145aa80e)
      Line_07b55205a97847268e57bb74145aa80e.Name <- "t1"
      Line_07b55205a97847268e57bb74145aa80e.X1 <- 0.0
      Line_07b55205a97847268e57bb74145aa80e.Y1 <- -108.0
      Line_07b55205a97847268e57bb74145aa80e.X2 <- 0.0
      Line_07b55205a97847268e57bb74145aa80e.Y2 <- -100.0
      Line_07b55205a97847268e57bb74145aa80e.Stroke <- SolidColorBrush(white)
      Line_07b55205a97847268e57bb74145aa80e.StrokeThickness <- 2.0
      let CompositeTransform_c98d398f3ea944409e8cb2a1a92c7881 = CompositeTransform()
      CompositeTransform_c98d398f3ea944409e8cb2a1a92c7881.Rotation <- 30.0
      CompositeTransform_c98d398f3ea944409e8cb2a1a92c7881.TranslateX <- 120.0
      CompositeTransform_c98d398f3ea944409e8cb2a1a92c7881.TranslateY <- 120.0
      Line_07b55205a97847268e57bb74145aa80e.RenderTransform <- CompositeTransform_c98d398f3ea944409e8cb2a1a92c7881
      this.RegisterName("t2", Line_fb3bc95e98b647bea6ae4a5f5618336)
      Line_fb3bc95e98b647bea6ae4a5f5618336.Name <- "t2"
      Line_fb3bc95e98b647bea6ae4a5f5618336.X1 <- 0.0
      Line_fb3bc95e98b647bea6ae4a5f5618336.Y1 <- -108.0
      Line_fb3bc95e98b647bea6ae4a5f5618336.X2 <- 0.0
      Line_fb3bc95e98b647bea6ae4a5f5618336.Y2 <- -100.0
      Line_fb3bc95e98b647bea6ae4a5f5618336.Stroke <- SolidColorBrush(white)
      Line_fb3bc95e98b647bea6ae4a5f5618336.StrokeThickness <- 2.0
      let CompositeTransform_c86c40df4f894159b06efc67b90e7c94 = CompositeTransform()
      CompositeTransform_c86c40df4f894159b06efc67b90e7c94.Rotation <- 60.0
      CompositeTransform_c86c40df4f894159b06efc67b90e7c94.TranslateX <- 120.0
      CompositeTransform_c86c40df4f894159b06efc67b90e7c94.TranslateY <- 120.0
      Line_fb3bc95e98b647bea6ae4a5f5618336.RenderTransform <- CompositeTransform_c86c40df4f894159b06efc67b90e7c94
      this.RegisterName("t3", Line_f62bafc10bd2428db6b124730c5bd68)
      Line_f62bafc10bd2428db6b124730c5bd68.Name <- "t3"
      Line_f62bafc10bd2428db6b124730c5bd68.X1 <- 0.0
      Line_f62bafc10bd2428db6b124730c5bd68.Y1 <- -116.0
      Line_f62bafc10bd2428db6b124730c5bd68.X2 <- 0.0
      Line_f62bafc10bd2428db6b124730c5bd68.Y2 <- -100.0
      Line_f62bafc10bd2428db6b124730c5bd68.Stroke <- SolidColorBrush(white)
      Line_f62bafc10bd2428db6b124730c5bd68.StrokeThickness <- 3.0
      let CompositeTransform_8c2367e3064947a7a032c79e174cbe81 = CompositeTransform()
      CompositeTransform_8c2367e3064947a7a032c79e174cbe81.Rotation <- 90.0
      CompositeTransform_8c2367e3064947a7a032c79e174cbe81.TranslateX <- 120.0
      CompositeTransform_8c2367e3064947a7a032c79e174cbe81.TranslateY <- 120.0
      Line_f62bafc10bd2428db6b124730c5bd68.RenderTransform <- CompositeTransform_8c2367e3064947a7a032c79e174cbe81
      this.RegisterName("t4", Line_3111bd2585ed41fdabf46071774c0ef)
      Line_3111bd2585ed41fdabf46071774c0ef.Name <- "t4"
      Line_3111bd2585ed41fdabf46071774c0ef.X1 <- 0.0
      Line_3111bd2585ed41fdabf46071774c0ef.Y1 <- -108.0
      Line_3111bd2585ed41fdabf46071774c0ef.X2 <- 0.0
      Line_3111bd2585ed41fdabf46071774c0ef.Y2 <- -100.0
      Line_3111bd2585ed41fdabf46071774c0ef.Stroke <- SolidColorBrush(white)
      Line_3111bd2585ed41fdabf46071774c0ef.StrokeThickness <- 2.0
      let CompositeTransform_0c6bc292556f41cd8c5fbd48a21f02e3 = CompositeTransform()
      CompositeTransform_0c6bc292556f41cd8c5fbd48a21f02e3.Rotation <- 120.0
      CompositeTransform_0c6bc292556f41cd8c5fbd48a21f02e3.TranslateX <- 120.0
      CompositeTransform_0c6bc292556f41cd8c5fbd48a21f02e3.TranslateY <- 120.0
      Line_3111bd2585ed41fdabf46071774c0ef.RenderTransform <- CompositeTransform_0c6bc292556f41cd8c5fbd48a21f02e3
      this.RegisterName("t5", Line_5bcc5b9f3acb429683fdaf56a617098c)
      Line_5bcc5b9f3acb429683fdaf56a617098c.Name <- "t5"
      Line_5bcc5b9f3acb429683fdaf56a617098c.X1 <- 0.0
      Line_5bcc5b9f3acb429683fdaf56a617098c.Y1 <- -108.0
      Line_5bcc5b9f3acb429683fdaf56a617098c.X2 <- 0.0
      Line_5bcc5b9f3acb429683fdaf56a617098c.Y2 <- -100.0
      Line_5bcc5b9f3acb429683fdaf56a617098c.Stroke <- SolidColorBrush(white)
      Line_5bcc5b9f3acb429683fdaf56a617098c.StrokeThickness <- 2.0
      let CompositeTransform_de40d1f2f83543479adff03ad1495ff4 = CompositeTransform()
      CompositeTransform_de40d1f2f83543479adff03ad1495ff4.Rotation <- 150.0
      CompositeTransform_de40d1f2f83543479adff03ad1495ff4.TranslateX <- 120.0
      CompositeTransform_de40d1f2f83543479adff03ad1495ff4.TranslateY <- 120.0
      Line_5bcc5b9f3acb429683fdaf56a617098c.RenderTransform <- CompositeTransform_de40d1f2f83543479adff03ad1495ff4
      this.RegisterName("t6", Line_e1c531f3b4324e81842edde85476ecef)
      Line_e1c531f3b4324e81842edde85476ecef.Name <- "t6"
      Line_e1c531f3b4324e81842edde85476ecef.X1 <- 120.0
      Line_e1c531f3b4324e81842edde85476ecef.Y1 <- 236.0
      Line_e1c531f3b4324e81842edde85476ecef.X2 <- 120.0
      Line_e1c531f3b4324e81842edde85476ecef.Y2 <- 220.0
      Line_e1c531f3b4324e81842edde85476ecef.Stroke <- SolidColorBrush(white)
      Line_e1c531f3b4324e81842edde85476ecef.StrokeThickness <- 3.0
      this.RegisterName("t7", Line_e525ac7b5f76403aa895b4e74cfaa204)
      Line_e525ac7b5f76403aa895b4e74cfaa204.Name <- "t7"
      Line_e525ac7b5f76403aa895b4e74cfaa204.X1 <- 0.0
      Line_e525ac7b5f76403aa895b4e74cfaa204.Y1 <- -108.0
      Line_e525ac7b5f76403aa895b4e74cfaa204.X2 <- 0.0
      Line_e525ac7b5f76403aa895b4e74cfaa204.Y2 <- -100.0
      Line_e525ac7b5f76403aa895b4e74cfaa204.Stroke <- SolidColorBrush(white)
      Line_e525ac7b5f76403aa895b4e74cfaa204.StrokeThickness <- 2.0
      let CompositeTransform_33250148d7b74ff49368dd100a0f3840 = CompositeTransform()
      CompositeTransform_33250148d7b74ff49368dd100a0f3840.Rotation <- 210.0
      CompositeTransform_33250148d7b74ff49368dd100a0f3840.TranslateX <- 120.0
      CompositeTransform_33250148d7b74ff49368dd100a0f3840.TranslateY <- 120.0
      Line_e525ac7b5f76403aa895b4e74cfaa204.RenderTransform <- CompositeTransform_33250148d7b74ff49368dd100a0f3840
      this.RegisterName("t8", Line_42c86cb3a7aa4d5ab2345ab6901ebba)
      Line_42c86cb3a7aa4d5ab2345ab6901ebba.Name <- "t8"
      Line_42c86cb3a7aa4d5ab2345ab6901ebba.X1 <- 0.0
      Line_42c86cb3a7aa4d5ab2345ab6901ebba.Y1 <- -108.0
      Line_42c86cb3a7aa4d5ab2345ab6901ebba.X2 <- 0.0
      Line_42c86cb3a7aa4d5ab2345ab6901ebba.Y2 <- -100.0
      Line_42c86cb3a7aa4d5ab2345ab6901ebba.Stroke <- SolidColorBrush(white)
      Line_42c86cb3a7aa4d5ab2345ab6901ebba.StrokeThickness <- 2.0
      let CompositeTransform_0222d130c0ad4ee4a3c68824964e3e93 = CompositeTransform()
      CompositeTransform_0222d130c0ad4ee4a3c68824964e3e93.Rotation <- 240.0
      CompositeTransform_0222d130c0ad4ee4a3c68824964e3e93.TranslateX <- 120.0
      CompositeTransform_0222d130c0ad4ee4a3c68824964e3e93.TranslateY <- 120.0
      Line_42c86cb3a7aa4d5ab2345ab6901ebba.RenderTransform <- CompositeTransform_0222d130c0ad4ee4a3c68824964e3e93
      this.RegisterName("t9", Line_1c44d98d5e90452792d12bf42b71e17c)
      Line_1c44d98d5e90452792d12bf42b71e17c.Name <- "t9"
      Line_1c44d98d5e90452792d12bf42b71e17c.X1 <- 0.0
      Line_1c44d98d5e90452792d12bf42b71e17c.Y1 <- -116.0
      Line_1c44d98d5e90452792d12bf42b71e17c.X2 <- 0.0
      Line_1c44d98d5e90452792d12bf42b71e17c.Y2 <- -100.0
      Line_1c44d98d5e90452792d12bf42b71e17c.Stroke <- SolidColorBrush(white)
      Line_1c44d98d5e90452792d12bf42b71e17c.StrokeThickness <- 3.0
      let CompositeTransform_7fc1efb20c734523bf0d0bbbe927af78 = CompositeTransform()
      CompositeTransform_7fc1efb20c734523bf0d0bbbe927af78.Rotation <- 270.0
      CompositeTransform_7fc1efb20c734523bf0d0bbbe927af78.TranslateX <- 120.0
      CompositeTransform_7fc1efb20c734523bf0d0bbbe927af78.TranslateY <- 120.0
      Line_1c44d98d5e90452792d12bf42b71e17c.RenderTransform <- CompositeTransform_7fc1efb20c734523bf0d0bbbe927af78
      this.RegisterName("t10", Line_9402226581604a909271e44e66f44e6c)
      Line_9402226581604a909271e44e66f44e6c.Name <- "t10"
      Line_9402226581604a909271e44e66f44e6c.X1 <- 0.0
      Line_9402226581604a909271e44e66f44e6c.Y1 <- -108.0
      Line_9402226581604a909271e44e66f44e6c.X2 <- 0.0
      Line_9402226581604a909271e44e66f44e6c.Y2 <- -100.0
      Line_9402226581604a909271e44e66f44e6c.Stroke <- SolidColorBrush(white)
      Line_9402226581604a909271e44e66f44e6c.StrokeThickness <- 2.0
      let CompositeTransform_4a22bc517bd74f29913366402bba021c = CompositeTransform()
      CompositeTransform_4a22bc517bd74f29913366402bba021c.Rotation <- 300.0
      CompositeTransform_4a22bc517bd74f29913366402bba021c.TranslateX <- 120.0
      CompositeTransform_4a22bc517bd74f29913366402bba021c.TranslateY <- 120.0
      Line_9402226581604a909271e44e66f44e6c.RenderTransform <- CompositeTransform_4a22bc517bd74f29913366402bba021c
      this.RegisterName("t11", Line_8274b4c25e614aa687fde6d2babf7a74)
      Line_8274b4c25e614aa687fde6d2babf7a74.Name <- "t11"
      Line_8274b4c25e614aa687fde6d2babf7a74.X1 <- 0.0
      Line_8274b4c25e614aa687fde6d2babf7a74.Y1 <- -108.0
      Line_8274b4c25e614aa687fde6d2babf7a74.X2 <- 0.0
      Line_8274b4c25e614aa687fde6d2babf7a74.Y2 <- -100.0
      Line_8274b4c25e614aa687fde6d2babf7a74.Stroke <- SolidColorBrush(white)
      Line_8274b4c25e614aa687fde6d2babf7a74.StrokeThickness <- 2.0
      let CompositeTransform_3a1d3723ec8f40a49254fa6c3fad78f0 = CompositeTransform()
      CompositeTransform_3a1d3723ec8f40a49254fa6c3fad78f0.Rotation <- 330.0
      CompositeTransform_3a1d3723ec8f40a49254fa6c3fad78f0.TranslateX <- 120.0
      CompositeTransform_3a1d3723ec8f40a49254fa6c3fad78f0.TranslateY <- 120.0
      Line_8274b4c25e614aa687fde6d2babf7a74.RenderTransform <- CompositeTransform_3a1d3723ec8f40a49254fa6c3fad78f0
      this.RegisterName("t12", Line_77de60c5678e4b9e8916dcde2aa365bf)
      Line_77de60c5678e4b9e8916dcde2aa365bf.Name <- "t12"
      Line_77de60c5678e4b9e8916dcde2aa365bf.X1 <- 120.0
      Line_77de60c5678e4b9e8916dcde2aa365bf.Y1 <- 4.0
      Line_77de60c5678e4b9e8916dcde2aa365bf.X2 <- 120.0
      Line_77de60c5678e4b9e8916dcde2aa365bf.Y2 <- 20.0
      Line_77de60c5678e4b9e8916dcde2aa365bf.Stroke <- SolidColorBrush(white)
      Line_77de60c5678e4b9e8916dcde2aa365bf.StrokeThickness <- 3.0
      this.RegisterName("hour", Line_13e17309867948ebab76151e70066531)
      Line_13e17309867948ebab76151e70066531.Name <- "hour"
      Line_13e17309867948ebab76151e70066531.X1 <- 0.0
      Line_13e17309867948ebab76151e70066531.Y1 <- 0.0
      Line_13e17309867948ebab76151e70066531.X2 <- 0.0
      Line_13e17309867948ebab76151e70066531.Y2 <- -60.0
      Line_13e17309867948ebab76151e70066531.Stroke <- SolidColorBrush(white)
      Line_13e17309867948ebab76151e70066531.StrokeThickness <- 4.0
      let CompositeTransform_72dbdf28372b4768a9e62c20cf2c2b92 = CompositeTransform()
      CompositeTransform_72dbdf28372b4768a9e62c20cf2c2b92.Rotation <- 0.0
      CompositeTransform_72dbdf28372b4768a9e62c20cf2c2b92.TranslateX <- 120.0
      CompositeTransform_72dbdf28372b4768a9e62c20cf2c2b92.TranslateY <- 120.0
      Line_13e17309867948ebab76151e70066531.RenderTransform <- CompositeTransform_72dbdf28372b4768a9e62c20cf2c2b92
      this.RegisterName("minute", Line_54fe6a97eca846f7865bf0fac5e23a47)
      Line_54fe6a97eca846f7865bf0fac5e23a47.Name <- "minute"
      Line_54fe6a97eca846f7865bf0fac5e23a47.X1 <- 0.0
      Line_54fe6a97eca846f7865bf0fac5e23a47.Y1 <- 0.0
      Line_54fe6a97eca846f7865bf0fac5e23a47.X2 <- 0.0
      Line_54fe6a97eca846f7865bf0fac5e23a47.Y2 <- -90.0
      Line_54fe6a97eca846f7865bf0fac5e23a47.Stroke <- SolidColorBrush(white)
      Line_54fe6a97eca846f7865bf0fac5e23a47.StrokeThickness <- 3.0
      let CompositeTransform_376e42b0dca34ba2b67a2b9284625b5e = CompositeTransform()
      CompositeTransform_376e42b0dca34ba2b67a2b9284625b5e.Rotation <- 0.0
      CompositeTransform_376e42b0dca34ba2b67a2b9284625b5e.TranslateX <- 120.0
      CompositeTransform_376e42b0dca34ba2b67a2b9284625b5e.TranslateY <- 120.0
      Line_54fe6a97eca846f7865bf0fac5e23a47.RenderTransform <- CompositeTransform_376e42b0dca34ba2b67a2b9284625b5e
      this.RegisterName("second", Line_073c33c5d0b74cb18a795dd9d80c1741)
      Line_073c33c5d0b74cb18a795dd9d80c1741.Name <- "second"
      Line_073c33c5d0b74cb18a795dd9d80c1741.X1 <- 0.0
      Line_073c33c5d0b74cb18a795dd9d80c1741.Y1 <- 0.0
      Line_073c33c5d0b74cb18a795dd9d80c1741.X2 <- 0.0
      Line_073c33c5d0b74cb18a795dd9d80c1741.Y2 <- -85.0
      Line_073c33c5d0b74cb18a795dd9d80c1741.Stroke <- SolidColorBrush(white)
      Line_073c33c5d0b74cb18a795dd9d80c1741.StrokeThickness <- 1.0
      let CompositeTransform_c000617de7e7473db3ecc0fc8406d186 = CompositeTransform()
      CompositeTransform_c000617de7e7473db3ecc0fc8406d186.Rotation <- 0.0
      CompositeTransform_c000617de7e7473db3ecc0fc8406d186.TranslateX <- 120.0
      CompositeTransform_c000617de7e7473db3ecc0fc8406d186.TranslateY <- 120.0
      Line_073c33c5d0b74cb18a795dd9d80c1741.RenderTransform <- CompositeTransform_c000617de7e7473db3ecc0fc8406d186
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(TextBlock_de66fe85b8f3438ebc19dbd944bfc1cd)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(TextBlock_486ee42817174faf90971786822d41b5)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(TextBlock_78eae0252b22427680509cba47b1c287)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(TextBlock_c8fbfb22281d42f89936e8eb63b730dd)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Path_6bdc3f886ca24b33ad618db3d2498060)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Path_aedd051fe35e466ea3cb612eea7de959)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Path_6d547957803f431280a93cf039aad576)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Path_6a4ed605ffb64e919b7ac2434392740a)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Path_5665bc5d56774050ba9a8d88d6641d23)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Path_9a23d0d2e8624a4cb3cfa7276659b7df)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Path_894e7284504c43d3a1d7482844df77f)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Ellipse_191634773284424c92edf060a412661c)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_07b55205a97847268e57bb74145aa80e)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_fb3bc95e98b647bea6ae4a5f5618336)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_f62bafc10bd2428db6b124730c5bd68)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_3111bd2585ed41fdabf46071774c0ef)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_5bcc5b9f3acb429683fdaf56a617098c)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_e1c531f3b4324e81842edde85476ecef)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_e525ac7b5f76403aa895b4e74cfaa204)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_42c86cb3a7aa4d5ab2345ab6901ebba)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_1c44d98d5e90452792d12bf42b71e17c)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_9402226581604a909271e44e66f44e6c)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_8274b4c25e614aa687fde6d2babf7a74)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_77de60c5678e4b9e8916dcde2aa365bf)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_13e17309867948ebab76151e70066531)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_54fe6a97eca846f7865bf0fac5e23a47)
      Canvas_dd0e680653b6466babf48a3176a4babf.Children.Add(Line_073c33c5d0b74cb18a795dd9d80c1741)
      Canvas_316cb7aad42e47789407fcffdd7a04a6.Height <- 72.0
      Canvas_316cb7aad42e47789407fcffdd7a04a6.Margin <- Thickness(0.0, 0.0, 0.0, 51.0)
      this.RegisterName("expander1", Canvas_316cb7aad42e47789407fcffdd7a04a6)
      Canvas_316cb7aad42e47789407fcffdd7a04a6.Name <- "expander1"
      Canvas_316cb7aad42e47789407fcffdd7a04a6.VerticalAlignment <- VerticalAlignment.Bottom
      TextBlock_baff82024ac64fe499f12bfb43e02ede.Height <- 28.0
      Canvas.SetTop(TextBlock_baff82024ac64fe499f12bfb43e02ede, 0.0)
      this.RegisterName("label1", TextBlock_baff82024ac64fe499f12bfb43e02ede)
      TextBlock_baff82024ac64fe499f12bfb43e02ede.Name <- "label1"
      TextBlock_baff82024ac64fe499f12bfb43e02ede.VerticalAlignment <- VerticalAlignment.Bottom
      TextBlock_baff82024ac64fe499f12bfb43e02ede.HorizontalAlignment <- HorizontalAlignment.Left
      TextBlock_baff82024ac64fe499f12bfb43e02ede.Width <- 87.0
      TextBlock_baff82024ac64fe499f12bfb43e02ede.Text <- ""
      TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3.Height <- 28.0
      Canvas.SetTop(TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3, 28.0)
      this.RegisterName("label2", TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3)
      TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3.Name <- "label2"
      TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3.VerticalAlignment <- VerticalAlignment.Bottom
      TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3.HorizontalAlignment <- HorizontalAlignment.Left
      TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3.Width <- 87.0
      TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3.Text <- ""
      Slider_f77653af2d59408881db98942bea491c.Orientation <- Orientation.Horizontal
      Slider_f77653af2d59408881db98942bea491c.Height <- 22.0
      Canvas.SetTop(Slider_f77653af2d59408881db98942bea491c, 0.0)
      Canvas.SetLeft(Slider_f77653af2d59408881db98942bea491c, 120.0)
      this.RegisterName("slider1", Slider_f77653af2d59408881db98942bea491c)
      Slider_f77653af2d59408881db98942bea491c.Name <- "slider1"
      Slider_f77653af2d59408881db98942bea491c.VerticalAlignment <- VerticalAlignment.Bottom
      Slider_f77653af2d59408881db98942bea491c.Maximum <- 90.0
      Slider_f77653af2d59408881db98942bea491c.Minimum <- -90.0
      Slider_f77653af2d59408881db98942bea491c.LargeChange <- 10.0
      Slider_f77653af2d59408881db98942bea491c.SmallChange <- 1.0
      Slider_f77653af2d59408881db98942bea491c.Width <- 360.0
      Slider_f77653af2d59408881db98942bea491c.Value <- 52.0
      Slider_f77653af2d59408881db98942bea491c.Background <- SolidColorBrush(silver)
      Slider_94ef856dcb2a4c80be45b6139f5145e5.Orientation <- Orientation.Horizontal
      Slider_94ef856dcb2a4c80be45b6139f5145e5.Height <- 22.0
      Canvas.SetTop(Slider_94ef856dcb2a4c80be45b6139f5145e5, 28.0)
      Canvas.SetLeft(Slider_94ef856dcb2a4c80be45b6139f5145e5, 120.0)
      this.RegisterName("slider2", Slider_94ef856dcb2a4c80be45b6139f5145e5)
      Slider_94ef856dcb2a4c80be45b6139f5145e5.Name <- "slider2"
      Slider_94ef856dcb2a4c80be45b6139f5145e5.VerticalAlignment <- VerticalAlignment.Bottom
      Slider_94ef856dcb2a4c80be45b6139f5145e5.Maximum <- 180.0
      Slider_94ef856dcb2a4c80be45b6139f5145e5.Minimum <- -180.0
      Slider_94ef856dcb2a4c80be45b6139f5145e5.LargeChange <- 10.0
      Slider_94ef856dcb2a4c80be45b6139f5145e5.SmallChange <- 1.0
      Slider_94ef856dcb2a4c80be45b6139f5145e5.Width <- 360.0
      Slider_94ef856dcb2a4c80be45b6139f5145e5.Value <- 0.0
      Slider_94ef856dcb2a4c80be45b6139f5145e5.Background <- SolidColorBrush(silver)
      Canvas_316cb7aad42e47789407fcffdd7a04a6.Children.Add(TextBlock_baff82024ac64fe499f12bfb43e02ede)
      Canvas_316cb7aad42e47789407fcffdd7a04a6.Children.Add(TextBlock_4890043a93ca4d5ba3c60b0fbb0146e3)
      Canvas_316cb7aad42e47789407fcffdd7a04a6.Children.Add(Slider_f77653af2d59408881db98942bea491c)
      Canvas_316cb7aad42e47789407fcffdd7a04a6.Children.Add(Slider_94ef856dcb2a4c80be45b6139f5145e5)
      HyperlinkButton_d406e374622b47ecadb424ed65015a8e.Height <- 28.0
      HyperlinkButton_d406e374622b47ecadb424ed65015a8e.HorizontalAlignment <- HorizontalAlignment.Left
      HyperlinkButton_d406e374622b47ecadb424ed65015a8e.Margin <- Thickness(0.0, 0.0, 0.0, 17.0)
      this.RegisterName("hyperlink", HyperlinkButton_d406e374622b47ecadb424ed65015a8e)
      HyperlinkButton_d406e374622b47ecadb424ed65015a8e.Name <- "hyperlink"
      HyperlinkButton_d406e374622b47ecadb424ed65015a8e.VerticalAlignment <- VerticalAlignment.Bottom
      HyperlinkButton_d406e374622b47ecadb424ed65015a8e.Width <- 87.0
      HyperlinkButton_d406e374622b47ecadb424ed65015a8e.NavigateUri <- DotNetForHtml5.Core.TypeFromStringConverters.ConvertFromInvariantString(typeof<Uri>, "astroclock.html") :?> Uri
      HyperlinkButton_d406e374622b47ecadb424ed65015a8e.Content <- "Permalink"
      Button_d88415801d1f4a5f809c0944f85c9ccd.Height <- 23.0
      Button_d88415801d1f4a5f809c0944f85c9ccd.HorizontalAlignment <- HorizontalAlignment.Right
      Button_d88415801d1f4a5f809c0944f85c9ccd.Margin <- Thickness(0.0, 0.0, 12.0, 22.0)
      this.RegisterName("button1", Button_d88415801d1f4a5f809c0944f85c9ccd)
      Button_d88415801d1f4a5f809c0944f85c9ccd.Name <- "button1"
      Button_d88415801d1f4a5f809c0944f85c9ccd.VerticalAlignment <- VerticalAlignment.Bottom
      Button_d88415801d1f4a5f809c0944f85c9ccd.Width <- 75.0
      Button_d88415801d1f4a5f809c0944f85c9ccd.Content <- "Configure"
      let TextBlock_2fbc9aec67c34842b6df1873f6eb05cd = TextBlock()
      TextBlock_2fbc9aec67c34842b6df1873f6eb05cd.VerticalAlignment <- VerticalAlignment.Bottom
      TextBlock_2fbc9aec67c34842b6df1873f6eb05cd.TextWrapping <- TextWrapping.Wrap
      TextBlock_2fbc9aec67c34842b6df1873f6eb05cd.Text <- " The observer's location can be altered at any time with the slider controls. "
      let TextBlock_7636bd228b5b4b139d3195af47889274 = TextBlock()
      TextBlock_7636bd228b5b4b139d3195af47889274.TextWrapping <- TextWrapping.Wrap
      TextBlock_7636bd228b5b4b139d3195af47889274.VerticalAlignment <- VerticalAlignment.Bottom
      let Run_5c10b96aef0042b9b7786138c39fc93a = System.Windows.Documents.Run()
      Run_5c10b96aef0042b9b7786138c39fc93a.Text <- "Planetary positions, moon phases, etc. have not been rigorously tested away from roughly zero longitude but at least BST gives me some confidence in timezones. Check with "
      let Hyperlink_21e21c68f2ff4fa4844fb52569cf83b6 = System.Windows.Documents.Hyperlink()
      Hyperlink_21e21c68f2ff4fa4844fb52569cf83b6.NavigateUri <- DotNetForHtml5.Core.TypeFromStringConverters.ConvertFromInvariantString(typeof<Uri>, "http://www.fourmilab.ch/cgi-bin/Yoursky") :?> Uri
      let Run_75013b486d584c44bdf534a7b99a89ba = System.Windows.Documents.Run()
      Run_75013b486d584c44bdf534a7b99a89ba.Text <- "the \"Your Sky\" map"
      Hyperlink_21e21c68f2ff4fa4844fb52569cf83b6.Inlines.Add(Run_75013b486d584c44bdf534a7b99a89ba)
      let Run_e2dc78b00b3a4786b477b74eee6443c0 = System.Windows.Documents.Run()
      Run_e2dc78b00b3a4786b477b74eee6443c0.Text <- " to confirm."
      TextBlock_7636bd228b5b4b139d3195af47889274.Inlines.Add(Run_5c10b96aef0042b9b7786138c39fc93a)
      TextBlock_7636bd228b5b4b139d3195af47889274.Inlines.Add(Hyperlink_21e21c68f2ff4fa4844fb52569cf83b6)
      TextBlock_7636bd228b5b4b139d3195af47889274.Inlines.Add(Run_e2dc78b00b3a4786b477b74eee6443c0)
      StackPanel_a6f8a1caa8314c37b67b8a919fc985b4.Children.Add(TextBlock_9aea36c2b85447eaa82be834e5c453fc)
      StackPanel_a6f8a1caa8314c37b67b8a919fc985b4.Children.Add(TextBlock_0472d9dbdef54c539467b966fe38bafd)
      StackPanel_a6f8a1caa8314c37b67b8a919fc985b4.Children.Add(Canvas_dd0e680653b6466babf48a3176a4babf)
      StackPanel_a6f8a1caa8314c37b67b8a919fc985b4.Children.Add(Canvas_316cb7aad42e47789407fcffdd7a04a6)
      StackPanel_a6f8a1caa8314c37b67b8a919fc985b4.Children.Add(HyperlinkButton_d406e374622b47ecadb424ed65015a8e)
      StackPanel_a6f8a1caa8314c37b67b8a919fc985b4.Children.Add(Button_d88415801d1f4a5f809c0944f85c9ccd)
      StackPanel_a6f8a1caa8314c37b67b8a919fc985b4.Children.Add(TextBlock_2fbc9aec67c34842b6df1873f6eb05cd)
      StackPanel_a6f8a1caa8314c37b67b8a919fc985b4.Children.Add(TextBlock_7636bd228b5b4b139d3195af47889274)
      Grid_df7b4b7af1c248b38448cdb6a696d9e.Children.Add(StackPanel_a6f8a1caa8314c37b67b8a919fc985b4)
      base.Content <- Grid_df7b4b7af1c248b38448cdb6a696d9e

    member this.Begin() =
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

       this.UpdateSky ()
       this.UpdateTick ()

        //we set the time between each tick of the DispatcherTimer to 1 second:
       this.timer.Interval <- TimeSpan(0, 0, 0, 1)
       let tick = EventHandler(fun _ _ -> this.UpdateSky ()
                                          this.UpdateTick ())
       this.timer.add_Tick tick
       this.timer.Start()

    new () as this = AstroPage(0) then
      this.InitializeComponent()
end

type AstroApp = class
    inherit Application

    member this.InitializeComponent() =
      StartupAssemblyInfo.OutputRootPath <- "wwwroot\\";
      StartupAssemblyInfo.OutputAppFilesPath <- "app\\";
      StartupAssemblyInfo.OutputLibrariesPath <- "libs\\";
      StartupAssemblyInfo.OutputResourcesPath <- "resources\\";
      base.Resources <- ResourceDictionary()

    new () as this = {} then
        this.InitializeComponent()
        let page = new AstroPage()
        Window.Current.Content <- page
        this.RootVisual <- page
        this.Startup.Add(fun _ -> page.Begin())

end