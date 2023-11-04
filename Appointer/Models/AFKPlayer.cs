using Microsoft.Xna.Framework;

namespace Appointer.Models
{
    public class AFKPlayer
    {
        public string PlayerName { get; set; }

        public bool isAFK { get; set; }

        public Vector2 LastPosition { get; set; }

        public int afkTicks { get; set; }

        public AFKPlayer(string name, Vector2 lastnet)
        {
            PlayerName = name;
            LastPosition = lastnet;
        }
    }
}
