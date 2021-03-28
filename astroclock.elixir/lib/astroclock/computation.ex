defmodule Computation do
  @moduledoc """
      Computations for `Astroclock`.
  """

  defmodule Polar2 do
    @moduledoc """
    2d Polar coordinates.
    """
    defstruct [
      :r,
      :theta
    ]
  end

  @type polar2 :: %Polar2{r: float(), theta: float()}

  defmodule Polar3 do
    @moduledoc """
    3d Polar coordinates.
    """
    defstruct [
      :ra,
      :dec,
      :rg
    ]
  end

  @type polar3 :: %Polar3{ra: float(), dec: float(), rg: float()}

  defmodule Cartesian2 do
    @moduledoc """
    2d Cartesian coordinates.
    """
    defstruct [
      :x,
      :y
    ]
  end

  @type cartesian2 :: %Cartesian2{x: float(), y: float()}

  defmodule Cartesian3 do
    @moduledoc """
    3d Cartesian coordinates.
    """
    defstruct [
      :x,
      :y,
      :z
    ]
  end

  @type cartesian3 :: %Cartesian3{x: float(), y: float(), z: float()}

  defmodule Planet do
    @moduledoc """
    Celestial body (a wanderer).
    """
    defstruct [
      # :name # "Sun"
      # :tag # "Sun \u2609"
      # :colour # "Yellow"
      # :N # 0.0
      # :i # 0.0
      # :w # (282.9404 + 4.70935e-5 * d) * toRadians
      # a # 1.000000
      :semiMajor,
      # e # 0.016709 - 1.151e-9 * d
      :eccentricity
      # :M # (356.0470 + 0.9856002585 * d) * toRadians
      # :R # 5
    ]
  end

  @type planet :: %Planet{
          semiMajor: float(),
          eccentricity: float()
        }

  @spec sq(float()) :: float()
  def sq(t) do
    t * t
  end

  @spec radius_from_cartesian2(cartesian2()) :: float()
  def radius_from_cartesian2(c2) do
    r2 = sq(c2.x) + sq(c2.y)
    :math.sqrt(r2)
  end

  @spec radius_from_cartesian3(cartesian3()) :: float()
  def radius_from_cartesian3(c3) do
    r2 = sq(c3.x) + sq(c3.y) + sq(c3.z)
    :math.sqrt(r2)
  end

  @spec polar_from_cartesian2(cartesian2()) :: polar2()
  def polar_from_cartesian2(c2) do
    %Polar2{
      theta: :math.atan2(c2.y, c2.x),
      r: radius_from_cartesian2(c2)
    }
  end

  @spec polar_from_cartesian3(cartesian3()) :: polar3()
  def polar_from_cartesian3(c3) do
    c2 = %Cartesian2{x: c3.x, y: c3.y}

    %Polar3{
      ra: :math.atan2(c3.y, c3.x),
      dec: :math.atan2(c3.z, radius_from_cartesian2(c2)),
      rg: radius_from_cartesian3(c3)
    }
  end

  @spec cartesian_from_polar2(polar2()) :: cartesian2()
  def cartesian_from_polar2(p2) do
    %Cartesian2{
      x: p2.r * :math.cos(p2.theta),
      y: p2.r * :math.sin(p2.theta)
    }
  end

  @spec ellipse_to_polar(planet(), float()) :: polar2()
  def ellipse_to_polar(p, anomaly) do
    xv = p.semiMajor * (:math.cos(anomaly) - p.eccentricity)
    yv = p.semiMajor * (:math.sqrt(1.0 - sq(p.eccentricity)) * :math.sin(anomaly))
    c2 = %Cartesian2{x: xv, y: yv}
    polar_from_cartesian2(c2)
  end

  @spec rotate2(cartesian2(), float()) :: cartesian2()
  def rotate2(c2, theta) do
    %Cartesian2{
      x: c2.x * :math.cos(theta) - c2.y * :math.sin(theta),
      y: c2.x * :math.sin(theta) + c2.y * :math.cos(theta)
    }
  end
end
