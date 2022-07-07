using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Eva.Commands
{
    public partial class ReMangaCommands
    {
        private static string HtmlToPlainText(string html)
        {
            string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";
            string stripFormatting = @"<[^>]*(>|$)";
            string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";
            string newLines = @"(\r|\n)";
            Regex lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            Regex stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            Regex tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);
            Regex newLinesRegex = new Regex(newLines, RegexOptions.Multiline);

            string text = html;
            text = System.Net.WebUtility.HtmlDecode(text);
            text = tagWhiteSpaceRegex.Replace(text, "><");
            text = newLinesRegex.Replace(text, string.Empty);
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }

        public class Tag
        {
            [JsonProperty("id")] public int Id;
            [JsonProperty("name")] public string Name;
        }

        public class BaseManga
        {
            [JsonProperty("content")] public Manga Manga;
        }

        public class MangaList
        {
            [JsonProperty("content")] public List<MangaResult> Results;
        }

        public class MangaName
        {
            public string Russian;
            public string English;
            public string Another;
        }

        public class Manga
        {
            [JsonProperty("id")] public int Id;
            public MangaName Name;
            [JsonProperty("description")] public string Description;
            [JsonProperty("status")] public Tag Status;
            [JsonProperty("type")] public Tag Type;
            [JsonProperty("dir")] public string DirName;
            [JsonProperty("issue_year")] public int Year;
            [JsonProperty("avg_rating")] public float Rating;
            [JsonProperty("count_rating")] public int RatingVotes;
            [JsonProperty("count_chapters")] public int Chapters;
            [JsonProperty("total_views")] public int Views;
            [JsonProperty("total_votes")] public int Votes;
            [JsonProperty("count_bookmarks")] public int Bookmarks;
            [JsonProperty("genres")] public List<Tag> Genres;
            [JsonProperty("categories")] public List<Tag> Tags;
            [JsonProperty("img")] public MangaImage Cover;

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData;

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                string russian = (string)_additionalData["rus_name"];
                string english = (string)_additionalData["en_name"];
                string another = (string)_additionalData["another_name"];

                Description = HtmlToPlainText(Description);
                Name = new MangaName
                {
                    Russian = russian,
                    English = english,
                    Another = another
                };
            }

            public Manga()
            {
                _additionalData = new Dictionary<string, JToken>();
            }

        }

        public class MangaResult
        {
            [JsonProperty("id")] public int Id;
            [JsonProperty("rus_name")] public string Name;
            [JsonProperty("dir")] public string DirName;
            [JsonProperty("issue_year")] public int? Year;
            [JsonProperty("avg_rating")] public float Rating;
            [JsonProperty("count_chapters")] public int Chapters;
            [JsonProperty("img")] public MangaImage Cover;
        }

        public class MangaImage
        {
            [JsonProperty("high")] public string High;
            [JsonProperty("mid")] public string Middle;
            [JsonProperty("low")] public string Low;

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                High = $"https://api.remanga.org{High}";
                Middle = $"https://api.remanga.org{Middle}";
                Low = $"https://api.remanga.org{Low}";
            }
        }
    }
}