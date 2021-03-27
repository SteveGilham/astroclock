defmodule Astroclock.Window do
  # TODO specs and types
  @moduledoc """
  UI for `Astroclock`.
  """

  @behaviour :wx_object

  # Record.extract & extract_all can be used to extract records from Erlang files.
  # May need to `import` modules from the wx.hrl to replace `-import` directives

  import :wx_adapter

  defmodule State do
    @moduledoc """
    UI state for `Astroclock`.
    """

    defstruct [:frame]
    @type state :: %State{frame: :wxFrame}
  end

  # API

  def start do
    :wx_object.start(__MODULE__, [], [])
  end

  def start_link do
    :wx_object.start_link(__MODULE__, [], [])
  end

  def stop(astroclock) do
    :wx_object.stop(astroclock)
  end

  def run(astroclock) do
    :wx_adapter.run(astroclock)
  end

  # object_wx callbacks

  @impl :wx_object
  def init(_) do
    :wx.new()

    frame = :wxFrame.new(:wx.null(), wxID_ANY(), "Astroclock", [])

    :wxFrame.show(frame)
    :wxFrame.raise(frame)

    {frame, %State{frame: frame}}
  end

  @impl :wx_object
  def handle_event(event, state) do
    :io.format("Unhandled Event:~n~p~n", [event])
    {:noreply, state}
  end

  @impl :wx_object
  def handle_call(:noreply, _from, state) do
    # wait until window closed
    {:noreply, state}
  end

  @impl :wx_object
  def handle_call(request, _from, state) do
    :io.format("Unhandled Call:~n~p~n", [request])
    {:reply, :ok, state}
  end

  @impl :wx_object
  def handle_cast(request, state) do
    :io.format("Unhandled Cast:~n~p~n", [request])
    {:noreply, state}
  end

  @impl :wx_object
  def handle_info(info, state) do
    :io.format("Unhandled Info:~n~p~n", [info])
    {:noreply, state}
  end

  @impl :wx_object
  def terminate(_reason, state) do
    :wxFrame.destroy(state.frame)
    :wx.destroy()
    :ok
  end

  @impl :wx_object
  def code_change(_oldversion, state, _extra) do
    {:ok, state}
  end
end
