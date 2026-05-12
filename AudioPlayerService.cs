using NAudio.Wave;

namespace DeathCounter;

internal sealed class AudioPlayerService : IDisposable
{
    private readonly Random random = new();
    private readonly object syncLock = new();
    private WaveOutEvent? outputDevice;
    private AudioFileReader? audioReader;
    private string? lastPlayedFilePath;

    public void PlayRandomSound(string directoryPath, float volume)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            return;
        }

        var files = Directory
            .EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file =>
            {
                var extension = Path.GetExtension(file);
                return extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".wav", StringComparison.OrdinalIgnoreCase);
            })
            .ToArray();

        if (files.Length == 0)
        {
            return;
        }

        var playableFiles = files;
        if (files.Length > 1 && !string.IsNullOrWhiteSpace(lastPlayedFilePath))
        {
            playableFiles = files
                .Where(file => !string.Equals(file, lastPlayedFilePath, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        var selectedFile = playableFiles[random.Next(playableFiles.Length)];

        lock (syncLock)
        {
            StopAndDisposeCurrentPlayback();

            audioReader = new AudioFileReader(selectedFile)
            {
                Volume = Math.Clamp(volume, 0f, 1f),
            };

            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
            outputDevice.Init(audioReader);
            outputDevice.Play();
            lastPlayedFilePath = selectedFile;
        }
    }

    public void StopPlayback()
    {
        lock (syncLock)
        {
            StopAndDisposeCurrentPlayback();
        }
    }

    public void UpdateVolume(float volume)
    {
        lock (syncLock)
        {
            if (audioReader is not null)
            {
                audioReader.Volume = Math.Clamp(volume, 0f, 1f);
            }
        }
    }

    public void Dispose()
    {
        lock (syncLock)
        {
            StopAndDisposeCurrentPlayback();
        }
    }

    private void OutputDevice_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        lock (syncLock)
        {
            DisposePlaybackObjects();
        }
    }

    private void StopAndDisposeCurrentPlayback()
    {
        if (outputDevice is not null)
        {
            outputDevice.PlaybackStopped -= OutputDevice_PlaybackStopped;
            if (outputDevice.PlaybackState != PlaybackState.Stopped)
            {
                outputDevice.Stop();
            }
        }

        DisposePlaybackObjects();
    }

    private void DisposePlaybackObjects()
    {
        outputDevice?.Dispose();
        outputDevice = null;
        audioReader?.Dispose();
        audioReader = null;
    }
}
