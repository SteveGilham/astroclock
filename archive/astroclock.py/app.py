# formulae from http://stjarnhimlen.se/comp/ppcomp.html

import math

from System.Windows import Application, Visibility, Point
from System.Windows.Controls import Canvas
import System
import System.Windows.Browser
from System.ComponentModel import BackgroundWorker
from System.Threading import Thread
from System.Globalization import CultureInfo
from System.Windows.Media import SweepDirection
from System.Windows import Size

##========================================================================

def ignore(x):
  return None
  
def select (a, b, c):
  if a:
    return b
  else:
    return c

def merge (f, l1, l2):
  for (a,b) in zip(l1,l2):
	f(a,b)

class Planet:
  def __init__(self, N_, i_, w_, a_, e_, M_):
    self.N = N_
    self.i = i_
    self.w = w_
    self.a = a_
    self.e = e_
    self.M = M_

class Plot:
  def __init__(self, x_, y_, v_):
    self.x = x_
    self.y = y_
    self.visible = v_
 
class Display:
  def __init__(self, lat, long, cX, cY, r):
    self.latitude = lat
    self.longitude = long
    self.centreX = cX
    self.centreY = cY
    self.radius = r
 
def degToRad (x):
  toRadians = math.pi/180.0
  return x * toRadians
  
def radToDeg (x):
  return x * 180.0/math.pi

## Time-keeping

def dayNumber (y, m, d):
 return 367*y - 7*(y+(m+9)/12)/4  + 275*m/9 + d - 730530
  
def dayFrac (d):##DateTimeOffset) =
  return ( d.Hour+(d.Minute/60.0) ) /24.0

def decimalDay (date):##DateTimeOffset) =
  start = dayNumber (date.Year, date.Month, date.Day)
  return start + dayFrac(date)
  
def now ():
  return decimalDay(System.DateTimeOffset.UtcNow)
  
def localNoon(): 
  now = System.DateTimeOffset.Now
  r = System.DateTimeOffset(now.Year, now.Month, now.Day, 12,0,0, now.Offset)
  return r.ToUniversalTime()
 
def diffInSeconds (dt1, dt2):##:DateTimeOffset) (dt2:DateTimeOffset) =
  span = dt1.Subtract(dt2)
  return abs(span.Ticks / System.TimeSpan.TicksPerSecond)

 
## Planetary motions  
    
def Sun(d): # decimalDay
  return Planet(
    degToRad(0.0),
    degToRad(0.0),
    degToRad (282.9404 + 4.70935E-5 * d),
    1.000000,
    0.016709 - 1.151E-9 * d,
    degToRad (356.0470 + 0.9856002585 * d))

def Moon(d):
  return Planet(
    degToRad (125.1228 - 0.0529538083 * d),
    degToRad (5.1454),
    degToRad (318.0634 + 0.1643573223 * d),
    60.2666, ##  (Earth radii)
    0.054900,
    degToRad (115.3654 + 13.0649929509 * d))

def Mercury (d):
  return Planet(
    degToRad (48.3313 + 3.24587E-5 * d),
    degToRad (7.0047 + 5.00E-8 * d),
    degToRad (29.1241 + 1.01444E-5 * d),
    0.387098,
    0.205635 + 5.59E-10 * d,
    degToRad (168.6562 + 4.0923344368 * d))

def Venus (d):
  return Planet(
    degToRad (76.6799 + 2.46590E-5 * d),
    degToRad (3.3946 + 2.75E-8 * d),
    degToRad (54.8910 + 1.38374E-5 * d),
    0.723330,
    0.006773 - 1.302E-9 * d,
    degToRad (48.0052 + 1.6021302244 * d))

def Mars (d):
  return Planet (
    degToRad (49.5574 + 2.11081E-5 * d),
    degToRad (1.8497 - 1.78E-8 * d),
    degToRad (286.5016 + 2.92961E-5 * d),
    1.523688,
    0.093405 + 2.516E-9 * d,
    degToRad (18.6021 + 0.5240207766 * d) )

def Jupiter (d):
  return Planet (
    degToRad (100.4542 + 2.76854E-5 * d),
    degToRad (1.3030 - 1.557E-7 * d),
    degToRad (273.8777 + 1.64505E-5 * d),
    5.20256,
    0.048498 + 4.469E-9 * d,
    degToRad (19.8950 + 0.0830853001 * d))

