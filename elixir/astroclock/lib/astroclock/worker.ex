defmodule Astroclock.Worker do
  @moduledoc """
  OTP main process for `Astroclock`.
  """

  @spec main() :: :ok
  def main do
    :application.load(Astroclock)

    case Astroclock.Window.start_link() do
      {:error, _} = error -> :io.format("Error~n~p~n", [error])
      window -> Astroclock.Window.run(window)
    end
  end

  @spec start_link(any()) :: {:error, any()} | {:ok, pid(), {:wx_ref, any(), any(), pid()}}
  def start_link(_) do
    case Astroclock.Window.start_link() do
      {:error, _} = e -> e
      window -> {:ok, :wx_object.get_pid(window), window}
    end
  end

  @spec child_spec(any()) :: %{
          :id => Astroclock.Worker,
          :restart => :temporary,
          :shutdown => 500,
          :start => {Astroclock.Worker, :start_link, [any(), ...]},
          :type => :worker
        }
  def child_spec(opts) do
    %{
      id: __MODULE__,
      start: {__MODULE__, :start_link, [opts]},
      type: :worker,
      restart: :temporary,
      shutdown: 500
    }
  end
end
