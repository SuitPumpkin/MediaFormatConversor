# Multimedia Format Converter

![image](https://github.com/user-attachments/assets/0fe8106f-0674-4c3b-97e4-77403812006a)

**Multimedia Format Converter** is a desktop application developed in WPF (.NET) that allows you to convert multimedia files between different formats. The app supports images, audio, and videos, providing an intuitive interface for selecting and converting files efficiently.

## Features

- **Image Conversion**: Supports image formats like PNG, JPEG, BMP, GIF, WebP, and TIFF.
- **Audio Conversion**: Converts between audio formats like MP3, WAV, AAC, FLAC, OGG, and WMA.
- **Video Conversion**: Compatible with video formats like MP4, AVI, MKV, WebM, MOV, FLV, and WMV.
- **Drag and Drop**: Lets you drag and drop files directly into the app to add them to the list.
- **Multi-Selection**: Load and process multiple files at once.
- **Conversion Progress**: Shows a progress bar during file conversion.
- **User Interface**: Modern, sleek design with support for animations and customization.

## Requirements

- **.NET Framework**: The app is developed for the .NET Framework.
- **Libraries**:
  - **NAudio**: For audio file conversion.
  - **ImageSharp**: For image conversion.
  - **NReco.VideoConverter**: For video conversion.
  - **FFmpeg**: Required for converting additional audio formats not directly supported by NAudio.

## Installation

1. Clone the repository to your local machine:
   ```bash
   git clone https://github.com/SuitPumpkin/MultimediaFormatConverter.git
   ```

2. Open the solution in Visual Studio.

3. Restore the NuGet packages and build the project.

4. Make sure you have FFmpeg installed and accessible in your PATH.

## Usage

1. Launch the app.
2. Select the type of input file (Image, Audio, or Video) from the `ComboBox`.
3. Choose the desired output format.
4. Drag and drop files into the window or use the button to browse for files.
5. Click the "Convert" button to start the conversion.
6. Wait for the conversion to complete; the converted files will be available in the selected folder.

## Contributions

Contributions are welcome. If you encounter any issues or have suggestions to improve the app, please open an issue or submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE).
