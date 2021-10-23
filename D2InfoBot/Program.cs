using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Threading.Tasks;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

namespace D2InfoBot {
    internal static class Program {
        public static DiscordClient Discord;
        
        private static async Task Main() {
            Discord = new DiscordClient(new DiscordConfiguration() {
                Token = "0.0",
                TokenType = TokenType.Bot,
            });

            CommandsNextExtension commands = Discord.UseCommandsNext(new CommandsNextConfiguration() {
                StringPrefixes = new[] { "!" }
            });
            Discord.UseInteractivity(new InteractivityConfiguration() 
            { 
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
            
            commands.RegisterCommands<Commands.Dotabuff>();
            commands.RegisterCommands<Commands.Roll>();
            
            await Discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}