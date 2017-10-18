#
#   eduVPN - End-user friendly VPN
#
#   Copyright: 2017, The Commons Conservancy eduVPN Programme
#   SPDX-License-Identifier: GPL-3.0+
#

PRODUCT_NAME=eduVPN Client
PRODUCT_VERSION=1.0.8
PRODUCT_VERSION_STR=1.0-alpha6

OUTPUT_DIR=bin
SETUP_DIR=$(OUTPUT_DIR)\Setup
SETUP_NAME=eduVPN-Client-Win

# Default testing configuration and platform
CFG=Debug
!IF "$(PROCESSOR_ARCHITECTURE)" == "AMD64"
PLAT=x64
!ELSE
PLAT=x86
!ENDIF

# Utility default flags
!IF "$(PROCESSOR_ARCHITECTURE)" == "AMD64"
REG_FLAGS=/f /reg:64
REG_FLAGS32=/f /reg:32
!ELSE
REG_FLAGS=/f
!ENDIF
DEVENV_FLAGS=/NoLogo
CSCRIPT_FLAGS=//Nologo
WIX_WIXCOP_FLAGS=-nologo "-set1$(MAKEDIR)\wixcop.xml"
WIX_CANDLE_FLAGS=-nologo -deduVPN.Version="$(PRODUCT_VERSION)" -ext WixNetFxExtension -ext WixUtilExtension -ext WixBalExtension
WIX_LIGHT_FLAGS=-nologo -dcl:high -spdb -sice:ICE03 -sice:ICE61 -sice:ICE69 -sice:ICE82 -ext WixNetFxExtension -ext WixUtilExtension -ext WixBalExtension
WIX_INSIGNIA_FLAGS=-nologo


######################################################################
# Default target
######################################################################

All :: \
	Setup


######################################################################
# Registration
######################################################################

Register :: \
	RegisterSettings \
	RegisterShortcuts

Unregister :: \
	UnregisterShortcuts \
	UnregisterSettings

RegisterSettings :: \
	"$(PROGRAMFILES)\OpenVPN\config\eduVPN\Spool" \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe"
	reg.exe add "HKCR\org.eduvpn.app"                    /v "URL Protocol" /t REG_SZ /d ""                                                                     $(REG_FLAGS) > NUL
	reg.exe add "HKCR\org.eduvpn.app\DefaultIcon"        /ve               /t REG_SZ /d "$(MAKEDIR)\$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe,1"          $(REG_FLAGS) > NUL
	reg.exe add "HKCR\org.eduvpn.app\shell\open\command" /ve               /t REG_SZ /d "\"$(MAKEDIR)\$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe\" \"%1\"" $(REG_FLAGS) > NUL

"$(PROGRAMFILES)\OpenVPN\config" : "$(PROGRAMFILES)\OpenVPN"

"$(PROGRAMFILES)\OpenVPN\config\eduVPN" : "$(PROGRAMFILES)\OpenVPN\config"

"$(PROGRAMFILES)\OpenVPN" \
"$(PROGRAMFILES)\OpenVPN\config" \
"$(PROGRAMFILES)\OpenVPN\config\eduVPN" :
	if not exist $@ md $@

"$(PROGRAMFILES)\OpenVPN\config\eduVPN\Spool" : "$(PROGRAMFILES)\OpenVPN\config\eduVPN"
	if not exist $@ md $@
	echo y | cacls.exe $@ /S:"D:PAI(A;OICI;FA;;;SY)(A;OICI;FA;;;BA)(A;;0x100003;;;BU)(A;OIIO;FA;;;CO)" > NUL

UnregisterSettings ::
	-reg.exe delete "HKCR\org.eduvpn.app" $(REG_FLAGS) > NUL

RegisterShortcuts :: \
	"$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME).lnk"

UnregisterShortcuts ::
	-if exist "$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME).lnk" rd /s /q "$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME).lnk"


######################################################################
# Setup
######################################################################

Setup :: \
	SetupBuild \
	SetupMSI \
	SetupExe


######################################################################
# Shortcut creation
######################################################################

"$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME).lnk" : "$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe"
	cscript.exe "bin\MkLnk.wsf" //Nologo $@ "$(MAKEDIR)\$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe"


######################################################################
# Building
######################################################################

"$(OUTPUT_DIR)\Release\eduVPN.wixobj" : "eduVPN.wxs"
	"$(WIX)bin\wixcop.exe" $(WIX_WIXCOP_FLAGS) $**
	"$(WIX)bin\candle.exe" $(WIX_CANDLE_FLAGS) -deduVPN.VersionInformational="$(PRODUCT_VERSION_STR)" -out $@ $**

Clean ::
	-if exist "$(OUTPUT_DIR)\Release\eduVPN.wixobj" del /f /q "$(OUTPUT_DIR)\Release\eduVPN.wixobj"

"$(OUTPUT_DIR)\Release\OpenVPN.wixobj" : "OpenVPN.wxs"
	"$(WIX)bin\wixcop.exe" $(WIX_WIXCOP_FLAGS) $**
	"$(WIX)bin\candle.exe" $(WIX_CANDLE_FLAGS) -out $@ $**

Clean ::
	-if exist "$(OUTPUT_DIR)\Release\OpenVPN.wixobj"                 del /f /q "$(OUTPUT_DIR)\Release\OpenVPN.wixobj"
	-if exist "$(OUTPUT_DIR)\Release\openvpn-install-2.4.3-I601.exe" del /f /q "$(OUTPUT_DIR)\Release\openvpn-install-2.4.3-I601.exe"


######################################################################
# Configuration and platform specific rules
######################################################################

CFG=Debug
PLAT=x86
!INCLUDE "BuildCfgPlat.mak"
PLAT=x64
!INCLUDE "BuildCfgPlat.mak"

CFG=Release
PLAT=x86
!INCLUDE "BuildCfgPlat.mak"
PLAT=x64
!INCLUDE "BuildCfgPlat.mak"


######################################################################
# Locale-specific rules
######################################################################

LANG=en-US
!INCLUDE "BuildLang.mak"

LANG=sl
!INCLUDE "BuildLang.mak"
