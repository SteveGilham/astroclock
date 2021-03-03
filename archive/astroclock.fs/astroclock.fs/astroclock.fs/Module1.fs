#light
namespace astroclock.fs
open System
open System.ComponentModel
open System.Globalization
open System.Windows
open System.Windows.Browser
open System.Windows.Controls
open System.Windows.Data
open System.Windows.Markup
open System.Windows.Media
open System.Windows.Shapes

open Computation

type Page = class
    inherit UserControl
    
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
    
    member this.everySecond() =
        System.Threading.Thread.Sleep(1000)
        this.animate.ReportProgress(0)
        this.everySecond()
        
    member this.updateSky () =
      let display = {latitude = this.latitude * 1.0<deg>; 
                     longitude = this.longitude * 1.0<deg>; 
                     centreX = 120.0;
                     centreY = 120.0;
                     radius = 100.0;}
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

    member this.everyMinute() =
        System.Threading.Thread.Sleep(60*1000)
        this.updateSky () 
        this.everyMinute()        
        
    member this.moveHand name angle =
      let line = this.FindName(name) :?> Line
      let tg = line.RenderTransform :?> TransformGroup
      let rotate = tg.Children.[0] :?> RotateTransform
      rotate.Angle <- angle
      
    member this.setText name value =
      let block = this.FindName(name) :?> TextBlock
      block.Text <- value
      block
      
    member this.setPlace name where =
      let item = this.FindName(name) :?> Path
      (item.Data :?> EllipseGeometry).Center <- Point(where.x, where.y)
      item.Visibility <- select where.visible Visibility.Visible Visibility.Collapsed
      
    member this.drawMoon () =
      let item = this.FindName("moon") :?> Path
      let where = this.moon
      let figure = (item.Data :?> PathGeometry).Figures.[0]
      let rh = (figure.Segments.[0] :?> ArcSegment)
      let lh = (figure.Segments.[1] :?> ArcSegment)
      item.Visibility <- select where.visible Visibility.Visible Visibility.Collapsed
            
      figure.StartPoint <- Point(where.x, where.y - 5.0)
      rh.Point <- Point(where.x, where.y + 5.0)
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
      
      (*
      if this.phase > 0.0 then //waxing
        rh.Size <- new Size(5.0, 5.0)
        if tmp > 0.5 then // gibbous
          lh.Size <- new Size(10.0 * (tmp - 0.5), 5.0)
        else
          lh.Size <- new Size(10.0 * (0.5 - tmp), 5.0)
          lh.SweepDirection <- SweepDirection.Counterclockwise
      else // waning
        lh.Size <- new Size(5.0, 5.0)
        if tmp > 0.5 then // gibbous
          rh.Size <- new Size(10.0 * (tmp - 0.5), 5.0)
        else
          rh.Size <- new Size(10.0 * (0.5 - tmp), 5.0)
          rh.SweepDirection <- SweepDirection.Counterclockwise
       *)


      
    member this.updateTick() =
        let now = System.DateTime.Now
        this.setText "hms" (now.ToString(this.hms, this.culture))   |> ignore
        this.setText "day" (now.ToString("dddd", this.culture))     |> ignore
        this.setText "date" (now.ToString(this.date, this.culture)) |> ignore
        try 
          this.moveHand "second" (6.0 * float(now.Second))
          this.moveHand "minute" (float(6*now.Minute)+(0.1*float(now.Second)))
          this.moveHand "hour" (float(30*now.Hour)+(float(now.Minute)/2.0)+(float(now.Second)/300.0))
          this.setText "sunup" (String.Format("{0}-{1}", this.sunup, this.sundown)) |> ignore
          this.setPlace "sun" this.sun
          merge this.setPlace this.planetNames this.planets
          this.drawMoon ()    
        with x -> // debug technique
          (this.setText "sunup" x.Message).FontSize <- 8.0
          
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
          
    member this.getParam<'T> (name:string) (transform:(string->'T)) (fallback:'T) : ('T * bool)  =
       try
          let value = this.query.[name]
          ((transform value), true)
       with _ ->
          (fallback, false)
          
    member this.toggleUI state =
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
      
    member this.button1_Click() =
      let button = this.FindName("button1") :?> UIElement
      let configured = (button.Visibility = Visibility.Visible)
      this.toggleUI (not configured)
      
    member this.updateURL () =
      let hyperlink = this.FindName("hyperlink") :?> HyperlinkButton
      hyperlink.NavigateUri <- System.Uri(String.Format("{0}?lat={1}&long={2}", 
                                           this.page, this.Latitude, this.Longitude))
                                           
    member this.latValueChanged() =
      let lat = this.FindName("slider1") :?> Slider
      this.Latitude <- lat.Value
      this.setText "label1" this.LatitudeCaption |> ignore
      this.updateSky () 
      this.updateURL ()
      
    member this.longValueChanged() =
      let lat = this.FindName("slider2") :?> Slider
      this.Longitude <- lat.Value
      this.setText "label2" this.LongitudeCaption |> ignore      
      this.updateSky () 
      this.updateURL ()
    
    new () as this = {page = String.Empty; 
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
                      sun = {x=60.0; y=60.0; visible=false;}
                      planets = [];
                      moon = {x=60.0; y=60.0; visible=true;}                      
                      planetNames = ["mercury"; "venus"; "mars"; "jupiter"; "saturn"];
                      phase = 0.5
                      } then
       System.Windows.Application.LoadComponent(this, new System.Uri("/astroclock.xaml", System.UriKind.Relative));
       this.query <- System.Windows.Browser.HtmlPage.Document.QueryString
       
       this.page <- System.Windows.Browser.HtmlPage.Document.DocumentUri.GetComponents(
                          System.UriComponents.SchemeAndServer ||| System.UriComponents.Path,
                          System.UriFormat.Unescaped).Split( [|'?'|]).[0]
          
       (this.setText "label1" this.LatitudeCaption).DataContext <- this.LatitudeCaption
       (this.setText "label2" this.LongitudeCaption).DataContext <- this.LongitudeCaption
       
       (this.FindName("button1") :?> Button).Click.Add(fun _ -> this.button1_Click())
       
       this.culture <- fst (this.getParam "locale" (fun x -> new CultureInfo(x)) CultureInfo.CurrentCulture) 
       this.hms <- fst ( this.getParam "hms" (fun x -> x) this.culture.DateTimeFormat.LongTimePattern  ) // HH:mm:ss
       this.date <- fst ( this.getParam "date" (fun x -> x) this.culture.DateTimeFormat.ShortDatePattern ) // d-MMM-yyyy
       
       (this.FindName("slider1") :?> Slider).ValueChanged.Add(fun _ -> this.latValueChanged())
       let islat = this.getParam "lat" (fun x -> Double.Parse(x)) this.latitude
       this.latitude <- fst(islat)
       (this.FindName("slider1") :?> Slider).Value <- this.latitude
       
       (this.FindName("slider2") :?> Slider).ValueChanged.Add(fun _ -> this.longValueChanged())
       let islong = this.getParam "long" (fun x -> Double.Parse(x)) this.longitude
       this.longitude <- fst(islong)
       (this.FindName("slider2") :?> Slider).Value <- this.longitude
       
       let version = System.Reflection.Assembly.GetExecutingAssembly().FullName.ToString()
       let bits = version.Split([|','|])
       let legend = String.Format("Astroclock {0}", bits.[1])
       this.setText "legend" legend |> ignore
       
       
       (*
       let grid = this.FindName("LayoutRoot") :?> Grid
       let tip = ToolTipService.GetToolTip(grid) :?> ContentControl
       tip.Content <- legend
       *)
             
       this.toggleUI ( snd(islat) && snd(islong) )

       this.compute.DoWork.Add(fun _ -> this.everyMinute())
       this.compute.RunWorkerAsync()
       this.updateSky ()
                       
       this.animate.WorkerReportsProgress <- true
       this.animate.DoWork.Add(fun _ -> this.everySecond())
       this.animate.ProgressChanged.Add(fun _ -> this.updateTick())
       this.animate.RunWorkerAsync()
       this.updateTick ()
       

       
end

type MyApp = class
    inherit Application
    
    new () as this = {} then
        this.Startup.Add(fun _ ->  this.RootVisual <- new Page())

end    
