var fileName = input.value.FileName.toLowerCase();
var description = input.value.Description.toLowerCase();
var model = input.value.ModelName.toLowerCase();
var genericKey = fileName + "-" + model;

var description_contains = function (sub) {

    return description.indexOf(sub) >= 0;
};

fileName = fileName.replace(/.zip$/gi, "");
fileName = fileName.replace("_D_", "");
fileName = fileName.replace(/\_/gi, "");
fileName = fileName.replace(/\-/gi, "");
fileName = fileName.replace(/[0-9]/gi, "");
fileName = fileName.trim();

////
// by description

if (description_contains("11ad ")) {
    return "(Exclude) WiGig/802.11ad";
}

if (description_contains("intel vga ") ||
    description_contains("intel graphics driver ") ||
    fileName.indexOf("ivga") >= 0) {
    return "(Exclude) Intel VGA";
}

if (description_contains("8821ce") ||
    description_contains("realtek combo card") ||
    description_contains("realtek bt driver") ||
    description_contains("realtek wlan") ||
    description_contains("8723ae")) {
    return "(Exclude) Realtek WLAN";
}

if (description_contains("airplane driver") ||
    description_contains("airplane mode ap") ||
    description_contains("airplan mode") ||
    description_contains("airplane mode driver") ||
    description_contains("airplane ap") ||
    description_contains("airplanemode driver")) {
    return "Airplane Mode";
}

if (fileName == "anx" ||
    description_contains("anx")) {
    return "ANX USB Driver";
}

if (description_contains("audio driver") ||
    description_contains("audi driver") ||
    description_contains("audio version 6.0") ||
    fileName == "audio") {
    return "Audio Driver (Realtek)";
}

if (description_contains("control center") &&
    description_contains("5.0001.")) {
    return "Control Center 1";
}

if (description_contains("control center") &&
    description_contains("5.0000.")) {
    return "Control Center 1";
}

if (description_contains("control center") &&
    (
        description_contains("ver 1.") ||
        description_contains("version 1.") ||
        description_contains("version. 1.")
    )) {
    return "Control Center 1";
}

if (description_contains("hotkey") &&
    description_contains("5.0001.")) {
    return "Control Center 1";
}

if (description_contains("hotkey driver 3.20.35")) {
    return "Control Center 1";
}

if (
    description_contains("control center") &&
        (description_contains("2.0") ||
        description_contains("version 2."))) {

    if (description_contains("center3.0") ||
        description_contains("center 3.0")) {

        return "Control Center 3";
    }

    return "Control Center 2";
}

if (description_contains("control center 3.0") ||
    description_contains("control center3.0") ||
    description_contains("cc3.0") ||
    description_contains("cc30") ||
    description_contains("control center ap 3") ||
    description_contains("control center version 3.") ||
    description_contains("control center driver version 3.") ||
    description_contains("hotkey v3.42") ||
    (description_contains("control center") && description_contains("ver. 3.")) ||
    (description_contains("control center") && description_contains("3.55")) ||
    (description_contains("controlcenter") && description_contains("3.55")) ||
    (description_contains("control center ap") && description_contains("version 3.01")) ||
    (description_contains("c_center") && description_contains("version 3."))) {
    return "Control Center 3";
}

if (description_contains("card reader") ||
    description_contains("cardreader")) {

    if (model == "nv4xmj_k(-d)") {
        return "Card Reader (NV4x 11th Gen)"
    }

    return "Card Reader";
}

if (description_contains("chipset ") ||
    fileName == "chipset") {
    return "Intel Chipset INF";
}

if (description_contains("fingerprint ") ||
    description_contains("finger print ") ||
    description_contains("fingerprnt")) {

    return "Synaptics Fingerprint";
}

if (description_contains("gamma f/p")) {

    return "Synaptics Fingerprint (Gamma)";
}

if (description_contains("dptf ")) {
    return "Intel DPTF";
}

if (description_contains("dtt ") ||
    fileName == "dttg") {
    return "Intel Dynamic Tuning";
}

if (description_contains("intel mei ") ||
    description_contains("intel me ") ||
    description_contains("intel management engine interface ") ||
    description_contains("intel(r) management engine interface ") ||
    description_contains("mei driver") ||
    description_contains("ime driver") ||
    fileName == "mei") {
    return "Intel Management Engine Interface";
}

