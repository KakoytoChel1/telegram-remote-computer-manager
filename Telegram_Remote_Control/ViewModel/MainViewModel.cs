using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram_Remote_Control.Model;
using AudioSwitcher.AudioApi.CoreAudio;
using Telegram.Bot.Types.InputFiles;
using System.IO;
using System.Windows.Forms;

namespace Telegram_Remote_Control.ViewModel
{
#nullable disable
    internal class MainViewModel : INotifyPropertyChanged
    {
        //implement INotifyPropertyChanged and create method to work with property changing
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        CoreAudioDevice defaultAudioDevice;
        ConfigSettings settings;

        //collections with items
        public ObservableCollection<FileCommand> FileCommands { get; set; } //custom commands
        public ObservableCollection<WhiteListItem> WhiteListItems { get; set; } //white list users

        //token to cancel server (turn telegram bot off)
        CancellationTokenSource cts;

        public MainViewModel()
        {
            IsTopMost = false;

            //set default audio device
            defaultAudioDevice = new CoreAudioController().DefaultPlaybackDevice;
            //set base settings for data base
            DataBaseLogic.StartSettings();

            //initialize class for working with app's settings
            settings = new ConfigSettings("AppSettings.xml");

            //fill collections with items from data base
            FileCommands = new ObservableCollection<FileCommand>(DataBaseLogic.GetFileItems());
            WhiteListItems = new ObservableCollection<WhiteListItem>(DataBaseLogic.GetWhiteListItems());

            IsStarting = false;

            //get settings from config
            NewApiKey = settings.GetValueByKey("apiKey");
            IsTrayChecked = Convert.ToBoolean(settings.GetValueByKey("isTray"));

            if (!string.IsNullOrWhiteSpace(settings.GetValueByKey("screenPath")))
            {
                NewScreenPath = settings.GetValueByKey("screenPath");
            }
            else
            {
                settings.SetValueByKey("screenPath", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                NewScreenPath = settings.GetValueByKey("screenPath");
            }

        }

        private void StartTelegramBot(string apiKey , CancellationTokenSource cts)
        {
            try
            {
                var botClient = new TelegramBotClient(apiKey);

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = { }
                };

                botClient.StartReceiving(
                HandleUpdatesAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);
            }
            catch(Exception ex) { System.Windows.MessageBox.Show(ex.Message, "Something went wrong!", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        #region Instructions about app's functionallity

        string partText1 = "<b>1. Custom commands</b> - a mechanism that allows the user to create many necessary commands that can:" +
             "open the programs and URLs selected by the user on the computer. To execute them, you will only need to write a bot command";

        string partText2 = "<b>2. Brightness and volume control</b> - application mechanics that allow you to adjust the overall brightness of the monitor" +
             "the computer on which the program is running, and the system volume, through the bot";

        string partText3 = "<b>3. Screenshot</b> - the ability of the program to take a screenshot: save it to the folder selected by the user" +
             "and then send the result to the dialogue with the bot in Telegram";

        string partText4 = "<b>4. White List</b> - a list of those users who are allowed to control your computer through the bot has been created" +
             "to prevent unwanted management if the bot address accidentally falls into the hands of a stranger";

        string partText5 = "<b>Additionally:</b> After entering the API key, the user can click on the \"Enable\" button, now the application will be" +
             " work as a host for a Telegram bot.";

        string partText6 = "<b>/keyboard</b> - if the keyboard has failed to open for any reason, use this command to restart it.";

        #endregion

        #region Properties

        // keyboard bot commands
        public string CustomList { get; set; } = "Custom commands";
        public string Brightness { get; set; } = "Brightness";
        public string Volume { get; set; } = "Volume";
        public string MakeSreenshot { get; set; } = "ScreenShot";
        public string BasicInfo { get; set; } = "Info";

        //britness changing commands
        public string Add_10_Brightness { get; set; } = "add_10_brightness";
        public string Substract_10_Brightness { get; set; } = "substract_10_brightness";
        public string Add_1_Brightness { get; set; } = "add_1_brightness";
        public string Substract_1_Brightness { get; set; } = "substract_1_brightness";
        public string Current_Brightness { get; set; } = "current_brightness";
        public string Set_0_Brightness { get; set; } = "set_0_brightness";
        public string Set_100_Brightness { get; set; } = "set_100_brightness";

        //volume changing commands
        public string Add_10_Volume { get; set; } = "add_10_volume";
        public string Substract_10_Volume { get; set; } = "substract_10_volume";
        public string Add_1_Volume { get; set; } = "add_1_volume";
        public string Substract_1_Volume { get; set; } = "substract_1_volume";
        public string Current_Volume { get; set; } = "current_volume";
        public string Set_0_Volume { get; set; } = "set_0_volume";
        public string Set_100_Volume { get; set; } = "set_100_volume";


        private FileCommand _selectedFileCommandItem;
        public FileCommand SelectedFileCommandItem
        {
            get { return _selectedFileCommandItem; }
            set { _selectedFileCommandItem = value; OnPropertyChanged(nameof(SelectedFileCommandItem)); }
        }

        private WhiteListItem _selectedWhiteListItem;
        public WhiteListItem SelectedWhiteListItem
        {
            get { return _selectedWhiteListItem; }
            set { _selectedWhiteListItem = value; OnPropertyChanged(nameof(SelectedWhiteListItem)); }
        }

        private string _newFilePath;
        public string NewFilePath
        {
            get { return _newFilePath; }
            set { _newFilePath = value; OnPropertyChanged(nameof(NewFilePath)); }
        }

        private string _newFileCommand;
        public string NewFileCommand
        {
            get { return _newFileCommand; }
            set { _newFileCommand = value; OnPropertyChanged(nameof(NewFileCommand)); }
        }

        private string _newUserName;
        public string NewUserName
        {
            get { return _newUserName; }
            set { _newUserName = value; OnPropertyChanged(nameof(NewUserName));}
        }

        private bool _isTopMost;
        public bool IsTopMost
        {
            get { return _isTopMost; }
            set { _isTopMost = value; OnPropertyChanged(nameof(IsTopMost)); }
        }

        private string _newApiKey;
        public string NewApiKey
        {
            get { return _newApiKey; }
            set { _newApiKey = value; OnPropertyChanged(nameof(NewApiKey)); }
        }

        private string _newScreenPath;
        public string NewScreenPath
        {
            get { return _newScreenPath; }
            set { _newScreenPath = value; OnPropertyChanged(nameof(NewScreenPath)); }
        }

        private bool _isStarting;
        public bool IsStarting
        {
            get { return _isStarting; }
            set { _isStarting = value; OnPropertyChanged(nameof(IsStarting)); }
        }

        private bool _isTrayChecked;
        public bool IsTrayChecked
        {
            get { return _isTrayChecked; }
            set { _isTrayChecked = value; OnPropertyChanged(nameof(IsTrayChecked)); }
        }
        #endregion

        #region Commands

        private ICommand _turnOnServer;
        public ICommand TurnOnServer
        {
            get
            {
                if (_turnOnServer == null)
                {
                    _turnOnServer = new RelayCommand(p =>
                    {
                        cts = new CancellationTokenSource();
                        StartTelegramBot(NewApiKey, cts);
                        IsStarting = true;

                        settings.SetValueByKey("apiKey", NewApiKey);

                    }, (p) => !string.IsNullOrWhiteSpace(NewApiKey) && IsStarting == false);
                }

                return _turnOnServer;
            }
        }

        private ICommand _turnOffServer;
        public ICommand TurnOffServer
        {
            get
            {
                if (_turnOffServer == null)
                {
                    _turnOffServer = new RelayCommand(p =>
                    {
                        cts.Cancel();
                        IsStarting = false;

                    }, (p) => IsStarting == true );
                }

                return _turnOffServer;
            }
        }

        private ICommand _addFileCommand;
        public ICommand AddFileCommand
        {
            get
            {
                if(_addFileCommand == null)
                {
                    _addFileCommand = new RelayCommand(p =>
                    {
                        if(NewFilePath != null && NewFileCommand != null)
                        {
                            FileCommand item = new FileCommand { Path = NewFilePath, Command = NewFileCommand.ToLower()};

                            FileCommands.Add(item);
                            DataBaseLogic.AddNewFileItem(item);

                            NewFilePath = String.Empty;
                            NewFileCommand = String.Empty;
                        }
                    }, (p) => !string.IsNullOrWhiteSpace(NewFilePath) & !string.IsNullOrWhiteSpace(NewFileCommand));
                }

                return _addFileCommand;
            }
        }

        private ICommand _addWhiteListItem;
        public ICommand AddWhiteListItemCommand
        {
            get
            {
                if(_addWhiteListItem == null)
                {
                    _addWhiteListItem = new RelayCommand(p =>
                    {
                        if(NewUserName != null)
                        {
                            WhiteListItem item = new WhiteListItem { UserName = NewUserName };

                            WhiteListItems.Add(item);
                            DataBaseLogic.AddWhiteListItem(item);

                            NewUserName = String.Empty;
                        }

                    }, (p) => !string.IsNullOrWhiteSpace(NewUserName));
                }

                return _addWhiteListItem;
            }
        }

        private ICommand _removeFileCommand;
        public ICommand RemoveFileCommand
        {
            get
            {
                if (_removeFileCommand == null)
                {
                    _removeFileCommand = new RelayCommand(p =>
                    {
                        DataBaseLogic.RemoveFileItem(SelectedFileCommandItem);
                        FileCommands.Remove(SelectedFileCommandItem);
                    });
                }

                return _removeFileCommand;
            }
        }

        private ICommand _removeWhiteListItem;
        public ICommand RemoveWhiteListItemCommand
        {
            get
            {
                if(_removeWhiteListItem == null)
                {
                    _removeWhiteListItem = new RelayCommand(p =>
                    {
                        DataBaseLogic.RemoveWhiteListItem(SelectedWhiteListItem);
                        WhiteListItems.Remove(SelectedWhiteListItem);
                    });
                }

                return _removeWhiteListItem;
            }
        }

        private ICommand _editFileCommand;
        public ICommand EditFileCommand
        {
            get
            {
                if(_editFileCommand == null)
                {
                    _editFileCommand = new RelayCommand(p =>
                    {
                        DataBaseLogic.UpdateFileItem(SelectedFileCommandItem);
                    });
                }

                return _editFileCommand;
            }
        }

        private ICommand _editWhiteListItem;
        public ICommand EditWhiteListItemCommand
        {
            get
            {
                if (_editWhiteListItem == null)
                {
                    _editWhiteListItem = new RelayCommand(p =>
                    {
                        DataBaseLogic.UpdateWhiteListItem(SelectedWhiteListItem);
                    });
                }

                return _editWhiteListItem;
            }
        }

        private ICommand _chooseFilePathAdd;
        public ICommand ChooseFilePathAdd
        {
            get
            {
                if(_chooseFilePathAdd == null)
                {
                    _chooseFilePathAdd = new RelayCommand(p =>
                    {
                        NewFilePath = GetFilePath();
                    });
                }

                return _chooseFilePathAdd;
            }
        }

        private ICommand _chooseFilePathEdit;
        public ICommand ChooseFilePathEdit
        {
            get
            {
                if (_chooseFilePathEdit == null)
                {
                    _chooseFilePathEdit = new RelayCommand(p =>
                    {
                        SelectedFileCommandItem.Path = GetFilePath();
                    });
                }

                return _chooseFilePathEdit;
            }
        }

        private ICommand _chooseScreenPath;
        public ICommand ChooseScreenPath
        {
            get
            {
                if(_chooseScreenPath == null)
                {
                    _chooseScreenPath = new RelayCommand(p =>
                    {
                        NewScreenPath = GetFolderPath();
                        settings.SetValueByKey("screenPath", NewScreenPath);
                    });
                }

                return _chooseScreenPath;
            }
        }

        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if(_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(p =>
                    {
                        MainWindow window = p as MainWindow;

                        if (IsTrayChecked)
                        {
                            window.Hide();
                        }
                        else
                        {
                            window.Close();
                        }
                    });
                }

                return _closeCommand;
            }
        }

        private ICommand _minimizeCommand;
        public ICommand MinimizeCommand
        {
            get
            {
                if(_minimizeCommand == null)
                {
                    _minimizeCommand = new RelayCommand(p =>
                    {
                        MainWindow window = p as MainWindow;

                        window.WindowState = WindowState.Minimized;
                    });
                }

                return _minimizeCommand;
            }
        }

        private ICommand _pinCommand;
        public ICommand PinCommand
        {
            get
            {
                if(_pinCommand == null)
                {
                    _pinCommand = new RelayCommand(p =>
                    {
                        MainWindow window = p as MainWindow;

                        if(IsTopMost == false)
                        {
                            window.Topmost = true;
                            IsTopMost = true;
                        }
                        else
                        {
                            window.Topmost = false;
                            IsTopMost = false;
                        }

                    });
                }

                return _pinCommand;
            }
        }

        private ICommand _saveTraySettings;
        public ICommand SaveTraySettings
        {
            get
            {
                if(_saveTraySettings == null)
                {
                    _saveTraySettings = new RelayCommand(p =>
                    {
                        if(NewApiKey != null && NewScreenPath != null)
                        {
                            settings.SetValueByKey("isTray", IsTrayChecked.ToString());
                        }
                    });
                }

                return _saveTraySettings;
            }
        }


        private ICommand _onClosingCommand;
        public ICommand OnClosingCommand
        {
            get
            {
                if (_onClosingCommand == null)
                {
                    _onClosingCommand = new RelayCommand(p =>
                    {
                        settings.SetValueByKey("apiKey", NewApiKey);
                        settings.SetValueByKey("screenPath", NewScreenPath);
                        settings.SetValueByKey("isTray", IsTrayChecked.ToString());

                        if (IsStarting)
                        {
                            cts.Cancel();
                        }
                    });
                }

                return _onClosingCommand;
            }
        }

        private ICommand _dragWindowCommand;
        public ICommand DragWindowCommand
        {
            get
            {
                if(_dragWindowCommand == null)
                {
                    _dragWindowCommand = new RelayCommand(p =>
                    {
                        MainWindow window = p as MainWindow;

                        window.DragMove();
                    });
                }

                return _dragWindowCommand;
            }
        }

        private ICommand _doubleWindowClickCommand;
        public ICommand DoubleWindowClickCommand
        {
            get
            {
                if(_doubleWindowClickCommand == null)
                {
                    _doubleWindowClickCommand = new RelayCommand(p =>
                    {
                        MainWindow window = p as MainWindow;

                        window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                    });
                }

                return _doubleWindowClickCommand;
            }
        }
        #endregion

        #region Telegram.Bot task methods

        //catch errors from telegram
        private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancelletionToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API error:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.Message.ToString()
            };
            Console.WriteLine(ErrorMessage);

