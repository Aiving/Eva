// importing installed packages
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
// importing custom namespaces
using Eva.Util;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eva.Commands
{
    public partial class OwnerCommands
    {
        [Group("util")]
        [Description("Различные утилиты.")]
        public class OwnerUtilCommands : BaseCommandModule
        {
            [Command("screenshot")]
            [Description("Делает скриншот компьютера на котором хостится бот.")]
            public async Task ScreenshotAsync(CommandContext context)
            {
                if (!config.Owners.Contains(context.Message.Author.Id)) return;
                var png = System.Drawing.Imaging.ImageFormat.Png;
                Stream str = Screen.Capture(png);
                string userName = Environment.UserName;
                DiscordMessageBuilder message = new DiscordMessageBuilder()
                    .WithContent($"Экран компьютера \"{userName}\".")
                    .WithFile("screenshot.png", str);
                await context.RespondAsync(message);
            }

            [Command("showbox")]
            [Description("Показывает диалоговое окно пользователю компьютера на котором хостится бот.")]
            public async Task ShowBoxAsync(CommandContext context, string data)
            {
                if (!config.Owners.Contains(context.Message.Author.Id)) return;
                string result = MessageBox.Show(data).ToString();
                await context.RespondAsync($"Нажата кнопка: {result.ToUpper()}");
            }
        }

    }
}