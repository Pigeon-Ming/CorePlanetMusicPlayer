using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace PlanetMusicPlayer.Controls.DevControls.LyricControls
{
    public sealed partial class ScrollingLyricControl : UserControl
    {
        List<Lyric> lyrics = new List<Lyric>();
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer indexTimer = new DispatcherTimer();
        public ScrollingLyricControl(List<Lyric>lyrics)
        {
            this.InitializeComponent();
            this.lyrics = lyrics;

            AddLyrics();


            timer.Interval = TimeSpan.FromSeconds(0.75);
            timer.Tick += Timer_Tick;
            timer.Start();

            indexTimer.Interval = TimeSpan.FromSeconds(0.5);
            indexTimer.Tick += IndexTimer_Tick; ;
            indexTimer.Start();
        }

        private void IndexTimer_Tick(object sender, object e)
        {
            newLyricIndex = LyricManager.GetCurrentLyricIndex(lyrics,CurrentIndex);


        }

        void AddLyrics()
        {
            ContentStackPanel.Children.Clear();
            for (int i = 0; i < lyrics.Count; i++)
            {
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.WrapWholeWords;
                tb.Text = lyrics[i].Content;
                tb.FontSize = 25;
                tb.Margin = new Thickness(0, 10, 0, 10);
                tb.FontWeight = Windows.UI.Text.FontWeights.Bold;
                tb.Name = "LyricItem" + i.ToString();
                tb.Opacity = 0.1f;


                //tb.Height = Double.NaN;
                ContentStackPanel.Children.Add(tb);
            }
        }

        public int CurrentIndex = -1;
        public int newLyricIndex = -1;
        

        private void Timer_Tick(object sender, object e)
        {
            
            Debug.WriteLine(CurrentIndex+"|"+newLyricIndex);
            if (CurrentIndex != newLyricIndex && newLyricIndex != -1)
            {
                CurrentIndex = newLyricIndex;
                ScrollLyricAsync();
                Animation_ScrollToLyric();
            }
        }

        public void ScrollLyricAsync()
        {
            if (CurrentIndex >= 0 && CurrentIndex < lyrics.Count)
            {
                if (CurrentIndex > 0)
                {
                    TextBlock textBlock_last = (TextBlock)FindName("LyricItem" + (CurrentIndex - 1).ToString());
                    Animation_LyricDown(textBlock_last);
                }
                TextBlock textBlock = (TextBlock)FindName("LyricItem" + CurrentIndex.ToString());
                if (textBlock != null)
                {
                    Animation_LyricUp(textBlock);
                }
            }
        }



        /*以下为test内容*/



        public void Animation_LyricDown(TextBlock textBlock)
        {
            //Debug.WriteLine("Down:" + textBlock.Name + "  " + CurrentIndex);

            var storyBoard = new Storyboard();

            var opAnimation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(0.5)), From = 1f, To = 0.1f, EnableDependentAnimation = true };
            Storyboard.SetTarget(opAnimation, textBlock);
            Storyboard.SetTargetProperty(opAnimation, "(UIElement.Opacity)");
            storyBoard.FillBehavior = FillBehavior.HoldEnd;
            storyBoard.Children.Add(opAnimation);
            storyBoard.Begin();
        }

        public void Animation_LyricUp(TextBlock textBlock)
        {
            //Debug.WriteLine("UP:"+textBlock.Name+"  "+CurrentIndex);

            var storyBoard = new Storyboard();

            var opAnimation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(0.5)), From = 0.1f, To = 1f, EnableDependentAnimation = true };
            Storyboard.SetTarget(opAnimation, textBlock);
            Storyboard.SetTargetProperty(opAnimation, "(UIElement.Opacity)");
            storyBoard.FillBehavior = FillBehavior.HoldEnd;
            storyBoard.Children.Add(opAnimation);
            storyBoard.Begin();
        }

        public void GetControlHeightList()
        {
            ControlHeight.Clear();
            for (int i = 0; i < ContentStackPanel.Children.Count; i++)
            {
                ContentStackPanel.UpdateLayout();
                ControlHeight.Add((ContentStackPanel.Children[i] as TextBlock).ActualHeight + 20);
            }
        }

        public List<double> ControlHeight = new List<double>();



        public void Animation_ScrollToLyric()
        {
            GetControlHeightList();
            double Height = 0;
            if (CurrentIndex != 0)
            {
                if (!SmallScreen)
                {
                    for (int i = 0; i < CurrentIndex - 1; i++)
                    {
                        Height += ControlHeight[i];
                    }
                }
                else
                {
                    for (int i = 0; i < CurrentIndex; i++)
                    {
                        Height += ControlHeight[i];
                    }
                    Height += 10;
                }
            }

            ContentScrollViewer.ChangeView(0,/*(CurrentIndex-1)*lyricitem_height*/Height, null);


        }

        bool SmallScreen = false;

        private void lyricScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Window.Current.Bounds.Width > 690)
            {
                SmallScreen = false;

            }
            else
            {
                SmallScreen = true;

            }
        }



        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Tick -= Timer_Tick;
            timer.Stop();
        }
    }
}
