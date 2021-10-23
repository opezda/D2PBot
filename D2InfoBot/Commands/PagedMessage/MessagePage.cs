using DSharpPlus.Entities;

namespace D2InfoBot.Commands.PagedMessage {
    public class MessagePage : IMessagePage {
        public DiscordEmbedBuilder Embed { get; set; }
        public MessagePage(DiscordEmbedBuilder embed){
            this.Embed = embed;
        }
    }
}