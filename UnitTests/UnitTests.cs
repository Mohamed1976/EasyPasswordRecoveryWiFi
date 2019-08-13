using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Models.Wlan;
using EasyPasswordRecoveryWiFi.Services;
using NUnit.Framework;
using System;

namespace Tests
{
	public class Tests
	{
		private IProfileService _profileService = null;

		[SetUp]
		public void Initialize()
		{
			_profileService = new ProfileService(new ProfileFactory());
		}

		[TearDown]
		public void CleanUp()
		{
		}

		#region [ Password rules validation ]

		const string WepErrorMsg = "WEP key is 10, 26 or 40 hex digits long.";
		const string AesAndTKIPErrorMsg = "AES/TKIP password is 8 to 63 ASCII characters long.";
		const string EmptyErrorMsg = null;

		[Test]
		[TestCase(null, WiFiEncryptionType.None, true, EmptyErrorMsg)]
		[TestCase(null, WiFiEncryptionType.WEP, false, WepErrorMsg)]
		[TestCase(null, WiFiEncryptionType.AES, false, AesAndTKIPErrorMsg)]
		[TestCase(null, WiFiEncryptionType.TKIP, false, AesAndTKIPErrorMsg)]
		[TestCase("506173737", WiFiEncryptionType.WEP, false, WepErrorMsg)]
		[TestCase("5061737377", WiFiEncryptionType.WEP, true, EmptyErrorMsg)] //key.Length == 10 (hex)
		[TestCase("50617373774", WiFiEncryptionType.WEP, false, WepErrorMsg)]
		[TestCase("3132333435363738393061626", WiFiEncryptionType.WEP, false, WepErrorMsg)]
		[TestCase("31323334353637383930616263", WiFiEncryptionType.WEP, true, EmptyErrorMsg)] //key.Length == 26 (hex)
		[TestCase("313233343536373839306162636", WiFiEncryptionType.WEP, false, WepErrorMsg)]
		[TestCase("313233343536373839306162636465666768696", WiFiEncryptionType.WEP, false, WepErrorMsg)]
		[TestCase("313233343536373839306162636465666768696a", WiFiEncryptionType.WEP, true, EmptyErrorMsg)] //key.Length == 40 (hex)
		[TestCase("313233343536373839306162636465666768696A", WiFiEncryptionType.WEP, true, EmptyErrorMsg)] //key.Length == 40 (hex)
		[TestCase("313233343536373839306162636465666768696a6", WiFiEncryptionType.WEP, false, WepErrorMsg)]
		[TestCase("313233343536373839306162636465666768696g", WiFiEncryptionType.WEP, false, WepErrorMsg)]
		[TestCase("313233343536373839306162636465666768696-", WiFiEncryptionType.WEP, false, WepErrorMsg)]
		[TestCase("ABcd01#", WiFiEncryptionType.AES, false, AesAndTKIPErrorMsg)]
		[TestCase("ABcd01#@", WiFiEncryptionType.AES, true, EmptyErrorMsg)]  //key.Length == 8 (ASCII)
		[TestCase("ABcd01#@Z", WiFiEncryptionType.AES, true, EmptyErrorMsg)]
		[TestCase("tgjrbYlMtSVgHrXJZuVPDRO6iL5zJ9MNh8L14uZg7f0slHFrrO2wnYFiiVyMkB", WiFiEncryptionType.AES, true, EmptyErrorMsg)]
		[TestCase("tgjrbYlMtSVgHrXJZuVPDRO6iL5zJ9MNh8L14uZg7f0slHFrrO2wnYFiiVyMkBF", WiFiEncryptionType.AES, true, EmptyErrorMsg)] //key.Length == 63 (ASCII)
		[TestCase("tgjrbYlMtSVgHrXJZuVPDRO6iL5zJ9MNh8L14uZg7f0slHFrrO2wnYFiiVyMkBFk", WiFiEncryptionType.AES, false, AesAndTKIPErrorMsg)]
		[TestCase("ABcd01#", WiFiEncryptionType.TKIP, false, AesAndTKIPErrorMsg)]
		[TestCase("ABcd01#@", WiFiEncryptionType.TKIP, true, EmptyErrorMsg)] //key.Length == 8 (ASCII)
		[TestCase("ABcd01#@Z", WiFiEncryptionType.TKIP, true, EmptyErrorMsg)]
		[TestCase("tgjrbYlMtSVgHrXJZuVPDRO6iL5zJ9MNh8L14uZg7f0slHFrrO2wnYFiiVyMkB", WiFiEncryptionType.TKIP, true, EmptyErrorMsg)]
		[TestCase("tgjrbYlMtSVgHrXJZuVPDRO6iL5zJ9MNh8L14uZg7f0slHFrrO2wnYFiiVyMkBF", WiFiEncryptionType.TKIP, true, EmptyErrorMsg)] //key.Length == 63 (ASCII)
		[TestCase("tgjrbYlMtSVgHrXJZuVPDRO6iL5zJ9MNh8L14uZg7f0slHFrrO2wnYFiiVyMkBFk", WiFiEncryptionType.TKIP, false, AesAndTKIPErrorMsg)]

