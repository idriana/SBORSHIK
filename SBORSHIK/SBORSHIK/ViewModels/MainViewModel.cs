using ReactiveUI;
using SBORSHIK.core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using System.Collections.Generic;
using DynamicData.Kernel;
using System.Web;
using System.Reflection;
using System.ComponentModel.DataAnnotations;


namespace SBORSHIK.ViewModels;

public class MainViewModel : ViewModelBase, INotifyPropertyChanged
{
    #region private fields
    private CompileData _data = new();
    private bool _ButtonsEnabled = true;
    #endregion

    #region getters/setters
    
    // Выбрасывание ошибок в сеттере обсолютно нормально, авалония обрабатывает их как валидацию переменной.
    public string MKVToolNixFile
    {
        get => _data.MKVToolNixFile;
        set 
        {
            _data.MKVToolNixFile = value; 
            OnPropertyChanged(nameof(MKVToolNixFile));
            if (string.IsNullOrEmpty(value) || !FileSystemHelper.CheckFileExists(value.Trim()))
                throw new Exception();
        }
    }
    public string VideoFile
    {
        get => _data.VideoFile;
        set
        {
            _data.VideoFile = value; 
            OnPropertyChanged(nameof(VideoFile));
            if (string.IsNullOrEmpty(value) || !FileSystemHelper.CheckFileExists(value.Trim()))
                throw new Exception();
        }
        }
    public string SourceAudioFile
    {
        get => _data.SourceAudioFile;
        set
        {
            _data.SourceAudioFile = value;
            OnPropertyChanged(nameof(SourceAudioFile));
            if (string.IsNullOrEmpty(value) || !FileSystemHelper.CheckFileExists(value.Trim()))
                throw new Exception();
        }
    }
    public string NewAudioFile
    {
        get => _data.NewAudioFile;
        set
        {
            _data.NewAudioFile = value; 
            OnPropertyChanged(nameof(SourceAudioFile));
            if (string.IsNullOrEmpty(value) || !FileSystemHelper.CheckFileExists(value.Trim()))
                throw new Exception();
        }
    }
    public string StudioName
    {
        get => _data.StudioName;
        set
        {
            _data.StudioName = value;
            OnPropertyChanged(nameof(SourceAudioFile));
            if (string.IsNullOrEmpty(value))
                throw new Exception();
        }
    }
    public string SourceLanguage
    {
        get => _data.SourceLanguage;
        set
        {
            _data.SourceLanguage = value;
            OnPropertyChanged(nameof(SourceAudioFile));
            if (string.IsNullOrEmpty(value))
                throw new Exception();
        }
    }

    public bool Subtitles
    {
        get => _data.Subtitles;
        set { 
            _data.Subtitles = value; 
            OnPropertyChanged(nameof(Subtitles));
            // Обновляем зависимые поля, чтобы UI знал, что они не используются, и не проводил валидацию
            OnPropertyChanged(nameof(SubtitlesFile));
            OnPropertyChanged(nameof(InsctiptionFile));
            OnPropertyChanged(nameof(Translation));
            OnPropertyChanged(nameof(FontsFile));
        }
    }

    public string SubtitlesFile
    {
        get => _data.SubtitlesFile;
        set
        {
            _data.SubtitlesFile = value;
            OnPropertyChanged(nameof(SubtitlesFile));
            if (Subtitles && (string.IsNullOrEmpty(value) || !FileSystemHelper.CheckFileExists(value.Trim())))
                throw new Exception();
        }
    }

    public string InsctiptionFile
    {
        get => _data.InsctiptionFile;
        set
        {
            _data.InsctiptionFile = value;
            OnPropertyChanged(nameof(InsctiptionFile));
            if (Subtitles && (string.IsNullOrEmpty(value) || !FileSystemHelper.CheckFileExists(value.Trim())))
                throw new Exception();
        }
    }

    public string Translation
    {
        get => _data.Translation;
        set
        {
            _data.Translation = value;
            OnPropertyChanged(nameof(Translation));
            if (Subtitles && (string.IsNullOrEmpty(value)))
                throw new Exception();
        }
    }

    public string FontsFile
    {
        get { return string.Join(";", _data.FontsFile); }
        set
        {
            _data.FontsFile = value.Split(";").AsList();
            OnPropertyChanged(nameof(FontsFile)); 
            foreach (string font in _data.FontsFile)
            {
                if (Subtitles && (string.IsNullOrEmpty(font) || !FileSystemHelper.CheckFileExists(font.Trim())))
                    throw new Exception();
            }
        }
    }

