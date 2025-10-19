using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xaml;
using System.Xml;

namespace projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isPlaying;
        bool isLoopOn = false;
        bool isPlaylistOpen = false;
        bool isSliding = false;
        bool isFullscreen = false;

        bool mouseOverControllerPanel = false;
        bool isAnimating = false;

        double actualWidth, actualHeight;

        List<Uri> playlistItems = new List<Uri>();

        string[] audioFormats = new string[3] {"mp3","wav","ogg"};
        int selectedMediaIndex = -1;
        double volume = 50;

        public MainWindow()
        {
            InitializeComponent();
            //RadialGradientBrush RadialGradient = (RadialGradientBrush)FindResource("RadialGradientLigh");
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    // If the current maximum value of the progressbar isn't the duration of the video
                    if (mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds != mediaPlayerProgress.Maximum)
                    {
                        mediaPlayerProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                    }
                    if (mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds == mediaPlayer.Position.TotalSeconds && isLoopOn)
                    {
                        mediaPlayer.Position = new TimeSpan(0);
                    }

                    // Update the progressbar
                    mediaPlayerProgress.Value = Convert.ToDouble(mediaPlayer.Position.TotalMilliseconds);
                    setCurrentTimeLabelContent();
                }
            }
        }

        private void setCurrentTimeLabelContent()
        {
            int currHours = (int)mediaPlayer.Position.Hours;
            string currHoursOut = currHours >= 10 ? currHours.ToString() : "0" + currHours.ToString();

            int totalHours = (int)mediaPlayer.NaturalDuration.TimeSpan.Hours;
            string totalHoursOut = totalHours >= 10 ? totalHours.ToString() : "0" + totalHours.ToString();
            timeLabel.Content = $"{currHoursOut}:{mediaPlayer.Position.ToString(@"mm\:ss")}/{totalHoursOut}:{mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}";
        }

        private void setWindowSizeByFormat(string format)
        {
            if (audioFormats.Contains(format))
            {
                SizeToContent = SizeToContent.Manual;
                Width = 300;
                Height = 400;
                mediaPlayer.Visibility = Visibility.Collapsed;
                placeholder.Visibility = Visibility.Visible;
            }
            else
            {
                placeholder.Visibility = Visibility.Collapsed;
                mediaPlayer.Visibility = Visibility.Visible;
                while (true)
                {
                    if (mediaPlayer.NaturalVideoHeight != 0)
                    {
                        Width = mediaPlayer.NaturalVideoWidth;
                        Height = mediaPlayer.NaturalVideoHeight + 119;
                        actualWidth = mediaPlayer.ActualWidth;
                        actualHeight = mediaPlayer.ActualHeight;
                        break;
                    }
                }
            }
        }

        #region file handling
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            
            String[] FileName = (String[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
            if (FileName.Length > 0)
            {
                String MediaPath = FileName[0];
                mediaPlayer.Source = new Uri(MediaPath);
                mediaPlayer.Play();
                isPlaying = true;
                addToPlaylist();
                string path = MediaPath.Split('\\')[MediaPath.Split('\\').Length - 1];
                Title = path;
                string[] splitPath = path.Split('.');
                //addToPlaylist();
                //MessageBox.Show(splitPath[splitPath.Length - 1]);
                setWindowSizeByFormat(splitPath[splitPath.Length - 1]);
            }
            e.Handled = true;
        }

        private void filebtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog().Value)
            {
                mediaPlayer.Source = new Uri(openFileDialog.FileName);
                mediaPlayer.Play();
                isPlaying = true;
                addToPlaylist();
                string path = openFileDialog.FileName.Split('\\')[openFileDialog.FileName.Split('\\').Length - 1];
                Title = path;
                string[] splitPath = path.Split('.');
                //addToPlaylist();
                setWindowSizeByFormat(splitPath[splitPath.Length - 1]);
            }
        }
        #endregion

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            isPlaying = true;
            // If the video is played through and the play button is clicked it resets the video
            if (mediaPlayer.Source != null && mediaPlayer.Position.TotalSeconds >= mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds)
            {
                mediaPlayer.Position = new TimeSpan(0);
            }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            isPlaying = false;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            ResetMediaPlayer();
        }

        private void ResetMediaPlayer()
        {
            mediaPlayer.Stop();
            mediaPlayer.Source = null;
            mediaPlayerProgress.Value = 0;
            timeLabel.Content = "00:00:00/00:00:00";
            SizeToContent = SizeToContent.Manual;
            Width = 300;
            Height = 400;
        }

        private void mediaPlayerProgress_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point position = Mouse.GetPosition(mediaPlayerProgress);
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Position = new TimeSpan(Convert.ToInt64(Math.Round(position.X / mediaPlayerProgress.ActualWidth, 2) * mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds * 10000)); // 10000000 = 1 sec
                
                setCurrentTimeLabelContent();
            }
        }

        

        private void themebtn_Click(object sender, RoutedEventArgs e)
        {
            Color themeColor_background = (Color)FindResource("themeColor_background");
            Color themeColor_menustrip = (Color)FindResource("themeColor_menustrip");
            Color themeColor_controllerpanel = (Color)FindResource("themeColor_controllerpanel");
            Color themeColor_progressbar = (Color)FindResource("themeColor_progressbar");
            Color themeColor_text = (Color)FindResource("themeColor_text");
            Button playButton = (Button)FindName("playButton");
            Button pauseButton = (Button)FindName("pauseButton");
            Button stopButton = (Button)FindName("stopButton");


            //MessageBox.Show(themebtn.Tag.ToString());

            if (themebtn.Tag.ToString() == "dark")
            {
                // dark theme

                themebtn.Tag = "light";
                themebtn.Content = "Light theme";
                themeColor_background = Color.FromArgb(255, 10, 10, 10);
                themeColor_menustrip = Color.FromArgb(255, 23, 23, 23);
                themeColor_controllerpanel = Color.FromArgb(255, 23, 23, 23);
                themeColor_progressbar = themeColor_background;
                themeColor_text = Colors.White;
                placeholder.Source = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory + "../../play_dark.png"));
                playButton.Content = (Image)FindResource("play_dark");
                pauseButton.Content = (Image)FindResource("pause_dark");
                stopButton.Content = (Image)FindResource("stop_dark");
                foreach (Button b in playlistPanel.Children)
                {
                    b.Foreground = Brushes.White;
                }
            }
            else
            {
                // light theme

                themebtn.Tag = "dark";
                themebtn.Content = "Dark theme";
                themeColor_background = Color.FromArgb(255, 200, 200, 200);
                themeColor_menustrip = Color.FromArgb(255, 230, 230, 230);
                themeColor_controllerpanel = Color.FromArgb(255, 230, 230, 230);
                themeColor_progressbar = Colors.White;
                themeColor_text = Colors.Black;
                placeholder.Source = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory + "../../play_light.png"));
                playButton.Content = (Image)FindResource("play_light");
                pauseButton.Content = (Image)FindResource("pause_light");
                stopButton.Content = (Image)FindResource("stop_light");
                foreach (Button b in playlistPanel.Children)
                {
                    b.Foreground = Brushes.Black;
                }
            }
            Resources["themeColor_background"] = themeColor_background;
            Resources["themeColor_menustrip"] = themeColor_menustrip;
            Resources["themeColor_controllerpanel"] = themeColor_controllerpanel;
            Resources["themeColor_progressbar"] = themeColor_progressbar;
            Resources["themeColor_text"] = themeColor_text;

        }

        private Color GetComplementary(Color color)
        {
            return Color.FromRgb((byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));
        }

        private void loopbtn_Click(object sender, RoutedEventArgs e)
        {
            isLoopOn = !isLoopOn;
            if (isLoopOn)
            {
                loopbtn.Background = new SolidColorBrush(Color.FromArgb(128, 126, 126, 126));
            }
            else loopbtn.Background = new SolidColorBrush(Colors.Transparent);
        }


        private void mediaPlayerProgress_MouseMove(object sender, MouseEventArgs e)
        {
            
            Point position = Mouse.GetPosition(mediaPlayerProgress);
            if (mediaPlayer.Source != null && e.LeftButton == MouseButtonState.Pressed)
            {    
                mediaPlayer.Position = new TimeSpan(Convert.ToInt64(Math.Round(position.X / mediaPlayerProgress.ActualWidth, 10) * mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds * 10000)); // 10000000 = 1 sec
                mediaPlayer.Pause();
                isPlaying = false;
                isSliding = true;
            }
        }

        private void mediaPlayerProgress_MouseLeave(object sender, MouseEventArgs e)
        {
            if (mediaPlayer.Source != null && !isPlaying && isSliding) 
            { 
                isPlaying = true;
                isSliding = false;
                mediaPlayer.Play();
                setCurrentTimeLabelContent();
            }
        }

        private void mediaPlayerProgress_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mediaPlayer.Source != null && !isPlaying) 
            {
                isPlaying = true;
                isSliding = false;
                mediaPlayer.Play();
                setCurrentTimeLabelContent();   
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = (volumeSlider.Value/100);
            volume = volumeSlider.Value;
        }

        private void closebtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mouseEnter(object sender, MouseEventArgs e)
        {
            Color gradientColor1 = (Color)FindResource("gradientColor1");
            Color gradientColor2 = (Color)FindResource("gradientColor2");
            Button btn = (Button)sender;
            GradientStopCollection gradientStopCollection = new GradientStopCollection
            {
                new GradientStop(GetComplementary(gradientColor1), 1),
                new GradientStop(GetComplementary(gradientColor2), 0)
            };
            btn.BorderBrush = new RadialGradientBrush
            {
                RadiusX = 1, 
                RadiusY = 10,
                GradientStops = gradientStopCollection,
                GradientOrigin = new Point(2, 0)
            };
            


        }

        private void mouseLeave(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            Color gradientColor1 = (Color)FindResource("gradientColor1");
            Color gradientColor2 = (Color)FindResource("gradientColor2");
            GradientStopCollection gradientStopCollection = new GradientStopCollection
            {
                new GradientStop(gradientColor1, 1),
                new GradientStop(gradientColor2, 0)
            };
            btn.BorderBrush = new RadialGradientBrush
            {
                RadiusX = 1,
                RadiusY = 10,
                GradientStops = gradientStopCollection,
                GradientOrigin = new Point(2, 0)
            };

        }

        #region playlist
        private void playlistbtn_Click(object sender, RoutedEventArgs e)
        {
            playlistPanel.Visibility = isPlaylistOpen ? Visibility.Hidden : Visibility.Visible;
            removeMediaButton.Visibility = playlistPanel.Visibility;
            isPlaylistOpen = !isPlaylistOpen;
        }

        private int indexOfItemInPlaylist(Button item)
        {
            for (int i = 0; i < playlistPanel.Children.Count; i++)
            {
                if (((Button)playlistPanel.Children[i]).Content.ToString() == item.Content.ToString())
                {
                    return i;
                }
            }
            return -1;
        }

        private void addToPlaylist()
        {
            Button button = new Button();
            string[] filePath = mediaPlayer.Source.LocalPath.Trim().Split('\\');
            button.Content = filePath[filePath.Length - 1];
            button.MouseDoubleClick += playMediaFromPlaylist;
            button.Click += selectMediaFromPlaylist;
            button.Background = Brushes.Transparent;
            button.BorderThickness = new Thickness(0);

            if (indexOfItemInPlaylist(button) == -1)
            {
                playlistPanel.Children.Add(button);
                playlistItems.Add(mediaPlayer.Source);
            }
            removeMediaButton.Width = playlistPanel.Width;
        }

        private void playMediaFromPlaylist(object sender, MouseEventArgs e)
        {
            selectedMediaIndex = indexOfItemInPlaylist((Button)sender);
            mediaPlayer.Source = playlistItems[selectedMediaIndex];
            string[] filePath = mediaPlayer.Source.LocalPath.Trim().Split('\\');
            setWindowSizeByFormat(filePath[filePath.Length - 1].Split('.')[filePath[filePath.Length - 1].Split('.').Length - 1]);
            Title = filePath[filePath.Length-1];
            mediaPlayer.Position = new TimeSpan(0);
            mediaPlayer.Play();
        }

        private void selectMediaFromPlaylist(object sender, RoutedEventArgs e)
        {
            selectedMediaIndex = indexOfItemInPlaylist((Button)sender);
        }

        private void removeFromPlaylist(object sender, RoutedEventArgs e)
        {
            if (selectedMediaIndex >= 0 && selectedMediaIndex < playlistPanel.Children.Count)
            {
                playlistItems.RemoveAt(selectedMediaIndex);
                playlistPanel.Children.RemoveAt(selectedMediaIndex);
            }
        }
        #endregion

        private void controllerPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isFullscreen)
            {
                mouseOverControllerPanel = true;
                showControllerPanel();
            }
        }

        private void controllerPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isFullscreen)
            {
                mouseOverControllerPanel = false;
                startHidingControllerPanel();
            }
        }

        private async void startHidingControllerPanel()
        {
            await Task.Delay(3000);
            if (!mouseOverControllerPanel) hideControllerPanel();
        }

        private void showControllerPanel()
        {
            if (isAnimating || !isFullscreen) return;
            isAnimating = true;

            var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
            fadeIn.Completed += (s, e) => isAnimating = false;
            controllerPanel.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void hideControllerPanel()
        {
            if (isAnimating || !isFullscreen) return;
            isAnimating = true;

            var fadeIn = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            fadeIn.Completed += (s, e) => isAnimating = false;
            controllerPanel.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Dictionary<string, int> playbackSeconds = new Dictionary<string, int> { { "J", 10 }, { "Left", 5 }, { "L", 10 }, { "Right", 5 } };
            if (IsLoaded)
            {
                if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    if (e.Key.ToString() == "J" || e.Key.ToString() == "Left")
                    {
                        TimeSpan newPosition = new TimeSpan((int)mediaPlayer.Position.Hours, (int)mediaPlayer.Position.Minutes, (int)mediaPlayer.Position.Seconds - playbackSeconds[e.Key.ToString()]);
                        if (newPosition < new TimeSpan(0))
                        {
                            newPosition = new TimeSpan(0);
                        }
                        else if (newPosition > mediaPlayer.NaturalDuration.TimeSpan)
                        {
                            newPosition = mediaPlayer.NaturalDuration.TimeSpan;
                        }
                        mediaPlayer.Position = newPosition;

                    }
                    if (e.Key.ToString() == "L" || e.Key.ToString() == "Right")
                    {
                        TimeSpan newPosition = new TimeSpan((int)mediaPlayer.Position.Hours, (int)mediaPlayer.Position.Minutes, (int)mediaPlayer.Position.Seconds + playbackSeconds[e.Key.ToString()]);
                        if (newPosition < new TimeSpan(0))
                        {
                            newPosition = new TimeSpan(0);
                        }
                        else if (newPosition > mediaPlayer.NaturalDuration.TimeSpan)
                        {
                            newPosition = mediaPlayer.NaturalDuration.TimeSpan;
                        }
                        mediaPlayer.Position = newPosition;
                    }
                    if (e.Key.ToString() == "K" || e.Key.ToString() == "Space")
                    {
                        if (mediaPlayer.Source != null && mediaPlayer.Position.TotalSeconds >= mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds)
                        {
                            mediaPlayer.Position = new TimeSpan(0);
                        }
                        else
                        {
                            if (isPlaying)
                            {
                                mediaPlayer.Pause();
                                isPlaying = false;
                            }
                            else
                            {
                                mediaPlayer.Play();
                                isPlaying = true;
                            }
                        }
                    }
                }
                if (e.Key.ToString() == "Up")
                {
                    volumeSlider.Value += 5;
                }
                if (e.Key.ToString() == "Down")
                {
                    volumeSlider.Value -= 5;
                }
                if (e.Key.ToString() == "M")
                {
                    if (mediaPlayer.Volume > 0)
                    {
                        double value = volumeSlider.Value;
                        mediaPlayer.Volume = 0;
                        volumeSlider.Value = 0;
                        volume = value;
                    }
                    else
                    {
                        mediaPlayer.Volume = (volume / 100);
                        volumeSlider.Value = volume;
                    }
                }
                if (e.Key.ToString() == "F")
                {
                    if (!isFullscreen && mediaPlayer.HasVideo)
                    {
                        mediaPlayer.Margin = new Thickness(0);
                        WindowStyle = WindowStyle.None;
                        WindowState = WindowState.Maximized;
                        mediaPlayer.Width = Width;
                        mediaPlayer.Height = Height;
                        menuStrip.Visibility = Visibility.Collapsed;
                        playlistPanel.Visibility = Visibility.Collapsed;
                        removeMediaButton.Visibility = Visibility.Collapsed;
                        controllerPanel.Opacity = 0;
                        controllerPanel.Background.Opacity = 0.6;
                        isFullscreen = !isFullscreen;
                        Resources["themeColor_background"] = Colors.Black;
                    }
                    else
                    {
                        
                        mediaPlayer.Margin = new Thickness(0,20,0,80);
                        WindowStyle = WindowStyle.ThreeDBorderWindow;
                        WindowState = WindowState.Normal;
                        mediaPlayer.ClearValue(FrameworkElement.WidthProperty);
                        mediaPlayer.ClearValue(FrameworkElement.HeightProperty);
                        menuStrip.Visibility = Visibility.Visible;
                        controllerPanel.BeginAnimation(UIElement.OpacityProperty, null);
                        controllerPanel.Opacity = 1;
                        controllerPanel.Background.Opacity = 1;
                        isAnimating = false;
                        mouseOverControllerPanel = false;
                        if (isPlaylistOpen)
                        {
                            playlistPanel.Visibility = Visibility.Visible;
                            removeMediaButton.Visibility = Visibility.Visible;
                        }
                        
                        isFullscreen = !isFullscreen;
                        if (themebtn.Tag.ToString() == "light")
                        {
                            Resources["themeColor_background"] = Color.FromArgb(255, 10, 10, 10);
                        }
                        else
                        {
                            Resources["themeColor_background"] = Color.FromArgb(255, 200, 200, 200);
                        }
                        
                    }
                }
                if (e.Key.ToString() == "Delete")
                {
                    if (selectedMediaIndex >= 0 && selectedMediaIndex < playlistPanel.Children.Count)
                    {
                        playlistItems.RemoveAt(selectedMediaIndex);
                        playlistPanel.Children.RemoveAt(selectedMediaIndex);
                    }
                }
            }
        }
    }
}