		/// <summary>
		/// Validates the password rules for different encryption types.  
		/// </summary>
		/// <param name="password">Password to validate.</param>
		/// <param name="encryption">Encryption type.</param>
		/// <param name="isValid">Is password expected to be valid.</param>
		/// <param name="expectedErrorMsg">Expected error message if password invalid.</param>
		public void ValidatePasswordRules(string password, WiFiEncryptionType encryption, bool isValid, 
			string expectedErrorMsg)
		{
			string errorMsg = null;

			if (isValid)
			{
				Assert.IsTrue(PasswordHelper.IsValid(password, encryption, ref errorMsg),
					"Password was expected to be valid but was actually invalid.");
				Assert.AreEqual(errorMsg, expectedErrorMsg, "Error message contained an unexpected value.");
			}
			else
			{
				Assert.IsFalse(PasswordHelper.IsValid(password, encryption, ref errorMsg),
					"Password was expected to be invalid but was actually valid.");
				Assert.AreEqual(errorMsg, expectedErrorMsg, "Error message contained an unexpected value.");
			}
		}

		#endregion

		#region [ Validate profile creation process ]

		[Test]
		[TestCase(WiFiAuthentication.Open, WiFiEncryptionType.None, "OpenWiFi", WiFiKeyType.None, false, null)]
		[TestCase(WiFiAuthentication.Open, WiFiEncryptionType.WEP, "WepSecuredWiFi", WiFiKeyType.NetworkKey, false, "5061737377")]
		[TestCase(WiFiAuthentication.WPA2_Personal, WiFiEncryptionType.AES, "Wpa2PskAesSecuredWiFi", WiFiKeyType.PassPhrase, false, "ABcd01#@")]
		[TestCase(WiFiAuthentication.WPA_Personal, WiFiEncryptionType.AES, "WpaPskAesSecuredWiFi", WiFiKeyType.PassPhrase, false, "tgjrbYlMtSVgHrXJZuVPDRO6iL5zJ9MNh8L14uZg7f0slHFrrO2wnYFiiVyMkBF")]
		[TestCase(WiFiAuthentication.WPA_Personal, WiFiEncryptionType.TKIP, "WpaPskTkipSecuredWiFi", WiFiKeyType.PassPhrase, false, "tgjrbYlMtSVgHrXJZuVPDRO6iL5zJ9MNh8L14uZg7f0slHFrrO2wnYFiiVyMkBF")]
		[TestCase(WiFiAuthentication.WPA2_Personal, WiFiEncryptionType.TKIP, "Wpa2PskTkipSecuredWiFi", WiFiKeyType.PassPhrase, false, "tgjrbYlMtSVgHrXJZuVPDRO6iL5zJ9MNh8L14uZg7f0slHFrrO2wnYFiiVyMkB")]
		/// <summary>
		/// Validates the profile creation process for different encryption and authentication types.  
		/// </summary>
		/// <param name="authentication">Authentication type of access point.</param>
		/// <param name="encryption">Encryption type of access point.</param>
		/// <param name="ssid">Ssid of access point.</param>
		/// <param name="keyType">KeyType of access point.</param>
		/// <param name="keyIsEncrypted">Key/password specified in profile is encrypted or not.</param>
		/// <param name="password">Password needed to connect to access point, null if open access point.</param>
		public void CreateAndValidateProfile(WiFiAuthentication authentication, 
			WiFiEncryptionType encryption, string ssid, WiFiKeyType keyType, 
			bool keyIsEncrypted, string password)
		{
			/* Create access point, needed in order to create connection profile. */
			AccessPoint accessPoint = new AccessPoint()
			{
				Ssid = ssid,
				Authentication = authentication,
				Encryption = encryption,
				BssType = WiFiBssType.Infrastructure
			};

			string profileXml = _profileService.CreateProfileXml(accessPoint, password);
			Assert.IsNotNull(profileXml, "Generated connection profile is null.");
			Assert.IsNotEmpty(profileXml, "Generated connection profile is string.Empty.");

			/* Attributes of generated profile are returned in profile object. */
			Profile profile = new Profile();
			bool result = _profileService.Parse(profileXml, ref profile);
			Assert.IsTrue(result, "Failed to parse generated profile.");
			/* Validate attributes of generated profile. */
			Assert.AreEqual(authentication, profile.Authentication, "Failed to validate authentication.");
			Assert.AreEqual(encryption, profile.Encryption, "Failed to validate encryption.");
			Assert.AreEqual(ssid, profile.Ssid, "Failed to validate ssid.");
			Assert.AreEqual(ssid, profile.ProfileName, "Failed to validate profile name.");
			Assert.AreEqual(WiFiBssType.Infrastructure, profile.BssType, "Failed to validate profile BSS type.");
			Assert.IsFalse(profile.IsAutoConnectEnabled, "Wireless profile automatic connection is enabled.");
			Assert.IsFalse(profile.IsAutoSwitchEnabled, "Wireless profile automatic switch is enabled.");
			Assert.AreEqual(keyType, profile.KeyType, "Failed to validate wireless profile KeyType.");
			Assert.AreEqual(keyIsEncrypted, profile.KeyIsEncrypted, "Failed to validate profile property KeyIsEncrypted.");
			Assert.AreEqual(password, profile.KeyValue, "Wireless profile password is not equal to expected password.");
		}

