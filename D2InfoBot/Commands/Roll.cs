using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace D2InfoBot.Commands {
    internal class Roll : BaseCommandModule {
        [Command("roll")]
        public async Task RollCommand(CommandContext ctx) {
            await ctx.RespondAsync($"`{new Random().Next(0, 100)}`");
        }
    }
}
