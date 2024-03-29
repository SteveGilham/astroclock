﻿namespace Astroclock

open System

module Computation =

  let select a b c =
    match a with
    | true -> b
    | _ -> c

  // Units and types
  [<Measure>]
  type deg

  [<Measure>]
  type days

  [<Measure>]
  type au

  [<Measure>]
  type erad

  type Planet<'a> =
    { N: float
      I: float
      W: float
      A: 'a
      E: float
      M: float }

  type Plot = { X: float; Y: float; Visible: bool }

  type Display =
    { Latitude: float<deg>
      Longitude: float<deg>
      CentreX: float
      CentreY: float
      Radius: float }

  let degToRad (x: float<deg>) =
    let toRadians = Math.PI / 180.0<deg>
    x * toRadians

  let radToDeg (x: float) = x * 180.0<deg> / Math.PI

  // Time-keeping

  let dayNumber y m d =
    367 * y - 7 * (y + (m + 9) / 12) / 4
    + 275 * m / 9
    + d
    - 730530

  let inDays: float<days> = 1.0<days>

  let dayFrac (d: DateTimeOffset) =
    inDays
    * ((float d.Hour) + ((float d.Minute) / 60.0))
    / 24.0

  let decimalDay (date: DateTimeOffset) =
    let start =
      float (dayNumber date.Year date.Month date.Day)

    (start * inDays) + (dayFrac date)

  let now () = decimalDay System.DateTimeOffset.UtcNow

  let localNoon () =
    let now = System.DateTimeOffset.Now

    let r =
      System.DateTimeOffset(now.Year, now.Month, now.Day, 12, 0, 0, now.Offset)

    r.ToUniversalTime()

  let diffInSeconds (dt1: DateTimeOffset) (dt2: DateTimeOffset) =
    let span = dt1.Subtract(dt2)
    abs (span.Ticks / System.TimeSpan.TicksPerSecond)

  // Planetary motions

  let Sun d = //): # decimalDay
    { N = degToRad 0.0<deg>
      I = degToRad 0.0<deg>
      W = degToRad (282.9404<deg> + 4.70935E-5<deg / days> * d)
      A = 1.000000<au>
      E = 0.016709 - 1.151E-9<1 / days> * d
      M = degToRad (356.0470<deg> + 0.9856002585<deg / days> * d) }

  let Moon d =
    { N = degToRad (125.1228<deg> - 0.0529538083<deg / days> * d)
      I = degToRad 5.1454<deg>
      W = degToRad (318.0634<deg> + 0.1643573223<deg / days> * d)
      A = 60.2666<erad> //  (Earth radii)
      E = 0.054900
      M = degToRad (115.3654<deg> + 13.0649929509<deg / days> * d) }

  let Mercury d =
    { N = degToRad (48.3313<deg> + 3.24587E-5<deg / days> * d)
      I = degToRad (7.0047<deg> + 5.00E-8<deg / days> * d)
      W = degToRad (29.1241<deg> + 1.01444E-5<deg / days> * d)
      A = 0.387098<au>
      E = 0.205635 + 5.59E-10<1 / days> * d
      M = degToRad (168.6562<deg> + 4.0923344368<deg / days> * d) }

  let Venus d =
    { N = degToRad (76.6799<deg> + 2.46590E-5<deg / days> * d)
      I = degToRad (3.3946<deg> + 2.75E-8<deg / days> * d)
      W = degToRad (54.8910<deg> + 1.38374E-5<deg / days> * d)
      A = 0.723330<au>
      E = 0.006773 - 1.302E-9<1 / days> * d
      M = degToRad (48.0052<deg> + 1.6021302244<deg / days> * d) }

  let Mars d =
    { N = degToRad (49.5574<deg> + 2.11081E-5<deg / days> * d)
      I = degToRad (1.8497<deg> - 1.78E-8<deg / days> * d)
      W = degToRad (286.5016<deg> + 2.92961E-5<deg / days> * d)
      A = 1.523688<au>
      E = 0.093405 + 2.516E-9<1 / days> * d
      M = degToRad (18.6021<deg> + 0.5240207766<deg / days> * d) }

  let Jupiter d =
    { N = degToRad (100.4542<deg> + 2.76854E-5<deg / days> * d)
      I = degToRad (1.3030<deg> - 1.557E-7<deg / days> * d)
      W = degToRad (273.8777<deg> + 1.64505E-5<deg / days> * d)
      A = 5.20256<au>
      E = 0.048498 + 4.469E-9<1 / days> * d
      M = degToRad (19.8950<deg> + 0.0830853001<deg / days> * d) }

  let Saturn d =
    { N = degToRad (113.6634<deg> + 2.38980E-5<deg / days> * d)
      I = degToRad (2.4886<deg> - 1.081E-7<deg / days> * d)
      W = degToRad (339.3939<deg> + 2.97661E-5<deg / days> * d)
      A = 9.55475<au>
      E = 0.055546 - 9.499E-9<1 / days> * d
      M = degToRad (316.9670<deg> + 0.0334442282<deg / days> * d) }

  let planets =
    [ Mercury
      Venus
      Mars
      Jupiter
      Saturn ]

  // geometry

  let sq (t: float<_>) = t * t

  let radiusFromCartesian2 (x: float<_>) (y: float<_>) =
    let r2 = (sq x) + (sq y)
    sqrt r2

  let radiusFromCartesian3 (x: float<_>) (y: float<_>) (z: float<_>) =
    let r2 = (sq x) + (sq y) + (sq z)
    sqrt r2

  let polarFromCartesian2 (x: float<_>) (y: float<_>) =
    let theta = atan2 y x
    (theta, radiusFromCartesian2 x y)

  let polarFromCartesian3 (x: float<_>) (y: float<_>) (z: float<_>) =
    let ra = atan2 y x
    let dec = atan2 z (radiusFromCartesian2 x y)
    let rg = radiusFromCartesian3 x y z
    (ra, dec, rg)

  let cartesianFromPolar2 (r: float<_>) (theta: float) =
    let x = r * (cos theta)
    let y = r * (sin theta)
    (x, y)

  let ellipseToPolar (semiMajor: float<_>) eccentricity anomaly =
    let xv =
      semiMajor * ((cos anomaly) - eccentricity)

    let yv =
      semiMajor
      * (sqrt (1.0 - (sq eccentricity)) * (sin anomaly))

    polarFromCartesian2 xv yv

  let rotate2 (x: float<_>) (y: float<_>) (theta: float) =
    let xo = x * (cos theta) - y * (sin theta)
    let yo = x * (sin theta) + y * (cos theta)
    (xo, yo)

  //  Sun's upper limb touches the horizon; atmospheric refraction accounted for
  let sunhide = degToRad (-0.833<deg>) |> sin

  let altazToPlot alt az display =
    let r =
      display.Radius * (1.0 - 2.0 * (alt / Math.PI))

    { X = display.CentreX - r * (sin az)
      Y = display.CentreY - r * (cos az)
      Visible = alt > sunhide }

  // Ephemeris computations

  let ecl (d: float<days>) = //obliquity of ecliptic
    (23.4393<deg> - 3.563E-7<deg / days> * d)
    |> degToRad

  let getGMST0 sun =
    let rec reduce a : float<deg> =
      match a with
      | x when x > 360.0<deg> -> reduce (a - 360.0<deg>)
      | _ -> a

    reduce (180.0<deg> + radToDeg (sun.M + sun.W))

  let SunLongitude dsun =
    let e =
      dsun.M
      + dsun.E
        * (sin dsun.M)
        * (1.0 + dsun.E * (cos dsun.M))

    let (v, r) = ellipseToPolar 1.0<au> dsun.E e

    let lonsun = v + dsun.W
    (v, r, lonsun)

  let SunPos d = // decimalDay
    let dsun = Sun d
    let (v, r, lonsun) = SunLongitude dsun

    let lonsun = v + dsun.W
    let (xs, ys) = cartesianFromPolar2 r lonsun

    let xe = xs

    let (ye, ze) =
      cartesianFromPolar2 ys (ecl d)

    let (ra, dec, rg) =
      polarFromCartesian3 xe ye ze

    (ra, dec, dsun)

  let LHA (d: DateTimeOffset) (longitude: float<deg>) ra = // RA radians, return radians
    let lst d (*UTC*) longitude =
      let quant = d |> decimalDay |> Sun

      (getGMST0 quant)
      + longitude
      + 360.0<deg / days> * (dayFrac d)

    (lst d longitude |> degToRad) - ra

  let altaz dec lat lha = // #radian, deg, radian
    let xt =
      asin (
        sin (dec) * sin (lat)
        + cos (dec) * cos (lat) * cos (lha)
      )

    let yt =
      acos (
        (sin (dec) - sin (lat) * sin (xt))
        / (cos (lat) * cos (xt))
      )

    match sin (lha) with
    | x when x > 0.0 -> (xt, 2.0 * Math.PI - yt)
    | _ -> (xt, yt)

  let getSunHorizonLHA prev lat =
    let (ra, decl, quant) =
      prev |> decimalDay |> SunPos

    let cosLHA =
      (sunhide - sin (lat) * sin (decl))
      / (cos (lat) * cos (decl))

    match cosLHA with
    | x when x >= 1.0 -> (false, "+", 0.0)
    | x when x <= -1.0 -> (false, "-", 0.0)
    | _ -> (true, String.Empty, cosLHA)

  let localsolarnoon prev longitude =
    printfn "%A" prev

    let (ra, decl, quant) =
      prev |> decimalDay |> SunPos

    let utSuninsouth' =
      ((ra * 180.0<deg> / Math.PI)
       - (getGMST0 quant)
       - longitude)
      / 360.0<deg>

    let utSuninsouth =
      utSuninsouth'
      + (if utSuninsouth' < 0.0 then 1.0 else 0.0)

    printfn "%A" utSuninsouth
    let utcnow = System.DateTimeOffset.UtcNow

    let utczero =
      System.DateTimeOffset(prev.Year, prev.Month, prev.Day, 0, 0, 0, (TimeSpan(int64 0)))

    let ticks =
      new System.TimeSpan(int64 (utSuninsouth * (float System.TimeSpan.TicksPerDay)))

    printfn "%A" ticks

    let utcsolarnoon = utczero.Add(ticks)
    utcsolarnoon.ToLocalTime()

  let sunrise isSunup display =
    let lat = degToRad display.Latitude

    let getNext sign cosLHA (noon: DateTimeOffset) =
      let lha =
        (float System.TimeSpan.TicksPerHour)
        * sign
        * (acos cosLHA)
        / (Math.PI * 15.04107) // adjusted ticks

      noon.Add(new System.TimeSpan(int64 (lha)))

    let rec step guess lat iterate sign =
      let (good, mark, cosLHA) =
        getSunHorizonLHA guess lat

      match good with
      | false -> mark
      | _ ->
        let noon =
          localsolarnoon guess display.Longitude

        iterate guess (getNext sign cosLHA noon) lat sign cosLHA

    let rec iterate prev next lat sign cosLHA =
      let jolt = diffInSeconds prev next

      match jolt with
      | x when x > 30L -> step next lat iterate sign
      | _ -> next.ToString("HH:mm")

    let prev = localNoon ()
    let sign = select isSunup -180.0 180.0

    step prev lat iterate sign

  let oneAltaz radec d display =
    let (ra, dec, rg) = radec
    let lha = LHA d display.Longitude ra

    let (alt, az) =
      altaz dec (display.Latitude |> degToRad) lha

    altazToPlot alt az display

  let eccAnomaly planet =
    let rec next e0 planet =
      let e1 =
        e0
        - (e0 - planet.E * (sin e0) - planet.M)
          / (1.0 - planet.E * (cos e0))

      match abs (e1 - e0) with
      | x when x < 1.0e-8 -> e1
      | _ -> next e1 planet

    let e0 =
      planet.M
      + planet.E
        * (sin planet.M)
        * (1.0 + planet.E * (cos planet.M))

    next e0 planet

  let position planet =
    ellipseToPolar planet.A planet.E (eccAnomaly planet)

  let primaryCentric planet =
    let (v, r) = position planet
    let vw = v + planet.W
    let n = planet.N
    let i = planet.I

    let xh =
      r
      * (cos (n) * cos (vw) - sin (n) * sin (vw) * cos (i))

    let yh =
      r
      * (sin (n) * cos (vw) + cos (n) * sin (vw) * cos (i))

    let zh = r * (sin (vw) * sin (i))

    (xh, yh, zh)

  let geocentric planet sun =
    let (xh, yh, zh) = planet
    let (xs, ys, zs) = sun
    (xh + xs, yh + ys, zh)

  let radec planet sun d =
    let (xg, yg, zg) =
      geocentric (primaryCentric planet) (primaryCentric sun)

    let (ye, ze) = rotate2 yg zg (ecl d)
    polarFromCartesian3 xg ye ze

  let geoEr planet date display =
    let d = decimalDay date
    let (xg, yg, zg) = primaryCentric planet
    let (ye, ze) = rotate2 yg zg (ecl d)

    let (ra, dec, rg) =
      polarFromCartesian3 xg ye ze

    let lha = LHA date display.Longitude ra

    let (alt, az) =
      altaz dec (display.Latitude |> degToRad) lha

    let (dimensionless: float) = 1.0<erad> / rg

    let alttopoc =
      alt - (asin dimensionless) * (cos alt)

    altazToPlot alttopoc az display

  // main entry points

  let plotSun display =
    let time = System.DateTimeOffset.UtcNow
    oneAltaz (time |> decimalDay |> SunPos) time display

  let getMoon display =
    let time = System.DateTimeOffset.Now
    geoEr (time |> decimalDay |> Moon) time display

  let getPP display =
    let time = System.DateTimeOffset.Now
    let d = decimalDay time
    let sun = Sun d
    [ for p in planets -> oneAltaz (radec (p d) sun d) time display ]

  let getDefaultDisplay () =
    { Latitude = 52.0<deg>
      Longitude = 0.0<deg>
      CentreX = 120.0
      CentreY = 120.0
      Radius = 100.0 }

  // Positive for waxing, negative for waning, +/- 0.0 for full

  let getAnyMoonPhase now display =
    let (v, r, lonsun) =
      now |> decimalDay |> Sun |> SunLongitude

    let (xg, yg, zg) =
      now |> decimalDay |> Moon |> primaryCentric

    let lonecl = atan2 yg xg

    let latecl =
      atan2 zg (radiusFromCartesian2 xg yg)

    let elong =
      acos ((cos (lonsun - lonecl)) * (cos latecl))

    let phase =
      (1.0 - cos (Math.PI - elong)) / 2.0

    select (sin (lonsun - lonecl) > 0.0) (-phase) phase

  let getMoonPhase display =
    let now = System.DateTimeOffset.UtcNow
    getAnyMoonPhase now display