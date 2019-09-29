<?xml version='1.0' encoding='UTF-8'?>
<Project Type="Project" LVVersion="17008000">
	<Item Name="My Computer" Type="My Computer">
		<Property Name="server.app.propertiesEnabled" Type="Bool">true</Property>
		<Property Name="server.control.propertiesEnabled" Type="Bool">true</Property>
		<Property Name="server.tcp.enabled" Type="Bool">false</Property>
		<Property Name="server.tcp.port" Type="Int">0</Property>
		<Property Name="server.tcp.serviceName" Type="Str">My Computer/VI Server</Property>
		<Property Name="server.tcp.serviceName.default" Type="Str">My Computer/VI Server</Property>
		<Property Name="server.vi.callsEnabled" Type="Bool">true</Property>
		<Property Name="server.vi.propertiesEnabled" Type="Bool">true</Property>
		<Property Name="specify.custom.address" Type="Bool">false</Property>
		<Item Name="Code 0.0.3.vi" Type="VI" URL="../Code 0.0.3.vi"/>
		<Item Name="RayonSerialProgramerConfigurationFile.ini" Type="Document" URL="../data/RayonSerialProgramerConfigurationFile.ini"/>
		<Item Name="Dependencies" Type="Dependencies">
			<Item Name="vi.lib" Type="Folder">
				<Item Name="8.6CompatibleGlobalVar.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/config.llb/8.6CompatibleGlobalVar.vi"/>
				<Item Name="Application Directory.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/file.llb/Application Directory.vi"/>
				<Item Name="Check if File or Folder Exists.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/libraryn.llb/Check if File or Folder Exists.vi"/>
				<Item Name="Clear Errors.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Clear Errors.vi"/>
				<Item Name="Error Cluster From Error Code.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Error Cluster From Error Code.vi"/>
				<Item Name="High Resolution Relative Seconds.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/High Resolution Relative Seconds.vi"/>
				<Item Name="Is Path and Not Empty.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/file.llb/Is Path and Not Empty.vi"/>
				<Item Name="NI_FileType.lvlib" Type="Library" URL="/&lt;vilib&gt;/Utility/lvfile.llb/NI_FileType.lvlib"/>
				<Item Name="NI_LVConfig.lvlib" Type="Library" URL="/&lt;vilib&gt;/Utility/config.llb/NI_LVConfig.lvlib"/>
				<Item Name="NI_PackedLibraryUtility.lvlib" Type="Library" URL="/&lt;vilib&gt;/Utility/LVLibp/NI_PackedLibraryUtility.lvlib"/>
				<Item Name="Select Event Type.ctl" Type="VI" URL="/&lt;vilib&gt;/Instr/_visa.llb/Select Event Type.ctl"/>
				<Item Name="Space Constant.vi" Type="VI" URL="/&lt;vilib&gt;/dlg_ctls.llb/Space Constant.vi"/>
				<Item Name="subTimeDelay.vi" Type="VI" URL="/&lt;vilib&gt;/express/express execution control/TimeDelayBlock.llb/subTimeDelay.vi"/>
				<Item Name="Trim Whitespace.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Trim Whitespace.vi"/>
				<Item Name="VISA Configure Serial Port" Type="VI" URL="/&lt;vilib&gt;/Instr/_visa.llb/VISA Configure Serial Port"/>
				<Item Name="VISA Configure Serial Port (Instr).vi" Type="VI" URL="/&lt;vilib&gt;/Instr/_visa.llb/VISA Configure Serial Port (Instr).vi"/>
				<Item Name="VISA Configure Serial Port (Serial Instr).vi" Type="VI" URL="/&lt;vilib&gt;/Instr/_visa.llb/VISA Configure Serial Port (Serial Instr).vi"/>
				<Item Name="VISA Open Access Mode.ctl" Type="VI" URL="/&lt;vilib&gt;/Instr/_visa.llb/VISA Open Access Mode.ctl"/>
				<Item Name="whitespace.ctl" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/whitespace.ctl"/>
			</Item>
			<Item Name="Data_Convert.vi" Type="VI" URL="../Data_Convert.vi"/>
		</Item>
		<Item Name="Build Specifications" Type="Build">
			<Item Name="My Installer" Type="Installer">
				<Property Name="Destination[0].name" Type="Str">SerialBurn</Property>
				<Property Name="Destination[0].parent" Type="Str">{3912416A-D2E5-411B-AFEE-B63654D690C0}</Property>
				<Property Name="Destination[0].tag" Type="Str">{454E79C2-2EAA-4FFB-BD48-397C62465F64}</Property>
				<Property Name="Destination[0].type" Type="Str">userFolder</Property>
				<Property Name="DestinationCount" Type="Int">1</Property>
				<Property Name="DistPart[0].flavorID" Type="Str">_full_</Property>
				<Property Name="DistPart[0].productID" Type="Str">{6B2D1DEB-CFE9-43D9-B325-AB607C62A29C}</Property>
				<Property Name="DistPart[0].productName" Type="Str">NI-488.2 Runtime 17.0</Property>
				<Property Name="DistPart[0].upgradeCode" Type="Str">{357F6618-C660-41A2-A185-5578CC876D1D}</Property>
				<Property Name="DistPart[1].flavorID" Type="Str">_full_</Property>
				<Property Name="DistPart[1].productID" Type="Str">{5E17F5AF-F47C-4C3A-A219-91F4638CE9D1}</Property>
				<Property Name="DistPart[1].productName" Type="Str">NI-Serial Runtime 17.0</Property>
				<Property Name="DistPart[1].upgradeCode" Type="Str">{01D82F43-B48D-46FF-8601-FC4FAAE20F41}</Property>
				<Property Name="DistPart[2].flavorID" Type="Str">_deployment_</Property>
				<Property Name="DistPart[2].productID" Type="Str">{F9B5B433-547E-4A74-AFE6-91C16787824E}</Property>
				<Property Name="DistPart[2].productName" Type="Str">NI-VISA Runtime 17.0</Property>
				<Property Name="DistPart[2].upgradeCode" Type="Str">{8627993A-3F66-483C-A562-0D3BA3F267B1}</Property>
				<Property Name="DistPart[3].flavorID" Type="Str">DefaultFull</Property>
				<Property Name="DistPart[3].productID" Type="Str">{F8C28DE1-1E13-4D22-AC1B-A09B86F991EC}</Property>
				<Property Name="DistPart[3].productName" Type="Str">NI LabVIEW Runtime 2017 f2</Property>
				<Property Name="DistPart[3].SoftDep[0].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[0].productName" Type="Str">NI LabVIEW Runtime 2017 Non-English Support.</Property>
				<Property Name="DistPart[3].SoftDep[0].upgradeCode" Type="Str">{182AE811-85B6-4238-B67E-F19497CC186B}</Property>
				<Property Name="DistPart[3].SoftDep[1].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[1].productName" Type="Str">NI ActiveX Container</Property>
				<Property Name="DistPart[3].SoftDep[1].upgradeCode" Type="Str">{1038A887-23E1-4289-B0BD-0C4B83C6BA21}</Property>
				<Property Name="DistPart[3].SoftDep[10].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[10].productName" Type="Str">NI mDNS Responder 14.0</Property>
				<Property Name="DistPart[3].SoftDep[10].upgradeCode" Type="Str">{9607874B-4BB3-42CB-B450-A2F5EF60BA3B}</Property>
				<Property Name="DistPart[3].SoftDep[11].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[11].productName" Type="Str">NI Deployment Framework 2017</Property>
				<Property Name="DistPart[3].SoftDep[11].upgradeCode" Type="Str">{838942E4-B73C-492E-81A3-AA1E291FD0DC}</Property>
				<Property Name="DistPart[3].SoftDep[12].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[12].productName" Type="Str">NI Error Reporting 2017</Property>
				<Property Name="DistPart[3].SoftDep[12].upgradeCode" Type="Str">{42E818C6-2B08-4DE7-BD91-B0FD704C119A}</Property>
				<Property Name="DistPart[3].SoftDep[2].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[2].productName" Type="Str">Math Kernel Libraries</Property>
				<Property Name="DistPart[3].SoftDep[2].upgradeCode" Type="Str">{699C1AC5-2CF2-4745-9674-B19536EBA8A3}</Property>
				<Property Name="DistPart[3].SoftDep[3].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[3].productName" Type="Str">NI Logos 5.9</Property>
				<Property Name="DistPart[3].SoftDep[3].upgradeCode" Type="Str">{5E4A4CE3-4D06-11D4-8B22-006008C16337}</Property>
				<Property Name="DistPart[3].SoftDep[4].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[4].productName" Type="Str">NI TDM Streaming 17.0</Property>
				<Property Name="DistPart[3].SoftDep[4].upgradeCode" Type="Str">{4CD11BE6-6BB7-4082-8A27-C13771BC309B}</Property>
				<Property Name="DistPart[3].SoftDep[5].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[5].productName" Type="Str">NI LabVIEW Web Server 2017</Property>
				<Property Name="DistPart[3].SoftDep[5].upgradeCode" Type="Str">{0960380B-EA86-4E0C-8B57-14CD8CCF2C15}</Property>
				<Property Name="DistPart[3].SoftDep[6].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[6].productName" Type="Str">NI LabVIEW Real-Time NBFifo 2017</Property>
				<Property Name="DistPart[3].SoftDep[6].upgradeCode" Type="Str">{4F261250-2C38-488D-A9EC-9D1EFCC24D4B}</Property>
				<Property Name="DistPart[3].SoftDep[7].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[7].productName" Type="Str">NI VC2008MSMs</Property>
				<Property Name="DistPart[3].SoftDep[7].upgradeCode" Type="Str">{FDA3F8BB-BAA9-45D7-8DC7-22E1F5C76315}</Property>
				<Property Name="DistPart[3].SoftDep[8].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[8].productName" Type="Str">NI VC2010MSMs</Property>
				<Property Name="DistPart[3].SoftDep[8].upgradeCode" Type="Str">{EFBA6F9E-F934-4BD7-AC51-60CCA480489C}</Property>
				<Property Name="DistPart[3].SoftDep[9].exclude" Type="Bool">false</Property>
				<Property Name="DistPart[3].SoftDep[9].productName" Type="Str">NI VC2015 Runtime</Property>
				<Property Name="DistPart[3].SoftDep[9].upgradeCode" Type="Str">{D42E7BAE-6589-4570-B6A3-3E28889392E7}</Property>
				<Property Name="DistPart[3].SoftDepCount" Type="Int">13</Property>
				<Property Name="DistPart[3].upgradeCode" Type="Str">{620DBAE1-B159-4204-8186-0813C8A6434C}</Property>
				<Property Name="DistPartCount" Type="Int">4</Property>
				<Property Name="INST_author" Type="Str">Engineer</Property>
				<Property Name="INST_autoIncrement" Type="Bool">true</Property>
				<Property Name="INST_buildLocation" Type="Path">../SerialBurn/My Installer</Property>
				<Property Name="INST_buildLocation.type" Type="Str">relativeToCommon</Property>
				<Property Name="INST_buildSpecName" Type="Str">My Installer</Property>
				<Property Name="INST_defaultDir" Type="Str">{454E79C2-2EAA-4FFB-BD48-397C62465F64}</Property>
				<Property Name="INST_productName" Type="Str">SerialBurn</Property>
				<Property Name="INST_productVersion" Type="Str">1.0.6</Property>
				<Property Name="InstSpecBitness" Type="Str">32-bit</Property>
				<Property Name="InstSpecVersion" Type="Str">17008011</Property>
				<Property Name="MSI_arpCompany" Type="Str">Engineer</Property>
				<Property Name="MSI_arpURL" Type="Str">http://www.engineer.com/</Property>
				<Property Name="MSI_autoselectDrivers" Type="Bool">true</Property>
				<Property Name="MSI_distID" Type="Str">{C4BF1657-7C48-40AB-9CE9-DC287980AB10}</Property>
				<Property Name="MSI_hideNonRuntimes" Type="Bool">true</Property>
				<Property Name="MSI_osCheck" Type="Int">0</Property>
				<Property Name="MSI_upgradeCode" Type="Str">{5D93A6C6-0124-4C61-AC02-8989764ED5FF}</Property>
				<Property Name="RegDest[0].dirName" Type="Str">Software</Property>
				<Property Name="RegDest[0].dirTag" Type="Str">{DDFAFC8B-E728-4AC8-96DE-B920EBB97A86}</Property>
				<Property Name="RegDest[0].parentTag" Type="Str">2</Property>
				<Property Name="RegDestCount" Type="Int">1</Property>
				<Property Name="Source[0].dest" Type="Str">{454E79C2-2EAA-4FFB-BD48-397C62465F64}</Property>
				<Property Name="Source[0].File[0].dest" Type="Str">{454E79C2-2EAA-4FFB-BD48-397C62465F64}</Property>
				<Property Name="Source[0].File[0].name" Type="Str">SerialBurn 0.0.1.exe</Property>
				<Property Name="Source[0].File[0].Shortcut[0].destIndex" Type="Int">1</Property>
				<Property Name="Source[0].File[0].Shortcut[0].name" Type="Str">SerialBurn 0.0.1</Property>
				<Property Name="Source[0].File[0].Shortcut[0].subDir" Type="Str"></Property>
				<Property Name="Source[0].File[0].ShortcutCount" Type="Int">1</Property>
				<Property Name="Source[0].File[0].tag" Type="Str">{0B77534B-3601-4226-94D4-D52C1E18C641}</Property>
				<Property Name="Source[0].FileCount" Type="Int">1</Property>
				<Property Name="Source[0].name" Type="Str">My Application</Property>
				<Property Name="Source[0].tag" Type="Ref">/My Computer/Build Specifications/Rayon-Serial-Programer</Property>
				<Property Name="Source[0].type" Type="Str">EXE</Property>
				<Property Name="SourceCount" Type="Int">1</Property>
			</Item>
			<Item Name="Rayon-Serial-Programer" Type="EXE">
				<Property Name="App_copyErrors" Type="Bool">true</Property>
				<Property Name="App_INI_aliasGUID" Type="Str">{4DA86312-706F-409C-99ED-7B56F23A2334}</Property>
				<Property Name="App_INI_GUID" Type="Str">{2DF17FCB-D1E8-4E94-AF73-A121A185C692}</Property>
				<Property Name="App_serverConfig.httpPort" Type="Int">8002</Property>
				<Property Name="Bld_autoIncrement" Type="Bool">true</Property>
				<Property Name="Bld_buildCacheID" Type="Str">{AFE6CE99-0464-4F51-BE3B-12396734E9E2}</Property>
				<Property Name="Bld_buildSpecName" Type="Str">Rayon-Serial-Programer</Property>
				<Property Name="Bld_excludeInlineSubVIs" Type="Bool">true</Property>
				<Property Name="Bld_excludeLibraryItems" Type="Bool">true</Property>
				<Property Name="Bld_excludePolymorphicVIs" Type="Bool">true</Property>
				<Property Name="Bld_localDestDir" Type="Path">../NI_AB_PROJECTNAME/Rayon-Serial-Programer</Property>
				<Property Name="Bld_localDestDirType" Type="Str">relativeToCommon</Property>
				<Property Name="Bld_modifyLibraryFile" Type="Bool">true</Property>
				<Property Name="Bld_previewCacheID" Type="Str">{27235CE0-0A77-4186-A695-FFE07C7C8084}</Property>
				<Property Name="Bld_version.build" Type="Int">6</Property>
				<Property Name="Bld_version.major" Type="Int">1</Property>
				<Property Name="Destination[0].destName" Type="Str">Rayon-Serial-Programer 1.0.5.exe</Property>
				<Property Name="Destination[0].path" Type="Path">../NI_AB_PROJECTNAME/Rayon-Serial-Programer/Rayon-Serial-Programer 1.0.5.exe</Property>
				<Property Name="Destination[0].preserveHierarchy" Type="Bool">true</Property>
				<Property Name="Destination[0].type" Type="Str">App</Property>
				<Property Name="Destination[1].destName" Type="Str">Support Directory</Property>
				<Property Name="Destination[1].path" Type="Path">../NI_AB_PROJECTNAME/Rayon-Serial-Programer/data</Property>
				<Property Name="DestinationCount" Type="Int">2</Property>
				<Property Name="Source[0].itemID" Type="Str">{CDEB160E-6909-449E-AC4A-BBAC032785EE}</Property>
				<Property Name="Source[0].type" Type="Str">Container</Property>
				<Property Name="Source[1].destinationIndex" Type="Int">0</Property>
				<Property Name="Source[1].itemID" Type="Ref">/My Computer/Code 0.0.3.vi</Property>
				<Property Name="Source[1].properties[0].type" Type="Str">Show Abort button</Property>
				<Property Name="Source[1].properties[0].value" Type="Bool">false</Property>
				<Property Name="Source[1].properties[1].type" Type="Str">Show toolbar</Property>
				<Property Name="Source[1].properties[1].value" Type="Bool">false</Property>
				<Property Name="Source[1].propertiesCount" Type="Int">2</Property>
				<Property Name="Source[1].sourceInclusion" Type="Str">TopLevel</Property>
				<Property Name="Source[1].type" Type="Str">VI</Property>
				<Property Name="Source[2].destinationIndex" Type="Int">0</Property>
				<Property Name="Source[2].itemID" Type="Ref">/My Computer/RayonSerialProgramerConfigurationFile.ini</Property>
				<Property Name="Source[2].sourceInclusion" Type="Str">Include</Property>
				<Property Name="SourceCount" Type="Int">3</Property>
				<Property Name="TgtF_companyName" Type="Str">Redler</Property>
				<Property Name="TgtF_fileDescription" Type="Str">Rayon-Serial-Programer</Property>
				<Property Name="TgtF_internalName" Type="Str">Rayon-Serial-Programer</Property>
				<Property Name="TgtF_legalCopyright" Type="Str">Copyright © 2018 Redler</Property>
				<Property Name="TgtF_productName" Type="Str">Rayon-Serial-Programer</Property>
				<Property Name="TgtF_targetfileGUID" Type="Str">{0B77534B-3601-4226-94D4-D52C1E18C641}</Property>
				<Property Name="TgtF_targetfileName" Type="Str">Rayon-Serial-Programer 1.0.5.exe</Property>
				<Property Name="TgtF_versionIndependent" Type="Bool">true</Property>
			</Item>
		</Item>
	</Item>
</Project>
