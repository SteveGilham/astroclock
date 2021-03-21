defmodule AstroclockTest do
  use ExUnit.Case
  doctest Astroclock

  test "greets the world" do
    assert Astroclock.hello() == :world
  end
end