		const string BssTypeErrorMsg = "Only BssType.Infrastructure is supported by Microsoft.";
		const string AccessPointTypeNotSupported = "Profile generation for access point is not implemented.";

		[Test]
		[TestCase(WiFiAuthentication.Open, WiFiEncryptionType.None, "OpenWiFi", WiFiBssType.Independent, typeof(NotSupportedException), BssTypeErrorMsg)]
		[TestCase(WiFiAuthentication.WPA2_Enterprise, WiFiEncryptionType.AES, "Wpa2AesSecuredWiFi", WiFiBssType.Infrastructure, typeof(NotImplementedException), AccessPointTypeNotSupported)]
		[TestCase(WiFiAuthentication.WPA_Enterprise, WiFiEncryptionType.TKIP, "WpaTkipSecuredWiFi", WiFiBssType.Infrastructure, typeof(NotImplementedException), AccessPointTypeNotSupported)]
		[TestCase(WiFiAuthentication.Shared, WiFiEncryptionType.WEP, "SharedWepSecuredWiFi", WiFiBssType.Infrastructure, typeof(NotImplementedException), AccessPointTypeNotSupported)]
		/// <summary>
		/// Validates the access point types that are not supported.  
		/// </summary>
		/// <param name="authentication">Authentication type of access point.</param>
		/// <param name="encryption">Encryption type of access point.</param>
		/// <param name="ssid">Ssid of access point.</param>
		/// <param name="bssType">BssType of access point, only infrastructure is supported by Microsoft.</param>
		/// <param name="exceptionType">Type of exception thrown when creating (not supported) connection profile.</param>
		/// <param name="expectedErrorMsg">Expected error message when creating (not supported) connection profile.</param>
		public void CreateProfileExceptions(WiFiAuthentication authentication,
			WiFiEncryptionType encryption,
			string ssid,
			WiFiBssType bssType,
			Type exceptionType,
			string expectedErrorMsg)
		{
			AccessPoint accessPoint = new AccessPoint()
			{
				Ssid = ssid,
				Authentication = authentication,
				Encryption = encryption,
				BssType = bssType
			};

			/* Create profile with parameters that result in an exception. */
			Exception ex = Assert.Throws(exceptionType, () => _profileService.CreateProfileXml(accessPoint, null),
				"CreateProfileXml does not throw an exception as expected.");
			/* Check error message */
			Assert.AreEqual(expectedErrorMsg, ex.Message);
		}

