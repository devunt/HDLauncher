﻿using System.Text.RegularExpressions;

namespace HDLauncher
{
    internal static class Constants
    {
        public const decimal VERSION = 0.3m;
        public const string APPNAME = "HDLauncher";

        public const string RAVEN_DSN = "http://891bfd02549143beb6440056786e3ce9:0d84f2575cd341c69aa68a6c342eec2b@s.devunt.kr/6";

        #region Settings

        public const string SETTINGS_FILEPATH = @"HDLauncher.cfg";
        public const string PASSWORD_CRYPT_KEY = "2Y*}F972ue6r7J99.%cg";

        public const string REG_PATH_VALNAME = "InstallLocation";

        public const string REG_UNINSTALL_KEYPATH1 =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\FINAL FANTASY XIV - KOREA_is1";

        public const string REG_UNINSTALL_KEYPATH2 =
            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FINAL FANTASY XIV - KOREA_is1";

        #endregion

        #region Updater

        public const string GITHUB_REPO = "devunt/HDLauncher";
        public const string UPDATE_TEMP_DIRPATH = @"Updates\";

        #endregion

        #region Login

        public const string LAUNCHER_BASE_URL = "https://launcher.ff14.co.kr";

        public const string LAUNCHER_USER_AGENT = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.2; WOW64; Trident/7.0; .NET4.0C; .NET4.0E; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729)";

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

        public const string LOBBY_HOST = "lobbyf-live.ff14.co.kr";
        public const string GM_HOST = "gm-live.ff14.co.kr";

        public const string FFXIV_BINARY_PATH_DX_9 = @"game\ffxiv.exe";
        public const string FFXIV_BINARY_PATH_DX_11 = @"game\ffxiv_dx11.exe";

        public const string FFXIV_ARGUMENTS =
            "DEV.LobbyHost01={0} DEV.LobbyPort01={1} DEV.GMServerHost={2} DEV.TestSID={3} SYS.resetConfig={4}";

        #endregion

        #region Regexes

        public static readonly Regex RE_RECAPTCHA_CKEY = new Regex("challenge : '(.+?)',");
        public static readonly Regex RE_RECAPTCHA_RELOAD_CKEY = new Regex("reload\\('(.+?)',");

        #endregion
    }
}