def Saturn (d):
  return Planet(
    degToRad (113.6634 + 2.38980E-5 * d),
    degToRad (2.4886 - 1.081E-7 * d),
    degToRad (339.3939 + 2.97661E-5 * d),
    9.55475,
    0.055546 - 9.499E-9 * d,
    degToRad (316.9670 + 0.0334442282 * d) )
     
planets = [Mercury, Venus, Mars, Jupiter, Saturn]

## geometry

def sq (t):
  return t * t

def radiusFromCartesian2 (x, y):
  r2 = sq(x) + sq(y)
  return math.sqrt (r2)
  
def radiusFromCartesian3 (x, y, z):
  r2 = sq (x) + sq (y) + sq (z)
  return math.sqrt (r2) 

def polarFromCartesian2 (x, y):
  theta = math.atan2 (y, x)
  return (theta, radiusFromCartesian2 (x, y))
  
def polarFromCartesian3 (x, y, z):
  RA  = math.atan2 (y, x) 
  Dec = math.atan2 (z,  (radiusFromCartesian2 (x, y)))
  rg = radiusFromCartesian3 (x, y, z)
  return (RA, Dec, rg)  
  
def cartesianFromPolar2 (r, theta):
  x = r * math.cos(theta)
  y = r * math.sin(theta)
  return (x, y)
  
def ellipseToPolar (semiMajor, eccentricity, anomaly):
  xv = semiMajor * ( math.cos (anomaly) - eccentricity )
  yv = semiMajor * ( math.sqrt(1.0 - sq( eccentricity)) * math.sin (anomaly) )
  return polarFromCartesian2 (xv, yv)  
  
def rotate2 (x, y, theta):
  xo = x * math.cos (theta) - y * math.sin (theta)
  yo = x * math.sin (theta) + y * math.cos (theta)
  return (xo, yo)

##  Sun's upper limb touches the horizon; atmospheric refraction accounted for
sunhide = math.sin (degToRad (-0.833))
  
def altazToPlot (alt, az, display):
  global sunhide
  r = display.radius * (1.0 - 2.0* (alt/math.pi))
  return Plot(display.centreX - r*math.sin(az), display.centreY - r * math.cos (az), alt > sunhide )
  


## Ephemeris computations

def ecl (d): ## = //obliquity of ecliptic
  return degToRad(23.4393 - 3.563E-7 * d)
  
def getGMST0(sun):
  def reduce (a):
    if a > 360:
      return reduce(a - 360)
    else:
      return a
  return reduce (180.0 + radToDeg (sun.M + sun.w))
  
def SunLongitude (dsun):
  E = dsun.M + dsun.e * math.sin (dsun.M) * ( 1.0 + dsun.e * math.cos (dsun.M) )
  (v,r) = ellipseToPolar (1.0, dsun.e, E)
  lonsun = v + dsun.w
  return (v, r, lonsun)

def SunPos (d):## = // decimalDay
  dsun = Sun (d)
  (v,r, lonsun) = SunLongitude (dsun)

  lonsun = v + dsun.w
  (xs,ys) = cartesianFromPolar2 (r, lonsun)

  xe = xs
  (ye, ze) = cartesianFromPolar2 (ys, ecl (d))
  
  (RA, Dec, rg) = polarFromCartesian3 (xe, ye, ze)
  return (RA, Dec, dsun)

def LHA (d, longitude, RA): ## = // RA radians, return radians
  def  LST (d, longitude): ## d = (*UTC*)
    quant = Sun (decimalDay(d))
    return getGMST0 (quant) + longitude + 360.0 * dayFrac (d)
  return degToRad(LST (d, longitude) ) - RA
  
def altaz (dec, lat, lha):## = // #radian, deg, radian
    xt  =  math.asin ( math.sin(dec) * math.sin(lat) +
                  math.cos(dec) * math.cos(lat) * math.cos(lha) )
    yt  =  math.acos ( ( math.sin(dec) - math.sin(lat) * math.sin(xt) ) /
                  ( math.cos(lat) * math.cos(xt) ) )
    if math.sin(lha) > 0: 
      return (xt, 2.0*math.pi - yt)
    return(xt, yt)

  
