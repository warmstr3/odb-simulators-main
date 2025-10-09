using Simulator.Dnp3.Configurator;
using Spectre.Console.Cli;

var app = new CommandApp<ConfiguratorCommand>();

return app.Run(args);