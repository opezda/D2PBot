using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using D2InfoBot.Parser.Structures;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace D2InfoBot.Commands.PagedMessage {
    internal class DotabuffSearchMessage {
        private DiscordMessage _dMessage;
        private readonly List<IMessagePage> _pages  = new List<IMessagePage>();
        private bool _initialized;
        private int _currentPage;

        public async Task Show(CommandContext ctx, SearchResult[] results){
            this._initialized = true;
            for(int i = 0; i < this._pages.Count; i++) {
                string footerText = $"[Page {i + 1} of {this._pages.Count}]";
                if(this._pages[i].Embed.Footer == null)
                    this._pages[i].Embed.Footer = new DiscordEmbedBuilder.EmbedFooter {
                        Text = footerText
                    };
                else
                    this._pages[i].Embed.Footer.Text += footerText;
            }
            if(this._pages.Count < 1) {
                throw new Exception("No pages.");
            }
            
            this._dMessage = await ctx.Channel.SendMessageAsync(this._pages[0].Embed.Build());

            
            DiscordEmoji eLeft = DiscordEmoji.FromName(ctx.Client, ":point_left:");
            DiscordEmoji eRight = DiscordEmoji.FromName(ctx.Client, ":point_right:");
            DiscordEmoji eOk = DiscordEmoji.FromName(ctx.Client, ":ok:");
            await this._dMessage.CreateReactionAsync(eLeft);
            await this._dMessage.CreateReactionAsync(eRight);
            await this._dMessage.CreateReactionAsync(eOk);
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            while(stopwatch.ElapsedMilliseconds < 600000) {
                Task<InteractivityResult<MessageReactionAddEventArgs>> reactionResult = this._dMessage.WaitForReactionAsync(ctx.Message.Author, TimeSpan.FromSeconds(600));

                if(reactionResult.Result.Result.Emoji == eLeft) {
                    this._currentPage = this._currentPage > 0 ? this._currentPage - 1 : this._pages.Count - 1;
                    await this._dMessage.DeleteReactionAsync(eLeft, ctx.Message.Author);
                }
                else if(reactionResult.Result.Result.Emoji == eRight) {
                    this._currentPage = this._currentPage < this._pages.Count - 1 ? this._currentPage + 1 : 0;
                    await this._dMessage.DeleteReactionAsync(eRight, ctx.Message.Author);
                }
                else if(reactionResult.Result.Result.Emoji == eOk) {
                    await this._dMessage.DeleteAsync();
                    await new Dotabuff().DotabuffCommand(ctx, results[this._currentPage].Id);
                    break;
                }

                await this._dMessage.ModifyAsync(this._pages[this._currentPage].Embed.Build());
            }
        }
        public void AddPage(IMessagePage messagePage) {
            if(this._initialized) {
                throw new Exception("Can`t add new pages after message initialisation.");
            }
            this._pages.Add(messagePage);
        }
        
    }
}