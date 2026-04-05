namespace MS.Data
{
    public class DungeonGameData
    {
        public int CurrentZoneIndex { get; set; }
        public EZoneType CurrentZoneType { get; set; }
        public bool IsResting { get; set; }

        public DungeonGameData()
        {
            CurrentZoneIndex = 0;
            CurrentZoneType = EZoneType.Battle;
            IsResting = false;
        }
    }
}
