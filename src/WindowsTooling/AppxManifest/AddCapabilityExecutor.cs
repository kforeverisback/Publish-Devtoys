﻿// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using Serilog;
using System.Xml.Linq;
using System.Xml.XPath;

namespace WindowsTooling.AppxManifest;

public class AddCapabilityExecutor : AppxManifestEditExecutor<AddCapability>
{
    public AddCapabilityExecutor(XDocument manifest) : base(manifest)
    {
    }

    public override Task Execute(AddCapability command, CancellationToken cancellationToken = default)
    {
        if (Manifest.Root == null)
        {
            throw new InvalidOperationException("Document root cannot be empty.");
        }

        XElement? capabilities = Manifest.Root.XPathSelectElement("//*[local-name()='Capabilities']");
        if (capabilities == null)
        {
            (string _, XNamespace rootNamespace) = EnsureNamespace();
            capabilities = new XElement(rootNamespace + "Capabilities");
            Manifest.Root.Add(capabilities);
        }

        string nodeName;
        XNamespace ns;
        bool isRestricted;

        switch (command.Name)
        {
            // Restricted capabilities in RESCAP namespace
            case "cellularDeviceIdentity":
            case "deviceUnlock":
            case "networkingVpnProvider":
            case "inputSuppression":
            case "accessoryManager":
            case "appLicensing":
            case "cellularMessaging":
            case "userDataAccountsProvider":
            case "storeLicenseManagement":
            case "userPrincipalName":
            case "packageManagement":
            case "packagedServices":
            case "uiAutomation":
            case "confirmAppClose":
            case "cortanaPermissions":
            case "teamEditionView":
            case "customInstallActions":
            case "localSystemServices":
            case "teamEditionDeviceCredential":
            case "packagePolicySystem":
            case "modifiableApp":
            case "backgroundSpatialPerception":
            case "phoneLineTransportManagement":
            case "developmentModeNetwork":
            case "unvirtualizedResources":
            case "backgroundVoIP":
            case "gameMonitor":
            case "packageWriteRedirectionCompatibilityShim":
            case "cameraProcessingExtension":
            case "runFullTrust":
            case "allowElevation":
            case "smbios":
            case "appDiagnostics":
            case "devicePortalProvider":
            case "networkDataUsageManagement":
            case "gameBarServices":
            case "broadFileSystemAccess":
            case "backgroundMediaRecording":
            case "oneProcessVoIP":
            case "deviceManagementWapSecurityPolicies":
            case "previewInkWorkspace":
            case "teamEditionExperience":
            case "enterpriseCloudSSO":
            case "appCaptureServices":
            case "startScreenManagement":
            case "email":
            case "expandedResources":
            case "protectedApp":
            case "oemPublicDirectory":
            case "allAppMods":
            case "previewPenWorkspace":
            case "inputForegroundObservation":
            case "userSystemId":
            case "audioDeviceConfiguration":
            case "appBroadcastServices":
            case "targetedContent":
            case "interopServices":
            case "locationSystem":
            case "secondaryAuthenticationFactor":
            case "gameList":
            case "previewStore":
            case "xboxAccessoryManagement":
            case "oemDeployment":
            case "extendedBackgroundTaskTime":
            case "deviceManagementDmAccount":
            case "deviceManagementFoundation":
            case "cortanaSpeechAccessory":
            case "deviceManagementEmailAccount":
            case "extendedExecutionCritical":
            case "extendedExecutionBackgroundAudio":
            case "firstSignInSettings":
            case "extendedExecutionUnconstrained":
            case "appointmentsSystem":
            case "emailSystem":
            case "networkDataPlanProvisioning":
            case "phoneCallHistory":
            case "networkConnectionManagerProvisioning":
            case "chatSystem":
            case "remotePassportAuthentication":
            case "previewUiComposition":
            case "userDataSystem":
            case "slapiQueryLicenseValue":
            case "packageQuery":
            case "walletSystem":
            case "secureAssessment":
            case "smsSend":
            case "inputObservation":
            case "locationHistory":
            case "phoneCallHistorySystem":
            case "dualSimTiles":
            case "cellularDeviceControl":
            case "inputInjectionBrokered":
            case "contactsSystem":
            case "enterpriseDeviceLockdown":
            case "enterpriseDataPolicy":
                // restricted capabilities
                nodeName = "Capability";
                (_, ns) = EnsureNamespace(Namespaces.RestrictedCapabilities);
                isRestricted = true;
                break;

            // Restricted capabilities in UAP namespace
            case "documentsLibrary":
            case "sharedUserCertificates":
            case "enterpriseAuthentication":
                // general capabilities
                nodeName = "Capability";
                (_, ns) = EnsureNamespace(Namespaces.Uap);
                isRestricted = true;
                break;

            // General capabilities in UAP or UAP# namespaces
            case "videosLibrary":
            case "appointments":
            case "contacts":
            case "removableStorage":
            case "phoneCall":
            case "userAccountInformation":
            case "voipCall":
            case "objects3D":
            case "blockedChatMessages":
            case "chat":
            case "picturesLibrary":
            case "musicLibrary":
                isRestricted = false;
                nodeName = "Capability";
                (_, ns) = EnsureNamespace(Namespaces.Uap);
                break;
            case "recordedCallsFolder":
                isRestricted = false;
                nodeName = "Capability";
                (_, ns) = EnsureNamespace(Namespaces.Mobile);
                break;
            case "graphicsCaptureWithoutBorder":
            case "graphicsCaptureProgrammatic":
                isRestricted = false;
                nodeName = "Capability";
                (_, ns) = EnsureNamespace(Namespaces.Uap, 11);
                break;
            case "graphicsCapture":
                isRestricted = false;
                nodeName = "Capability";
                (_, ns) = EnsureNamespace(Namespaces.Uap, 6);
                break;
            case "globalMediaControl":
                isRestricted = false;
                nodeName = "Capability";
                (_, ns) = EnsureNamespace(Namespaces.Uap, 7);
                break;

            // General capabilities in IOT namespace
            case "lowLevelDevices":
            case "systemManagement":
                isRestricted = false;
                nodeName = "Capability";
                (_, ns) = EnsureNamespace(Namespaces.Iot);
                break;

            // Device capabilities
            case "bluetooth":
            case "location":
            case "microphone":
            case "gazeInput":
            case "radios":
            case "optical":
            case "lowLevel":
            case "wiFiControl":
            case "proximity":
            case "usb":
            case "serialcommunication":
            case "activity":
            case "humaninterfacedevice":
            case "pointOfService":
            case "webcam":
                isRestricted = false;
                nodeName = "DeviceCapability";
                (_, ns) = EnsureNamespace(Namespaces.Uap);
                break;

            // Custom capability
            default:
                if (command.Name.Length < 15)
                {
                    throw new InvalidOperationException(string.Format("The name of a custom capability must be longer than {2} characters. Capability '{0}' has only {1} characters.", command.Name, command.Name.Length, 15));
                }

                isRestricted = false;
                nodeName = "CustomCapability";
                (_, ns) = EnsureNamespace(Namespaces.Uap, 4);
                break;
        }

        XElement element = new(ns + nodeName);
        element.Add(new XAttribute("Name", command.Name));

        XElement? find = capabilities.Elements().FirstOrDefault(e => e.Name.Namespace == ns && e.Name.LocalName == nodeName && e.Attribute("Name")?.Value == command.Name);
        if (find != null)
        {
            Log.Warning("The capability '{0}' already exists and will not be added again.", command.Name);
            return Task.CompletedTask;
        }

        if (isRestricted)
        {
            capabilities.AddFirst(element);
        }
        else
        {
            capabilities.Add(element);
        }

        CapabilityAdded?.Invoke(this, new CapabilityChange(command.Name, isRestricted, nodeName == "CustomCapability"));
        return Task.CompletedTask;
    }

    public event EventHandler<CapabilityChange>? CapabilityAdded;

    public struct CapabilityChange
    {
        public CapabilityChange(string name, bool isRestricted, bool isCustom)
        {
            Name = name;
            IsRestricted = isRestricted;
            IsCustom = isCustom;
        }

        public string Name;
        public bool IsRestricted;
        public bool IsCustom;
    }
}