		#endregion

		#region [ Profile parser validation process ]

		[Test, TestCaseSource("ProfileTestCases")]
		/// <summary>
		/// Validates the profile Parser method.  
		/// </summary>
		/// <param name="index">Index of testcase see ProfileTestCases.</param>
		/// <param name="profileXml">Example profile used to call Parser method.</param>
		/// <param name="authentication">Authentication type of access point.</param>
		/// <param name="encryption">Encryption type of access point.</param>
		/// <param name="ssid">Ssid of access point.</param>
		/// <param name="bssType">BssType of access point.</param>
		/// <param name="IsAutoConnectEnabled">Is profile auto connectable.</param>
		/// <param name="IsAutoSwitchEnabled">Is auto switch enabled.</param>
		/// <param name="keyType">KeyType of access point.</param>
		/// <param name="keyIsEncrypted">Key/password specified in profile is encrypted or not.</param>
		/// <param name="password">Password needed to connect to access point, null if open access point.</param>
		public void ValidateProfileParser(int index, string profileXml, 
			WiFiAuthentication authentication, WiFiEncryptionType encryption, 
			string ssid, WiFiBssType bssType, bool IsAutoConnectEnabled, 
			bool IsAutoSwitchEnabled, WiFiKeyType keyType, bool keyIsEncrypted, string password)
		{
			/* Attributes of generated profile are returned in profile object. */
			Profile profile = new Profile();
			bool result = _profileService.Parse(profileXml, ref profile);
			Assert.IsTrue(result, $"Testcase[{index}]: failed to parse example profile.");
			/* Validate attributes of generated profile. */
			Assert.AreEqual(authentication, profile.Authentication, $"Testcase[{index}]: failed to validate authentication.");
			Assert.AreEqual(encryption, profile.Encryption, $"Testcase[{index}]: failed to validate encryption.");
			Assert.AreEqual(ssid, profile.Ssid, $"Testcase[{index}]: failed to validate ssid.");
			Assert.AreEqual(ssid, profile.ProfileName, $"Testcase[{index}]: failed to validate profile name.");
			Assert.AreEqual(bssType, profile.BssType, $"Testcase[{index}]: failed to validate profile BSS type.");
			Assert.AreEqual(IsAutoConnectEnabled, profile.IsAutoConnectEnabled, $"Testcase[{index}]: failed to validate profile automatic connection property.");
			Assert.AreEqual(IsAutoSwitchEnabled, profile.IsAutoSwitchEnabled, $"Testcase[{index}]: failed to validate profile automatic switch property.");
			Assert.AreEqual(keyType, profile.KeyType, $"Testcase[{index}]: failed to validate wireless profile KeyType.");
			Assert.AreEqual(keyIsEncrypted, profile.KeyIsEncrypted, $"Testcase[{index}]: failed to validate profile property KeyIsEncrypted.");
			Assert.AreEqual(password, profile.KeyValue, $"Testcase[{index}]: wireless profile password is not equal to expected password.");
		}

