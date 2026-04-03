using MS.Core;
using MS.Field;
using MS.Mode;
using MS.Skill;
using Cysharp.Threading.Tasks;
using System;

namespace MS.Data
{
    public class BattlePanelViewModel
    {
        public MSReactProp<int> KillCount { get; private set; }
        public MSReactProp<int> WaveCount { get; private set; }
        public MSReactProp<float> WaveTimer { get; private set; }
        public MSReactProp<int> PlayerGold { get; private set; }
        public MSReactProp<int> PlayerLevel { get; private set; }
        public MSReactProp<float> PlayerMaxExp { get; private set; }
        public MSReactProp<float> PlayerCurExp { get; private set; }

        public event Action<MonsterCharacter> OnBossSpawned;
        public event Action<string, BaseSkill> OnSkillAdded;
        public event Action<float, float> OnPlayerHPChanged;

        private SkillSystemComponent playerSSC;

        public BattlePanelViewModel(SurvivalMode _mode, PlayerCharacter _player)
        {
            KillCount = _mode.KillCount;
            WaveCount = _mode.CurWaveCount;
            WaveTimer = _mode.CurWaveTimer;

            PlayerGold = _player.LevelSystem.Gold;
            PlayerLevel = _player.LevelSystem.CurLevel;
            PlayerMaxExp = _player.LevelSystem.MaxExp;
            PlayerCurExp = _player.LevelSystem.CurExp;

            _mode.OnBossSpawned += (boss) => OnBossSpawned?.Invoke(boss);
            _player.SSC.OnSkillAdded += (_skillKey, _skill) => OnSkillAdded?.Invoke(_skillKey, _skill);
            _player.SSC.OnHealthChanged += (_curHP, _maxHP) => OnPlayerHPChanged?.Invoke(_curHP, _maxHP);

            playerSSC = _player.SSC;
        }

        public void UseSkill(string _key)
        {
            playerSSC.UseSkill(_key).Forget();
        }
    }
}