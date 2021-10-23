using DSharpPlus.Entities;

namespace D2InfoBot.Commands.PagedMessage {
    public interface IMessagePage {
        public DiscordEmbedBuilder Embed { get; set; }
    }
}