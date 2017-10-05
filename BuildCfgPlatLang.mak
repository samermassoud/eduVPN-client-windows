#
#   eduVPN - End-user friendly VPN
#
#   Copyright: 2017, The Commons Conservancy eduVPN Programme
#   SPDX-License-Identifier: GPL-3.0+
#

!IF "$(LANG)" == "en-US"
!INCLUDE "BuildStrings.mak"
WIX_LOC_FILE=eduVPN.wxl
!ELSE
!INCLUDE "BuildStrings.$(LANG).mak"
WIX_LOC_FILE=eduVPN.$(LANG).wxl
!ENDIF


######################################################################
# Setup
######################################################################

!IF "$(CFG)" == "Release"
SetupMSI :: \
	"$(SETUP_DIR)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).msi"
!ENDIF


######################################################################
# Building
######################################################################

"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).msi" : \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPNClient.wixobj" \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduEd25519.dll.wixobj" \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduJSON.dll.wixobj" \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduOAuth.dll.wixobj" \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduOpenVPN.dll.wixobj" \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.dll.wixobj" \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe.wixobj"
	-if exist $@ del /f /q $@
	"$(WIX)bin\light.exe" $(WIX_LIGHT_FLAGS) -cultures:$(LANG) -loc "$(WIX_LOC_FILE)" -out "$(@:"=).tmp" $**
!IFDEF MANIFESTCERTIFICATETHUMBPRINT
	signtool.exe sign /sha1 "$(MANIFESTCERTIFICATETHUMBPRINT)" /t "$(MANIFESTTIMESTAMPURL)" /d "$(SIGNTOOL_DESC)" /q "$(@:"=).tmp"
!ENDIF
	move /y "$(@:"=).tmp" $@ > NUL

Clean ::
	-if exist "$(OUTPUT_DIR)\$(CFG)\$(PLAT)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).msi" del /f /q "$(OUTPUT_DIR)\$(CFG)\$(PLAT)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).msi"

!IF "$(LANG)" == "en-US"
# The en-US localization serves as the base. Therefore, it does not require the diff MST.
!ELSE
"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).mst" : \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\$(SETUP_NAME)_$(SETUP_TARGET)_en-US.msi" \
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).msi"
	cscript.exe $(CSCRIPT_FLAGS) "bin\MSI.wsf" //Job:MakeMST $** $@

Clean ::
	-if exist "$(OUTPUT_DIR)\$(CFG)\$(PLAT)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).mst" del /f /q "$(OUTPUT_DIR)\$(CFG)\$(PLAT)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).mst"
!ENDIF

"$(SETUP_DIR)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).msi" : "$(OUTPUT_DIR)\$(CFG)\$(PLAT)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).msi"
	copy /y $** $@ > NUL

Clean ::
	-if exist "$(SETUP_DIR)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).msi" del /f /q "$(SETUP_DIR)\$(SETUP_NAME)_$(SETUP_TARGET)_$(LANG).msi"