def getSunHorizonLHA (prev, lat):
  (RA, decl, quant) = SunPos(decimalDay(prev))
  cosLHA = (sunhide - math.sin(lat)*math.sin(decl))/(math.cos(lat)*math.cos(decl))
  if cosLHA >= 1.0:
	return (False, "+", 0.0)
  if cosLHA <= -1.0:
    return (False, "-", 0.0)
  return (True, System.String.Empty, cosLHA)

def localsolarnoon (prev, longitude):
  (RA, decl, quant) = SunPos(decimalDay(prev))

  UT_Sun_in_south = ( (RA*180.0/math.pi) - getGMST0 (quant) - longitude ) / 360.0
  utcnow = System.DateTimeOffset.UtcNow
  utczero = System.DateTimeOffset(utcnow.Year, utcnow.Month, utcnow.Day, 0,0,0, (System.TimeSpan(0)) )
  ticks = UT_Sun_in_south * System.TimeSpan.TicksPerDay
  utcsolarnoon = utczero.Add( System.TimeSpan(ticks))
  return utcsolarnoon.ToLocalTime()
    
  
def sunrise (isSunup, display):
  lat = degToRad (display.latitude)

  def getNext (sign,  cosLHA,  noon):# DateTimeOffset) =
    lha = System.TimeSpan.TicksPerHour * sign * math.acos (cosLHA) / (math.pi * 15.04107) ## adjusted ticks 
    return noon.Add( System.TimeSpan(lha) )
    
  def step (guess, lat, iterate, sign):
    (good, mark, cosLHA) = getSunHorizonLHA (guess, lat)
    if not good:
      return mark
    noon = localsolarnoon (guess, display.longitude)
    return iterate (guess, getNext(sign, cosLHA,  noon), lat, sign,  cosLHA)

  def iterate (prev, next, lat, sign,  cosLHA):
    jolt = diffInSeconds (prev, next)
    if jolt > 30:
	  return step (next, lat, iterate, sign)
    return next.ToString("HH:mm")
      
  prev = localNoon()
  sign = select (isSunup, -180.0, 180.0)
  
  return step (prev, lat, iterate, sign)

def oneAltaz (radec, d, display):
  (RA, Dec, rg) = radec
  lha = LHA (d, display.longitude, RA)
  (alt,az) = altaz (Dec, degToRad(display.latitude), lha)
  return altazToPlot (alt, az, display)


def eccAnomaly (planet):
  def next (e0, planet):
    e1 = e0 - ( e0 - planet.e * math.sin (e0) - planet.M ) / ( 1.0 - planet.e * math.cos (e0) )
    if abs (e1 - e0) < 1.0e-8:
      return e1
    return next(e1, planet)
	
  e0 = planet.M + planet.e * math.sin (planet.M) * ( 1.0 + planet.e * math.cos (planet.M) )
  return next (e0, planet)
  
def position (planet):
  return ellipseToPolar (planet.a, planet.e, eccAnomaly (planet))
 
def primaryCentric (planet):
  (v, r) = position (planet)
  vw = v + planet.w
  n = planet.N
  i = planet.i
  
  xh = r * ( math.cos(n) * math.cos(vw) - math.sin(n) * math.sin(vw) * math.cos(i) )
  yh = r * ( math.sin(n) * math.cos(vw) + math.cos(n) * math.sin(vw) * math.cos(i) )
  zh = r * ( math.sin(vw) * math.sin(i) )
  
  return (xh, yh, zh)
  
def geocentric (planet, sun):
  (xh, yh, zh) = planet
  (xs, ys, zs) = sun
  return (xh+xs, yh+ys, zh)
  
def radec (planet, sun, d):
  (xg, yg, zg) = geocentric( primaryCentric (planet), primaryCentric (sun))
  (ye, ze) = rotate2 (yg, zg, ecl (d)) 
  return polarFromCartesian3 (xg, ye, ze)

def computeRadecs (d, plist):
  sun = Sun(d)
  return [radec( p(d), sun, d) for p in plist]
  
def computeAltaz (d, plist, display):
  radecs = computeRadecs (decimalDay (d), plist)
  return [oneAltaz (p, d, display) for p in radecs]
	
