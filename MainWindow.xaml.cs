using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HDLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        string cKey = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();

            CheckFFXIVBinaryExists();
        }

        private async void MetroWindow_Initialized(object sender, EventArgs e)
        {
            Visibility = Visibility.Hidden;

            await AutoUpdater.Update();

            Visibility = Visibility.Visible;
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.Load();

            CheckFFXIVBinaryExists();

            Username.Text = Settings.Username;

            if (Settings.SavePassword)
            {
                Password.Password = Settings.Password;
                SavePassword.IsChecked = true;
            }

            DC_Chocobo.IsChecked = Settings.DataCenter == Settings.DataCenters.Chocobo;

            await LoadRecaptcha(true);

            EventManager.RegisterClassHandler(typeof(TextBox),
                KeyUpEvent,
                new KeyEventHandler(TextBox_KeyUp));
            EventManager.RegisterClassHandler(typeof(PasswordBox),
                KeyUpEvent,
                new KeyEventHandler(TextBox_KeyUp));
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (GetWindow((DependencyObject)sender) != this) return;
            if (e.Key != Key.Enter) return;

            e.Handled = true;
            ProcessBtn_Click(sender, null);
        }

        private async void ReCaptcha_Reload_Click(object sender, RoutedEventArgs e)
        {
            await LoadRecaptcha();
            ReCaptcha.Clear();
        }

        private async void ProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            bool dryRun = Keyboard.IsKeyDown(Key.LeftCtrl);

            if (!CheckFFXIVBinaryExists())
            {
                return;
            }

            ShowInformation("정보 확인중...");

            Settings.Username = Username.Text;
            if (SavePassword.IsChecked == true)
            {
                Settings.Password = Password.Password;
                Settings.SavePassword = true;
            }
            else
            {
                Settings.Password = "";
                Settings.SavePassword = false;
            }
            Settings.DataCenter = (DC_Moogle.IsChecked == true) ? Settings.DataCenters.Moogle : Settings.DataCenters.Chocobo;
            Settings.Save();

            MainGrid.IsEnabled = false;

            try
            {
                if (string.IsNullOrEmpty(Username.Text))
                {
                    throw new FFException("아이디를 입력해주세요", Username);
                }
                if (string.IsNullOrEmpty(Password.Password))
                {
                    throw new FFException("비밀번호를 입력해주세요", Password);
                }
                if (string.IsNullOrEmpty(ReCaptcha.Text))
                {
                    throw new FFException("자동입력방지 값을 입력해주세요", ReCaptcha);
                }
            }
            catch (FFException ex)
            {
                MainGrid.IsEnabled = true;
                ShowError(ex.Message);
                ex.Cause.Focus();
                return;
            }

            try {
                using (var client = new HttpClient() { BaseAddress = new Uri(Constants.LAUNCHER_BASE_URL) })
                {
                    client.DefaultRequestHeaders
                        .TryAddWithoutValidation("User-Agent", Constants.WEBCLIENT_USER_AGENT);
                    client.DefaultRequestHeaders
                        .Referrer = new Uri(Constants.LAUNCHER_BASE_URL);

                    var values = new Dictionary<string, string>
                    {
                        { "csiteNo", Constants.CSITE_NO },
                        { "gameServiceID", Constants.GAME_SERVICE_ID },
                        { "memberID", Username.Text.Trim() },
                        { "passWord", Password.Password.Trim() },
                        { "recaptcha_challenge_field", cKey.Trim() },
                        { "recaptcha_response_field", ReCaptcha.Text.Trim() }
                    };

                    var content = new FormUrlEncodedContent(values);
                    var response = await client.PostAsync(Constants.LOGIN_URL, content);
                    var resp = await response.Content.ReadAsStringAsync();

                    Match result = Constants.RE_JSON_RESULT.Match(resp);
                    string code = result.Groups[1].Value;

                    if (code != "0")
                    {
                        string message = "오류가 발생했습니다";
                        Control cause = null;
                        switch (code)
                        {
                            case "-101":
                                message = "정상 회원이 아닙니다";
                                break;
                            case "-102":
                                message = "정보가 일치하지 않습니다";
                                break;
                            case "-106":
                                message = "자동입력방지 값이 잘못되었습니다";
                                cause = ReCaptcha;
                                break;
                        }
                        throw new FFException(message);
                    }

                    result = Constants.RE_JSON_LOGIN_RESULT.Match(resp);
                    code = result.Groups[1].Value;

                    if (code != "O")
                    {
                        string message = "오류가 발생했습니다";
                        Control cause = null;
                        switch (code)
                        {
                            case "L": // "고객님의 계정은 비밀번호 5회 입력 실패로 인하여 접속이 제한되었습니다.\r\n개인 정보 보호를 위해 본인 인증 완료 후 로그인이 가능합니다."
                            case "Z": // "모험가님의 계정은 개인정보 보호에 취약한 계정으로 확인됩니다.\r\n홈페이지로 이동하셔서 로그인 후\r\n본인 인증 및 비밀번호 변경, U-OTP+ 설정을 통해\r\n모험가님의 소중한 계정을 보호해주시기 바랍니다."
                            case "Q": // "현재 휴면계정 입니다.\r\n홈페이지를 통해서 휴면계정해지 후 사용 바랍니다.";
                            case "T": // 아이피 유효성 잠금
                            case "C": // 게임 미동의
                                message = "기본 런쳐로 로그인해 문제를 확인하세요";
                                break;
                            case "E":
                                message = "아이디가 잘못되었습니다";
                                cause = Username;
                                break;
                            case "P":
                                message = "비밀번호가 잘못되었습니다";
                                cause = Password;
                                break;
                        }
                        message = "기본 런쳐로 로그인해 문제를 확인하세요";
                        throw new FFException(message, cause);
                    }

                    string motpID = Constants.RE_JSON_MOTP_ID.Match(resp).Groups[1].Value;
                    string memberKey = Constants.RE_JSON_MEMBER_KEY.Match(resp).Groups[1].Value;

                    Match isUsingOTP = Constants.RE_JSON_MOTP_USE.Match(resp);
                    if (isUsingOTP.Success)
                    {
                        ShowInformation("OTP 확인중...");

                        values = new Dictionary<string, string>
                        {
                            { "csiteNo", Constants.CSITE_NO },
                            { "motpID", motpID },
                            { "otpNum", OTP.Text.Trim() },
                            { "memberID", Username.Text.Trim() },
                            { "memberKey", memberKey },
                        };

                        content = new FormUrlEncodedContent(values);
                        response = await client.PostAsync(Constants.OTP_AUTH_URL, content);
                        var otpResp = await response.Content.ReadAsStringAsync();

                        Match otpResult = Constants.RE_JSON_OTP_RESULT.Match(otpResp);
                        if (otpResult.Success)
                        {
                            throw new FFException("OTP 넘버가 잘못되었습니다", OTP);
                        }
                    }

                    ShowInformation("게임 로그인 토큰 취득중...");

                    values = new Dictionary<string, string>
                    {
                        { "csiteNo", Constants.CSITE_NO },
                        { "gameServiceID", Constants.GAME_SERVICE_ID },
                        { "InternetCafeType", Constants.INTERNET_CAFE_TYPE },
                        { "memberID", Username.Text.Trim() },
                        { "memberKey", memberKey },
                    };

                    content = new FormUrlEncodedContent(values);
                    response = await client.PostAsync(Constants.MAKE_TOKEN_URL, content);
                    var tokenResp = await response.Content.ReadAsStringAsync();

                    Match tokenMatch = Constants.RE_JSON_TOKEN.Match(tokenResp);
                    if (!tokenMatch.Success)
                    {
                        throw new FFException("토큰 취득중 오류가 발생했습니다");
                    }

                    ShowInformation("게임 시작!");

                    string lobbyHost, gmHost;

                    if (DC_Moogle.IsChecked == true)
                    {
                        lobbyHost = Constants.MOOGLE_LOBBY_HOST;
                        gmHost = Constants.MOOGLE_GM_HOST;
                    }
                    else
                    {
                        lobbyHost = Constants.CHOCOBO_LOBBY_HOST;
                        gmHost = Constants.CHOCOBO_GM_HOST;
                    }

                    string token = tokenMatch.Groups[1].Value;
                    string commandLine = string.Format(Constants.COMMAND_LINE, lobbyHost, Constants.LOBBY_TCP_PORT, gmHost, token, Constants.RESET_CONFIG);

                    string ffxivPath = Path.Combine(Settings.FFXIVPath, Constants.FFXIV_PROGRAM_PATH);

                    if (dryRun)
                    {
                        string cl = string.Format("\"{0} {1}\"", ffxivPath, commandLine);
                        MessageBoxResult res = MessageBox.Show(cl + "\r\n\r\n==========\r\n\r\nPress OK to copy", "Command Line", MessageBoxButton.OKCancel);
                        if (res == MessageBoxResult.OK)
                        {
                            Clipboard.SetText(cl);
                        }
                    }
                    else
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(ffxivPath, commandLine);
                        if (Settings.RunAsAdministrator)
                        {
                            startInfo.Verb = "runas";
                        }
                        Process.Start(startInfo);
                    }

                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                ReCaptcha_Reload_Click(sender, null);
                MainGrid.IsEnabled = true;

                if (ex is FFException)
                {
                    FFException ffex = (FFException)ex;
                    ShowError(ex.Message);
                    if (ffex.Cause != null)
                    {
                        ffex.Cause.Focus();
                    }
                }
                else
                {
                    ShowError("문제가 발생했습니다");
                }
            }
        }

        private async Task LoadRecaptcha(bool firstRun = false)
        {
            try {
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders
                        .TryAddWithoutValidation("User-Agent", Constants.WEBCLIENT_USER_AGENT);
                    client.DefaultRequestHeaders
                        .Referrer = new Uri(Constants.LAUNCHER_BASE_URL);

                    int iSize = 1024;
                    StringBuilder sbValue = new StringBuilder(iSize);

                    bool res = WinAPI.InternetGetCookieEx(Constants.COOKIE_GOOGLE_URL, "NID", sbValue, ref iSize, Constants.COOKIE_FLAG_HTTPONLY, IntPtr.Zero);
                    if (res)
                    {
                        cookieContainer.Add(new Uri(Constants.COOKIE_GOOGLE_URL), new Cookie("NID", sbValue.ToString()));
                    }

                    if (firstRun)
                    {
                        var fResponse = await client.GetAsync(new Uri(Constants.RECAPTCHA_TOKEN_URL + Constants.RECAPTCHA_KEY));
                        var fResp = await fResponse.Content.ReadAsStringAsync();

                        Match fMatch = Constants.RE_RECAPTCHA_CKEY.Match(fResp);
                        cKey = fMatch.Groups[1].Value;
                    }

                    var response = await client.GetAsync(new Uri(string.Format(Constants.RECAPTCHA_RELOAD_URL, Constants.RECAPTCHA_KEY, cKey)));
                    var resp = await response.Content.ReadAsStringAsync();

                    Match match = Constants.RE_RECAPTCHA_RELOAD_CKEY.Match(resp);
                    cKey = match.Groups[1].Value;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(Constants.RECAPTCHA_IMAGE_URL + cKey);
                    bitmap.EndInit();

                    ReCaptcha_Image.Source = bitmap;
                }
            }
            catch
            {
                ShowError("자동입력방지 이미지를 불러오지 못했습니다");
            }
        }

        private bool CheckFFXIVBinaryExists()
        {
            if (File.Exists(Path.Combine(Settings.FFXIVPath, Constants.FFXIV_PROGRAM_PATH)))
            {
                MainGrid.IsEnabled = true;
                HideMessage();
                return true;
            }

            MainGrid.IsEnabled = false;
            ShowError("FF14 설치 경로를 찾을 수 없습니다.", "상단 설정에서 설치 경로를 지정해 주세요.");
            return false;
        }

        private void ShowError(string message, string description = "")
        {
            ShowMessage(Brushes.Orange, message, description);
        }

        private void ShowInformation(string message, string description = "")
        {
            ShowMessage(Brushes.SkyBlue, message, description);
        }

        private void ShowMessage(Brush backgroundColor, string message, string description)
        {
            MessageContainer.Background = backgroundColor;
            MessageTextBox.Text = message;
            if (!string.IsNullOrEmpty(description))
            {
                MessageTextBox.Text += "\r\n" + description;
            }
            MessageContainer.Visibility = Visibility.Visible;
        }

        private void HideMessage()
        {
            MessageContainer.Visibility = Visibility.Hidden;
        }
    }
}