            return Task.CompletedTask;
        }

        //catch updates from telegram
        async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancelletionToken)
        {
            if (update.Type == UpdateType.Message)
            {
                if (IsUserNameInWhiteList(update.Message.Chat.Username))
                {
                    if (update?.Message?.Text != null)
                    {
                        await HandleMessage(botClient, update.Message);
                        return;
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Sorry, you don't have permission to use it!");
                    return;
                }
            }
            if (update?.Type == UpdateType.CallbackQuery)
            {
                if (IsUserNameInWhiteList(update.CallbackQuery.From.Username))
                {
                    await HandleCallbackQuery(botClient, update.CallbackQuery);
                    return;
                }
                else
                {
                    await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, "Sorry, you don't have permission to use it!");
                    return;
                }
            }

        }

        //catch query buttons clicks
        async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            #region Control brightness
            if (callbackQuery.Data == Current_Brightness)
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Current monitor brightness: {WindowsBrightnessClass.Get()}%");
                return;
            }
            else if (callbackQuery.Data == Add_10_Brightness)
            {
                WindowsBrightnessClass.Set(WindowsBrightnessClass.Get() + 10);
                return;
            }
            else if (callbackQuery.Data == Add_1_Brightness)
            {
                WindowsBrightnessClass.Set(WindowsBrightnessClass.Get() + 1);
                return;
            }
            else if (callbackQuery.Data == Substract_10_Brightness)
            {
                WindowsBrightnessClass.Set(WindowsBrightnessClass.Get() - 10);
                return;
            }
            else if (callbackQuery.Data == Substract_1_Brightness)
            {
                WindowsBrightnessClass.Set(WindowsBrightnessClass.Get() - 1);
                return;
            }
            else if(callbackQuery.Data == Set_0_Brightness)
            {
                WindowsBrightnessClass.Set(0);
                return;
            }
            else if(callbackQuery.Data == Set_100_Brightness)
            {
                WindowsBrightnessClass.Set(100);
                return;
            }
            #endregion

            #region Control volume
            if (callbackQuery.Data == Current_Volume)
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Current system volume: {defaultAudioDevice.Volume}%");
                return;
            }
            else if (callbackQuery.Data == Add_10_Volume)
            {
                defaultAudioDevice.Volume += 10;
                return;
            }
            else if (callbackQuery.Data == Add_1_Volume)
            {
                defaultAudioDevice.Volume += 1;
                return;
            }
            else if (callbackQuery.Data == Substract_10_Volume)
            {
                defaultAudioDevice.Volume = defaultAudioDevice.Volume >= 10 ? (defaultAudioDevice.Volume - 10) : 0;
                return;
            }
            else if (callbackQuery.Data == Substract_1_Volume)
            {
                defaultAudioDevice.Volume -= 1;
                return;
            }
            else if (callbackQuery.Data == Set_0_Volume)
            {
                defaultAudioDevice.Volume = 0;
                return;
            }
            else if (callbackQuery.Data == Set_100_Volume)
            {
                defaultAudioDevice.Volume = 100;
                return;
            }
            #endregion
        }

        //text message processing
        async Task HandleMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
        {
            //get sent message
            var receivedMessage = message.Text;

            if (receivedMessage != null)
            {
                if (IsCustomCommand(receivedMessage.ToLower()))
                {
                    await CheckAndExecute(receivedMessage.ToLower(), botClient, message.Chat.Id);
                }
                else
                {
                    await CheckAndExecute(receivedMessage, botClient, message.Chat.Id);
                }
            }

        }
        #endregion

        #region Other methods

        public string GetFolderPath()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                string folderPath = String.Empty;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    folderPath = fbd.SelectedPath;
                }

                return folderPath;
            }
        }
        public string GetFilePath()
        {
            using(var fileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                string result = String.Empty;
                //System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Multiselect = false;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    result = fileDialog.FileName;
                }
                return result;
            }
        }
        //check if custom command exists
        private bool IsCustomCommand(string command)
        {
            var result = false;

            foreach(var item in FileCommands)
            {
                if(item.Command == command)
                {
                    result = true;
                }
            }
            return result;
        }

        private bool IsUserNameInWhiteList(string username)
        {
            var result = false;

            foreach(var item in WhiteListItems)
            {
                if(item.UserName == username)
                {
                    result = true;
                }
            }
            return result;
        }

        //method that receive data from user ,compare it with commands and execute
        async Task CheckAndExecute(string receivedMessage, ITelegramBotClient botClient, long id)
        {
            try
            {
                if (receivedMessage == "/keyboard" || receivedMessage == "/start")
                {
                    ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton[] {Brightness, Volume, MakeSreenshot},
                    new KeyboardButton[] {CustomList, BasicInfo}
                })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(id, "Keyboard have been updated!", replyMarkup: keyboard);
                    return;
                }
                //send list of custom commands
                else if (receivedMessage == CustomList)
                {
                    var msg = "<b>List of custom commands</b>\n\n";

                    foreach (var item in FileCommands)
                    {
                        msg += $"<b>{item.Command}</b> --> \n{item.Path}.\n\n";
                    }
                    if (msg != null)
                    {
                        await botClient.SendTextMessageAsync(id, $"{msg}", ParseMode.Html);
                        return;
                    }
                }
                //control brightness
                else if (receivedMessage == Brightness)
                {
                    InlineKeyboardMarkup keyboard = new(new[]
                    {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Increase to 100%", Set_100_Brightness)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("-10", Substract_10_Brightness),
                        InlineKeyboardButton.WithCallbackData("+10", Add_10_Brightness),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("-1", Substract_1_Brightness),
                        InlineKeyboardButton.WithCallbackData("+1", Add_1_Brightness),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Reduce to 0%", Set_0_Brightness)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Current monitor brightness...", Current_Brightness)
                    }
                });

                    await botClient.SendTextMessageAsync(id, "Brightness control -->", replyMarkup: keyboard);
                    return;
                }
                //control volume
                else if (receivedMessage == Volume)
                {
                    InlineKeyboardMarkup keyboard = new(new[]
                    {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Increase to 100%", Set_100_Volume)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("-10", Substract_10_Volume),
                        InlineKeyboardButton.WithCallbackData("+10", Add_10_Volume),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("-1", Substract_1_Volume),
                        InlineKeyboardButton.WithCallbackData("+1", Add_1_Volume),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Reduce to 0%", Set_0_Volume)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Current system volume...", Current_Volume)
                    }
                });

                    await botClient.SendTextMessageAsync(id, "Volume control -->", replyMarkup: keyboard);
                    return;
                }
                //make screenshot and send to chat
                else if (receivedMessage == MakeSreenshot)
                {
                    //set default save-path if the textbox is empty
                    string selectedFolderPath = !string.IsNullOrWhiteSpace(NewScreenPath) ? NewScreenPath : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                    var path = WindowsScreenShotClass.CaptureMyScreen(selectedFolderPath);

                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        InputOnlineFile file = new InputOnlineFile(stream);

                        await botClient.SendPhotoAsync(id, file, "Received screenshot.");
                        return;
                    }

                }
                //send the instructions
                else if (receivedMessage == BasicInfo)
                {
                    await botClient.SendTextMessageAsync(id, $"{partText1}\n\n{partText2}\n\n{partText3}\n\n{partText4}\n\n{partText5}\n\n{partText6}", ParseMode.Html);
                    return;
                }
                //check and execute custom command
                else if (IsCustomCommand(receivedMessage))
                {
                    ExecuteCustomCommand(receivedMessage);

                    await botClient.SendTextMessageAsync(id, "Request have been completed!");
                    return;
                }
                //send as echo if there weren't matches
                else
                {
                    await botClient.SendTextMessageAsync(id, $"You said: {receivedMessage}");
                    return;
                }
            }
            catch(Exception ex) { System.Windows.MessageBox.Show(ex.Message, "Something went wrong!", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        //call this method when we wanna execute custom user's command
        private void ExecuteCustomCommand(string command)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    foreach (var item in FileCommands)
                    {
                        if (item.Command == command)
                        {
                            Process.Start(new ProcessStartInfo { FileName = item.Path, UseShellExecute = true });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Something went wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
