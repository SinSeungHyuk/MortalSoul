using Core;
using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MS.Battle
{
    public class SkillSystemComponent
    {
        public FieldCharacter Owner { get; private set; }
        public AttributeSet AttributeSet { get; private set; }

        private Dictionary<string, BaseSkill> ownedSkillDict;
        private Dictionary<string, CancellationTokenSource> runningSkillDict;

        public event Action<string, BaseSkill> OnSkillAdded;
        public event Action<string> OnSkillUsed;


        public void InitSSC(FieldCharacter _owner, AttributeSet _attributeSet)
        {
            Owner = _owner;
            AttributeSet = _attributeSet;
            ownedSkillDict = new Dictionary<string, BaseSkill>();
            runningSkillDict = new Dictionary<string, CancellationTokenSource>();
        }

        public void OnUpdate(float _deltaTime)
        {
            foreach (var skill in ownedSkillDict.Values)
            {
                skill.OnUpdate(_deltaTime);
            }
        }

        public void GiveSkill(string _skillKey)
        {
            if (ownedSkillDict.ContainsKey(_skillKey))
                return;

            Type skillType = Type.GetType("MS.Battle." + _skillKey);
            if (skillType == null)
            {
                Debug.LogError($"[SSC] 스킬 타입을 찾을 수 없음: MS.Battle.{_skillKey}");
                return;
            }

            BaseSkill skillInstance = Activator.CreateInstance(skillType) as BaseSkill;
            if (skillInstance == null)
            {
                Debug.LogError($"[SSC] 스킬 인스턴스 생성 실패: {_skillKey}");
                return;
            }

            if (!Main.Instance.DataManager.SettingData.SkillSettingDict.TryGetValue(_skillKey, out SkillSettingData skillData))
            {
                Debug.LogError($"[SSC] SkillSettingData를 찾을 수 없음: {_skillKey}");
                return;
            }

            skillInstance.InitSkill(this, skillData);
            ownedSkillDict.Add(_skillKey, skillInstance);
            OnSkillAdded?.Invoke(_skillKey, skillInstance);
        }

        public async UniTask UseSkill(string _skillKey)
        {
            if (!ownedSkillDict.TryGetValue(_skillKey, out BaseSkill skillToUse)) return;
            if (skillToUse.IsCooltime) return;
            if (!skillToUse.CanActivateSkill()) return;
            if (runningSkillDict.ContainsKey(_skillKey)) return;

            CancellationTokenSource cts = new CancellationTokenSource();
            runningSkillDict[_skillKey] = cts;
            if (!skillToUse.IsPostUseCooltime) skillToUse.SetCooltime();

            try
            {
                await skillToUse.ActivateSkillAsync(cts.Token);
                OnSkillUsed?.Invoke(_skillKey);
                if (skillToUse.IsPostUseCooltime) skillToUse.SetCooltime();
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[SSC] {_skillKey} 스킬 캔슬");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SSC] {_skillKey} 스킬 사용 중 에러: {e.Message}");
                if (!skillToUse.IsPostUseCooltime) skillToUse.ResetCooltime();
            }
            finally
            {
                // 동일 키로 새로운 UseSkill이 이미 진입했으면 해당 엔트리를 건드리지 않기 위한 동일성 체크
                if (runningSkillDict.TryGetValue(_skillKey, out var current) && ReferenceEquals(current, cts))
                    runningSkillDict.Remove(_skillKey);
                cts.Dispose();
            }
        }

        public void CancelSkill(string _skillKey)
        {
            if (runningSkillDict.TryGetValue(_skillKey, out CancellationTokenSource cts))
            {
                cts.Cancel();
                runningSkillDict.Remove(_skillKey);
            }
        }

        public void CancelAllSkills()
        {
            var ctsList = new List<CancellationTokenSource>(runningSkillDict.Values);
            runningSkillDict.Clear();
            foreach (var cts in ctsList)
                cts.Cancel();
        }

        public bool IsRunningSkill(string _skillKey)
        {
            return runningSkillDict.ContainsKey(_skillKey);
        }

        public bool IsCooltime(string _key)
        {
            if (ownedSkillDict.TryGetValue(_key, out BaseSkill skill))
                return skill.IsCooltime;
            return false;
        }

        public bool HasSkill(string _key)
        {
            return ownedSkillDict.ContainsKey(_key);
        }

        public void ClearSSC()
        {
            // ClearSSC는 Cancel만
            CancelAllSkills();
            ownedSkillDict.Clear();
        }
    }
}
