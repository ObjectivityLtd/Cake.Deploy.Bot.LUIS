using System;

namespace Cake.Deploy.Bot.LUIS
{
    public class LuisDetails
    {
        public string AppId { get; set; }

        public string Domain { get; set; }

        public Version Version { get; set; }
    }
}