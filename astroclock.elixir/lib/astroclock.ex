defmodule Astroclock do
  @moduledoc """
  Entry point for `Astroclock`.
  """

  @spec main() :: :ok
  def main do
    Astroclock.Worker.main()
  end
end