if (description_contains("intel wlan ") ||
    description_contains("intel bt ") ||
    description_contains("intel combo card ") ||
    description_contains("intel combo bluetooth ") ||
    (description_contains("intel") && description_contains("wlan ")) ||
    (description_contains("combo card bt") && description_contains("intel")) ||
    (description_contains("combo card") && description_contains("intel")) ||
    fileName == "icombo" ||
    fileName == "icombod" ||
    fileName == "intelbt" ||
    fileName == "intelwlan" ||
    fileName == "wlan"
) {
    return "(Exclude) Intel WiFi and Bluetooth";
}

if (description_contains("intel rapid storage ") ||
    description_contains("intel rst ") ||
    description_contains("irst") ||
    fileName == "irst") {
    return "Intel Rapid Storage Technology";
}

if (fileName == "irstart") {
    return "(Exclude) Intel Rapid Start Technology";
}

if (description_contains("serial io ") ||
    fileName == "serialio" ||
    fileName == "serialiod") {
    return "Intel Serial IO";
}

if (fileName == "sgx") {
    return "Intel SGX (Software Guard Extension)";
}

if (description_contains("intel speed shift") ||
    fileName == "speedshift" ||
    fileName == "speedshiftd") {
    return "Intel SpeedShift Driver";
}

if (description_contains("tbt ") ||
    description_contains("thunderbolt ") ) {
    return "Intel Thunderbolt Driver";
}

if (fileName == "txe") {
    return "Intel TXE";
}
if (fileName == "intelwidi") {
    return "(Exclude) Intel WiDi";
}

if (description_contains("gna ")) {
    return "Intel GNA";
}

if (description_contains("hid filter ") ||
    description_contains("hidfilter ") ||
    description_contains("hid driver")) {
    return "Intel HID Filter";
}

if (description_contains("killer ") ||
    description_contains("killer") ||
    fileName == "kcombo") {
    return "(Exclude) Killer Wireless";
}

if (description_contains("nvidia vga ") ||
    description_contains("nvidia graphics ") ||
    fileName == "nvvga" ||
    fileName == "nvidiavga") {
    return "(Exclude) Nvidia VGA";
}

if (fileName == "lan" ||
    fileName == "land" ||
    fileName == "lanw") {

    return "Realtek LAN";
}

if (fileName == "nsb" &&
    model == "n15xrd1/n17xrd1") {
    return "Sound Blaster Cinema 2";
}

if (description_contains("cinema3 ap")) {
    return "Sound Blaster Cinema 3";
}

if (description_contains("cinema5")) {
    return "Sound Blaster Cinema 5";
}

if (description_contains("xfi mb5") ||
    description_contains("x-fi mb5") ||
    description_contains("sound blaster mb5") ||
    description_contains("sbx5") ||
    description_contains("xi-fi mb5") ||
    description_contains("x-fi5")) {
    return "Sound Blaster X-Fi MB5";
}

if (description_contains("sound blaster cinema2 ap") ||
    description_contains("sound blaster cinema 2 ap") ||
    description_contains("sound blaster 2 cinema ap")) {
    return "Sound Blaster Cinema 2";
}

if (description_contains("creative atlas")) {
    return "Sound Blaster (Next Gen)";
}
if (description_contains("creative driver") &&
    description_contains("2.")) {
    return "Sound Blaster (Next Gen)";
}

if (description_contains("sbx ") &&
    description_contains("2.")) {
    return "Sound Blaster (Next Gen)";
}

if (description_contains("touchpad") ||
    description_contains("touch pad") ||
    fileName == "touchpad") {
    return "Synaptics Touchpad";
}

if (description_contains("usi combo") ||
    description_contains("usi bt") ||
    description_contains("usi wlan") ||
    description_contains("usi driver") ||
    description_contains("usi ") ||
    fileName == "usiwigig" ||
    fileName == "usiwigiga") {
    return "(Exclude) USI Driver";
}

if (fileName == "vga" ||
    fileName == "vgad" ||
    fileName == "vgan" ||
    fileName == "vgabeta") {
    return "(Exclude) Intel/Nvidia Graphic";
}
if (fileName == "preinstallkit" ||
    description_contains("audio preinstall ") ||
    description_contains(" preinstall ")) {

    return "Windows 10/11 DCH Driver Pre-Install Kit";
}

if (description.indexOf("sbx-fi mb3") >= 0) {

    return "Sound Blaster X-Fi MB3";
}

// --- Specific Fix

if (genericKey == "raid_0708.zip-n960kp_kr") {

    return "Intel Rapid Storage Technology";
}
input.log(fileName);

return "(Ungrouped) " + genericKey;
