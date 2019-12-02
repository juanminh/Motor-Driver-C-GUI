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
		<Item Name="Programmer.vi" Type="VI" URL="../Programmer.vi"/>
		<Item Name="SerialProgrammer.ini" Type="Document" URL="../data/SerialProgrammer.ini"/>
		<Item Name="seriaporgrammer_X5P_icon.ico" Type="Document" URL="../data/seriaporgrammer_X5P_icon.ico"/>
		<Item Name="Dependencies" Type="Dependencies">
			<Item Name="vi.lib" Type="Folder">
				<Item Name="8.6CompatibleGlobalVar.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/config.llb/8.6CompatibleGlobalVar.vi"/>
				<Item Name="Acquire Semaphore.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/semaphor.llb/Acquire Semaphore.vi"/>
				<Item Name="AddNamedSemaphorePrefix.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/semaphor.llb/AddNamedSemaphorePrefix.vi"/>
				<Item Name="Application Directory.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/file.llb/Application Directory.vi"/>
				<Item Name="Check if File or Folder Exists.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/libraryn.llb/Check if File or Folder Exists.vi"/>
				<Item Name="Clear Errors.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Clear Errors.vi"/>
				<Item Name="Error Cluster From Error Code.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Error Cluster From Error Code.vi"/>
				<Item Name="ex_CorrectErrorChain.vi" Type="VI" URL="/&lt;vilib&gt;/express/express shared/ex_CorrectErrorChain.vi"/>
				<Item Name="GetNamedSemaphorePrefix.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/semaphor.llb/GetNamedSemaphorePrefix.vi"/>
				<Item Name="High Resolution Relative Seconds.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/High Resolution Relative Seconds.vi"/>
				<Item Name="NI_FileType.lvlib" Type="Library" URL="/&lt;vilib&gt;/Utility/lvfile.llb/NI_FileType.lvlib"/>
				<Item Name="NI_LVConfig.lvlib" Type="Library" URL="/&lt;vilib&gt;/Utility/config.llb/NI_LVConfig.lvlib"/>
				<Item Name="NI_PackedLibraryUtility.lvlib" Type="Library" URL="/&lt;vilib&gt;/Utility/LVLibp/NI_PackedLibraryUtility.lvlib"/>
				<Item Name="Not A Semaphore.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/semaphor.llb/Not A Semaphore.vi"/>
				<Item Name="Obtain Semaphore Reference.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/semaphor.llb/Obtain Semaphore Reference.vi"/>
				<Item Name="Release Semaphore.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/semaphor.llb/Release Semaphore.vi"/>
				<Item Name="Semaphore RefNum" Type="VI" URL="/&lt;vilib&gt;/Utility/semaphor.llb/Semaphore RefNum"/>
				<Item Name="Semaphore Refnum Core.ctl" Type="VI" URL="/&lt;vilib&gt;/Utility/semaphor.llb/Semaphore Refnum Core.ctl"/>
				<Item Name="Set Busy.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/cursorutil.llb/Set Busy.vi"/>
				<Item Name="Set Cursor (Cursor ID).vi" Type="VI" URL="/&lt;vilib&gt;/Utility/cursorutil.llb/Set Cursor (Cursor ID).vi"/>
				<Item Name="Set Cursor (Icon Pict).vi" Type="VI" URL="/&lt;vilib&gt;/Utility/cursorutil.llb/Set Cursor (Icon Pict).vi"/>
				<Item Name="Set Cursor.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/cursorutil.llb/Set Cursor.vi"/>
				<Item Name="Space Constant.vi" Type="VI" URL="/&lt;vilib&gt;/dlg_ctls.llb/Space Constant.vi"/>
				<Item Name="subDisplayMessage.vi" Type="VI" URL="/&lt;vilib&gt;/express/express output/DisplayMessageBlock.llb/subDisplayMessage.vi"/>
				<Item Name="subTimeDelay.vi" Type="VI" URL="/&lt;vilib&gt;/express/express execution control/TimeDelayBlock.llb/subTimeDelay.vi"/>
				<Item Name="Trim Whitespace.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Trim Whitespace.vi"/>
				<Item Name="Unset Busy.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/cursorutil.llb/Unset Busy.vi"/>
				<Item Name="Validate Semaphore Size.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/semaphor.llb/Validate Semaphore Size.vi"/>
				<Item Name="VISA Configure Serial Port" Type="VI" URL="/&lt;vilib&gt;/Instr/_visa.llb/VISA Configure Serial Port"/>
				<Item Name="VISA Configure Serial Port (Instr).vi" Type="VI" URL="/&lt;vilib&gt;/Instr/_visa.llb/VISA Configure Serial Port (Instr).vi"/>
				<Item Name="VISA Configure Serial Port (Serial Instr).vi" Type="VI" URL="/&lt;vilib&gt;/Instr/_visa.llb/VISA Configure Serial Port (Serial Instr).vi"/>
				<Item Name="VISA Find Search Mode.ctl" Type="VI" URL="/&lt;vilib&gt;/Instr/_visa.llb/VISA Find Search Mode.ctl"/>
				<Item Name="whitespace.ctl" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/whitespace.ctl"/>
			</Item>
			<Item Name="BuildSunCheckSum (SubVI).vi" Type="VI" URL="../Functions/BuildSunCheckSum (SubVI).vi"/>
			<Item Name="Connect COM Port &amp; AutoBaud (SubVI).vi" Type="VI" URL="../Functions/Connect COM Port &amp; AutoBaud (SubVI).vi"/>
			<Item Name="ConvertDataFrom File (SubVI).vi" Type="VI" URL="../Functions/ConvertDataFrom File (SubVI).vi"/>
			<Item Name="CRC16_Calc.vi" Type="VI" URL="../Functions/CRC16_Calc.vi"/>
			<Item Name="Crc16DLL.dll" Type="Document" URL="../data/Crc16DLL.dll"/>
			<Item Name="CRC_compare.vi" Type="VI" URL="../Functions/CRC_compare.vi"/>
			<Item Name="Data_Convert.vi" Type="VI" URL="../C280xxx_Flash_programmer_with_cmd_example/Data_Convert.vi"/>
			<Item Name="DescriptReceivedMsg SingleBoard (SubVI).vi" Type="VI" URL="../../../SingleBoard/LabVIEW/BurnProjet/Functions/DescriptReceivedMsg SingleBoard (SubVI).vi"/>
			<Item Name="ExpectedSumAndRxSum (SubVI).vi" Type="VI" URL="../Functions/ExpectedSumAndRxSum (SubVI).vi"/>
			<Item Name="FrameCmdB.vi" Type="VI" URL="../Functions/FrameCmdB.vi"/>
			<Item Name="Idx_calc.vi" Type="VI" URL="../../../SingleBoard/LabVIEW/BurnProjet/Functions/Idx_calc.vi"/>
			<Item Name="Int_Float.vi" Type="VI" URL="../Functions/Int_Float.vi"/>
			<Item Name="iterate func (SubVI).vi" Type="VI" URL="../Functions/iterate func (SubVI).vi"/>
			<Item Name="LoadData (SubVI).vi" Type="VI" URL="../Functions/LoadData (SubVI).vi"/>
			<Item Name="PanelMsg (SubVI).vi" Type="VI" URL="../Functions/PanelMsg (SubVI).vi"/>
			<Item Name="Save SerialParameters (SubVI).vi" Type="VI" URL="../Functions/Save SerialParameters (SubVI).vi"/>
			<Item Name="Select COM for AutoBaud.vi" Type="VI" URL="../Functions/Select COM for AutoBaud.vi"/>
			<Item Name="SendBuffReceiveCS (SubVI).vi" Type="VI" URL="../Functions/SendBuffReceiveCS (SubVI).vi"/>
			<Item Name="TxRx SingleBoard (SubVI).vi" Type="VI" URL="../Functions/TxRx SingleBoard (SubVI).vi"/>
		</Item>
		<Item Name="Build Specifications" Type="Build">
			<Item Name="Serial Programmer" Type="EXE">
				<Property Name="App_copyErrors" Type="Bool">true</Property>
				<Property Name="App_INI_aliasGUID" Type="Str">{D2FB9264-D530-440B-8216-DE289B7ECA0E}</Property>
				<Property Name="App_INI_GUID" Type="Str">{C16C649C-7EFD-4271-B2DC-63B01AE338A0}</Property>
				<Property Name="App_serverConfig.httpPort" Type="Int">8002</Property>
				<Property Name="App_winsec.description" Type="Str">http://www.Engineer.com</Property>
				<Property Name="Bld_autoIncrement" Type="Bool">true</Property>
				<Property Name="Bld_buildCacheID" Type="Str">{6EA63138-3B23-4D1F-9596-73569CB8F833}</Property>
				<Property Name="Bld_buildSpecName" Type="Str">Serial Programmer</Property>
				<Property Name="Bld_excludeInlineSubVIs" Type="Bool">true</Property>
				<Property Name="Bld_excludeLibraryItems" Type="Bool">true</Property>
				<Property Name="Bld_excludePolymorphicVIs" Type="Bool">true</Property>
				<Property Name="Bld_localDestDir" Type="Path">../SuperButton MotorController/SuperButton/bin/Debug/SerialProgrammer</Property>
				<Property Name="Bld_localDestDirType" Type="Str">relativeToCommon</Property>
				<Property Name="Bld_modifyLibraryFile" Type="Bool">true</Property>
				<Property Name="Bld_previewCacheID" Type="Str">{4655D5B0-E7E0-4AA9-B222-089A3AE77F5C}</Property>
				<Property Name="Bld_version.build" Type="Int">8</Property>
				<Property Name="Bld_version.major" Type="Int">1</Property>
				<Property Name="Destination[0].destName" Type="Str">Serial Programmer.exe</Property>
				<Property Name="Destination[0].path" Type="Path">../SuperButton MotorController/SuperButton/bin/Debug/SerialProgrammer/Serial Programmer.exe</Property>
				<Property Name="Destination[0].preserveHierarchy" Type="Bool">true</Property>
				<Property Name="Destination[0].type" Type="Str">App</Property>
				<Property Name="Destination[1].destName" Type="Str">Support Directory</Property>
				<Property Name="Destination[1].path" Type="Path">../SuperButton MotorController/SuperButton/bin/Debug/SerialProgrammer/data</Property>
				<Property Name="DestinationCount" Type="Int">2</Property>
				<Property Name="Exe_iconItemID" Type="Ref">/My Computer/seriaporgrammer_X5P_icon.ico</Property>
				<Property Name="Source[0].itemID" Type="Str">{79E2F7D0-2F73-4190-8A41-30C5CB0EA23A}</Property>
				<Property Name="Source[0].type" Type="Str">Container</Property>
				<Property Name="Source[1].destinationIndex" Type="Int">0</Property>
				<Property Name="Source[1].itemID" Type="Ref">/My Computer/Programmer.vi</Property>
				<Property Name="Source[1].sourceInclusion" Type="Str">TopLevel</Property>
				<Property Name="Source[1].type" Type="Str">VI</Property>
				<Property Name="Source[2].destinationIndex" Type="Int">0</Property>
				<Property Name="Source[2].itemID" Type="Ref">/My Computer/SerialProgrammer.ini</Property>
				<Property Name="Source[2].sourceInclusion" Type="Str">Include</Property>
				<Property Name="SourceCount" Type="Int">3</Property>
				<Property Name="TgtF_companyName" Type="Str">Engineer</Property>
				<Property Name="TgtF_fileDescription" Type="Str">Serial Programmer</Property>
				<Property Name="TgtF_internalName" Type="Str">Serial Programmer</Property>
				<Property Name="TgtF_legalCopyright" Type="Str">Copyright © 2019 Engineer</Property>
				<Property Name="TgtF_productName" Type="Str">Serial Programmer</Property>
				<Property Name="TgtF_targetfileGUID" Type="Str">{5863E68F-1C0A-4D9F-B3CC-1ABE6A1EE560}</Property>
				<Property Name="TgtF_targetfileName" Type="Str">Serial Programmer.exe</Property>
				<Property Name="TgtF_versionIndependent" Type="Bool">true</Property>
			</Item>
		</Item>
	</Item>
</Project>
