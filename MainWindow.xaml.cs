using NAudio.Wave;//to read songs
using System.IO;
using System.Windows;
using System.Windows.Input;
//using TagLib;

namespace MusicPlayer {
    public partial class MainWindow : Window {
        private WaveOutEvent outputDevice;//need to update to use MediaFoundationReader instead of WaveOutEvent
        private AudioFileReader audioFile;
        bool closing = false;
        bool playing = false;
        readonly int defaultVolume = 20;
        bool useDefaultVolume = true;
        string pathToFolder;
        public MainWindow() {
            InitializeComponent();
        }

        private void Play_Button_Test_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (outputDevice == null) { //if no song is playing we open the filebrowser window
                FileBrowser_Button_Click(sender, e);
            }
            if (playing) { //while playing == true(if a song is being played) it wont create a new outputDevice
                outputDevice.Play();//will continue the current song
            }
        }

        private void PauseButton_Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if(outputDevice != null) {
                outputDevice.Stop();//will pause the currunt song
            }
        }

        private void RewindButton_Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var currentSelected = ListOfSongs_ListView.SelectedIndex;
            if (currentSelected != -1) { 
                if (currentSelected - 1 != -1) {
                    ListOfSongs_ListView.SelectedIndex = currentSelected - 1;
                    NewMusicLoader(pathToFolder + "\\" + ListOfSongs_ListView.Items.GetItemAt(currentSelected - 1));
                }
            }
        }

        private void FowardButton_Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var currentSelected = ListOfSongs_ListView.SelectedIndex;
            ListOfSongs_ListView.SelectedIndex = currentSelected + 1;
            //need to put something here to stop causing a bug
            if(currentSelected+1 < ListOfSongs_ListView.Items.Count) {
                NewMusicLoader(pathToFolder + "\\" + ListOfSongs_ListView.Items.GetItemAt(currentSelected + 1));
            }
        }

        private void Volume_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            int volume = (int)Volume_Slider.Value;
            if (outputDevice != null) {
                outputDevice.Volume = volume / 100f;
            }
            VolumeNumber_TextBox.Text = volume.ToString();
        }

        public void UseDefaultVolume() {
            //this function makes sure that the first song that the player opens, will start at volume 20
            if (useDefaultVolume) {//if true we will use deafultVolume as the volume
                outputDevice.Volume = defaultVolume / 100f;
                VolumeNumber_TextBox.Text = defaultVolume.ToString();
                Volume_Slider.Value = defaultVolume;
                useDefaultVolume = false;//we set to not use the default volume until the app is re opened
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            closing = true;
            if (outputDevice != null) {
                outputDevice.Stop();
            }
        }
        private void OnPlaybackStopped(object sender, StoppedEventArgs args) {
            //making sure that we release all resources that the program is using
            if (closing) {
                outputDevice.Dispose();
                audioFile.Dispose();
            }
            outputDevice.Dispose();
            outputDevice = null;
            audioFile.Dispose();
            audioFile = null;
        }

        private void FileBrowser_Button_Click(object sender, RoutedEventArgs e) {
            var pathDialog = new System.Windows.Forms.OpenFileDialog(); //we use windows 32 namespace to open the dialogue
            pathDialog.Filter = "mp3 file(*.mp3)|*mp3";
            var result = pathDialog.ShowDialog();// will store the path to the file
            switch (result) {
                case System.Windows.Forms.DialogResult.OK: // if we press ok
                    var file = pathDialog.FileName;
                    MusicPlaying_TextBox.Text = file;
                    outputDevice?.Stop();//we stop the current song
                    NewMusicLoader(file);//function to load a new song
                    UseDefaultVolume();
                    SongListUpdater(PathToFolder(file));
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                //if we press cancel nothing happnes
                default:
                    //default, nothing happens
                    break;
            }
        }

        private void NewMusicLoader(string pathToSong) {
            if (outputDevice != null) {
                outputDevice?.Stop();
            }
            MusicPlaying_TextBox.Text = pathToSong;
            outputDevice = new WaveOutEvent();
            audioFile = new AudioFileReader(pathToSong);
            outputDevice.Init(audioFile);
            playing = true;
            outputDevice.Play();
            MetaDataLoader(pathToSong);
        }
        private void MetaDataLoader(string pathToFile) {
            var tfile = TagLib.File.Create(pathToFile);
            string artist;
            artist = (tfile.Tag.FirstPerformer).ToString();
            var songTitle = (tfile.Tag.Title).ToString();
            ArtistName_Label.Content = artist;
            SongName_Label.Content = songTitle;
        }

        private string PathToFolder(string pathToBeTrimmed) {
            //this function takes the path to song and returns the path to the folder in which the song is
            //if a song is in "C:\Music\song.mp3" ,this function will return "C:\Music\"
            int trimTo = 1;
            int pathToBeTrimmedSize = pathToBeTrimmed.Length;
            char lastChar = pathToBeTrimmed[pathToBeTrimmedSize - 1];
            while (lastChar != '\\') {
                lastChar = pathToBeTrimmed[pathToBeTrimmedSize - trimTo];
                trimTo++;
            }
            string trimmedPathToFile = pathToBeTrimmed.Remove(pathToBeTrimmedSize - trimTo + 1);
            pathToFolder = trimmedPathToFile;
            return trimmedPathToFile;
        }
        private void SongListUpdater(string pathToFolder) {
            ListOfSongs_ListView.Items.Clear();
            DirectoryInfo directoryInfo = new DirectoryInfo(pathToFolder);
            FileInfo[] musics = directoryInfo.GetFiles("*.mp3");//we get all files that end with .mp3
            foreach (FileInfo song in musics) {
                ListOfSongs_ListView.Items.Add(song.Name);
            }

        }

        private void ListOfSongs_ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            var selectedSong = ListOfSongs_ListView.SelectedItem;
            if(selectedSong != null) {
                NewMusicLoader(pathToFolder + "\\" + selectedSong.ToString());
            }
            
        }

    }

}
