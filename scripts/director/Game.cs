using Incremental.scripts.planet.data;

namespace Incremental.scripts.director;

public class Game
{
    public static readonly Game I = new Game();

    public readonly PlanetData _data = new(100, 16);
}