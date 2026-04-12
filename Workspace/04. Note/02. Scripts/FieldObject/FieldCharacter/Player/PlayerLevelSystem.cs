using MS.Core;
using MS.Manager;
using System;
using UnityEngine;


namespace MS.Field
{
    public class PlayerLevelSystem : MonoBehaviour
    {
        public MSReactProp<float> MaxExp { get; private set; } = new MSReactProp<float>();
        public MSReactProp<float> CurExp { get; private set; } = new MSReactProp<float>();
        public MSReactProp<int> CurLevel { get; private set; } = new MSReactProp<int>();
        public MSReactProp<int> Gold { get; private set; } = new MSReactProp<int>();


        public void InitLevelSystem()
        {
            MaxExp.Value = DataManager.Instance.CharacterSettingData.LevelSettingData.BaseExp;
            CurExp.Value = 0f;
            CurLevel.Value = 1;
            Gold.Value = 0;
        }

        public void AddExp(float _amount)
        {
            Gold.Value += (int)_amount;
            float curExp = CurExp.Value + _amount;
            float curMaxExp = MaxExp.Value;
            int curLevel = CurLevel.Value;

            float increaseValue = DataManager.Instance.CharacterSettingData.LevelSettingData.IncreaseExpPerLevel;
            bool isLevelUp = false;

            while (curExp >= curMaxExp)
            {
                curExp -= curMaxExp;
                curLevel++;
                curMaxExp += (curMaxExp * (increaseValue * 0.01f));
                isLevelUp = true;
            }

            if (isLevelUp)
            {
                CurLevel.Value = curLevel;
                MaxExp.Value = curMaxExp;
            }

            CurExp.Value = curExp;
        }
    }
}