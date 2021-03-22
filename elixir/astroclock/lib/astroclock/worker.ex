defmodule Astroclock.Worker do
  # See https://hexdocs.pm/elixir/Application.html
  # for more information on OTP Applications
  @moduledoc false

  def main() do
    :application.load(Astroclock)
    case Astroclock.Worker.start_link() do
        {:error, _} = error -> :io.format("Error~n~p~n", [error])
        window -> Astroclock.Worker.run(window)
    end
  end

  def start_link() do
    case Astroclock.Worker.start_link() do
       {:error, _} = e -> e;
        window -> {:ok, :wx_object.get_pid(window), window}
    end
  end
end
