using MS.Data;

namespace Core
{
    public class DataManager
    {
        public SettingData SettingData { get; private set; }
        public GameData GameData { get; private set; }

        public DataManager()
        {
            SettingData = new SettingData();
        }

        public void InitGameData()
        {
            GameData = new GameData();
        }

        public void ReleaseGameData()
        {
            GameData = null;
        }
    }
}
