using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HDLauncher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string cKey = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
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

            DX_9.IsChecked = Settings.GraphicApi == Settings.GraphicApis.DX_9;

            await LoadRecaptcha(true);

            EventManager.RegisterClassHandler(typeof(TextBox), KeyUpEvent, new KeyEventHandler(TextBox_KeyUp));
            EventManager.RegisterClassHandler(typeof(PasswordBox), KeyUpEvent, new KeyEventHandler(TextBox_KeyUp));
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
            var dryRun = Keyboard.IsKeyDown(Key.LeftCtrl);

            if (!CheckFFXIVBinaryExists())
                return;

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
            Settings.GraphicApi = DX_9.IsChecked == true ? Settings.GraphicApis.DX_9 : Settings.GraphicApis.DX_11;
            Settings.Save();

            MainGrid.IsEnabled = false;

            try
            {
                if (string.IsNullOrEmpty(Username.Text))
                    throw new FFException("아이디를 입력해주세요", Username);
                if (string.IsNullOrEmpty(Password.Password))
                    throw new FFException("비밀번호를 입력해주세요", Password);
                if (string.IsNullOrEmpty(ReCaptcha.Text))
                    throw new FFException("자동입력방지 값을 입력해주세요", ReCaptcha);
            }
            catch (FFException ex)
            {
                MainGrid.IsEnabled = true;
                ShowError(ex.Message);
                ex.Cause.Focus();
                return;
            }

            try
            {
                using (var client = new HttpClient { BaseAddress = new Uri(Constants.LAUNCHER_BASE_URL) })
                {
                    client.DefaultRequestHeaders
                        .TryAddWithoutValidation("User-Agent", Constants.LAUNCHER_USER_AGENT);
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

                    var loginJson = await client.GetJson(Constants.LOGIN_URL, values);

                    if (loginJson.result.ToString() != "0")
                    {
                        string message;
                        Control cause = null;
                        switch (loginJson.result.ToString())
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
                            default:
                                message = "오류가 발생했습니다";
                                break;
                        }

                        throw new FFException(message, cause);
                    }

                    if (loginJson.loginResult.ToString() != "O")
                    {
                        string message;
                        Control cause = null;
                        switch (loginJson.loginResult.ToString())
                        {
                            case "E":
                                message = "아이디가 잘못되었습니다";
                                cause = Username;
                                break;
                            case "P":
                                message = "비밀번호가 잘못되었습니다";
                                cause = Password;
                                break;
                            case "L": // "고객님의 계정은 비밀번호 5회 입력 실패로 인하여 접속이 제한되었습니다.\r\n개인 정보 보호를 위해 본인 인증 완료 후 로그인이 가능합니다."
                            case "Z": // "모험가님의 계정은 개인정보 보호에 취약한 계정으로 확인됩니다.\r\n홈페이지로 이동하셔서 로그인 후\r\n본인 인증 및 비밀번호 변경, U-OTP+ 설정을 통해\r\n모험가님의 소중한 계정을 보호해주시기 바랍니다."
                            case "D": // "중복 접속으로 로그인이 제한됩니다.\r\n로그아웃 후 재 접속하신 경우 잠시 후에 다시 시도해주세요."
                            case "N": // "현재 블럭 처리 된 상태입니다."
                            case "Q": // "현재 휴면계정 입니다.\r\n홈페이지를 통해서 휴면계정해지 후 사용 바랍니다."
                            case "V": // "장기간 접속하지 않아 휴면계정 처리 되었습니다. 홈페이지를 통해서 휴면계정 해제를 진행해주세요."
                            case "T": // 아이피 유효성 잠금
                            case "C": // 게임 미동의
                            default:
                                message = "기본 런쳐로 로그인해 문제를 확인하세요";
                                break;
                        }

                        throw new FFException(message, cause);
                    }

                    if (loginJson.motpUse == "O")
                    {
                        ShowInformation("OTP 확인중...");

                        var token = OTP.Text.Trim();
                        if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(Settings.UOtpId) && !string.IsNullOrEmpty(Settings.UOtpSeed))
                        {
                            var oid = int.Parse(Settings.UOtpId);
                            var seed = Convert.FromBase64String(Settings.UOtpSeed);
                            token = OTPTokenGenerator.GenerateToken(oid, seed);
                        }

                        values = new Dictionary<string, string>
                        {
                            { "csiteNo", Constants.CSITE_NO },
                            { "motpID", loginJson.motpID.ToString() },
                            { "otpNum", token },
                            { "memberID", Username.Text.Trim() },
                            { "memberKey", loginJson.memberKey.ToString() }
                        };

                        var otpJson = await client.GetJson(Constants.OTP_AUTH_URL, values);
                        if (otpJson.Result != "")
                            throw new FFException("OTP 번호가 잘못되었습니다", OTP);
                    }

                    ShowInformation("게임 로그인 토큰 취득중...");

                    values = new Dictionary<string, string>
                    {
                        { "csiteNo", Constants.CSITE_NO },
                        { "gameServiceID", Constants.GAME_SERVICE_ID },
                        { "InternetCafeType", Constants.INTERNET_CAFE_TYPE },
                        { "memberID", Username.Text.Trim() },
                        { "memberKey", loginJson.memberKey.ToString() }
                    };

                    var tokenJson = await client.GetJson(Constants.MAKE_TOKEN_URL, values);

                    if (tokenJson.tokenResult != "0")
                        throw new FFException("토큰 취득중 오류가 발생했습니다");

                    ShowInformation("게임 시작!");

                    string binaryPath;

                    if (DX_9.IsChecked == true)
                    {
                        binaryPath = Constants.FFXIV_BINARY_PATH_DX_9;
                    }
                    else
                    {
                        binaryPath = Constants.FFXIV_BINARY_PATH_DX_11;
                    }

                    var commandLine = string.Format(Constants.FFXIV_ARGUMENTS,
                        Constants.LOBBY_HOST, Constants.LOBBY_TCP_PORT, Constants.GM_HOST, tokenJson.toKen, Constants.RESET_CONFIG);

                    var ffxivPath = Path.Combine(Settings.FFXIVPath, binaryPath);

                    if (dryRun)
                    {
                        var cl = $"\"{ffxivPath}\" {commandLine}";
                        var res = MessageBox.Show(cl + "\r\n\r\n==========\r\n\r\nPress OK to copy",
                            "Command Line", MessageBoxButton.OKCancel);
                        if (res == MessageBoxResult.OK)
                            Clipboard.SetText(cl);
                    }
                    else
                    {
                        var startInfo = new ProcessStartInfo(ffxivPath, commandLine);
                        if (Settings.RunAsAdministrator)
                            startInfo.Verb = "runas";
                        Process.Start(startInfo);
                    }

                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                ReCaptcha_Reload_Click(sender, null);
                MainGrid.IsEnabled = true;

                if (ex is FFException ffex)
                {
                    ShowError(ex.Message);
                    ffex.Cause?.Focus();
                }
                else
                {
                    ShowError("문제가 발생했습니다");
                }
            }
        }

        private async Task LoadRecaptcha(bool firstRun = false)
        {
            try
            {
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler { CookieContainer = cookieContainer })
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders
                        .TryAddWithoutValidation("User-Agent", Constants.LAUNCHER_USER_AGENT);
                    client.DefaultRequestHeaders
                        .Referrer = new Uri(Constants.LAUNCHER_BASE_URL);

                    var iSize = 1024;
                    var sbValue = new StringBuilder(iSize);

                    var res = WinAPI.InternetGetCookieEx(Constants.COOKIE_GOOGLE_URL, "NID", sbValue, ref iSize,
                        Constants.COOKIE_FLAG_HTTPONLY, IntPtr.Zero);
                    if (res)
                        cookieContainer.Add(new Uri(Constants.COOKIE_GOOGLE_URL),
                            new Cookie("NID", sbValue.ToString()));

                    if (firstRun)
                    {
                        var fResponse =
                            await client.GetAsync(
                                new Uri(Constants.RECAPTCHA_TOKEN_URL + Constants.RECAPTCHA_SITE_KEY));
                        var fResp = await fResponse.Content.ReadAsStringAsync();

                        var fMatch = Constants.RE_RECAPTCHA_CKEY.Match(fResp);
                        cKey = fMatch.Groups[1].Value;
                    }

                    var response = await client.GetAsync(new Uri(string.Format(Constants.RECAPTCHA_RELOAD_URL,
                        Constants.RECAPTCHA_SITE_KEY, cKey)));
                    var resp = await response.Content.ReadAsStringAsync();

                    var match = Constants.RE_RECAPTCHA_RELOAD_CKEY.Match(resp);
                    cKey = match.Groups[1].Value;

                    var bitmap = new BitmapImage();
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
            if (File.Exists(Path.Combine(Settings.FFXIVPath, Constants.FFXIV_BINARY_PATH_DX_9)))
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
                MessageTextBox.Text += "\r\n" + description;
            MessageContainer.Visibility = Visibility.Visible;
        }

        private void HideMessage()
        {
            MessageContainer.Visibility = Visibility.Hidden;
        }
    }
}