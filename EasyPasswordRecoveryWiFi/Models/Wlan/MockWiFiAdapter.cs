using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Models.Wlan
{
	public class MockWiFiAdapter : IWiFiService
	{
		#region [ Lists ]

		private List<AccessPoint> _sourcWiFiAccessPoints;
		private List<Interface> _sourceWiFiInterfaces;
		private List<Profile> _sourceWiFiProfiles;

		#endregion

		#region[ Injected instances ]

		private readonly IProfileService _profileService = null;

		#endregion

		#region [ Constructor ]

		public MockWiFiAdapter(IProfileService profileService)
		{
			_profileService = profileService;
			_sourcWiFiAccessPoints = PopulateWifiAccesspoints().ToList();
			_sourceWiFiInterfaces = PopulateWifiInterfaces().ToList();
			_sourceWiFiProfiles = PopulateWifiProfiles().ToList();
		}

		#endregion

		#region [ Dispose ]

		private bool _disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
		}

		#endregion

		#region [ Connect/Disconnect ]

		private const string defaultPassword = "Welcome123";

		/// <summary>
		/// Mocks an attempt to connect to the specified WiFi access point using a generated xml profile.
		/// </summary>
		/// <param name="accessPoint">WiFi access point to connect to.</param>
		/// <param name="password">Password required for authorization.</param>
		/// <param name="timeout">Timeout duration.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>True if successfully connected, false if failed or timed out.</returns>
		public Task<bool> ConnectNetworkAsync(AccessPoint accessPoint, string password,
			TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (accessPoint == null)
				throw new ArgumentNullException(nameof(accessPoint));

			if (accessPoint.IsPasswordRequired && string.IsNullOrEmpty(password))
				throw new ArgumentNullException(nameof(password));

			if (timeout.TotalSeconds < 0)
				throw new ArgumentOutOfRangeException(nameof(timeout));

			return Task.Run(async () =>
			{
				bool bConnected = false;

				/* Disconnect from WiFi interface. */
				DisconnectNetwork(accessPoint.Id);
				/* Generate XML profile for Access Point. */
				string profile = _profileService.CreateProfileXml(accessPoint, password);
				Interface wiFiInterface = _sourceWiFiInterfaces.Where(x => x.Id == accessPoint.Id).FirstOrDefault();
				await SetProfileAsync(wiFiInterface, profile);
				/* Update connection status, if correct password is provided. */
				if (!accessPoint.IsPasswordRequired || password == defaultPassword)
				{
					/* Find access point with given ssid and profile name. */
					AccessPoint wiFiAccessPoint = _sourcWiFiAccessPoints.Where(x => x.Id == accessPoint.Id &&
					x.ProfileName == accessPoint.Ssid &&
					x.Ssid == accessPoint.Ssid).FirstOrDefault();
					if (wiFiAccessPoint == default(AccessPoint))
					{
						wiFiAccessPoint = _sourcWiFiAccessPoints.Where(x => x.Id == accessPoint.Id &&
						x.Ssid == accessPoint.Ssid).FirstOrDefault(); ;
					}

					Profile wiFiProfile = _sourceWiFiProfiles.Where(x => x.Id == accessPoint.Id &&
					x.ProfileName == accessPoint.Ssid).FirstOrDefault();

					wiFiProfile.IsConnected = true;
					wiFiAccessPoint.IsConnected = true;
					wiFiAccessPoint.ProfileName = accessPoint.Ssid;
					wiFiInterface.IsConnected = true;
					wiFiInterface.ProfileName = accessPoint.Ssid;
					wiFiInterface.InterfaceState = WiFiInterfaceState.Connected;
					bConnected = true;
				}

				return bConnected;
			});
		}

		/// <summary>
		/// Mocks an attempt to disconnects from the WiFi access point associated with the 
		/// specified wireless interface.
		/// </summary>
		/// <param name="wiFiInterface">WiFi Interface to disconnect from.</param>
		/// <param name="timeout">Timeout duration.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>True if successfully disconnected, false if failed or timed out.</returns>
		public Task<bool> DisconnectNetworkAsync(Interface wiFiInterface, TimeSpan timeout,
			CancellationToken cancellationToken)
		{
			if (wiFiInterface == null)
				throw new ArgumentNullException(nameof(wiFiInterface));

			if (timeout.TotalSeconds < 0)
				throw new ArgumentOutOfRangeException(nameof(timeout));

			return Task.Run(() =>
			{
				bool bRet = DisconnectNetwork(wiFiInterface.Id);
				return Task.FromResult(bRet);
			});
		}

		private bool DisconnectNetwork(Guid interfaceId)
		{
			/* Update connection status of Access Point. */
			_sourcWiFiAccessPoints.Where(x => x.Id == interfaceId)
				.ToList().ForEach(x =>
				{
					x.IsConnected = false;
				});

			/* Update connection status of Profile. */
			_sourceWiFiProfiles.Where(x => x.Id == interfaceId)
				.ToList().ForEach(x =>
				{
					x.IsConnected = false;
				});

			/* Update connection status of Interface. */
			Interface wifiInterface = _sourceWiFiInterfaces.Where(x => x.Id ==
			interfaceId).FirstOrDefault();
			if (wifiInterface != default(Interface))
			{
				wifiInterface.IsConnected = false;
				wifiInterface.InterfaceState = WiFiInterfaceState.Disconnected;
				wifiInterface.ProfileName = null;
			}

			return true;
		}

		#endregion

		#region [ Network methods ] 

		/// <summary>
		/// Asynchronously requests wireless interfaces to scan for available WiFi access points.
		/// </summary>
		/// <param name="timeout">Timeout duration.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>A list of interface IDs that were successfully scanned.</returns>
		public Task<IEnumerable<Guid>> ScanNetworkAsync(TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (timeout.TotalSeconds < 0)
				throw new ArgumentNullException(nameof(timeout));

			return Task.FromResult(new List<Guid>(_sourceWiFiInterfaces.Select(x => x.Id)).AsEnumerable());
		}

		/// <summary>
		/// Retrieves available WiFi access points.
		/// </summary>
		/// <returns>A list of available WiFi access points.</returns>
		/// <remarks>
		/// If multiple profiles are associated with the same access point, there will be multiple entries with
		/// the same ssid.
		/// </remarks>
		public Task<IEnumerable<AccessPoint>> GetWiFiAccessPointsAsync()
		{
			return Task.FromResult(new List<AccessPoint>(_sourcWiFiAccessPoints).AsEnumerable());
		}

		/// <summary>
		/// Retrieves WiFi profiles from all WiFi interfaces.
		/// </summary>
		/// <returns>A list of WiFi profiles.</returns>
		public Task<IEnumerable<Profile>> GetWiFiProfilesAsync()
		{
			return Task.FromResult(new List<Profile>(_sourceWiFiProfiles).AsEnumerable());
		}

		/// <summary>
		/// Retrieves WiFi interfaces and related connection information.
		/// </summary>
		/// <returns>A list of WiFi interfaces and related connection information.</returns>
		public Task<IEnumerable<Interface>> GetWiFiInterfacesAsync()
		{
			return Task.FromResult(new List<Interface>(_sourceWiFiInterfaces).AsEnumerable());
		}

		#endregion

		#region [ Profile methods ]

		/// <summary>
		/// Sets the position of the specified WiFi profile to the specified position.
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <param name="position">The specified position.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		public Task<bool> SetProfilePositionAsync(Profile wiFiProfile, int position)
		{
			if (wiFiProfile == null)
				throw new ArgumentNullException(nameof(wiFiProfile));

			if (position < 0)
				throw new ArgumentOutOfRangeException(nameof(position));

			return Task.Run(() =>
			{
				bool bSuccess = false;

				/* Insert the WiFiProfile on the specified position in the list.*/
				int index1 = _sourceWiFiProfiles.FindIndex(x => x.Id == wiFiProfile.Id &&
				x.Position == position);

				int index2 = _sourceWiFiProfiles.FindIndex(x => x.Id == wiFiProfile.Id &&
				x.ProfileName == wiFiProfile.ProfileName);

				if (index1 >= 0 && index2 >= 0 && index1 != index2)
				{
					int insertIndex = index2 > index1 ? index1 : index1 + 1;
					_sourceWiFiProfiles.Insert(insertIndex, _sourceWiFiProfiles[index2]);
					int removeIndex = index2 > index1 ? index2 + 1 : index2;
					_sourceWiFiProfiles.RemoveAt(removeIndex);

					int pos = 0;
					/* Make profile positions consecutively for the interface. */
					_sourceWiFiProfiles.Where(x => x.Id == wiFiProfile.Id)
					.ToList().ForEach(x =>
					{
						x.Position = pos++;
					});

					bSuccess = true;
				}

				return Task.FromResult(bSuccess);
			});
		}

		/// <summary>
		/// Renames the specified wireless profile.
		/// </summary>
		/// <param name="wiFiProfile">Profile to be renamed.</param>
		/// <param name="newProfileName">New profile name.</param>
		/// <returns>True if successfully renamed, false otherwise.</returns>
		public Task<bool> RenameProfileAsync(Profile wiFiProfile, string newProfileName)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets (adds or overwrites) the content of a specified WiFi profile.
		/// </summary>
		/// <param name="wiFiInterface">WiFi Interface to add WiFi profile to.</param>
		/// <param name="profileXml">Profile XML.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		public Task<bool> SetProfileAsync(Interface wiFiInterface, string profileXml)
		{
			if (wiFiInterface == null)
				throw new ArgumentNullException(nameof(wiFiInterface));

			if (string.IsNullOrEmpty(profileXml))
				throw new ArgumentNullException(nameof(profileXml));

			return Task.Run(() =>
			{
				bool bSuccess = false;

				/* Retrieve WiFi data from XML file. */
				Profile profile = new Profile();
				/* Parse profile XML file and check validity. */
				if (_profileService.Parse(profileXml, ref profile))
				{
					/* Set interface specific properties. */
					profile.Id = wiFiInterface.Id;
					profile.ProfileType = WiFiProfileType.AllUser;
					profile.IsConnected = false;
					profile.Position = 0;

					/* Check if profile name already exists. */
					int index = _sourceWiFiProfiles.FindIndex(x => x.Id == wiFiInterface.Id &&
					x.ProfileName == profile.ProfileName);

					/* Profile name exists and is not connected, replace it with new one. */
					if (index >= 0 && !_sourceWiFiProfiles[index].IsConnected)
					{
						profile.Position = _sourceWiFiProfiles[index].Position;
						_sourceWiFiProfiles[index] = profile;
						bSuccess = true;
					}
					/* Profile name does not exists. */
					else if (index < 0)
					{
						index = _sourceWiFiProfiles.FindIndex(x => x.Id == wiFiInterface.Id && x.Position == 0);
						/* Interface has at least one profile. */
						if (index >= 0)
						{
							_sourceWiFiProfiles.Insert(index, profile);
							int pos = 0;
							/* Make profile positions consecutively for the interface. */
							_sourceWiFiProfiles.Where(x => x.Id == profile.Id)
							.ToList().ForEach(x =>
							{
								x.Position = pos++;
							});

							bSuccess = true;
						}
						else
						{
							_sourceWiFiProfiles.Add(profile);
							bSuccess = true;
						}
					}
				}
				return bSuccess;
			});
		}

		/// <summary>
		/// Deletes a specified WiFi profile from its corresponding WiFi interface.
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <returns>True if successfully deleted, false otherwise.</returns>
		public Task<bool> DeleteProfileAsync(Profile wiFiProfile)
		{
			if (wiFiProfile == null)
				throw new ArgumentNullException(nameof(wiFiProfile));

			return Task.Run(() =>
			{
				bool bSuccess = false;

				Profile profile = _sourceWiFiProfiles.Where(x => x.Id == wiFiProfile.Id &&
				x.ProfileName == wiFiProfile.ProfileName).FirstOrDefault();

				if (profile != default(Profile))
				{
					_sourceWiFiProfiles.Remove(profile);

					int pos = 0;
					/* Make profile positions consecutively for the interface. */
					_sourceWiFiProfiles.Where(x => x.Id == wiFiProfile.Id)
					.ToList().ForEach(x =>
					{
						x.Position = pos++;
					});

					bSuccess = true;
				}

				return Task.FromResult(bSuccess);
			});
		}

		#endregion

		#region [ Interface radio ]

		/// <summary>
		/// Turns on/off the radio of a specified wireless interface (software radio state only).
		/// </summary>
		/// <param name="wiFiInterface">WiFi interface.</param>
		/// <returns>True if successfully changed radio state. False if failed.</returns>
		/// <exception cref="UnauthorizedAccessException">
		/// If the user is not logged on as a member of Administrators group.
		/// </exception>
		public Task<bool> TurnOnOffInterfaceRadio(Interface wiFiInterface, bool turnRadioOn)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region [ Private populate methods ] 

		private Interface[] PopulateWifiInterfaces()
		{
			return new[]
			{
				new Interface(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					description: "Intel(R) Centrino(R) Advanced-N 6205",
					profileName: "Webgate",
					isRadioOn: true,
					isConnected: true,
					connectionMode: WiFiConnectionMode.Profile,
					interfaceState: WiFiInterfaceState.Connected),
				new Interface(
					id: new Guid("84af3ffe-44d0-44b5-06e9-6a77e21adeda"),
					description: "WLI-UC-GNM",
					profileName: "H368N9D1BBC",
					isRadioOn: true,
					isConnected: true,
					connectionMode: WiFiConnectionMode.Profile,
					interfaceState: WiFiInterfaceState.Connected),
				new Interface(
					id: new Guid("9D2B0228-4D0D-4C23-8B49-01A698857709"),
					description: "Marvel AVASTAR Wireless-AC Network Controller",
					profileName: null,
					isRadioOn: true,
					isConnected: false,
					connectionMode: WiFiConnectionMode.Profile,
					interfaceState: WiFiInterfaceState.Disconnected),
				new Interface(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					description: "Qualcomm Atheros QCA9377 Wireless Network Adapter",
					profileName: null,
					isRadioOn: false,
					isConnected: false,
					connectionMode: WiFiConnectionMode.Profile,
					interfaceState: WiFiInterfaceState.Disconnected),
				new Interface(
					id: new Guid("f58b4e99-a1ab-40b5-d43a-e7537e283dab"),
					description: "TP-Link TL-POE150S",
					profileName: "fontysWPA",
					isRadioOn: true,
					isConnected: true,
					connectionMode: WiFiConnectionMode.Profile,
					interfaceState: WiFiInterfaceState.Connected)
			};
		}

		private Profile[] PopulateWifiProfiles()
		{
			return new[]
			{
				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: true,
					profileName: "Webgate",
					ssid: "Webgate",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "Welcome123",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>Webgate</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>57656267617465</hex>
                              <name>Webgate</name>
                            </SSID>
                          </SSIDConfig>
                          <connectionType>ESS</connectionType>
                          <connectionMode>manual</connectionMode>
                          <autoSwitch>false</autoSwitch>
                          <MSM>
                            <security>
                              <authEncryption>
                                <authentication>WPA2PSK</authentication>
                                <encryption>AES</encryption>
                                <useOneX>false</useOneX>
                              </authEncryption>
                              <sharedKey>
                                <keyType>passPhrase</keyType>
                                <protected>false</protected>
                                <keyMaterial>Saida0407</keyMaterial>
                              </sharedKey>
                            </security>
                          </MSM>
                        </WLANProfile>",
					position: 0),

				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: false,
					profileName: "KPN Fon",
					ssid: "KPN Fon",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "testtest123",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>KPN Fon</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>4b504e20466f6e</hex>
                              <name>KPN Fon</name>
                            </SSID>
                          </SSIDConfig>
                          <connectionType>ESS</connectionType>
                          <connectionMode>manual</connectionMode>
                          <autoSwitch>false</autoSwitch>
                          <MSM>
                            <security>
                              <authEncryption>
                                <authentication>WPA2PSK</authentication>
                                <encryption>AES</encryption>
                                <useOneX>false</useOneX>
                              </authEncryption>
                              <sharedKey>
                                <keyType>passPhrase</keyType>
                                <protected>false</protected>
                                <keyMaterial>testtest123</keyMaterial>
                              </sharedKey>
                            </security>
                          </MSM>
                        </WLANProfile>",
					position: 1),

				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: false,
					profileName: "VFNL-6F2368",
					ssid: "VFNL-6F2368",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA_Enterprise,
					encryption: WiFiEncryptionType.TKIP,
					keyType: WiFiKeyType.None,
					keyIsEncrypted: false,
					keyValue: null,
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>VFNL-6F2368</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>56464e4c2d364632333638</hex>
                              <name>VFNL-6F2368</name>
                            </SSID>
                          </SSIDConfig>
                          <connectionType>ESS</connectionType>
                          <connectionMode>manual</connectionMode>
                          <autoSwitch>false</autoSwitch>
                          <MSM>
                            <security>
                              <authEncryption>
                                <authentication>WPA</authentication>
                                <encryption>TKIP</encryption>
                                <useOneX>true</useOneX>
                              </authEncryption>
                              <OneX xmlns=""http://www.microsoft.com/networking/OneX/v1"">
                                <EAPConfig>
                                  <EapHostConfig xmlns=""http://www.microsoft.com/provisioning/EapHostConfig"" xmlns:eapCommon=""http://www.microsoft.com/provisioning/EapCommon"" xmlns:baseEap=""http://www.microsoft.com/provisioning/BaseEapMethodConfig"">
                                    <EapMethod>
                                      <eapCommon:Type>25</eapCommon:Type>
                                      <eapCommon:AuthorId>0</eapCommon:AuthorId>
                                    </EapMethod>
                                    <Config xmlns:baseEap=""http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1"" xmlns:msPeap=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1"" xmlns:msChapV2=""http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1"">
                                      <baseEap:Eap>
                                        <baseEap:Type>25</baseEap:Type>
                                        <msPeap:EapType>
                                          <msPeap:ServerValidation>
                                            <msPeap:DisableUserPromptForServerValidation>false</msPeap:DisableUserPromptForServerValidation>
                                            <msPeap:TrustedRootCA />
                                          </msPeap:ServerValidation>
                                          <msPeap:FastReconnect>true</msPeap:FastReconnect>
                                          <msPeap:InnerEapOptional>0</msPeap:InnerEapOptional>
                                          <baseEap:Eap>
                                            <baseEap:Type>26</baseEap:Type>
                                            <msChapV2:EapType>
                                              <msChapV2:UseWinLogonCredentials>false</msChapV2:UseWinLogonCredentials>
                                            </msChapV2:EapType>
                                          </baseEap:Eap>
                                          <msPeap:EnableQuarantineChecks>false</msPeap:EnableQuarantineChecks>
                                          <msPeap:RequireCryptoBinding>false</msPeap:RequireCryptoBinding>
                                          <msPeap:PeapExtensions />
                                        </msPeap:EapType>
                                      </baseEap:Eap>
                                    </Config>
                                  </EapHostConfig>
                                </EAPConfig>
                              </OneX>
                            </security>
                          </MSM>
                        </WLANProfile>",
					position: 2),

				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: false,
					profileName: "Sitecom4A711C",
					ssid: "Sitecom4A711C",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA2_Enterprise,
					encryption: WiFiEncryptionType.AES,
					keyType: WiFiKeyType.None,
					keyIsEncrypted: false,
					keyValue: null,
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>Sitecom4A711C</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>53697465636f6d344137313143</hex>
                              <name>Sitecom4A711C</name>
                            </SSID>
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
                              </authEncryption>
                              <OneX xmlns=""http://www.microsoft.com/networking/OneX/v1"">
                                <EAPConfig>
                                  <EapHostConfig xmlns=""http://www.microsoft.com/provisioning/EapHostConfig"" xmlns:eapCommon=""http://www.microsoft.com/provisioning/EapCommon"" xmlns:baseEap=""http://www.microsoft.com/provisioning/BaseEapMethodConfig"">
                                    <EapMethod>
                                      <eapCommon:Type>25</eapCommon:Type>
                                      <eapCommon:AuthorId>0</eapCommon:AuthorId>
                                    </EapMethod>
                                    <Config xmlns:baseEap=""http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1"" xmlns:msPeap=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1"" xmlns:msChapV2=""http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1"">
                                      <baseEap:Eap>
                                        <baseEap:Type>25</baseEap:Type>
                                        <msPeap:EapType>
                                          <msPeap:ServerValidation>
                                            <msPeap:DisableUserPromptForServerValidation>false</msPeap:DisableUserPromptForServerValidation>
                                            <msPeap:TrustedRootCA />
                                          </msPeap:ServerValidation>
                                          <msPeap:FastReconnect>true</msPeap:FastReconnect>
                                          <msPeap:InnerEapOptional>0</msPeap:InnerEapOptional>
                                          <baseEap:Eap>
                                            <baseEap:Type>26</baseEap:Type>
                                            <msChapV2:EapType>
                                              <msChapV2:UseWinLogonCredentials>false</msChapV2:UseWinLogonCredentials>
                                            </msChapV2:EapType>
                                          </baseEap:Eap>
                                          <msPeap:EnableQuarantineChecks>false</msPeap:EnableQuarantineChecks>
                                          <msPeap:RequireCryptoBinding>false</msPeap:RequireCryptoBinding>
                                          <msPeap:PeapExtensions />
                                        </msPeap:EapType>
                                      </baseEap:Eap>
                                    </Config>
                                  </EapHostConfig>
                                </EAPConfig>
                              </OneX>
                            </security>
                          </MSM>
                        </WLANProfile>",
						position: 3),

				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: false,
					profileName: "TMNL-6E34DB",
					ssid: "TMNL-6E34DB",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.TKIP,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "Welcome123",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>TMNL-6E34DB</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>544d4e4c2d364533344442</hex>
                              <name>TMNL-6E34DB</name>
                            </SSID>
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
                              </authEncryption>
                              <sharedKey>
                                <keyType>passPhrase</keyType>
                                <protected>false</protected>
                                <keyMaterial>Welcome123</keyMaterial>
                              </sharedKey>
                            </security>
                          </MSM>
                        </WLANProfile>",
						position: 4),

				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: false,
					profileName: "VGV7519531B41",
					ssid: "VGV7519531B41",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA_Personal,
					encryption: WiFiEncryptionType.TKIP,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "Welcome123",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>VGV7519531B41</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>56475637353139353331423431</hex>
                              <name>VGV7519531B41</name>
                            </SSID>
                          </SSIDConfig>
                          <connectionType>ESS</connectionType>
                          <connectionMode>manual</connectionMode>
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
                                <keyMaterial>Welcome123</keyMaterial>
                              </sharedKey>
                            </security>
                          </MSM>
                        </WLANProfile>",
						position: 5),

				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: false,
					profileName: "DefaultConnection",
					ssid: "OfficeNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA_Personal,
					encryption: WiFiEncryptionType.AES,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "Welcome123",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>DefaultConnection</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>4f66666963654e6574</hex>
                              <name>OfficeNet</name>
                            </SSID>
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
                                <keyMaterial>Welcome123</keyMaterial>
                              </sharedKey>
                            </security>
                          </MSM>
                        </WLANProfile>",
						position: 6),

				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: false,
					profileName: "UtrechtNet",
					ssid: "UtrechtNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "Welcome123",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>UtrechtNet</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>557472656368744e6574</hex>
                              <name>UtrechtNet</name>
                            </SSID>
                          </SSIDConfig>
                          <connectionType>ESS</connectionType>
                          <connectionMode>manual</connectionMode>
                          <autoSwitch>false</autoSwitch>
                          <MSM>
                            <security>
                              <authEncryption>
                                <authentication>WPA2PSK</authentication>
                                <encryption>AES</encryption>
                                <useOneX>false</useOneX>
                              </authEncryption>
                              <sharedKey>
                                <keyType>passPhrase</keyType>
                                <protected>false</protected>
                                <keyMaterial>Welcome123</keyMaterial>
                              </sharedKey>
                            </security>
                          </MSM>
                        </WLANProfile>",
						position: 7),

				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: false,
					profileName: "CityOpenNet",
					ssid: "CityOpenNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.Open,
					encryption: WiFiEncryptionType.WEP,
					keyType: WiFiKeyType.NetworkKey,
					keyIsEncrypted: false,
					keyValue: "696A6B6C6D",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>CityOpenNet</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>436974794f70656e4e6574</hex>
                              <name>CityOpenNet</name>
                            </SSID>
                          </SSIDConfig>
                          <connectionType>ESS</connectionType>
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
                                <protected>false</protected>
                                <keyMaterial>696A6B6C6D</keyMaterial>
                                </sharedKey>
                            </security>
                          </MSM>
                        </WLANProfile>",
						position: 8),

				new Profile(
					id: new Guid("AC761785-ED42-11CE-DACB-00BDD0057645"),
					isConnected: false,
					profileName: "SkyNet",
					ssid: "SkyNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.Open,
					encryption: WiFiEncryptionType.None,
					keyType: WiFiKeyType.None,
					keyIsEncrypted: false,
					keyValue: null,
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>SkyNet</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>536b794e6574</hex>
                              <name>SkyNet</name>
                            </SSID>
                          </SSIDConfig>
                          <connectionType>ESS</connectionType>
                          <connectionMode>manual</connectionMode>
                          <autoSwitch>false</autoSwitch>
                          <MSM>
                            <security>
                              <authEncryption>
                                <authentication>open</authentication>
                                <encryption>none</encryption>
                                <useOneX>false</useOneX>
                              </authEncryption>
                            </security>
                          </MSM>
                        </WLANProfile>",
						position: 9),

			new Profile(
					id: new Guid("84af3ffe-44d0-44b5-06e9-6a77e21adeda"),
					isConnected: true,
					profileName: "H368N9D1BBC",
					ssid: "H368N9D1BBC",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA_Personal,
					encryption: WiFiEncryptionType.AES,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "Welcome123",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>H368N9D1BBC</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>483336384e394431424243</hex>
                              <name>H368N9D1BBC</name>
                            </SSID>
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
                                <keyMaterial>Welcome123</keyMaterial>
                              </sharedKey>
                            </security>
                          </MSM>
                        </WLANProfile>",
						position: 0),

			new Profile(
					id: new Guid("84af3ffe-44d0-44b5-06e9-6a77e21adeda"),
					isConnected: false,
					profileName: "SecurityGateway",
					ssid: "SecurityXploded",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.TKIP,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "Welcome123",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                          <name>SecurityGateway</name>
                          <SSIDConfig>
                            <SSID>
                              <hex>536563757269747958706c6f646564</hex>
                              <name>SecurityXploded</name>
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
                              </authEncryption>
                              <sharedKey>
                                <keyType>passPhrase</keyType>
                                <protected>false</protected>
                                <keyMaterial>Welcome123</keyMaterial>
                              </sharedKey>
                            </security>
                          </MSM>
                        </WLANProfile>",
						position: 1),

			new Profile(
					id: new Guid("84af3ffe-44d0-44b5-06e9-6a77e21adeda"),
					isConnected: false,
					profileName: "EuroNet",
					ssid: "EuroNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.Open,
					encryption: WiFiEncryptionType.WEP,
					keyType: WiFiKeyType.None,
					keyIsEncrypted: false,
					keyValue: null,
					isAutoConnectEnabled: true,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                    <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                      <name>EuroNet</name>
                      <SSIDConfig>
                        <SSID>
                          <hex>4575726f4e6574</hex>
                          <name>EuroNet</name>
                        </SSID>
                        <nonBroadcast>false</nonBroadcast>
                      </SSIDConfig>
                      <connectionType>ESS</connectionType>
                      <connectionMode>auto</connectionMode>
                      <autoSwitch>false</autoSwitch>
                      <MSM>
                        <security>
                          <authEncryption>
                            <authentication>open</authentication>
                            <encryption>WEP</encryption>
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
                                        <PerformServerValidation xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">true</PerformServerValidation>
                                        <AcceptServerName xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">false</AcceptServerName>
                                        <PeapExtensionsV2 xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">
                                          <AllowPromptingWhenServerCANotFound xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV3"">true</AllowPromptingWhenServerCANotFound>
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
                    </WLANProfile>",
					position: 2),

			new Profile(
					id: new Guid("84af3ffe-44d0-44b5-06e9-6a77e21adeda"),
					isConnected: false,
					profileName: "ShopNet",
					ssid: "ShopNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA_Enterprise,
					encryption: WiFiEncryptionType.TKIP,
					keyType: WiFiKeyType.None,
					keyIsEncrypted: false,
					keyValue: null,
					isAutoConnectEnabled: true,
					isAutoSwitchEnabled: true,
					xml: $@" <?xml version=""1.0""?>
                    <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                      <name>ShopNet</name>
                      <SSIDConfig>
                        <SSID>
                          <hex>53686f704e6574</hex>
                          <name>ShopNet</name>
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
                    </WLANProfile>",
					position: 3),

			new Profile(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					isConnected: false,
					profileName: "AmazonNet",
					ssid: "AmazonNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "Welcome123",
					isAutoConnectEnabled: true,
					isAutoSwitchEnabled: true,
					xml: $@"<?xml version=""1.0"" encoding=""US-ASCII""?>
                    <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                        <name>AmazonNet</name>
                        <SSIDConfig>
                        <SSID>
                            <hex>416d617a6f6e4e6574</hex>
                            <name>AmazonNet</name>
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
                            <FIPSMode xmlns=""http://www.microsoft.com/networking/WLAN/profile/v2"">false</FIPSMode>
                            </authEncryption>
                            <sharedKey>
                            <keyType>passPhrase</keyType>
                            <protected>false</protected>
                            <keyMaterial>Welcome123</keyMaterial>
                            </sharedKey>
                        </security>
                        </MSM>
                    </WLANProfile>",
					position: 0),

			new Profile(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					isConnected: false,
					profileName: "YCollege",
					ssid: "YCollege",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA2_Enterprise,
					encryption: WiFiEncryptionType.AES,
					keyType: WiFiKeyType.None,
					keyIsEncrypted: false,
					keyValue: null,
					isAutoConnectEnabled: true,
					isAutoSwitchEnabled: true,
					xml: $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                    <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                      <name>YCollege</name>
                      <SSIDConfig>
                        <SSID>
                          <hex>59436f6c6c656765</hex>
                          <name>YCollege</name>
                        </SSID>
                        <nonBroadcast>true</nonBroadcast>
                      </SSIDConfig>
                      <connectionType>ESS</connectionType>
                      <connectionMode>auto</connectionMode>
                      <autoSwitch>true</autoSwitch>
                      <MSM>
                        <security>
                          <authEncryption>
                            <authentication>WPA2</authentication>
                            <encryption>AES</encryption>
                            <useOneX>true</useOneX>
                            <FIPSMode xmlns=""http://www.microsoft.com/networking/WLAN/profile/v2"">false</FIPSMode>
                          </authEncryption>
                          <PMKCacheMode>enabled</PMKCacheMode>
                          <PMKCacheTTL>720</PMKCacheTTL>
                          <PMKCacheSize>128</PMKCacheSize>
                          <preAuthMode>disabled</preAuthMode>
                          <OneX xmlns=""http://www.microsoft.com/networking/OneX/v1"">
                            <cacheUserData>true</cacheUserData>
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
                                        <PerformServerValidation xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">true</PerformServerValidation>
                                        <AcceptServerName xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">false</AcceptServerName>
                                        <PeapExtensionsV2 xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2"">
                                          <AllowPromptingWhenServerCANotFound xmlns=""http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV3"">true</AllowPromptingWhenServerCANotFound>
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
                    </WLANProfile>",
					position: 1),

			new Profile(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					isConnected: false,
					profileName: "PrintNet",
					ssid: "PrintNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.Open,
					encryption: WiFiEncryptionType.WEP,
					keyType: WiFiKeyType.NetworkKey,
					keyIsEncrypted: false,
					keyValue: "5061737377",
					isAutoConnectEnabled: false,
					isAutoSwitchEnabled: false,
					xml: $@"<?xml version=""1.0""?>
                    <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                        <name>PrintNet</name>
                        <SSIDConfig>
                        <SSID>
                            <hex>5072696e744e6574</hex>
                            <name>PrintNet</name>
                        </SSID>
                        <nonBroadcast>true</nonBroadcast>
                        </SSIDConfig>
                        <connectionType>ESS</connectionType>
                        <connectionMode>auto</connectionMode>
                        <autoSwitch>true</autoSwitch>
                        <MSM>
                        <security>
                            <authEncryption>
                            <authentication>open</authentication>
                            <encryption>WEP</encryption>
                            <useOneX>false</useOneX>
                            <FIPSMode xmlns=""http://www.microsoft.com/networking/WLAN/profile/v2"">false</FIPSMode>
                            </authEncryption>
                            <sharedKey>
                            <keyType>networkKey</keyType>
                            <protected>false</protected>
                            <keyMaterial>5061737377</keyMaterial>
                            </sharedKey>
                        </security>
                        </MSM>
                    </WLANProfile>",
					position: 2),

			new Profile(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					isConnected: false,
					profileName: "Backup_AmazonNet",
					ssid: "AmazonNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					keyType: WiFiKeyType.PassPhrase,
					keyIsEncrypted: false,
					keyValue: "Welcome123",
					isAutoConnectEnabled: true,
					isAutoSwitchEnabled: true,
					xml: $@"<?xml version=""1.0""?>
                    <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                        <name>AmazonNet</name>
                        <SSIDConfig>
                        <SSID>
                            <hex>416d617a6f6e4e6574</hex>
                            <name>AmazonNet</name>
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
                            <FIPSMode xmlns=""http://www.microsoft.com/networking/WLAN/profile/v2"">false</FIPSMode>
                            </authEncryption>
                            <sharedKey>
                            <keyType>passPhrase</keyType>
                            <protected>false</protected>
                            <keyMaterial>Welcome123</keyMaterial>
                            </sharedKey>
                        </security>
                        </MSM>
                    </WLANProfile>",
					position: 3),

			new Profile(
					id: new Guid("f58b4e99-a1ab-40b5-d43a-e7537e283dab"),
					isConnected: false,
					profileName: "OpenOfficeNet",
					ssid: "OpenOfficeNet",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.Open,
					encryption: WiFiEncryptionType.None,
					keyType: WiFiKeyType.None,
					keyIsEncrypted: false,
					keyValue: null,
					isAutoConnectEnabled: true,
					isAutoSwitchEnabled: true,
					xml: $@"<?xml version=""1.0""?>
                    <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                        <name>OpenOfficeNet</name>
                        <SSIDConfig>
                        <SSID>
                            <hex>4f70656e4f66666963654e6574</hex>
                            <name>OpenOfficeNet</name>
                        </SSID>
                        <nonBroadcast>true</nonBroadcast>
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
                            <FIPSMode xmlns=""http://www.microsoft.com/networking/WLAN/profile/v2"">false</FIPSMode>
                            </authEncryption>
                        </security>
                        </MSM>
                    </WLANProfile>",
					position: 0),

			new Profile(
					id: new Guid("f58b4e99-a1ab-40b5-d43a-e7537e283dab"),
					isConnected: true,
					profileName: "fontysWPA",
					ssid: "fontysWPA",
					profileType: WiFiProfileType.AllUser,
					bssType: WiFiBssType.Infrastructure,
					authentication: WiFiAuthentication.Open,
					encryption: WiFiEncryptionType.WEP,
					keyType: WiFiKeyType.NetworkKey,
					keyIsEncrypted: false,
					keyValue: "616C666161",
					isAutoConnectEnabled: true,
					isAutoSwitchEnabled: true,
					xml: $@"<?xml version=""1.0""?>
                    <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                        <name>fontysWPA</name>
                        <SSIDConfig>
                        <SSID>
                            <hex>666f6e747973575041</hex>
                            <name>fontysWPA</name>
                        </SSID>
                        <nonBroadcast>true</nonBroadcast>
                        </SSIDConfig>
                        <connectionType>ESS</connectionType>
                        <connectionMode>auto</connectionMode>
                        <autoSwitch>true</autoSwitch>
                        <MSM>
                        <security>
                            <authEncryption>
                            <authentication>open</authentication>
                            <encryption>WEP</encryption>
                            <useOneX>false</useOneX>
                            <FIPSMode xmlns=""http://www.microsoft.com/networking/WLAN/profile/v2"">false</FIPSMode>
                            </authEncryption>
                            <sharedKey>
                            <keyType>networkKey</keyType>
                            <protected>false</protected>
                            <keyMaterial>616C666161</keyMaterial>
                            </sharedKey>
                        </security>
                        </MSM>
                    </WLANProfile>",
					position: 1)
			};
		}

		private AccessPoint[] PopulateWifiAccesspoints()
		{
			return new[]
			{
				new AccessPoint(
					id: new Guid("ac761785-ed42-11ce-dacb-00bdd0057645"),
					ssid: "Webgate",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "Webgate",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: true,
					linkQuality: 100,
					frequency: 2457000,
					band: (float)2.4,
					channel: 10),

			new AccessPoint(
					id: new Guid("ac761785-ed42-11ce-dacb-00bdd0057645"),
					ssid: "KPN Fon",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: false,
					profileName: "KPN Fon",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.Open,
					encryption: WiFiEncryptionType.None,
					isConnected: false,
					linkQuality: 100,
					frequency: 2457000,
					band: (float)2.4,
					channel: 10),

			new AccessPoint(
					id: new Guid("ac761785-ed42-11ce-dacb-00bdd0057645"),
					ssid: "VFNL-6F2368",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "VFNL-6F2368",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 19,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("ac761785-ed42-11ce-dacb-00bdd0057645"),
					ssid: "Sitecom4A711C",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "Sitecom4A711C",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 70,
					frequency: 2462000,
					band: (float)2.4,
					channel: 11),

			new AccessPoint(
					id: new Guid("ac761785-ed42-11ce-dacb-00bdd0057645"),
					ssid: "TMNL-6E34DB",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "TMNL-6E34DB",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 18,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("ac761785-ed42-11ce-dacb-00bdd0057645"),
					ssid: "VGV7519531B41",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "VGV7519531B41",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 52,
					frequency: 2417000,
					band: (float)2.4,
					channel: 2),

			new AccessPoint(
					id: new Guid("ac761785-ed42-11ce-dacb-00bdd0057645"),
					ssid: "Chromcast",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: null,
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 60,
					frequency: 2437000,
					band: (float)2.4,
					channel: 6),

			 new AccessPoint(
					id: new Guid("ac761785-ed42-11ce-dacb-00bdd0057645"),
					ssid: "[Washer] Samsung",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: null,
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 57,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("84af3ffe-44d0-44b5-06e9-6a77e21adeda"),
					ssid: "H368N9D1BBC",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "H368N9D1BBC",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: true,
					linkQuality: 78,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("84af3ffe-44d0-44b5-06e9-6a77e21adeda"),
					ssid: "SecurityXploded",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "SecurityGateway",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 78,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("84af3ffe-44d0-44b5-06e9-6a77e21adeda"),
					ssid: "EuroNet",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "EuroNet",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 78,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("84af3ffe-44d0-44b5-06e9-6a77e21adeda"),
					ssid: "ShopNet",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "ShopNet",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 45,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					ssid: "AmazonNet",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "AmazonNet",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 45,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					ssid: "AmazonNet",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "Backup_AmazonNet",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 45,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					ssid: "YCollege",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "YCollege",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 45,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					ssid: "PrintNet",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "PrintNet",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 45,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					ssid: "LibraryNet",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: string.Empty,
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA_Enterprise,
					encryption: WiFiEncryptionType.TKIP,
					isConnected: false,
					linkQuality: 73,
					frequency: 2414000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("beb40faf-60c6-46fd-8771-0f096c42aeef"),
					ssid: "SchipholFreeNet",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: string.Empty,
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Enterprise,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 89,
					frequency: 2414000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("f58b4e99-a1ab-40b5-d43a-e7537e283dab"),
					ssid: "OpenOfficeNet",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "OpenOfficeNet",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: false,
					linkQuality: 45,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),

			new AccessPoint(
					id: new Guid("f58b4e99-a1ab-40b5-d43a-e7537e283dab"),
					ssid: "fontysWPA",
					bssType: WiFiBssType.Infrastructure,
					isSecurityEnabled: true,
					profileName: "fontysWPA",
					networkConnectable: true,
					wlanNotConnectableReason: null,
					authentication: WiFiAuthentication.WPA2_Personal,
					encryption: WiFiEncryptionType.AES,
					isConnected: true,
					linkQuality: 45,
					frequency: 2412000,
					band: (float)2.4,
					channel: 1),
			};
		}
		#endregion
	}
}
