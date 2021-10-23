#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CsQuery;
using D2InfoBot.Parser.Structures;

namespace D2InfoBot.Parser {
    internal class DbParser {
        private Random _rand = new Random();
        private Stopwatch _timeLoggerStopwatch = new Stopwatch();
        private void TimeLog(string text){ 
            Console.WriteLine(text + $" {this._timeLoggerStopwatch.ElapsedMilliseconds}ms elapsed.");
            this._timeLoggerStopwatch.Restart();
        }
        private HttpWebResponse GetPageResponse(string url) {
            HttpWebRequest r = WebRequest.CreateHttp(url);
            //anti-bot bypass
            r.Headers = new WebHeaderCollection {
                {
                    "User-Agent",
                    $"{this._rand.Next()}"
                }
            };
            return (HttpWebResponse)r.GetResponse();
        }
        //page html code
        private string LoadPage(WebResponse response) {
            return new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
        }
        private (ProfileАvailability аvailability, CQ page) GetProfile(ulong id){
            string url = "https://dotabuff.com/players/" + id;
            HttpWebResponse response;
            try {
                response = this.GetPageResponse(url);
            }
            catch(WebException e) {
                //profile doesn`t exists
                if(((HttpWebResponse) e.Response!).StatusCode == HttpStatusCode.NotFound)
                    return (ProfileАvailability.DoesNotExists, null)!;
                throw;
            }
            CQ page = CQ.Create(LoadPage(response));
            return (
                page.Find(".fa-lock").Length >= 1 ? //profile closed
                (ProfileАvailability.Closed, null) : 
                (ProfileАvailability.Aviable, page)
            )!;
        }
        private string NormalizeNumericString(string text){
            return text.Replace(",", "").Replace("%", "");
        }
        public ProfileInfo GetProfileInfo(ulong id){
            this._timeLoggerStopwatch.Start();
            ProfileInfo info = new ProfileInfo();
            info.Url = "https://dotabuff.com/players/" + id;
            
            (ProfileАvailability availability, CQ overviewPage) = GetProfile(id);
            
TimeLog("Page");
            
            info.Availability = availability;
            switch(info.Availability) {
                case ProfileАvailability.Aviable:
                    info.Availability = ProfileАvailability.Aviable;
                    break;
                case ProfileАvailability.Closed:
                    throw new Exception("Profile closed");
                case ProfileАvailability.DoesNotExists:
                    throw new Exception("Profile doesn`t exists");
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            info.Name = 
                System.Web.HttpUtility.HtmlDecode(overviewPage.Find("h1")[0].InnerText);

            CQ abadonsCq = overviewPage.Find(".abandons");
            
            int abadons = 
                abadonsCq != null ? 
                int.Parse(this.NormalizeNumericString(abadonsCq.Text())) 
                : 0;

            info.Wins = 
                int.Parse(this.NormalizeNumericString(overviewPage.Find(".wins").Text()));
            
            info.Losses = 
                int.Parse(this.NormalizeNumericString(overviewPage.Find(".losses").Text())) + abadons;
            
            info.Winrate = 
                Math.Round(info.Wins * 100 / (double)(info.Wins + info.Losses), 2);

            info.Rank = 
                overviewPage.Find(".rank-tier-wrapper").Attr("title");
            info.RankImageUrl = 
                overviewPage.Find(".rank-tier-base").Attr("src");
            info.RankStarsImageUrl = 
                overviewPage.Find(".rank-tier-pip")?.Attr("src") ?? string.Empty;
            
            info.AvatarImageUrl = 
                overviewPage.Find(".image-player").Attr("src");

            info.SkillBracket = 
                overviewPage.Find(".match-skill-acronym")[0].InnerText;
TimeLog("Parsed overview");
            //heroes list
            CQ cqHeroes = overviewPage.Find(".heroes-overview").Find(".r-row");
            info.Heroes = cqHeroes.Select(item => new Hero {
                Name = item.Cq().Find(".r-fluid")[0].Cq().Find(".r-none-mobile").Find("a")[0].InnerText,
                Matches = int.Parse(item.Cq().Find(".r-fluid")[1].Cq().Find(".r-body").Text()),
                Winrate = double.Parse(NormalizeNumericString(item.Cq().Find(".r-fluid")[2].Cq().Find(".r-body").Text()))
            }).ToArray();
TimeLog("Parsed heroes");
            
            //matches list
            CQ cqMatches = overviewPage.Find(".performances-overview")[0].Cq().Find(".r-row");
            info.Matches = cqMatches.Select(item => new Match {
                Hero = item.Cq().Find("a")[1].InnerText,
                Result = item.Cq().Find(".r-match-result")[0].Cq().Find("a")[0].InnerText,
                Kda = item.Cq().Find(".kda-record").Text()
            }).ToArray();
TimeLog("Parsed matches");
            return info;
        }
        public SearchResult[] FindProfile(string name, int count){
            string pageHtml = this.LoadPage(GetPageResponse($"https://www.dotabuff.com/search?utf8=%E2%9C%93&q={name}&commit=Search"));
            CQ page = CQ.Create(pageHtml);
            if(page.Find("title")[0].InnerText.Contains("Overview"))
                return new[] {
                    new SearchResult {
                        Id = ulong.Parse(page.Find(".image-container-bigavatar")[0].Cq().Find("a").Attr("href")
                            .Replace("/players/", ""))
                    }
                };
            
            CQ profiles = page.Find(".result-player")[..count];
            return profiles.Select(item => new SearchResult {
                    Avatar = item.Cq().Find("img")[0].Cq().Attr("src"),
                    Id = ulong.Parse(item.Cq().Find(".inner")[0].Cq().Attr("data-player-id")),
                    Name = System.Web.HttpUtility.HtmlDecode(item.Cq().Find(".link-type-player")[0].InnerText)
                }
            ).ToArray();
        }
    }
}
