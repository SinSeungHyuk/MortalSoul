namespace MS.Data
{
    public class GameData
    {
        public SoulGameData Soul { get; private set; }
        public LevelGameData Level { get; private set; }
        public DungeonGameData Dungeon { get; private set; }
        public BattleGameData Battle { get; private set; }

        public GameData()
        {
            Soul = new SoulGameData();
            Level = new LevelGameData();
            Dungeon = new DungeonGameData();
            Battle = new BattleGameData();
        }
    }
}
