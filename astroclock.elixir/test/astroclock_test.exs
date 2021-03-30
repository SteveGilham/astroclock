defmodule AstroclockTest do
  use ExUnit.Case
  doctest Astroclock

  test "test sq" do
    assert Astroclock.Computation.sq(3) == 9
  end

  # a = window.astroclock

  # test( "test radiusFromCartesian2", ()  ->
  #   ok( a.radiusFromCartesian2(3, 4) == 5 , "Passed!" ))

  # test( "test radiusFromCartesian3", ()  ->
  #   ok( a.radiusFromCartesian3(3, 4, 12) == 13 , "Passed!" ))

  # t = () -> new Date(2013, 4-1, 23, 10, 50, 0, 0);
  # decimal = a.DecimalDay(t())

  # okWithin = (actual, expected, epsilon, message) ->
  #   delta = actual - expected
  #   message = if message then message + " : " else ""
  #   ok( Math.abs(delta) < epsilon ,  message + delta)

  # test( "test DecimalDay", ()  ->
  #   okWithin(decimal, 4862.409722, 1.0e-5))

  # test( "test toLocalNoon", ()  ->
  #   dd = a.toLocalNoon(t()).toString()
  #   ok( dd.indexOf("Tue Apr 23 2013 12:00:00") == 0 , dd ))

  # test( "test ecl", ()  ->
  #   dd = a.ecl(decimal)
  #   okWithin(dd, 0.4090627219, 1.0e-9, "ecl" ))

  # test( "test getGMST0", ()  ->
  #   sun = a.Sun(decimal)
  #   dd = a.getGMST0(sun)
  #   okWithin(dd, 211.608667, 1.0e-5))

  # test( "test SunPos RA", ()  ->
  #   sun = a.SunPos(decimal)
  #   okWithin(sun.RA, 0.5442777218, 1.0e-7))

  # test( "test SunPos Dec", ()  ->
  #   sun = a.SunPos(decimal)
  #   okWithin(sun.Dec, 0.2208150713, 1.0e-7))

  # test( "test dayFrac", ()  ->
  #   frac = a.dayFrac(t())
  #   okWithin(frac, 0.4097222222, 1.0e-7))  

  # test( "test LHA simple", ()  ->
  #   lha = a.LHA(t(), 0, 0)
  #   okWithin(lha, 6.267628612, 1.0e-7))

  # test( "test LHA longitude", ()  ->
  #   lha = a.LHA(t(), 10, 0)
  #   okWithin(lha, 6.442161538, 1.0e-7))

  # test( "test LHA RA", ()  ->
  #   lha = a.LHA(t(), 0, 0.1)
  #   okWithin(lha, 6.167628612, 1.0e-7))

  # test( "test altaz simple", ()  ->
  #   aa = a.altaz(0.1, 0.2, 0.3)
  #   okWithin(aa.alt, 1.25791172, 1.0e-7, "alt")
  #   okWithin(aa.az, 4.412246028, 1.0e-7, "az"))

  # test( "test altaz negatives", ()  ->
  #   aa = a.altaz(-0.1, 0.2, -0.3)
  #   okWithin(aa.alt, 1.147602474, 1.0e-7, "alt")
  #   okWithin(aa.az, 2.343534368, 1.0e-7, "az"))

  # test( "test getSunHorizonLHA simple", ()  ->
  #   aa = a.getSunHorizonLHA(t(), 0.2)
  #   ok(aa.safe, "safe")
  #   equal(aa.sign, "")
  #   okWithin(aa.angle, -0.06070632397, 1.0e-7))

  # test( "test getSunHorizonLHA -1.57", ()  ->
  #   aa = a.getSunHorizonLHA(t(), -1.57)
  #   ok(!aa.safe, "safe")
  #   equal(aa.sign, "+")
  #   okWithin(aa.angle, 0, 1.0e-7))

  # test( "test getSunHorizonLHA 1.57", ()  ->
  #   aa = a.getSunHorizonLHA(t(), 1.57)
  #   ok(!aa.safe, "safe")
  #   equal(aa.sign, "-")
  #   okWithin(aa.angle, 0, 1.0e-7))

  # test( "test altazToPlot 0.0 0.1 (getDefaultDisplay())", ()  ->
  #   aa = a.altazToPlot(0, 0.1, a.getDefaultDisplay())
  #   ok(aa.visible, "visible")
  #   okWithin(aa.x, 110.0166583, 1.0e-7, "alt")
  #   okWithin(aa.y, 20.49958347, 1.0e-7, "az"))

  # tn = () -> new Date(2013, 7-1, 3, 9, 50, 0, 0);

  # test( "test localsolarnoon when'' 0.0<deg>", ()  ->
  #   dd = a.localsolarnoon(tn(), 0)
  #   ok( dd.toString().indexOf("Wed Jul 03 2013 13:04:") == 0 , dd )) ## should be :26 get :15

  # test( "test localsolarnoon when' 90.0<deg>", ()  ->
  #   dd = a.localsolarnoon(t(), 90)
  #   ok( dd.toString().indexOf("Tue Apr 23 2013 06:58:18") == 0 , dd ))

  # test( "test sunrise", ()  ->
  #   sunup = a.anySunrise(tn(), true, a.getDefaultDisplay())
  #   equal( sunup, "04:47"))

  # test( "test sunset", ()  ->
  #   sunup = a.anySunrise(tn(), false, a.getDefaultDisplay())
  #   equal( sunup, "21:21"))

  # test( "test primaryCentric(Mercury(dd))", ()  ->
  #   aa = a.primaryCentric(a.Mercury(decimal))
  #   okWithin(aa.x, 0.3193190672, 1.0e-7, "x")
  #   okWithin(aa.y, -0.2467566634, 1.0e-7, "y")
  #   okWithin(aa.z, -0.04947528469, 1.0e-7, "z"))

  # test( "test primaryCentric(Mars(dd))", ()  ->
  #   aa = a.primaryCentric(a.Mars(decimal))
  #   okWithin(aa.x, 1.224362526, 1.0e-7, "x")
  #   okWithin(aa.y, 0.745669094, 1.0e-7, "y")
  #   okWithin(aa.z, -0.0145493891, 1.0e-7, "z"))

  # test( "test geocentric(Venus(dd))", ()  ->
  #   aa = a.geocentric(a.primaryCentric(a.Venus(decimal)), a.primaryCentric(a.Sun(decimal)))
  #   okWithin(aa.x, 1.310897754, 1.0e-7, "x")
  #   okWithin(aa.y, 1.100962191, 1.0e-7, "y")
  #   okWithin(aa.z, -0.01981968847, 1.0e-7, "z"))

  # test( "test geocentric(Jupiter(dd))", ()  ->
  #   aa = a.geocentric(a.primaryCentric(a.Jupiter(decimal)), a.primaryCentric(a.Sun(decimal)))
  #   okWithin(aa.x, 1.411295927, 1.0e-7, "x")
  #   okWithin(aa.y, 5.623084496, 1.0e-7, "y")
  #   okWithin(aa.z, -0.03395654336, 1.0e-7, "z"))

  # test( "test radec(Saturn(dd))", ()  ->
  #   aa = a.radec(a.Saturn(decimal), a.Sun(decimal), decimal)
  #   okWithin(aa.RA, -2.490568863, 1.0e-7, "RA")
  #   okWithin(aa.Dec, -0.207557937, 1.0e-7, "Dec")
  #   okWithin(aa.rg, 8.837370159, 1.0e-7, "rg"))

  # test( "test plotAnySun", ()  ->
  #   aa = a.plotAnySun(t(), a.getDefaultDisplay())
  #   console.log(aa)
  #   okWithin(aa.x, 82.99571988, 1.0e-7, "x")
  #   okWithin(aa.y, 156.8975748, 1.0e-7, "y")
  #   ok(aa.visible, "visible"))
end
