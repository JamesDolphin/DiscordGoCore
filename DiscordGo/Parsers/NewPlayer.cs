using CoreRCON.Parsers;
using System.Text.RegularExpressions;

namespace DiscordGo.Parsers
{
    public class NewPlayer : IParseable
    {
    }

    public class NewPlayerParser : DefaultParser<NewPlayer>
    {
        public override string Pattern { get; } = @""".+?"" entered the game";

        //"<°><<6><STEAM_1:0:19338979><>" entered the game

        // INPUT Team playing "CT": Counter-Terrorists

        // RETURN C

        public override NewPlayer Load(GroupCollection groups)
        {
            return new NewPlayer
            {
            };
        }
    }
}