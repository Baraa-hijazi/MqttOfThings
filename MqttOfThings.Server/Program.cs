using System.Reflection;
using MQTTnet.AspNetCore;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;

namespace MqttOfThings;

public static class ServerAspNetSamples
{
    private static Config _config = new();

    private static Config ReadConfiguration(string currentPath)
    {
        var filePath = $"{currentPath}\\config.json";

        using var r = new StreamReader(filePath);
        var json = r.ReadToEnd();
        return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
    }

    public static Task Main()
    {
        var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        _config = ReadConfiguration(currentPath);

        var host = Host.CreateDefaultBuilder(Array.Empty<string>())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(o =>
                {
                    o.ListenAnyIP(1883, l => l.UseMqtt());
                    o.ListenAnyIP(5000);
                }).UseStartup<Startup>();
            });

        return host.RunConsoleAsync();
    }

    sealed class Startup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapConnectionHandler<MqttConnectionHandler>(
                    "/mqtt",
                    httpConnectionDispatcherOptions =>
                        httpConnectionDispatcherOptions.WebSockets.SubProtocolSelector =
                            protocolList => protocolList.FirstOrDefault() ?? string.Empty);
            });

            app.UseMqttServer(server =>
            {
                server.ValidatingConnectionAsync += ValidateConnectionAsync;
                server.ClientConnectedAsync += OnClientConnected;
            });

            Task OnClientConnected(ClientConnectedEventArgs eventArgs)
            {
                Console.WriteLine($"Client '{eventArgs.ClientId}' connected.");
                return Task.CompletedTask;
            }

            Task ValidateConnectionAsync(ValidatingConnectionEventArgs args)
            {
                try
                {
                    var currentUser = _config.Users.FirstOrDefault(u => u.UserName == args.UserName);

                    if (currentUser == null)
                    {
                        args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return Task.CompletedTask;
                    }

                    if (args.UserName != currentUser.UserName)
                    {
                        args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return Task.CompletedTask;
                    }

                    if (args.Password != currentUser.Password)
                    {
                        args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return Task.CompletedTask;
                    }

                    if (args.ClientId != currentUser.ClientId)
                    {
                        args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return Task.CompletedTask;
                    }

                    args.SessionItems.Add(currentUser.ClientId, currentUser);

                    args.ReasonCode = MqttConnectReasonCode.Success;
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"############## FAILED CONNECTION ##############   ({args.ClientId})");
                    return Task.FromException(ex);
                }
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedMqttServer(optionsBuilder => { optionsBuilder.WithDefaultEndpoint(); });
            services.AddMqttConnectionHandler();
            services.AddConnections();
        }
    }
}