    public string OutputFile
    {
        get => _data.OutputFile;
        set
        {
            _data.OutputFile = value;
            OnPropertyChanged(nameof(OutputFile));
            if (string.IsNullOrEmpty(value))
                throw new Exception();
        }
    }

    public string Logs
    {
        get => _data.Logs;
        set { _data.Logs = value; OnPropertyChanged(nameof(Logs)); }
    }

    public bool ButtonsEnabled
    {
        get => _ButtonsEnabled;
        private set { _ButtonsEnabled = value; OnPropertyChanged(nameof(ButtonsEnabled)); }
    }
    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    public ReactiveCommand<Unit, Unit> CompileCommand { get; }

    public ReactiveCommand<Unit, Unit> CheckBoxToggledCommand { get; }

    public ReactiveCommand<string, Unit> OpenSingleFileDialogCommand { get; }

    public ReactiveCommand<string, Unit> OpenMiltipleFilesDialogCommand { get; }

    public ReactiveCommand<string, Unit> SaveFileDialogCommand { get; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainViewModel()
    {
        string serializarionPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\user_data\\data.xml";
        _data = DataSerializer.ReadFromXmlFile<CompileData>(serializarionPath);
        _data.Logs = "";

        // TODO: change 
        var isValidObservable = this.WhenAnyValue(
                x => x.MKVToolNixFile,
                x => !string.IsNullOrWhiteSpace(x));
        CompileCommand = ReactiveCommand.CreateFromObservable(Compile, isValidObservable);

        OpenSingleFileDialogCommand = ReactiveCommand.CreateFromTask<string>(OpenSingleFileDialog);

        OpenMiltipleFilesDialogCommand = ReactiveCommand.CreateFromTask<string>(OpenMiltipleFilesDialog);

        SaveFileDialogCommand = ReactiveCommand.CreateFromTask<string>(SaveFileDialog);

        PropertyChanged += SaveData;
    }

    private void SaveData(object? sender, PropertyChangedEventArgs e)
    {
        string serializarionPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\user_data\\data.xml";
        DataSerializer.WriteToXmlFile(serializarionPath, _data, false);
    }

    public void CheckBoxToggled()
    {
        Subtitles = !Subtitles;
    }

    private IStorageProvider GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");
        return provider;
    }

    public async Task<Unit> OpenSingleFileDialog(string bindedParameterName)
    {
        ButtonsEnabled = false;
        var provider = GetStorageProvider();
        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open file",
            AllowMultiple = false,
        });
        if (files.Count != 0)
        {
            var path = Uri.UnescapeDataString(files[0].Path.AbsolutePath);
            _data.SetVariable(bindedParameterName, path);
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(bindedParameterName));
        }
        ButtonsEnabled = true;
        return new Unit();
    }

    public async Task<Unit> OpenMiltipleFilesDialog(string bindedParameterName)
    {
        ButtonsEnabled = false;
        var provider = GetStorageProvider();
        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open file",
            AllowMultiple = true,
        });

        List<string> filePaths = new();
        if (files.Count != 0 && files != null)
        {
            int size = files.Count;
            for (int i = 0; i < size; i++)
                filePaths.Add(Uri.UnescapeDataString(files[i].Path.AbsolutePath));
        }
        _data.SetVariable(bindedParameterName, filePaths);
        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(bindedParameterName));
        ButtonsEnabled = true;
        return new Unit();
    }

    public async Task<Unit> SaveFileDialog(string bindedParameterName)
    {
        ButtonsEnabled = false;
        var provider = GetStorageProvider();
        var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save file",
            SuggestedFileName = "MKVResult",
            DefaultExtension = "mkv",
        }) ;

        if (file != null)
        {
            _data.SetVariable(bindedParameterName, Uri.UnescapeDataString(file.Path.AbsolutePath));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(bindedParameterName));
        }

        ButtonsEnabled = true;
        return new Unit();
    }

    public IObservable<Unit> Compile()
    {
        return Observable.Start(() =>
        {
            var compiler = new Compiler(_data, PropertyChanged, this, nameof(Logs));
            compiler.Compile();
        });
    }
}
