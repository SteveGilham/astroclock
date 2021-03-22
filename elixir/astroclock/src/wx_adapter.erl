% need to replace -define directives manually
% see https://groups.google.com/g/elixir-lang-talk/c/VbGTz7rKebM
% for this conbcept

% To add an erlang module to your Elixir project,
% just make a `src` directory and put an `.erl` 
% file in there

-module(wx_adapter).
-compile(export_all).

-include_lib("wx/include/wx.hrl").

run(Item) ->
    catch wx_object:call(Item, noreply), ok.  

wxID_ANY() ->
    ?wxID_ANY.