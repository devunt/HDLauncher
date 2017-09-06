using System.Text.RegularExpressions;

namespace HDLauncher
{
    internal static class Constants
    {
        public const decimal VERSION = 0.2m;

        #region Settings

        public const string SETTINGS_FILENAME = @".\HDLauncher.cfg";
        public const string PASSWORD_CRYPT_KEY = "2Y*}F972ue6r7J99.%cg";

        public const string REG_PATH_VALNAME = "InstallLocation";

        public const string REG_UNINSTALL_KEYPATH1 =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\FINAL FANTASY XIV - KOREA_is1";

        public const string REG_UNINSTALL_KEYPATH2 =
            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FINAL FANTASY XIV - KOREA_is1";

        #endregion

        #region Updater

        public const string UPDATE_BASE_URL = "http://hdlauncher.devunt.kr";
        public const string UPDATE_VERCHECK_URL = "/version.txt";
        public const string UPDATE_FILE_DIR = "/files/";
        public const string UPDATE_FILE_EXT = ".exe";

        public const string UPDATE_TEMP_FILENAME = @".\.HDLauncher.exe";

        #endregion

        #region Login

        public const string LAUNCHER_BASE_URL = "https://launcher.ff14.co.kr";

        public const string LAUNCHER_USER_AGENT =
                "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.2; WOW64; Trident/7.0; .NET4.0C; .NET4.0E; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729)"
            ;

        public const string LOGIN_URL = "/LauncherFF/LauncherProcess";
        public const string OTP_AUTH_URL = "/LauncherFF/OTPCheck";
        public const string MAKE_TOKEN_URL = "/LauncherFF/MakeToken";
        public const string NOTICES_URL = "/LauncherFF/LauncherBoardContets?CsiteNo=";

        public const string CSITE_NO = "0";
        public const string GAME_SERVICE_ID = "34";
        public const string INTERNET_CAFE_TYPE = "0";

        #endregion

        #region ReCaptcha

        public const string COOKIE_GOOGLE_URL = "https://google.com";
        public const int COOKIE_FLAG_HTTPONLY = 0x00002000;

        public const string RECAPTCHA_SITE_KEY = "6LcQRgsTAAAAAHn5zqd7QpMnTjn2tJb82U3NgR3r";
        public const string RECAPTCHA_TOKEN_URL = "https://www.google.com/recaptcha/api/challenge?k=";

        public const string RECAPTCHA_RELOAD_URL =
            "https://www.google.com/recaptcha/api/reload?reason=r&type=image&k={0}&c={1}";

        public const string RECAPTCHA_IMAGE_URL = "https://www.google.com/recaptcha/api/image?c=";

        #endregion

        #region FFXIV

        public const int LOBBY_TCP_PORT = 54994;
        public const int RESET_CONFIG = 0;

        public const string MOOGLE_LOBBY_HOST = "lobbyf-live.ff14.co.kr";
        public const string MOOGLE_GM_HOST = "gm-live.ff14.co.kr";

        public const string CHOCOBO_LOBBY_HOST = "lobbyf-live2.ff14.co.kr";
        public const string CHOCOBO_GM_HOST = "gm-live2.ff14.co.kr";

        public const string FFXIV_PROGRAM_PATH = @"game\ffxiv.exe";

        public const string FFXIV_ARGUMENTS =
            "DEV.LobbyHost01={0} DEV.LobbyPort01={1} DEV.GMServerHost={2} DEV.TestSID={3} SYS.resetConfig={4}";

        #endregion

        #region Regexes

        public static readonly Regex RE_RECAPTCHA_CKEY = new Regex("challenge : '(.+?)',");
        public static readonly Regex RE_RECAPTCHA_RELOAD_CKEY = new Regex("reload\\('(.+?)',");

        public static readonly Regex RE_JSON_RESULT = new Regex("\"result\":\"(.+?)\"");
        public static readonly Regex RE_JSON_LOGIN_RESULT = new Regex("\"loginResult\":\"(.+?)\"");
        public static readonly Regex RE_JSON_MEMBER_KEY = new Regex("\"memberKey\":\"(.+?)\"");
        public static readonly Regex RE_JSON_MOTP_USE = new Regex("\"motpUse\":\"O\"");
        public static readonly Regex RE_JSON_MOTP_ID = new Regex("\"motpID\":\"(.+?)\"");
        public static readonly Regex RE_JSON_TOKEN = new Regex("\"toKen\":\"(.+?)\"");
        public static readonly Regex RE_JSON_OTP_RESULT = new Regex("\"Result\":\"(.+?)\"");

        #endregion
    }
}