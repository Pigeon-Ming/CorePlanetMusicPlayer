using CorePlanetMusicPlayer.App;
using CorePlanetMusicPlayer.PlayCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls
{
    public sealed partial class EqualizerControl : UserControl
    {
        IPlayEngine playEngine;
        private Slider[] eqSliders;
        public EqualizerControl()
        {
            this.InitializeComponent();
            InitPresets();
            playEngine = ProgramData.PlayEngine;
            Loaded += EqualizerControl_Loaded;
        }

        private async void EqualizerControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 等待均衡器初始化完成
            int retry = 0;
            while (!playEngine.IsEqualizerSupported && retry < 30)
            {
                await System.Threading.Tasks.Task.Delay(100);
                retry++;
            }
            if (!playEngine.IsEqualizerSupported)
            {
                IsEnabled = false;
                return;
            }
            // 动态获取Band数量
            int bandCount = playEngine.EqualizerBandCount;
            eqSliders = new Slider[] { eq0, eq1, eq2, eq3, eq4, eq5, eq6, eq7, eq8, eq9 };
            if (bandCount < eqSliders.Length)
            {
                // 多余的Slider禁用
                for (int i = bandCount; i < eqSliders.Length; i++)
                    eqSliders[i].IsEnabled = false;
            }
            // 初始化Slider的值
            for (int i = 0; i < bandCount && i < eqSliders.Length; i++)
            {
                eqSliders[i].Value = playEngine.GetEqualizerGain(i);
                eqSliders[i].IsEnabled = true;
            }
            EqualizerToggleSwitch.IsOn = playEngine.IsEqualizerEnabled;
        }

        private void EqualizerToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            playEngine.IsEqualizerEnabled = EqualizerToggleSwitch.IsOn;
            if (eqSliders == null)
                return;
            foreach (var slider in eqSliders)
                slider.IsEnabled = EqualizerToggleSwitch.IsOn;
        }

        Dictionary<string, double[]> _presets;

        private void InitPresets()
        {
            _presets = new Dictionary<string, double[]>();

            // 基础通用预设
            _presets["Normal"] = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // 标准原声，无任何音效调整
            _presets["Reduce Bass"] = new double[] { -6, -5, -4, -3, -2, -1, 0, 0, 0, 0 }; // 削弱低音，避免轰头浑浊
            _presets["Reduce Treble"] = new double[] { 0, 0, 0, 0, 0, -1, -2, -3, -4, -6 }; // 削弱高音，减少刺耳感
            _presets["Small Speaker"] = new double[] { -9, -8, -7, -6, -4, 0, 2, 3, 3, 2 }; // 小音箱/手机专用，防止破音

            // 音乐风格预设
            _presets["R&B"] = new double[] { 3, 2, 1, 0, 0, 0, 1, 2, 3, 4 }; // R&B风格，温暖人声+柔和低音
            _presets["Bass"] = new double[] { 6, 5, 4, 2, 0, 0, -1, -2, -3, -4 }; // 重低音，强化鼓点与贝斯
            _presets["Electronic"] = new double[] { 5, 3, 1, 0, 0, 1, 2, 3, 5, 6 }; // 电子音乐，动感节奏+明亮音色
            _presets["Piano"] = new double[] { -2, -1, 0, 1, 2, 3, 2, 1, 0, -1 }; // 钢琴专用，突出通透质感与细节
            _presets["Treble"] = new double[] { -6, -5, -4, -3, -2, 0, 2, 4, 5, 6 }; // 增强高音，提升解析与通透度
            _presets["Classical"] = new double[] { -2, -2, -1, 0, 1, 2, 2, 1, 0, -1 }; // 古典音乐，还原声场与乐器细节
            _presets["Jazz"] = new double[] { -2, -1, 0, 2, 3, 2, 1, 0, -1, -2 }; // 爵士风格，温润中频+舒适听感
            _presets["Latin"] = new double[] { -1, 0, 1, 2, 3, 2, 1, 1, 0, -1 }; // 拉丁音乐，突出节奏与人声律动
            _presets["Pop"] = new double[] { 4, 3, 2, 1, 0, 0, 1, 2, 3, 4 }; // 流行音乐，人声清晰+均衡三频
            _presets["Vocal"] = new double[] { -6, -5, -4, -3, -2, 1, 3, 4, 5, 3 }; // 人声增强，适合听歌、播客、朗诵
            _presets["Dance"] = new double[] { 6, 5, 4, 3, 2, 2, 3, 4, 5, 6 }; // 舞曲风格，强劲低频+动感高频
            _presets["Hip Hop"] = new double[] { 6, 6, 5, 3, 1, 0, -1, -1, 0, 1 }; // 嘻哈说唱，厚重低音+清晰人声
            _presets["Relax"] = new double[] { 2, 1, 0, 0, 0, 0, 0, 1, 2, 3 }; // 放松模式，柔和舒缓，适合助眠
            _presets["Rock"] = new double[] { 5, 4, 3, 1, 0, 0, 1, 3, 4, 5 }; // 摇滚风格，强劲节奏+金属质感
            _presets["Acoustic"] = new double[] { -2, -1, 0, 1, 2, 2, 1, 1, 0, -1 }; // 原声民谣，还原吉他/人声本色

            // 增强型预设
            _presets["Boost Bass"] = new double[] { 6, 6, 5, 4, 2, 0, -1, -2, -3, -4 }; // 超强低音，震撼冲击感
            _presets["Boost Treble"] = new double[] { -3, -2, -1, 0, 1, 2, 4, 5, 6, 6 }; // 超强高音，极致细节与通透
            _presets["Boost Vocals"] = new double[] { -3, -2, -1, 0, 2, 4, 5, 3, 2, 0 }; // 强化人声，清晰贴耳

            foreach (var item in _presets.Keys)
            {
                PresetsComboBox.Items.Add(new ComboBoxItem { Tag = item.ToString() , Content = item.ToString()});
            }
        }


        private void EqSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //if (!(sender is Slider slider)) return;
            //int bandIndex = Array.IndexOf(eqSliders, slider);
            //if (bandIndex >= 0 && playEngine.IsEqualizerEnabled)
            //{
            //    playEngine.SetEqualizerGain(bandIndex, (float)slider.Value);
            //}
            List<float> gains = new List<float>();
            gains.Add((float)eq0.Value);
            gains.Add((float)eq1.Value);
            gains.Add((float)eq2.Value);
            gains.Add((float)eq3.Value);
            gains.Add((float)eq4.Value);
            gains.Add((float)eq5.Value);
            gains.Add((float)eq6.Value);
            gains.Add((float)eq7.Value);
            gains.Add((float)eq8.Value);
            gains.Add((float)eq9.Value);
            playEngine.SetAllEqualizerGains(gains);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            playEngine.ResetEqualizer();
            // 重置所有Slider的值为0
            for (int i = 0; i < eqSliders.Length; i++)
            {
                eqSliders[i].Value = 0;
            }
        }

        private void PresetsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double[] preset;
            if (_presets.TryGetValue(((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag.ToString(), out preset))
            {
                SetPreset(preset);
            }
        }
        
        void SetPreset(double[] preset)
        {
            for(int i = 0; i < preset.Length; i++)
            {
                eqSliders[i].Value = preset[i];
            }
        }
    }
}
