# .credo.exs or config/.credo.exs
%{
  configs: [
    %{
      name: "default",
      files: %{
        included: ["lib/", "src/", "web/", "apps/"],
        excluded: []
      },
      plugins: [],
      requires: [],
      strict: false,
      parse_timeout: 5000,
      color: true,
      checks: [
        {Credo.Check.Readability.Specs, []}          
        #{Credo.Check.Design.AliasUsage, priority: :low},
        # ... other checks omitted for readability ...
      ]
    }
  ]
}