using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Eva.Util.Anime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Eva.Commands
{
    [Group("anime")]
    [Description("Команды, связанные с аниме.\nP.S: Чтобы увидеть NSFW-категорию, перейдите в NSFW-канал.")]
    public partial class AnimeCommands : BaseCommandModule
    {
        private static readonly Configuration config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
        private readonly RestClient konachan = new("https://konachan.net");
        private readonly Random random = new();

        [Command("sfw")]
        [Description("\"Безопасная\" команда для просмотра аниме-картинок. Чтобы получить список доступных категорий, введите в качестве аргумента `categories`")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendSafeForWorkImage(CommandContext Context, string category = null)
        {
            if (category == "categories" || category == null)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithTitle("Список SFW категорий:")
                    .WithDescription(string.Join(", ", config.WaifuPics.SFW.Select((c) => $"`{c}`")))
                    .WithColor(DiscordColor.Black)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(
                        $"Запрошено {Context.Message.Author.Username}#{Context.Message.Author.Discriminator}.",
                        Context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                    );
                await Context.RespondAsync(embed: embed.Build());
            }
            else if (!config.WaifuPics.SFW.Contains(category))
            {
                await Context.RespondAsync("Данной категории нет в листе SFW категорий!");
                return;
            }
            else
            {
                RestClient apiClient = new("https://api.waifu.pics/");
                RestRequest request = new RestRequest($"sfw/{category}", Method.Get);
                RestResponse res = await apiClient.ExecuteGetAsync(request);
                Picture pic = JsonConvert.DeserializeObject<Picture>(res.Content);

                DiscordEmbedBuilder e = new DiscordEmbedBuilder()
                    .WithTitle($"WaifuPics ссылка на `{pic.File.Split("/").ToList().Last()}`")
                    .WithUrl(pic.File)
                    .WithImageUrl(pic.File)
                    .WithColor(DiscordColor.Black)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(
                        $"Запрошено {Context.Message.Author.Username}#{Context.Message.Author.Discriminator}.",
                        Context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                    );
                await Context.RespondAsync(e);
            };
        }

        [Command("nsfw")]
        [Description("\"Небезопасная\" команда для просмотра хентай-картинок. Чтобы получить список доступных категорий, введите в качестве аргумента `categories`")]
        [RequireNsfw]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendNotSafeForWorkImage(CommandContext Context, string category = null)
        {
            if (category == "categories" || category == null)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithTitle("Список NSFW категорий:")
                    .WithDescription(string.Join(", ", config.WaifuPics.NSFW.Select((c) => $"`{c}`")))
                    .WithColor(DiscordColor.Black)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(
                        $"Запрошено {Context.Message.Author.Username}#{Context.Message.Author.Discriminator}.",
                        Context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                    );
                await Context.RespondAsync(embed: embed.Build());
            }
            else if (!config.WaifuPics.NSFW.Contains(category))
            {
                await Context.RespondAsync("Данной категории нет в листе NSFW категорий!");
                return;
            }
            else
            {
                RestClient apiClient = new("https://api.waifu.pics/");
                RestRequest request = new RestRequest($"nsfw/{category}", Method.Get);
                RestResponse res = await apiClient.ExecuteGetAsync(request);
                Picture pic = JsonConvert.DeserializeObject<Picture>(res.Content);

                DiscordEmbedBuilder e = new DiscordEmbedBuilder()
                    .WithTitle($"WaifuPics ссылка на `{pic.File.Split("/").ToList().Last()}`")
                    .WithUrl(pic.File)
                    .WithImageUrl(pic.File)
                    .WithColor(DiscordColor.Black)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(
                        $"Запрошено {Context.Message.Author.Username}#{Context.Message.Author.Discriminator}.",
                        Context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                    );
                await Context.RespondAsync(e);
            };
        }

        [Command("wallpaper")]
        [Description("Показывает случайные обои с ресурса KonaChan с промежутком времени от 2016 года до нынешнего.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendWallpaperImage(CommandContext Context)
        {
            RestRequest randomPostsRequest = new RestRequest("/post/index.json", Method.Get);
            randomPostsRequest.AddQueryParameter("tags", "date:>=2016-1-1 rating:s order:date");
            randomPostsRequest.AddQueryParameter("limit", "100");
            randomPostsRequest.AddQueryParameter("page", random.Next(1, 450).ToString());

            RestResponse randomPostsResponse = await konachan.ExecuteGetAsync(randomPostsRequest);
            List<JObject> randomPostsList = JsonConvert.DeserializeObject<List<JObject>>(randomPostsResponse.Content);
            JObject randomPost = randomPostsList[random.Next(0, (randomPostsList.Count - 1))];
            Uri fileUrl = new(randomPost.SelectToken("file_url").ToString());

            DiscordEmbedBuilder e = new DiscordEmbedBuilder()
              .WithTitle($"KonaChan ссылка на `{$"{randomPost.SelectToken("md5")}.{fileUrl.Segments.Last().Split(".").Last()}"}`")
              .WithUrl(fileUrl)
              .WithDescription($@"Счёт: {randomPost.SelectToken("score")}
Создан: <t:{randomPost.SelectToken("created_at")}:F> (<t:{randomPost.SelectToken("created_at")}:R>)
{(randomPost.SelectToken("source").ToString() == "" ? null : $"Источник: [тык]({randomPost.SelectToken("source")})")}
        ")
              .AddField("Теги", string.Join(", ", randomPost.SelectToken("tags").ToString().Split(" ").Select((tag) => $"`{tag}`")))
              .WithImageUrl(fileUrl)
              .WithColor(DiscordColor.Black)
              .WithTimestamp(DateTime.Now)
              .WithFooter(
                  $"Запрошено {Context.Message.Author.Username}#{Context.Message.Author.Discriminator}.",
                  Context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
              );
            await Context.RespondAsync(embed: e.Build());
        }
    }
}