		/* Profiles used to validate the profile Parser method. */
		private static object[] ProfileTestCases =
		{
			new object[] {0, Profile001, WiFiAuthentication.Open, WiFiEncryptionType.None,
				"OpenWifi", WiFiBssType.Infrastructure, true, true, WiFiKeyType.None, false, null },
			new object[] {1, Profile002, WiFiAuthentication.Open, WiFiEncryptionType.WEP,
				"OpenWepSecuredWiFi", WiFiBssType.Infrastructure, true, false, WiFiKeyType.NetworkKey, false, "5061737377" },
			new object[] {3, Profile003, WiFiAuthentication.WPA2_Enterprise, WiFiEncryptionType.AES,
				"Wpa2AesSecuredWiFi", WiFiBssType.Infrastructure, false, false, WiFiKeyType.None, false, null },
			new object[] {4, Profile004, WiFiAuthentication.WPA2_Personal, WiFiEncryptionType.AES,
				"Wpa2PskAesSecuredWiFi", WiFiBssType.Infrastructure, true, true, WiFiKeyType.PassPhrase, false, "Password123" },
			new object[] {5, Profile005, WiFiAuthentication.WPA_Enterprise, WiFiEncryptionType.TKIP,
				"WpaTkipSecuredWiFi", WiFiBssType.Infrastructure, true, true, WiFiKeyType.None, false, null },
			new object[] {6, Profile006, WiFiAuthentication.WPA_Personal, WiFiEncryptionType.TKIP,
				"WpaPskTkipSecuredWiFi", WiFiBssType.Infrastructure, true, false, WiFiKeyType.PassPhrase, false, "Password123" },
			new object[] {7, Profile007, WiFiAuthentication.WPA2_Personal, WiFiEncryptionType.TKIP,
				"Wpa2PskTkipSecuredWiFi", WiFiBssType.Infrastructure, false, false, WiFiKeyType.PassPhrase, false, "Password123" },
			new object[] {8, Profile008, WiFiAuthentication.WPA_Personal, WiFiEncryptionType.AES,
				"WpaPskAesSecuredWiFi", WiFiBssType.Infrastructure, false, false, WiFiKeyType.PassPhrase, false, "Password123" },
			new object[] {8, Profile009, WiFiAuthentication.Open, WiFiEncryptionType.WEP,
				"OpenWepSecuredWiFi", WiFiBssType.Independent, false, false, WiFiKeyType.NetworkKey, true, "...11228B3" }
		};

		#endregion

		#region [ Example profiles ]

		private static string Profile001 =>
$@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
  <name>OpenWifi</name>
  <SSIDConfig>
    <SSID>
      <hex>4F70656E57696669</hex>
      <name>OpenWifi</name>
    </SSID>
    <nonBroadcast>false</nonBroadcast>
  </SSIDConfig>
  <connectionType>ESS</connectionType>
  <connectionMode>auto</connectionMode>
  <autoSwitch>true</autoSwitch>
  <MSM>
    <security>
      <authEncryption>
        <authentication>open</authentication>
        <encryption>none</encryption>
        <useOneX>false</useOneX>
      </authEncryption>
    </security>
  </MSM>
</WLANProfile>";

		private static string Profile002 =>
$@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
  <name>OpenWepSecuredWiFi</name>
  <SSIDConfig>
    <SSID>
      <hex>4f70656e5765705365637572656457694669</hex>
      <name>OpenWepSecuredWiFi</name>
    </SSID>
    <nonBroadcast>true</nonBroadcast>
  </SSIDConfig>
  <connectionType>ESS</connectionType>
  <connectionMode>auto</connectionMode>
  <autoSwitch>false</autoSwitch>
  <MSM>
    <security>
      <authEncryption>
        <authentication>open</authentication>
        <encryption>WEP</encryption>
        <useOneX>false</useOneX>
      </authEncryption>
      <sharedKey>
        <keyType>networkKey</keyType>
        <protected>false</protected>
        <keyMaterial>5061737377</keyMaterial>
      </sharedKey>
      <keyIndex>0</keyIndex>
    </security>
  </MSM>
</WLANProfile>";

