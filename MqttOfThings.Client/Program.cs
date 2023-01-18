using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Internal;
using MQTTnet.Protocol;

namespace MqttOfThings.Client;

public static class Program
{
    public static void Main()
    {
        Task.Run(RunAsync);
        Thread.Sleep(Timeout.Infinite);
    }

    private static async Task RunAsync()
    {
        try
        {
            var logger = new MqttNetEventLogger();
            
            const string password = "Test";
            var bytes = Encoding.ASCII.GetBytes(password);

            
            var factory = new MqttFactory(logger);
            var client = factory.CreateMqttClient();
            var clientOptions = new MqttClientOptions
            {
                ClientId = "Hans",
                Credentials = new MqttClientCredentials("Hans", bytes),
                ChannelOptions = new MqttClientTcpOptions
                {
                    Port = 1883,
                    Server = "localhost"
                }
            };

            try
            {
                await client.ConnectAsync(clientOptions);
                await client.SubscribeAsync("h/h");
            }
            catch (Exception exception)
            {
                Console.WriteLine("### CONNECTING FAILED ###" + Environment.NewLine + exception);
            }

            client.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("                                                                ");
                Console.WriteLine("                                                                ");
                Console.WriteLine("### ### ### ### RECEIVED APPLICATION MESSAGE ### ### ### ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine("### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ");
                Console.WriteLine("                                                                ");
                Console.WriteLine("                                                                ");
                
                return CompletedTask.Instance;
            };

            client.ConnectedAsync += async _ =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");
                
                await client.SubscribeAsync("h/h");
                Console.WriteLine("### SUBSCRIBED on topic 'h/h' ###");
            };

            client.DisconnectedAsync += async _ =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await client.ConnectAsync(clientOptions);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            };

            Console.WriteLine("                                        ");
            Console.WriteLine("                                        ");
            Console.WriteLine("### WAITING FOR APPLICATION MESSAGES ###");
            Console.WriteLine("                                        ");
            Console.WriteLine("                                        ");

            while (true)
            {
                Console.ReadLine();

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("f/f")
                    .WithPayload("Hello World from Code client")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await client.PublishAsync(applicationMessage);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}