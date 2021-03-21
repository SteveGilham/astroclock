defmodule Astroclock.Window do
  # See https://hexdocs.pm/elixir/Application.html
  # for more information on OTP Applications
  @moduledoc false

  def start_link() do
  end
end

#
#
#%%%-------------------------------------------------------------------
#%% @doc Astroclock top level window.
#%% @end
#%%%-------------------------------------------------------------------
#-module(Astroclock_win).
#
#-behaviour(wx_object).
#
#-include_lib("wx/include/wx.hrl").
#
#%% API
#-export_type([Astroclock/0]).
#-export([start/0, start_link/0, stop/1, run/1]).


#%% wx_object callbacks
#-export([init/1, handle_event/2, handle_call/3, handle_cast/2, handle_info/2,
#         terminate/2, code_change/3]).

#-record(state, {
#    frame      :: wxFrame:wxFrame()
#}).

#-type    state() :: #state{}.
#-type Astroclock() :: wxWindow:wxWindow().

#-spec start() -> Astroclock() | {'error', any()}.
#start()         -> wx_object:start(?MODULE, [], []).

#-spec start_link() -> Astroclock() | {'error', any()}.
#start_link()         -> wx_object:start_link(?MODULE, [], []).

#-spec stop(Astroclock()) -> 'ok'.
#stop(Astroclock) -> wx_object:stop(Astroclock).

#-spec run(Astroclock()) -> 'ok'.
#run(Astroclock) -> catch wx_object:call(Astroclock, noreply), ok.

#%% object_wx callbacks
#-spec init(list()) -> {wxFrame:wxFrame(), state()}.
#init(_) ->
#    wx:new(),
#    Frame   = Frame = wxFrame:new(wx:null(), ?wxID_ANY, "Astroclock", []),
    
#    wxFrame:show(Frame),
#    wxFrame:raise(Frame),

#    {Frame,
#        #state{
#            frame = Frame
#        }
#    }.

#-spec handle_event(Event :: wx(), State :: state())
#        -> {'noreply', state()}
#         | {'stop', 'normal', state()}.

#handle_event(Event, S) ->
#    io:format("Unhandled Event:~n~p~n", [Event]),
#    {noreply, S}.

#-spec handle_call(Request::any(), From::any(), State::state())
#        -> {'noreply', state()} | {reply, ok, state()}.
#handle_call(noreply, _From, State) ->
#    {noreply, State}; % wait until window closed

#handle_call(Request, _From, State) ->
#    io:format("Unhandled Call:~n~p~n", [Request]),
#    {reply, ok, State}.

#-spec handle_cast(Request::any(), State::state()) -> {'noreply', state()}.

#handle_cast(Request, State) ->
#    io:format("Unhandled Cast:~n~p~n", [Request]),
#    {noreply, State}.

#-spec handle_info(Info::any(), State::state()) -> {'noreply', state()}.
#handle_info(Info, State) ->
#    io:format("Unhandled Info:~n~p~n", [Info]),
#    {noreply, State}.

#-spec terminate(Reason::any(), State::state()) -> 'ok'.
#terminate(_Reason, S) ->
#    wxFrame:destroy(S#state.frame),
#    wx:destroy(),
#    ok.

#-spec code_change(any(), state(), any()) -> {'ok', state()}.
#code_change(_OldVsn, State, _Extra) ->
#    {ok, State}.