		private static string Profile003 =>
$@"<?xml version = ""1.0""?>
<WLANProfile xmlns = ""http://www.microsoft.com/networking/WLAN/profile/v1"">
  <name>Wpa2AesSecuredWiFi</name>
  <SSIDConfig>
    <SSID>
      <hex>577061324165735365637572656457694669</hex>
      <name>Wpa2AesSecuredWiFi</name>
    </SSID>
    <nonBroadcast>true</nonBroadcast>
  </SSIDConfig>
  <connectionType>ESS</connectionType>
  <connectionMode>manual</connectionMode>
  <autoSwitch>false</autoSwitch>
  <MSM>
    <security>
      <authEncryption>
        <authentication>WPA2</authentication>
        <encryption>AES</encryption>
        <useOneX>true</useOneX>
        <FIPSMode xmlns = ""http://www.microsoft.com/networking/WLAN/profile/v2"">false</FIPSMode>
      </authEncryption>
      <PMKCacheMode>enabled</PMKCacheMode>
      <PMKCacheTTL>720</PMKCacheTTL>
      <PMKCacheSize>128</PMKCacheSize>
      <preAuthMode>disabled</preAuthMode>
      <OneX xmlns = ""http://www.microsoft.com/networking/OneX/v1"">
        <cacheUserData>true</cacheUserData>
        <EAPConfig>
          <EapHostConfig xmlns = ""http://www.microsoft.com/provisioning/EapHostConfig"">
            <EapMethod>
              <Type xmlns = ""http://www.microsoft.com/provisioning/EapCommon"">25</Type>
              <VendorId xmlns = ""http://www.microsoft.com/provisioning/EapCommon"">0</VendorId>
              <VendorType xmlns = ""http://www.microsoft.com/provisioning/EapCommon"">0</VendorType>
              <AuthorId xmlns = ""http://www.microsoft.com/provisioning/EapCommon"">0</AuthorId>
            </EapMethod>
            <Config xmlns = ""http://www.microsoft.com/provisioning/EapHostConfig"">
              <Eap xmlns = ""http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1"">
                <Type>25</Type>
                <EapType xmlns = ""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1"">
                  <ServerValidation>
                    <DisableUserPromptForServerValidation>false</DisableUserPromptForServerValidation>
                    <ServerNames></ServerNames>
                  </ServerValidation>
                  <FastReconnect>true</FastReconnect>
                  <InnerEapOptional>false</InnerEapOptional>
                  <Eap xmlns = ""http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1"">
                    <Type>26</Type>
                    <EapType xmlns = ""http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1"">
                      <UseWinLogonCredentials>false</UseWinLogonCredentials>
                    </EapType>
                  </Eap>
                  <EnableQuarantineChecks>false</EnableQuarantineChecks>
                  <RequireCryptoBinding>false</RequireCryptoBinding>
                  <PeapExtensions>
                    <PerformServerValidation xmlns = ""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">true</PerformServerValidation>
                    <AcceptServerName xmlns = ""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">false</AcceptServerName>
                    <PeapExtensionsV2 xmlns = ""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">
                      <AllowPromptingWhenServerCANotFound xmlns = ""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV3"">true</AllowPromptingWhenServerCANotFound>
                    </PeapExtensionsV2>
                  </PeapExtensions>
                </EapType>
              </Eap>
            </Config>
          </EapHostConfig>
        </EAPConfig>
      </OneX>
    </security>
  </MSM>
</WLANProfile>";

