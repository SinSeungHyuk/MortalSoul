using MS.Data;
using MS.Field;
using MS.Manager;
using MS.Skill;
using MS.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MS.UI
{
    public class BattlePanel : BaseUI
    {
        private ExpBar expBar;
        private HPBar bossHPBar;
        private Button btnPause;
        private TextMeshProUGUI txtGold;
        private TextMeshProUGUI txtKillCount;
        private TextMeshProUGUI txtTimer;
        private TextMeshProUGUI txtWaveCount;
        private TextMeshProUGUI txtLevel;
        private TextMeshProUGUI txtHPBar;
        private Image imgHPBar;
        private List<SkillSlot> skillSlotList = new List<SkillSlot>();

        private BattlePanelViewModel curData;

        private int curSkillSlotIndex = 0;


        public void InitBattlePanel(BattlePanelViewModel _data)
        {
            if (expBar == null) FindUIComponents();

            expBar.InitExpBar();
            bossHPBar.gameObject.SetActive(false);

            curData = _data;
            curData.KillCount.Subscribe(OnKillCountChanged);
            curData.WaveTimer.Subscribe(OnTimerChanged);
            curData.WaveCount.Subscribe(OnWaveCountChanged);
            curData.PlayerGold.Subscribe(OnGoldChanged);
            curData.PlayerLevel.Subscribe(OnLevelChanged);
            curData.PlayerCurExp.Subscribe(OnExpChanged);

            curData.KillCount.ForceNotify();
            curData.WaveTimer.ForceNotify();
            curData.WaveCount.ForceNotify();
            curData.PlayerGold.ForceNotify();
            curData.PlayerLevel.ForceNotify();
            curData.PlayerCurExp.ForceNotify();

            curData.OnBossSpawned += OnBossSpawnedCallback;
            curData.OnSkillAdded += OnSkillAdded;
            curData.OnPlayerHPChanged += OnPlayerHPChanged;
        }

        public void OnUpdate(float _dt)
        {
            for (int i = 0; i < curSkillSlotIndex; i++)
            {
                skillSlotList[i].OnUpdate(_dt);
            }
        }

        private void FindUIComponents()
        {
            expBar = transform.FindChildComponentDeep<ExpBar>("ExpBar");
            bossHPBar = transform.FindChildComponentDeep<HPBar>("BossHPBar");
            btnPause = transform.FindChildComponentDeep<Button>("BtnPause");
            txtGold = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtGold");
            txtKillCount = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtKillCount");
            txtTimer = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtTimer");
            txtWaveCount = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtWaveCount");
            txtLevel = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtLevel");
            txtHPBar = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtPlayerHPBar");
            imgHPBar = transform.FindChildComponentDeep<Image>("ImgPlayerHPBar");

            btnPause.onClick.AddListener(() =>
            {
                PausePopup pausePopup = UIManager.Instance.ShowPopup<PausePopup>("PausePopup");
                pausePopup.InitPausePopup();
            });

            for (int i=1; i<=6;i++)
            {
                SkillSlot skillSlot = transform.FindChildComponentDeep<SkillSlot>("SkillSlot_"+i);
                skillSlotList.Add(skillSlot);
            }
        }

        #region Bind
        private void OnKillCountChanged(int _prev, int _cur)
        {
            txtKillCount.text = _cur.ToString("N0");
        }

        private void OnGoldChanged(int _prev, int _cur)
        {
            txtGold.text = _cur.ToString("N0");
        }

        private void OnTimerChanged(float _prev, float _cur)
        {
            int minutes = (int)_cur / 60;
            int seconds = (int)_cur % 60;
            txtTimer.text = string.Format("{0}:{1:D2}", minutes, seconds);
        }

        private void OnWaveCountChanged(int _prev, int _cur)
        {
            txtWaveCount.text = "Wave "+ _cur.ToString();
        }

        private void OnLevelChanged(int _prev, int _cur)
        {
            txtLevel.text = "Lv. " + _cur.ToString();
        }

        private void OnExpChanged(float _prev, float _cur)
        {
            float ratio = _cur / curData.PlayerMaxExp.Value;
            expBar.UpdateExpBar(ratio);
        }

        private void OnBossSpawnedCallback(MonsterCharacter _boss)
        {
            txtTimer.text = "Boss";
            bossHPBar.gameObject.SetActive(true);
            bossHPBar.InitHPBar(_boss);
        }

        private void OnPlayerHPChanged(float _curHP, float _maxHP)
        {
            txtHPBar.text = _curHP.ToString("F0") + "/" + _maxHP.ToString("F0");
            imgHPBar.fillAmount = _curHP / _maxHP;
        }

        private void OnSkillAdded(string _skillKey, BaseSkill _skill)
        {
            SkillSlot targetSlot = skillSlotList[curSkillSlotIndex];
            targetSlot.InitSkillSlot(_skillKey, _skill);

            targetSlot.OnSkillSlotClicked -= OnSkillSlotClicked;
            targetSlot.OnSkillSlotClicked += OnSkillSlotClicked;

            curSkillSlotIndex++;
        }

        private void OnSkillSlotClicked(string _skillKey)
        {
            curData.UseSkill(_skillKey);
        }
        #endregion

        public override void Close()
        {
            base.Close();

            foreach (var slot in skillSlotList)
            {
                slot.ClearSlot();
            }
            curSkillSlotIndex = 0;

            curData.KillCount.Unsubscribe(OnKillCountChanged);
            curData.WaveTimer.Unsubscribe(OnTimerChanged);
            curData.WaveCount.Unsubscribe(OnWaveCountChanged);
            curData.PlayerGold.Unsubscribe(OnGoldChanged);
            curData.PlayerLevel.Unsubscribe(OnLevelChanged);
            curData.PlayerCurExp.Unsubscribe(OnExpChanged);

            curData.OnBossSpawned -= OnBossSpawnedCallback;
            curData.OnSkillAdded -= OnSkillAdded;
            curData.OnPlayerHPChanged -= OnPlayerHPChanged;

            curData = null;
        }
    }
}