using FontAwesome.Sharp;
using SixLabors.ImageSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultimediaFormatConverter
{
    public partial class MainWindow : Window
    {
        public class ListItem
        {
            public string? FileName { get; set; }
            public MediaType? FileType { get; set; }
        }
        private ObservableCollection<ListItem> _items = new ObservableCollection<ListItem>();
        public enum MediaType
        {
            Image,
            Audio,
            Video
        }
        public enum ImageFormat
        {
            Png,
            Jpeg,
            Bmp,
            Gif,
            Webp,
            Tiff
        }
        public enum AudioFormat
        {
            Mp3,
            Wav,
            Aac,
            Flac,
            Ogg,
            Wma
        }
        public enum VideoFormat
        {
            Mp4,
            Avi,
            Mkv,
            WebM,
            Mov,
            Flv,
            Wmv
        }
        private static readonly Dictionary<MediaType, string[]> MediaExtensions = new Dictionary<MediaType, string[]>
        {
            { MediaType.Image, new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".tiff" } },
            { MediaType.Audio, new[] { ".mp3", ".wav", ".aac", ".flac", ".ogg", ".wma" } },
            { MediaType.Video, new[] { ".mp4", ".avi", ".mkv", ".webm", ".mov", ".flv", ".wmv" } }
        };
        private static readonly Dictionary<MediaType, Type> FormatTypeMapping = new Dictionary<MediaType, Type>
        {
            { MediaType.Image, typeof(ImageFormat) },
            { MediaType.Audio, typeof(AudioFormat) },
            { MediaType.Video, typeof(VideoFormat) }
        };
        private MediaType SelectedTypeOnComboBox = MediaType.Image;
        public MainWindow()
        {
            InitializeComponent();
            _items.CollectionChanged += (sender, e) => ListUpdated();
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void PrepareWindow(object sender, RoutedEventArgs e)
        {
            InputExtension.ItemsSource = Enum.GetValues(typeof(MediaType));
            InputExtension.SelectedIndex = 0;
        }
        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void LoadFile(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = GetFileFilter(),
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    ProcessFile(fileName);
                }
            }
        }
        private string GetFileFilter()
        {
            var selectedType = (MediaType)InputExtension.SelectedItem;
            return selectedType switch
            {
                MediaType.Image => "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp;*.tiff",
                MediaType.Audio => "Audio files|*.mp3;*.wav;*.aac;*.flac;*.ogg;*.wma",
                MediaType.Video => "Video files|*.mp4;*.avi;*.mkv;*.webm;*.mov;*.flv;*.wmv",
                _ => throw new InvalidOperationException("Unsupported file type")
            };
        }
        private void ProcessFile(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            var selectedType = (MediaType)InputExtension.SelectedItem;
            if (MediaExtensions[selectedType].Contains(extension))
            {
                if (!_items.Any(item => item.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                {
                    _items.Add(new ListItem() { FileName = filePath, FileType = selectedType });
                }
            }
            else
            {
                if (_items.Count > 0)
                {
                    DropAnnouncement.Opacity = 0;
                    MessageBox.Show("The selected file does not match the expected file type.");
                }
                else
                {
                    var matchedType = MediaExtensions.FirstOrDefault(pair => pair.Value.Contains(extension)).Key;

                    if (matchedType != default)
                    {
                        InputExtension.SelectedItem = matchedType;
                        _items.Add(new ListItem() { FileName = filePath, FileType = matchedType });
                        DropAnnouncement.Opacity = 0;
                    }
                    else
                    {
                        DropAnnouncement.Opacity = 1;
                        MessageBox.Show("The selected file does not match the expected file type.");
                    }
                }
            }
        }
        private void InputDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DropAnnouncement.Opacity = 1;
                DropAnnouncementText.Text = "Drop your file";
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        private void InputDragLeave(object sender, DragEventArgs e)
        {
            DropAnnouncementText.Text = "Drop or Search for a file";
            if (_items.Count > 0)
            {
                DropAnnouncement.Opacity = 0;
            }
            else
            {
                DropAnnouncement.Opacity = 1;
            }
        }
        private void InputDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
        private void InputDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    ProcessFile(file);
                }

                if (_items.Count < files.Length)
                {
                    MessageBox.Show("Some files were not supported and got discarded.");
                }
            }
        }
        private void SelectedNewInputFormat(object sender, RoutedEventArgs e)
        {
            var selectedType = (MediaType)InputExtension.SelectedItem;
            if (SelectedTypeOnComboBox != selectedType)
            {
                SelectedTypeOnComboBox = selectedType;
                if (_items.Count > 0)
                {
                    MessageBoxResult response = MessageBox.Show("Alert", "This will clear the list of files. Proceed anyway?", MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        _items.Clear();
                    }
                    else
                    {
                        switch (SelectedTypeOnComboBox)
                        {
                            case MediaType.Image: InputExtension.SelectedIndex = 0; break;
                            case MediaType.Audio: InputExtension.SelectedIndex = 1; break;
                            case MediaType.Video: InputExtension.SelectedIndex = 2; break;
                        }
                    }
                }
            }
            OutputExtension.ItemsSource = Enum.GetValues(FormatTypeMapping[SelectedTypeOnComboBox]);
            OutputExtension.SelectedIndex = 0;
        }
        private void Convert(object sender, MouseButtonEventArgs e)
        {
            if (_items.Count > 0)
            {
                Conversion();
            }
            else
            {
                MessageBox.Show("The list is empty!");
            }
        }
        private void ShowProgress()
        {
            AppMain.IsEnabled = false;
            ProgressBorder.Visibility = Visibility.Visible;
        }
        private void HideProgress()
        {
            AppMain.IsEnabled = true;
            ProgressBorder.Visibility = Visibility.Collapsed;
        }
        private void UpdateProgress(int processedFiles, int totalFiles)
        {
            if (totalFiles > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Value = (processedFiles / (double)totalFiles) * 100;
                });
            }
        }
        private async void Conversion()
        {
            ShowProgress();
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string outputFolderPath = folderDialog.SelectedPath;
                int totalFiles = _items.Count;
                int processedFiles = 0;
                var selectedItem = OutputExtension.SelectedItem;
                await Task.Run(async () =>
                {
                    foreach (var item in _items)
                    {
                        string outputFilePath = System.IO.Path.Combine(outputFolderPath, System.IO.Path.GetFileNameWithoutExtension(item.FileName) + "." + selectedItem.ToString().ToLower());
                        switch (item.FileType)
                        {
                            case MediaType.Audio:
                                await Task.Run(() => ConvertAudio(item.FileName, outputFilePath, (AudioFormat)selectedItem));
                                break;
                            case MediaType.Image:
                                await Task.Run(() => ConvertImage(item.FileName, outputFilePath, (ImageFormat)selectedItem));
                                break;
                            case MediaType.Video:
                                await Task.Run(() => ConvertVideo(item.FileName, outputFilePath, (VideoFormat)selectedItem));
                                break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            processedFiles++;
                            UpdateProgress(processedFiles, totalFiles);
                        });
                    }
                });
                HideProgress();
                MessageBox.Show("Done!");
            }
            else
            {
                MessageBox.Show("No folder selected!");
            }
        }
        private void ConvertAudio(string inputFilePath, string outputFilePath, AudioFormat outputFormat)
        {
            using var reader = new NAudio.Wave.AudioFileReader(inputFilePath);
            switch (outputFormat)
            {
                case AudioFormat.Mp3:
                    using (var writer = new NAudio.Lame.LameMP3FileWriter(outputFilePath, reader.WaveFormat, NAudio.Lame.LAMEPreset.STANDARD))
                    {
                        reader.CopyTo(writer);
                    }
                    break;
                case AudioFormat.Wav:
                    using (var writer = new NAudio.Wave.WaveFileWriter(outputFilePath, reader.WaveFormat))
                    {
                        reader.CopyTo(writer);
                    }
                    break;
                case AudioFormat.Aac:
                case AudioFormat.Flac:
                case AudioFormat.Ogg:
                case AudioFormat.Wma:
                    ConvertUsingFfmpeg(inputFilePath, outputFilePath, outputFormat);
                    break;
                default:
                    throw new NotSupportedException($"{outputFormat} format is not supported.");
            }
        }
        private void ConvertUsingFfmpeg(string inputFilePath, string outputFilePath, AudioFormat outputFormat)
        {
            var arguments = outputFormat switch
            {
                AudioFormat.Aac => $"-i \"{inputFilePath}\" -c:a aac \"{outputFilePath}\"",
                AudioFormat.Wma => $"-i \"{inputFilePath}\" -c:a wma \"{outputFilePath}\"",
                AudioFormat.Flac => $"-i \"{inputFilePath}\" -c:a flac \"{outputFilePath}\"",
                AudioFormat.Ogg => $"-i \"{inputFilePath}\" -c:a libvorbis \"{outputFilePath}\"",
                _ => throw new NotSupportedException($"{outputFormat} format is not supported.")
            };
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            process.WaitForExit();
        }
        private void ConvertImage(string inputFilePath, string outputFilePath, ImageFormat outputFormat)
        {
            using var image = SixLabors.ImageSharp.Image.Load(inputFilePath);
            SixLabors.ImageSharp.Formats.IImageEncoder encoder = outputFormat switch
            {
                ImageFormat.Png => new SixLabors.ImageSharp.Formats.Png.PngEncoder(),
                ImageFormat.Jpeg => new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder(),
                ImageFormat.Bmp => new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder(),
                ImageFormat.Gif => new SixLabors.ImageSharp.Formats.Gif.GifEncoder(),
                ImageFormat.Webp => new SixLabors.ImageSharp.Formats.Webp.WebpEncoder(),
                ImageFormat.Tiff => new SixLabors.ImageSharp.Formats.Tiff.TiffEncoder(),
                _ => throw new NotSupportedException($"{outputFormat} format is not supported."),
            };
            image.Save(outputFilePath, encoder);
        }
        private void ConvertVideo(string inputFilePath, string outputFilePath, VideoFormat outputFormat)
        {
            var ffmpeg = new NReco.VideoConverter.FFMpegConverter();
            string extension = outputFormat switch
            {
                VideoFormat.Mp4 => ".mp4",
                VideoFormat.Avi => ".avi",
                VideoFormat.Mkv => ".mkv",
                VideoFormat.WebM => ".webm",
                VideoFormat.Mov => ".mov",
                VideoFormat.Flv => ".flv",
                VideoFormat.Wmv => ".wmv",
                _ => throw new NotSupportedException($"{outputFormat} format is not supported."),
            };
            ffmpeg.ConvertMedia(inputFilePath, outputFilePath + extension, null);
        }
        private void ListUpdated()
        {
            FileList.ItemsSource = _items;
            if (_items.Count > 0)
            {
                DropAnnouncement.Opacity = 0;
            }
            else
            {
                DropAnnouncement.Opacity = 1;
            }
        }
        private void RemoveListItem(object sender, MouseButtonEventArgs e)
        {
            IconBlock? itemToRemove = sender as IconBlock;
            if (itemToRemove != null)
            {
                _items.Remove(_items.FirstOrDefault(item => item.FileName.Equals(itemToRemove.Uid, StringComparison.OrdinalIgnoreCase)));
            }
        }
    }
}

