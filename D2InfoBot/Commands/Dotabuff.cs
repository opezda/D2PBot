using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using D2InfoBot.Commands.PagedMessage;
using D2InfoBot.Parser.Structures;

namespace D2InfoBot.Commands {
    internal class UserOverviewMessagePage : IMessagePage {
        public DiscordEmbedBuilder Embed { get; set; }
        public UserOverviewMessagePage(ProfileInfo info){
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            builder.AddField("Wins", "" + info.Wins, true);
            builder.AddField("Loses", "" +  info.Losses, true);
            builder.AddField("Winrate", "" +  info.Winrate + "%", true);
            int mmr = Data.MmrByRank(info.Rank);
            builder.Description = $"{info.Rank}{(mmr > 0 ? "(" + mmr + ")" : string.Empty)} | {info.SkillBracket}";
            ColorThiefDotNet.Color color = Image.GetDominateColor(info.AvatarImageUrl);
            builder.Color = new DiscordColor(color.R, color.G, color.B);
            builder.Author = new DiscordEmbedBuilder.EmbedAuthor {
                Name = info.Name, Url = info.Url, IconUrl = info.AvatarImageUrl
            };
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter {
                IconUrl =
                    "https://cdn.discordapp.com/attachments/349844440782995456/884599360028041216/2f70cde8-bf3c-4241-8ef7-f82a37db874a.gif"
            };
            
            this.Embed = builder;
        }
    }
    internal class UserHeroesMessagePage : IMessagePage {
        public DiscordEmbedBuilder Embed { get; set; }
        public UserHeroesMessagePage(ProfileInfo info){
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            
            string heroes = "";
            string matches = "";
            string winrates = "";
            
            foreach(Hero item in info.Heroes) {
                heroes += $"{item.Name}\n";
                matches += $"{item.Matches}\n";
                winrates += $"{item.Winrate}%\n";
            }

            builder.AddField("Hero", heroes, true);
            builder.AddField("Matches", matches, true);
            builder.AddField("Winrate", winrates, true);
            
            builder.Description = $"Top 10 heroes";
            ColorThiefDotNet.Color color = Image.GetDominateColor(info.AvatarImageUrl);
            builder.Color = new DiscordColor(color.R, color.G, color.B);
            builder.Author = new DiscordEmbedBuilder.EmbedAuthor {
                Name = info.Name, Url = info.Url, IconUrl = info.AvatarImageUrl
            };
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter {
                IconUrl =
                    "https://cdn.discordapp.com/attachments/349844440782995456/884599360028041216/2f70cde8-bf3c-4241-8ef7-f82a37db874a.gif"
            };
            this.Embed = builder;
        }
    }
    internal class UserMatchesMessagePage : IMessagePage {
        public DiscordEmbedBuilder Embed { get; set; }
        public UserMatchesMessagePage(ProfileInfo info){
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            
            string heroNames = "";
            string results = "";
            string kdas = "";
            
            foreach(Match item in info.Matches) {
                heroNames += $"{item.Hero}\n";
                results += $"{item.Result}\n";
                kdas += $"{item.Kda}\n";
            }

            builder.AddField("Hero", heroNames, true);
            builder.AddField("Result", results, true);
            builder.AddField("KDA", kdas, true);
            
            builder.Description = $"Last 25 matches";
            ColorThiefDotNet.Color color = Image.GetDominateColor(info.AvatarImageUrl);
            builder.Color = new DiscordColor(color.R, color.G, color.B);
            builder.Author = new DiscordEmbedBuilder.EmbedAuthor {
                Name = info.Name, Url = info.Url, IconUrl = info.AvatarImageUrl
            };
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter {
                IconUrl =
                    "https://cdn.discordapp.com/attachments/349844440782995456/884599360028041216/2f70cde8-bf3c-4241-8ef7-f82a37db874a.gif"
            };
            this.Embed = builder;
        }
    }
    internal class Dotabuff : BaseCommandModule {
        private ProfileInfo _info;
        [Command("db")]
        public async Task DotabuffCommand(CommandContext ctx, ulong id){
            try {
                this._info = new Parser.DbParser().GetProfileInfo(id);
            }
            catch(Exception e) {
                await ctx.Channel.SendMessageAsync(e.Message + e.StackTrace);
                throw;
            }
            
            EmojiSwitchedMessage message = new EmojiSwitchedMessage();
            UserOverviewMessagePage page = new UserOverviewMessagePage(this._info);
            page.Embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail {
                Url = await Image.SaveImageToDsServer(
                    Image.MergeImages(this._info.RankImageUrl, this._info.RankStarsImageUrl), ctx)
            };
            message.AddPage(page);
            message.AddPage(new UserHeroesMessagePage(this._info));
            message.AddPage(new UserMatchesMessagePage(this._info));

            await message.Show(ctx.Channel, ctx.Message.Author, ctx.Client);
        }
        [Command("db")]
        public async Task DotabuffCommand(CommandContext ctx, params string[] str){
            string name = string.Join(" ", str);
            SearchResult[] results;
            DotabuffSearchMessage dsm = new DotabuffSearchMessage();
            try {
                results = new Parser.DbParser().FindProfile(name, 15);
            }
            catch(Exception e) {
                await ctx.Channel.SendMessageAsync(e.Message + e.StackTrace);
                throw;
            }
            switch(results.Length) {
                case 0:
                    await ctx.Channel.SendMessageAsync("nema takogo chelika:(");
                    return;
                case 1:
                    await this.DotabuffCommand(ctx, results[0].Id);
                    return;
                case > 1:
                    foreach(SearchResult result in results) {
                        ColorThiefDotNet.Color color = Image.GetDominateColor(result.Avatar);
                        MessagePage page = new MessagePage(new DiscordEmbedBuilder {
                            Author = new DiscordEmbedBuilder.EmbedAuthor {
                                IconUrl = result.Avatar,
                                Name = result.Name,
                                Url = "https://dotabuff.com/players/" + result.Id
                            },
                            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail {
                                Url = result.Avatar
                            },
                            Color = new DiscordColor(color.R, color.G, color.B),
                        });
                        dsm.AddPage(page);
                    }
                    await dsm.Show(ctx, results);
                    break;
            }
        }
    }
}