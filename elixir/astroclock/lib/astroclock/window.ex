defmodule Astroclock.Window do
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
  end


  @type astroclock() :: :wxWindow.wxWindow()
  @type state :: %State{frame: :wxFrame}

  # API

  @spec start() :: astroclock() | {:error, any()}
  def start do
    :wx_object.start(__MODULE__, [], [])
  end

  @spec start_link() :: astroclock() | {:error, any()}
  def start_link do
    :wx_object.start_link(__MODULE__, [], [])
  end

  @spec stop(astroclock()) :: :ok
  def stop(astroclock) do
    :wx_object.stop(astroclock)
  end

  @spec run(astroclock()) :: :ok
  def run(astroclock) do
    :wx_adapter.run(astroclock)
  end

  # object_wx callbacks

  @impl :wx_object
  @spec init(list()) :: {:wxFrame.wxFrame(), state()}
  def init(_) do
    :wx.new()

    frame = :wxFrame.new(:wx.null(), wxID_ANY(), "Astroclock", [])

    :wxFrame.show(frame)
    :wxFrame.raise(frame)

    {frame, %State{frame: frame}}
  end

  @impl :wx_object
  @spec handle_event(:wx.wx(), state()):: {:noreply, state()}
  def handle_event(event, state) do
    :io.format("Unhandled Event:~n~p~n", [event])
    {:noreply, state}
  end

  @impl :wx_object
  @spec handle_call(any(), any(), state()) :: {:reply, :ok, state()}
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
  @spec handle_cast(any(), state()) :: {:noreply, state()}
  def handle_cast(request, state) do
    :io.format("Unhandled Cast:~n~p~n", [request])
    {:noreply, state}
  end

  @impl :wx_object
  @spec handle_info(any(), state()) :: {:noreply, state()}
  def handle_info(info, state) do
    :io.format("Unhandled Info:~n~p~n", [info])
    {:noreply, state}
  end

  @impl :wx_object
  @spec terminate(any(), state()) :: :ok
  def terminate(_reason, state) do
    :wxFrame.destroy(state.frame)
    :wx.destroy()
    :ok
  end

  @impl :wx_object
  @spec code_change(any(), state(), any()) :: {:ok, state()}
  def code_change(_oldversion, state, _extra) do
    {:ok, state}
  end
end
