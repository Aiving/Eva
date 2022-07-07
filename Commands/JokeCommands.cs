// importing installed packages
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
// importing custom namespaces
using System;
using System.IO;
using System.Threading.Tasks;

namespace Eva.Commands
{
    public class JokeCommands : BaseCommandModule
    {
        /* [Command("webcum")]
        public async Task ScreenshotAsync(CommandContext context)
        {
            string userName = Environment.UserName;
            DiscordMessageBuilder message = new DiscordMessageBuilder()
                .WithContent($"Recorded cum of \"{userName}\" Desktop.")
                .WithFile(File.OpenRead(@"C:\Users\Aiving\Downloads\SPOILER_Camera_Snapshot.mp4"));
            await context.RespondAsync(message);
        } */
    }
}