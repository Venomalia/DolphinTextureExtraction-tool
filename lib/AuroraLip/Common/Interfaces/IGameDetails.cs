using AuroraLib.DiscImage.Dolphin;

namespace AuroraLib.Common.Interfaces
{
    public interface IGameDetails
    {
        GameID GameID { get; }
        string GameName { get; }
    }
}