def geoEr (planet, date, display):
  d = decimalDay (date)
  (xg, yg, zg) = primaryCentric (planet)
  (ye, ze) = rotate2 (yg, zg, ecl (d) ) 
  (RA, Dec, rg) = polarFromCartesian3 (xg, ye, ze)
  
  lha = LHA (date, display.longitude, RA)
  (alt,az) = altaz (Dec, degToRad(display.latitude), lha)
  dimensionless = 1.0/rg
  alt_topoc = alt - math.asin( dimensionless) * math.cos (alt)
  return altazToPlot (alt_topoc, az, display)
  
## main entry points
  
def plotSun (display):
  time = System.DateTimeOffset.UtcNow
  return oneAltaz ( SunPos(decimalDay(time)), time, display)
  
def getMoon (display):
  time = System.DateTimeOffset.Now
  return geoEr ( Moon(decimalDay(time)), time, display)
  
def getPP (display):
  global planets
  return computeAltaz (System.DateTimeOffset.Now, planets, display)
  
def getDefaultDisplay ():
  return Display(52.0,0.0,120.0,120.0,100.0)
  
## Positive for waxing, negative for waning, +/- 0.0 for full

def getAnyMoonPhase (now, display):
  (v,r, lonsun) = SunLongitude(Sun(decimalDay(now)))
  (xg, yg, zg) =  primaryCentric(Moon(decimalDay(now)))
  lonecl = math.atan2 (yg, xg)
  latecl = math.atan2 (zg, radiusFromCartesian2 (xg, yg))
  elong = math.acos ( (math.cos(lonsun - lonecl)) * math.cos (latecl) )
  phase = (1.0 - math.cos(math.pi - elong)) /2.0
  return select (math.sin(lonsun - lonecl) > 0.0, (-phase), phase)

def getMoonPhase (display):
  now = System.DateTimeOffset.UtcNow
  return getAnyMoonPhase (now, display)


##========================================================================

animate = BackgroundWorker()
animate.WorkerReportsProgress = True
compute = BackgroundWorker()
compute.WorkerReportsProgress = True

latitude = 52
longitude = 0
sunup = ""
sundown = ""
sun = None
planetPlots = []
moon = None
sundown = None
phase = 0
xaml = Application.Current.LoadRootVisual(Canvas(), "astroclock.xaml")
hms = None
culture = None
date = None
planetNames = ["mercury", "venus", "mars", "jupiter", "saturn"]

def everySecond(s,e):
 while True:
   System.Threading.Thread.Sleep(1000)
   animate.ReportProgress(0)

def updateSky (s, e):
  global sunup, sundown, sun, moon, planetPlots, phase
  display = Display(
  latitude,
  longitude,
  120.0,
  120.0,
  100.0)
  
  sunup = sunrise (True, display)
  sundown = sunrise (False, display)
  sun = plotSun (display)
  planetPlots = getPP (display)
  moon = getMoon (display)
  phase = getMoonPhase (display)


def everyMinute(s,e):
  while True:
    System.Threading.Thread.Sleep(60*1000)
    compute.ReportProgress(0)
        
        
def moveHand (name, angle):
  line = xaml.FindName(name)
  tg = line.RenderTransform
  rotate = tg.Children[0]
  rotate.Angle = angle
      
def setText (name, value):
  block = xaml.FindName(name)
  block.Text = value
  return block
      
def setPlace (name, where):
  item = xaml.FindName(name)
  item.Data.Center = Point(where.x, where.y)
  item.Visibility = select (where.visible, Visibility.Visible, Visibility.Collapsed)

def drawMoon ():
  item = xaml.FindName("moon")
  where = moon
  figure = item.Data.Figures[0]
  rh = figure.Segments[0]
  lh = figure.Segments[1]
  item.Visibility = select (where.visible, Visibility.Visible, Visibility.Collapsed)
            
  figure.StartPoint = Point(where.x, where.y - 5.0)
  rh.Point = Point(where.x, where.y + 5.0)
  lh.Point = figure.StartPoint
  lh.SweepDirection = SweepDirection.Clockwise
      
  tmp = 1.0 - abs (phase)
  if phase > 0.5: ##waxing crescent
     rh.Size = Size(5.0, 5.0)
     lh.Size = Size(10.0 * (phase - 0.5), 5.0)
     lh.SweepDirection = SweepDirection.Counterclockwise
  elif phase >= 0: ## waxing gibbous
     rh.Size = Size(5.0, 5.0)
     lh.Size = Size(10.0 * (0.5 - phase), 5.0)
  elif phase > -0.5: ## waning gibbous
     lh.Size = Size(5.0, 5.0)
     rh.Size = Size(10.0 * (phase + 0.5), 5.0)
  else: ## waning cresent
      lh.Size = Size(5.0, 5.0)
      rh.Size = Size(10.0 * (-(0.5 + phase)), 5.0)
      rh.SweepDirection = SweepDirection.Counterclockwise
	
