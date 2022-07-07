using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators.OAuth2;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eva.Commands
{
    [Group("remanga")]
    public partial class ReMangaCommands : BaseCommandModule
    {
        private readonly RestClient client = new RestClient("https://api.remanga.org/api")
        {
            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator("саси", "bearer")
        };

        [GroupCommand]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendMangaAsync(CommandContext context, string dirname)
        {
            RestRequest request = new RestRequest($"titles/{dirname}", Method.Get);
            RestResponse response = await client.ExecuteGetAsync(request);
            BaseManga baseNovel = JsonConvert.DeserializeObject<BaseManga>(response.Content);
            Manga manga = baseNovel.Manga;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"{manga.Name.English} / {manga.Name.Another}"
                },
                Url = $"https://remanga.org/manga/{manga.DirName}",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = manga.Cover.High
                },
                Title = $"Манга **`{manga.Name.Russian}`** [{manga.Status.Name}] (**{manga.Year}** г.)",
                Description = $"<:rating:965253583664713788> **`{manga.Rating}`** (**`голосов: {Eva.Util.Utilities.NumberFormatter(manga.RatingVotes, 1)}`**) | <:votes:965253583689895966> **`{Eva.Util.Utilities.NumberFormatter(manga.Votes, 1)}`** | <:views:965253583698296882> **`{Eva.Util.Utilities.NumberFormatter(manga.Views, 1)}`** | <:bookmarks:965253583769591878> **`{Eva.Util.Utilities.NumberFormatter(manga.Bookmarks, 1)}`** | **`{manga.Type.Name}`**\n\n{manga.Description}",
                Color = DiscordColor.Black,
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                    IconUrl = context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                }
            };

            embed.AddField("Теги", string.Join(", ", manga.Genres.Concat(manga.Tags).Select((tag) => $"**`{tag.Name}`**")));

            await context.RespondAsync(embed: embed.Build());
        }

        [Command("search")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SearchMangaAsync(CommandContext context, string query)
        {
            RestRequest request = new RestRequest($"search", Method.Get);
            request.AddQueryParameter("query", query);
            request.AddQueryParameter("count", "10");
            request.AddQueryParameter("field", "titles");
            RestResponse response = await client.ExecuteGetAsync(request);
            MangaList mangas = JsonConvert.DeserializeObject<MangaList>(response.Content);
            string desc = string.Join('\n', mangas.Results.Select((manga, index) => $"{index + 1}. [**`{manga.Name}`**](https://remanga.org/manga/{manga.DirName}) ({manga.Year}) [<:rating:965253583664713788> **{manga.Rating}**] [**{manga.Chapters}** {Eva.Util.Utilities.GetDeclension(new string[] { "глава", "главы", "глав" }, manga.Chapters)}]"));

            Console.WriteLine(desc.Length);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = $"Результаты поиска по запросу **`{query}`**",
                Description = desc,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = mangas.Results.First().Cover.High
                },
                Color = DiscordColor.Black,
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                    IconUrl = context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                }
            };

            await context.RespondAsync(embed: embed.Build());
        }
    }

    [Group("renovels")]
    public partial class ReNovelsCommands : BaseCommandModule
    {
        private readonly RestClient client = new RestClient("https://api.renovels.org/api")
        {
            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator("саси", "bearer")
        };

        [GroupCommand]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendNovelAsync(CommandContext context, string dirname)
        {
            RestRequest request = new RestRequest($"titles/{dirname}", Method.Get);
            RestResponse response = await client.ExecuteGetAsync(request);
            BaseNovel baseNovel = JsonConvert.DeserializeObject<BaseNovel>(response.Content);
            Novel novel = baseNovel.Novel;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"{novel.Name.English} / {novel.Name.Another}"
                },
                Url = $"https://renovels.org/novel/{novel.DirName}",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = novel.Cover.High
                },
                Title = $"Новелла **`{novel.Name.Russian}`** [{novel.Status.Name}] (**{novel.Year}** г.)",
                Description = $"<:rating:965253583664713788> **`{novel.Rating}`** (**`голосов: {Eva.Util.Utilities.NumberFormatter(novel.RatingVotes, 1)}`**) | <:votes:965253583689895966> **`{Eva.Util.Utilities.NumberFormatter(novel.Votes, 1)}`** | <:views:965253583698296882> **`{Eva.Util.Utilities.NumberFormatter(novel.Views, 1)}`** | <:bookmarks:965253583769591878> **`{Eva.Util.Utilities.NumberFormatter(novel.Bookmarks, 1)}`** | **`{novel.Type.Name}`**\n\n{novel.Description}",
                Color = DiscordColor.Black,
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                    IconUrl = context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                }
            };

            embed.AddField("Теги", string.Join(", ", novel.Genres.Concat(novel.Tags).Select((tag) => $"**`{tag.Name}`**")));

            await context.RespondAsync(embed: embed.Build());
        }

        [Command("search")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SearchNovelAsync(CommandContext context, [RemainingText] string query)
        {
            RestRequest request = new RestRequest($"search", Method.Get);
            request.AddQueryParameter("query", query);
            request.AddQueryParameter("count", "10");
            request.AddQueryParameter("field", "titles");
            RestResponse response = await client.ExecuteGetAsync(request);
            NovelList novels = JsonConvert.DeserializeObject<NovelList>(response.Content);
            string desc = string.Join('\n', novels.Results.Select((novel, index) => $"{index + 1}. [**`{novel.Name}`**](https://renovels.org/novel/{novel.DirName}) ({novel.Year}) [<:rating:965253583664713788> **{novel.Rating}**] [**{novel.Chapters}** {Eva.Util.Utilities.GetDeclension(new string[] { "глава", "главы", "глав" }, novel.Chapters)}]"));

            Console.WriteLine(desc.Length);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = $"Результаты поиска по запросу **`{query}`**",
                Description = desc,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = novels.Results.First().Cover.High
                },
                Color = DiscordColor.Black,
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                    IconUrl = context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                }
            };

            await context.RespondAsync(embed: embed.Build());
        }
    }

}