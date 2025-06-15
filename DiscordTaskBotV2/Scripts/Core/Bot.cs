using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System.Reflection;
using DiscordTaskBot.Services;

namespace DiscordTaskBot.Core
{
    public class Bot
    {
        public readonly DiscordSocketClient _client;

        private readonly InteractionService _interactionService;

        private readonly ButtonHandlerService _buttonHandlerService;

        public Bot(ButtonHandlerService buttonHandlerService)
        {
            _client = new(
                new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged
                });
            _interactionService = new InteractionService(_client.Rest);

            _buttonHandlerService = buttonHandlerService;

            _client.Ready += OnReady;
            _client.Log += LogAsync;
            _client.InteractionCreated += OnInteraction;
            _client.ButtonExecuted += _buttonHandlerService.HandleButton;
        }

        public async Task RunAsync()
        {
            // Checking if Discord Bot Token is Specified
            if (!CheckEnviromentalVariables())
            {
                return;
            }

            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TOKEN"));
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task OnReady()
        {
            Console.WriteLine("Bot connected.");

            await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), null);

            await _interactionService.RegisterCommandsToGuildAsync(ulong.Parse(Environment.GetEnvironmentVariable("GUILD")!), true);

            Console.WriteLine("Slash commands registered.");
        }

        private async Task OnInteraction(SocketInteraction interaction)
        {
            var context = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(context, null);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine($"[LOG] {log}");
            return Task.CompletedTask;
        }

        private bool CheckEnviromentalVariables()
        {
            if (Environment.GetEnvironmentVariable("TOKEN") == null)
            {
                Console.Error.Write("Token Not Specified!");
                return false;
            }
            if (Environment.GetEnvironmentVariable("GUILD") == null || !ulong.TryParse(Environment.GetEnvironmentVariable("GUILD"), out ulong _))
            {
                Console.Error.Write("Guild Not Specified or Incorrect!");
                return false;
            }
            if (Environment.GetEnvironmentVariable("ARCHIVE_CHANNEL") == null || !ulong.TryParse(Environment.GetEnvironmentVariable("GUILD"), out ulong _))
            {
                Console.Error.Write("Archive Channel ID Not Specified or Incorrect!");
                return false;
            }

            return true;
        }
    }
}