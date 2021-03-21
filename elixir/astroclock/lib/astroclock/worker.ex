defmodule Astroclock.Worker do
  # See https://hexdocs.pm/elixir/Application.html
  # for more information on OTP Applications
  @moduledoc false

  def main() do
#    application:load(drophash),
#    case Astroclock.Worker:start_link() of
#        {error, _} = Error ->io:format("Error~n~p~n", [Error]);
#        Window -> Astroclock.Worker:run(Window)
#    end.
  end

  def start_link() do
#    case Astroclock.Worker:start_link() of
#       {error, _} = E -> E;
#        Window -> {ok, wx_object:get_pid(Window), Window}
#    end.
  end
end
