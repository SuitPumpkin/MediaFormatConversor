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

namespace MediaFormatConvertor
{
    public partial class MainWindow : Window
    {
        public class ItemLista
        {
            public string? NombreArchivo { get; set; }
            public MediaType? TipoDeArchivo { get; set; }
        }
        private ObservableCollection<ItemLista> _items = new ObservableCollection<ItemLista>();
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
        private MediaType Tiposeleccionado = MediaType.Image;
        public MainWindow()
        {
            InitializeComponent();
            _items.CollectionChanged += (sender, e) => CambioEnLista();
        }
        private void MoverVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void PrepararVentana(object sender, RoutedEventArgs e)
        {
            InputExtention.ItemsSource = Enum.GetValues(typeof(MediaType));
            InputExtention.SelectedIndex = 0;
        }
        private void CerrarVentana(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void CargarArchivo(object sender, MouseButtonEventArgs e)
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
                    ProcesarArchivo(fileName);
                }
            }
        }
        private string GetFileFilter()
        {
            var selectedType = (MediaType)InputExtention.SelectedItem;
            return selectedType switch
            {
                MediaType.Image => "Archivos de imagen|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp;*.tiff",
                MediaType.Audio => "Archivos de audio|*.mp3;*.wav;*.aac;*.flac;*.ogg;*.wma",
                MediaType.Video => "Archivos de video|*.mp4;*.avi;*.mkv;*.webm;*.mov;*.flv;*.wmv",
                _ => throw new InvalidOperationException("Tipo de archivo no soportado")
            };
        }
        private void ProcesarArchivo(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            var selectedType = (MediaType)InputExtention.SelectedItem;
            if (MediaExtensions[selectedType].Contains(extension))
            {
                if (!_items.Any(item => item.NombreArchivo.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                {
                    _items.Add(new ItemLista() { NombreArchivo = filePath, TipoDeArchivo = selectedType });
                }
            }
            else
            {
                if (_items.Count > 0)
                {
                    DropAnouncement.Opacity = 0;
                    MessageBox.Show("El archivo seleccionado no coincide con el tipo de archivo esperado.");
                }
                else
                {
                    // Validar si coincide la extensión del archivo con alguno de los tipos soportados
                    var matchedType = MediaExtensions.FirstOrDefault(pair => pair.Value.Contains(extension)).Key;

                    if (matchedType != default)
                    {
                        InputExtention.SelectedItem = matchedType;
                        _items.Add(new ItemLista() { NombreArchivo = filePath, TipoDeArchivo = matchedType });
                        DropAnouncement.Opacity = 0;
                    }
                    else
                    {
                        DropAnouncement.Opacity = 1;
                        MessageBox.Show("El archivo seleccionado no coincide con el tipo de archivo esperado.");
                    }
                }
            }
        }
        private void InputDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DropAnouncement.Opacity = 1;
                DropAnouncementText.Text = "Drop your file";
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        private void InputDragLeave(object sender, DragEventArgs e)
        {
            DropAnouncementText.Text = "Drop or Search for a file";
            if (_items.Count > 0)
            {
                DropAnouncement.Opacity = 0;
            }
            else
            {
                DropAnouncement.Opacity = 1;
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
                    ProcesarArchivo(file);
                }

                if (_items.Count < files.Length)
                {
                    MessageBox.Show("Algunos archivos no son compatibles y fueron descartados.");
                }
            }
        }
        private void FormatoDeInputSeleccionado(object sender, RoutedEventArgs e)
        {
            var selectedType = (MediaType)InputExtention.SelectedItem;
            if (Tiposeleccionado != selectedType)
            {
                Tiposeleccionado = selectedType;
                if (_items.Count > 0)
                {
                    MessageBoxResult pregunta = MessageBox.Show("Cuidado", "Esto borrará los items que ya tienes cargados, estas seguro que deseas proceder?", MessageBoxButton.YesNo);
                    if (pregunta == MessageBoxResult.Yes)
                    {
                        _items.Clear();
                    }
                    else
                    {
                        switch (Tiposeleccionado) { case MediaType.Image: InputExtention.SelectedIndex = 0; break; case MediaType.Audio: InputExtention.SelectedIndex = 1; break; case MediaType.Video: InputExtention.SelectedIndex = 2; break; };
                    }
                }
            }
            OutputExtention.ItemsSource = Enum.GetValues(FormatTypeMapping[Tiposeleccionado]);
            OutputExtention.SelectedIndex = 0;
        }
        private void Convertir(object sender, MouseButtonEventArgs e)
        {
            if (_items.Count > 0)
            {
                Conversión();
            }
            else
            {
                MessageBox.Show("Aún no hay ningún archivo para convertir");
            }
        }
        private void MostrarProgreso()
        {
            AppMain.IsEnabled = false;
            ProgressBorder.Visibility = Visibility.Visible;
        }
        private void OcultarProgreso()
        {
            AppMain.IsEnabled = true;
            ProgressBorder.Visibility = Visibility.Collapsed;
        }
        private void UpdateProgress(int processedFiles, int totalFiles)
        {
            // Solo actualiza la barra si el total es mayor a cero
            if (totalFiles > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Value = (processedFiles / (double)totalFiles) * 100;
                });
            }
        }
        private async void Conversión()
        {
            MostrarProgreso();
            // Crear un diálogo para seleccionar la carpeta de destino
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string outputFolderPath = folderDialog.SelectedPath;
                int totalFiles = _items.Count;
                int processedFiles = 0;
                var selecteditemuwu = OutputExtention.SelectedItem;
                await Task.Run(async () =>
                {
                    foreach (var item in _items)
                    {
                        // Generar la ruta de salida para el archivo convertido
                        string outputFilePath = System.IO.Path.Combine(outputFolderPath, System.IO.Path.GetFileNameWithoutExtension(item.NombreArchivo) + "." + selecteditemuwu.ToString().ToLower());

                        // Convertir basado en el tipo de archivo
                        switch (item.TipoDeArchivo)
                        {
                            case MediaType.Audio:
                                await Task.Run(() => ConvertirAudio(item.NombreArchivo, outputFilePath, (AudioFormat)selecteditemuwu));
                                break;
                            case MediaType.Image:
                                await Task.Run(() => ConvertirImagen(item.NombreArchivo, outputFilePath, (ImageFormat)selecteditemuwu));
                                break;
                            case MediaType.Video:
                                await Task.Run(() => ConvertirVideo(item.NombreArchivo, outputFilePath, (VideoFormat)selecteditemuwu));
                                break;
                        }

                        // Actualizar el progreso en el hilo principal
                        Dispatcher.Invoke(() =>
                        {
                            processedFiles++;
                            UpdateProgress(processedFiles, totalFiles);
                        });
                    }
                });
                OcultarProgreso();
                MessageBox.Show("Conversión completa!");
            }
            else
            {
                MessageBox.Show("No se seleccionó ninguna carpeta.");
            }
        }
        private void ConvertirAudio(string inputFilePath, string outputFilePath, AudioFormat outputFormat)
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
                    ConvertirConFFmpeg(inputFilePath, outputFilePath, outputFormat);
                    break;

                default:
                    throw new NotSupportedException($"El formato {outputFormat} no es soportado.");
            }
        }
        private void ConvertirConFFmpeg(string inputFilePath, string outputFilePath, AudioFormat outputFormat)
        {
            var arguments = outputFormat switch
            {
                AudioFormat.Aac => $"-i \"{inputFilePath}\" -c:a aac \"{outputFilePath}\"",
                AudioFormat.Wma => $"-i \"{inputFilePath}\" -c:a wma \"{outputFilePath}\"",
                AudioFormat.Flac => $"-i \"{inputFilePath}\" -c:a flac \"{outputFilePath}\"",
                AudioFormat.Ogg => $"-i \"{inputFilePath}\" -c:a libvorbis \"{outputFilePath}\"",
                _ => throw new NotSupportedException($"El formato {outputFormat} no es soportado por FFmpeg.")
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
        private void ConvertirImagen(string inputFilePath, string outputFilePath, ImageFormat outputFormat)
        {
            // Lee la imagen del archivo de entrada
            using var image = SixLabors.ImageSharp.Image.Load(inputFilePath);
            // Declara el encoder
            SixLabors.ImageSharp.Formats.IImageEncoder encoder = outputFormat switch
            {
                ImageFormat.Png => new SixLabors.ImageSharp.Formats.Png.PngEncoder(),
                ImageFormat.Jpeg => new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder(),
                ImageFormat.Bmp => new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder(),
                ImageFormat.Gif => new SixLabors.ImageSharp.Formats.Gif.GifEncoder(),
                ImageFormat.Webp => new SixLabors.ImageSharp.Formats.Webp.WebpEncoder(),
                ImageFormat.Tiff => new SixLabors.ImageSharp.Formats.Tiff.TiffEncoder(),
                _ => throw new NotSupportedException($"El formato {outputFormat} no es soportado."),
            };
            // Guarda la imagen en el archivo de salida con el formato deseado
            image.Save(outputFilePath, encoder);
        }
        private void ConvertirVideo(string inputFilePath, string outputFilePath, VideoFormat outputFormat)
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
                _ => throw new NotSupportedException($"El formato {outputFormat} no es soportado."),
            };
            ffmpeg.ConvertMedia(inputFilePath, outputFilePath + extension, null);
        }
        private void CambioEnLista()
        {
            ListaDeArchivos.ItemsSource = _items;

            if (_items.Count > 0)
            {
                DropAnouncement.Opacity = 0;
            }
            else
            {
                DropAnouncement.Opacity = 1;
            }
        }
        private void RemoverItemLista(object sender, MouseButtonEventArgs e)
        {
            IconBlock? ItemABorrar = sender as IconBlock;
            if (ItemABorrar != null)
            {
                _items.Remove(_items.FirstOrDefault(item => item.NombreArchivo.Equals(ItemABorrar.Uid, StringComparison.OrdinalIgnoreCase)));
            }
        }
    }
}