		private static string Profile004 =>
$@"<?xml version = ""1.0""?>
<WLANProfile xmlns = ""http://www.microsoft.com/networking/WLAN/profile/v1"">
  <name>Wpa2PskAesSecuredWiFi</name>
  <SSIDConfig>
    <SSID>
      <hex>5770613250736b4165735365637572656457694669</hex>
      <name>Wpa2PskAesSecuredWiFi</name>
    </SSID>
    <nonBroadcast>true</nonBroadcast>
  </SSIDConfig>
  <connectionType>ESS</connectionType>
  <connectionMode>auto</connectionMode>
  <autoSwitch>true</autoSwitch>
  <MSM>
    <security>
      <authEncryption>
        <authentication>WPA2PSK</authentication>
        <encryption>AES</encryption>
        <useOneX>false</useOneX>
        <FIPSMode xmlns = ""http://www.microsoft.com/networking/WLAN/profile/v2"">false</FIPSMode>
      </authEncryption>
      <sharedKey>
        <keyType>passPhrase</keyType>
        <protected>false</protected>
        <keyMaterial>Password123</keyMaterial>
      </sharedKey>
    </security>
  </MSM>
  <MacRandomization xmlns=""http://www.microsoft.com/networking/WLAN/profile/v3"">
     <enableRandomization>false</enableRandomization>
  </MacRandomization>
</WLANProfile>";

		private static string Profile005 =>
$@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>WpaTkipSecuredWiFi</name>
    <SSIDConfig>
        <SSID>
            <hex>577061546b69705365637572656457694669</hex>
            <name>WpaTkipSecuredWiFi</name>
        </SSID>
        <nonBroadcast>false</nonBroadcast>
    </SSIDConfig>
    <connectionType>ESS</connectionType>
    <connectionMode>auto</connectionMode>
    <autoSwitch>true</autoSwitch>
    <MSM>
        <security>
            <authEncryption>
                <authentication>WPA</authentication>
                <encryption>TKIP</encryption>
                <useOneX>true</useOneX>
            </authEncryption>
            <OneX xmlns=""http://www.microsoft.com/networking/OneX/v1"">
                <EAPConfig>
                    <EapHostConfig xmlns=""http://www.microsoft.com/provisioning/EapHostConfig"">
                        <EapMethod>
                            <Type xmlns=""http://www.microsoft.com/provisioning/EapCommon"">25</Type>
                            <VendorId xmlns=""http://www.microsoft.com/provisioning/EapCommon"">0</VendorId>
                            <VendorType xmlns=""http://www.microsoft.com/provisioning/EapCommon"">0</VendorType>
                            <AuthorId xmlns=""http://www.microsoft.com/provisioning/EapCommon"">0</AuthorId>
                        </EapMethod>
                        <Config xmlns=""http://www.microsoft.com/provisioning/EapHostConfig"">
                            <Eap xmlns=""http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1"">
                                <Type>25</Type>
                                <EapType xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1"">
                                    <ServerValidation>
                                        <DisableUserPromptForServerValidation>false</DisableUserPromptForServerValidation>
                                        <ServerNames></ServerNames>
                                    </ServerValidation>
                                    <FastReconnect>true</FastReconnect>
                                    <InnerEapOptional>false</InnerEapOptional>
                                    <Eap xmlns=""http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1"">
                                        <Type>26</Type>
                                        <EapType xmlns=""http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1"">
                                            <UseWinLogonCredentials>false</UseWinLogonCredentials>
                                        </EapType>
                                    </Eap>
                                    <EnableQuarantineChecks>false</EnableQuarantineChecks>
                                    <RequireCryptoBinding>false</RequireCryptoBinding>
                                    <PeapExtensions>
                                        <PerformServerValidation xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">false</PerformServerValidation>
                                        <AcceptServerName xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">false</AcceptServerName>
                                    </PeapExtensions>
                                </EapType>
                            </Eap>
                        </Config>
                    </EapHostConfig>
                </EAPConfig>
            </OneX>
        </security>
    </MSM>
</WLANProfile>";

