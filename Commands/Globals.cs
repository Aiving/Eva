// importing installed packages
using DSharpPlus.CommandsNext;
// importing custom namespaces
using GenshinSharp;
using Legato;
using NHentaiSharp;
using SkiaSharp;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Eva.Commands
{
    public partial class OwnerCommands
    {
        public class Globals
        {
            public CommandContext context;
            public GenshinClient genhsinClient;
            public NClient nClient;
            public AimpCommands aimpCommands;
            public AimpProperties aimpProperties;
            public TagLib.File file;
            public MongoClient mongo;
        }

    }
}