def updateTick(s,e):
  global sunup, sundown, sun, planetNames, planetPlots
  now = System.DateTime.Now
  setText ("hms", now.ToString(hms, culture))
  setText ("day", now.ToString("dddd", culture))
  setText ("date", now.ToString(date, culture))
  
  moveHand ("second", (6.0 * now.Second))
  moveHand ("minute", ((6*now.Minute)+(0.1*now.Second)))
  moveHand ("hour", ((30*now.Hour)+(now.Minute/2.0)+(now.Second/300.0)))
  setText ("sunup", System.String.Format("{0}-{1}", sunup, sundown))
  setPlace ("sun", sun)
  merge (setPlace, planetNames, planetPlots)
  drawMoon ()    



xaml.hyperlink.NavigateUri = System.Windows.Browser.HtmlPage.Document.DocumentUri
thisPage = System.Windows.Browser.HtmlPage.Document.DocumentUri.GetComponents(
  System.UriComponents.SchemeAndServer | System.UriComponents.Path,
  System.UriFormat.SafeUnescaped)
query = System.Windows.Browser.HtmlPage.Document.QueryString
  
def get_property(fun, name, fallback):
  try:
    return (fun(query[name]), True)
  except:
    return (fallback, False)  

(xaml.slider1.Value, islat) = get_property(float, 'lat', 52.0)
(xaml.slider2.Value, islong) = get_property(float, 'long', 0.0)
(cultureName, dummy) = get_property(str, 'locale', None)

try:
  culture = CultureInfo(cultureName)
except:
  culture = CultureInfo.CurrentCulture
  
(hms, dummy) = get_property(str, 'hms', culture.DateTimeFormat.LongTimePattern) #'HH:mm:ss'
(date, dummy) = get_property(str, 'date', culture.DateTimeFormat.ShortDatePattern) #d-MMM-yyyy
  
sunpos = (120, 120, Visibility.Collapsed)
  
def toggle_UI(configured):
  if configured:
    v1 = Visibility.Collapsed
    v2 = Visibility.Visible
  else:
    v1 = Visibility.Visible
    v2 = Visibility.Collapsed
  xaml.expander1.Visibility = v1 
  xaml.button1.Visibility = v2
  
toggle_UI(islat and islong)

def button1_Click(s, e):
  configured = (xaml.button1.Visibility == Visibility.Visible)
  toggle_UI(not configured)
  
xaml.button1.Click += button1_Click

def latValueChanged(s, e):
  global latitude
  v = xaml.slider1.Value
  latitude = v
  if v > 0:
	xaml.label1.Text = "Latitude %.2fN" % (v)
  elif v < 0:
	xaml.label1.Text = "Latitude %.2fS" % (-v)
  else: 
	xaml.label1.Text = "Latitude 0"
  xaml.hyperlink.NavigateUri = System.Uri("%s?lat=%f&long=%f" % (thisPage, v, xaml.slider2.Value))

xaml.slider1.ValueChanged += latValueChanged
latValueChanged(None, None)

def longValueChanged(s, e):
  global longitude
  v = xaml.slider2.Value
  longitude = v
  if v > 0:
	xaml.label2.Text = "Longitude %.2fE" % (v)
  elif v < 0:
	xaml.label2.Text = "Longitude %.2fW" % (-v)
  else:
	xaml.label2.Text = "Longitude 0"
  xaml.hyperlink.NavigateUri = System.Uri("%s?lat=%f&long=%f" % (thisPage, xaml.slider1.Value, v))
 
xaml.slider2.ValueChanged += longValueChanged
longValueChanged(None, None)

updateSky (None, None)
updateTick(None, None)

animate.DoWork += everySecond
animate.ProgressChanged += updateTick ##
animate.RunWorkerAsync()

compute.DoWork += everyMinute
compute.ProgressChanged += updateSky
compute.RunWorkerAsync()
                       