		private static string Profile006 =>
$@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>WpaPskTkipSecuredWiFi</name>
    <SSIDConfig>
        <SSID>
			<hex>57706150736b546b69705365637572656457694669</hex>
            <name>WpaPskTkipSecuredWiFi</name>
        </SSID>
        <nonBroadcast>true</nonBroadcast>
    </SSIDConfig>
    <connectionType>ESS</connectionType>
    <connectionMode>auto</connectionMode>
    <autoSwitch>false</autoSwitch>
    <MSM>
        <security>
            <authEncryption>
                <authentication>WPAPSK</authentication>
                <encryption>TKIP</encryption>
                <useOneX>false</useOneX>
            </authEncryption>
            <sharedKey>
                <keyType>passPhrase</keyType>
                <protected>false</protected>
                <keyMaterial>Password123</keyMaterial>
            </sharedKey>
        </security>
    </MSM>
</WLANProfile>";

		private static string Profile007 =>
$@"<?xml version = ""1.0""?>
<WLANProfile xmlns = ""http://www.microsoft.com/networking/WLAN/profile/v1"">
  <name>Wpa2PskTkipSecuredWiFi</name>
  <SSIDConfig>
    <SSID>
      <hex>5770613250736b546b69705365637572656457694669</hex>
      <name>Wpa2PskTkipSecuredWiFi</name>
    </SSID>
    <nonBroadcast>true</nonBroadcast>
  </SSIDConfig>
  <connectionType>ESS</connectionType>
  <connectionMode>manual</connectionMode>
  <autoSwitch>false</autoSwitch>
  <MSM>
    <security>
      <authEncryption>
        <authentication>WPA2PSK</authentication>
        <encryption>TKIP</encryption>
        <useOneX>false</useOneX>
        <FIPSMode xmlns = ""http://www.microsoft.com/networking/WLAN/profile/v2"">false</FIPSMode>
      </authEncryption>
      <sharedKey>
        <keyType>passPhrase</keyType>
        <protected>false</protected>
        <keyMaterial>Password123</keyMaterial>
      </sharedKey>
    </security>
  </MSM>
  <MacRandomization xmlns=""http://www.microsoft.com/networking/WLAN/profile/v3"">
     <enableRandomization>false</enableRandomization>
  </MacRandomization>
</WLANProfile>";

		private static string Profile008 =>
$@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>WpaPskAesSecuredWiFi</name>
    <SSIDConfig>
        <SSID>
			<hex>57706150736b4165735365637572656457694669</hex>
            <name>WpaPskAesSecuredWiFi</name>
        </SSID>
        <nonBroadcast>true</nonBroadcast>
    </SSIDConfig>
    <connectionType>ESS</connectionType>
    <connectionMode>manual</connectionMode>
    <autoSwitch>false</autoSwitch>
    <MSM>
        <security>
            <authEncryption>
                <authentication>WPAPSK</authentication>
                <encryption>AES</encryption>
                <useOneX>false</useOneX>
            </authEncryption>
            <sharedKey>
                <keyType>passPhrase</keyType>
                <protected>false</protected>
                <keyMaterial>Password123</keyMaterial>
            </sharedKey>
        </security>
    </MSM>
</WLANProfile>";

		private static string Profile009 =>
$@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>OpenWepSecuredWiFi</name>
    <SSIDConfig>
        <SSID>
			<hex>4f70656e5765705365637572656457694669</hex>
            <name>OpenWepSecuredWiFi</name>
        </SSID>
        <nonBroadcast>false</nonBroadcast>
    </SSIDConfig>
    <connectionType>IBSS</connectionType>
    <connectionMode>manual</connectionMode>
    <autoSwitch>false</autoSwitch>
    <MSM>
        <security>
            <authEncryption>
                <authentication>open</authentication>
                <encryption>WEP</encryption>
                <useOneX>false</useOneX>
            </authEncryption>
            <sharedKey>
                <keyType>networkKey</keyType>
                <protected>true</protected>
                <keyMaterial>...11228B3</keyMaterial>
            </sharedKey>
        </security>
    </MSM>
</WLANProfile>";

		#endregion

	}
}