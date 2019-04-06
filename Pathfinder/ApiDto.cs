using System.Collections.Generic;

namespace Pathfinder
{
    public enum PlayerStatusDto
    {
        NotBad, Drifted, Hungry, Punished, HappyAsInsane
    }
    public class ApiRaceStateDto
    {
        public string SessionId { get; set; }
        public Location3 CurrentLocation { get; set; }
        public Location3 Finish { get; set; }
        public int Radius { get; set; }
        public int CurrentSpeed { get; set; }
        public Direction CurrentDirection { get; set; }
        public PlayerStatusDto PlayerStatus { get; set; }
        public List<(Location3, SurfaceType)> NeighbourCells { get; set; }
        public int Fuel { get; set; }
    }
}