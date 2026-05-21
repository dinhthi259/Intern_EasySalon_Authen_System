using System.Text.RegularExpressions;

public class IntentDetectionService
    : IIntentDetectionService
{
    private readonly List<string> _keywords =
[
    // =========================
    // MÁY LẠNH - ĐIỀU HÒA
    // =========================
    "máy lạnh",
    "điều hòa",
    "air conditioner",
    "inverter",
    "non inverter",
    "1 hp",
    "1.5 hp",
    "2 hp",
    "2.5 hp",
    "3 hp",
    "9000 btu",
    "12000 btu",
    "18000 btu",
    "24000 btu",
    "làm lạnh",
    "tiết kiệm điện",
    "lọc bụi",
    "khử mùi",
    "kháng khuẩn",
    "gas r32",
    "gas r410",

    // =========================
    // TV - TIVI
    // =========================
    "tivi",
    "tv",
    "smart tv",
    "google tv",
    "android tv",
    "oled",
    "qled",
    "mini led",
    "4k",
    "8k",
    "uhd",
    "full hd",
    "hd",
    "remote",
    "màn hình",
    "màn ảnh",
    "55 inch",
    "65 inch",
    "32 inch",
    "43 inch",
    "50 inch",

    // =========================
    // TỦ LẠNH
    // =========================
    "tủ lạnh",
    "fridge",
    "multi door",
    "side by side",
    "ngăn đá trên",
    "ngăn đá dưới",
    "inverter",
    "làm đá",
    "khử mùi",
    "lấy nước ngoài",
    "door cooling",
    "hygiene fresh",

    // =========================
    // MÁY GIẶT
    // =========================
    "máy giặt",
    "washer",
    "máy giặt sấy",
    "giặt sấy",
    "cửa trước",
    "cửa trên",
    "lồng ngang",
    "lồng đứng",
    "ecobubble",
    "ai dd",
    "turbo wash",
    "steam",
    "inverter",
    "8 kg",
    "9 kg",
    "10 kg",
    "11 kg",
    "15 kg",

    // =========================
    // QUẠT
    // =========================
    "quạt",
    "quạt điện",
    "quạt đứng",
    "quạt treo",
    "quạt hộp",
    "quạt điều hòa",
    "quạt hơi nước",
    "quạt trần",
    "quạt mini",
    "quạt sạc",
    "fan",

    // =========================
    // MÁY LỌC NƯỚC
    // =========================
    "máy lọc nước",
    "lọc nước",
    "water purifier",
    "ro",
    "nanoe",
    "karofi",
    "kangaroo",
    "sunhouse",
    "aosmith",
    "máy nước nóng lạnh",
    "nóng lạnh",
    "diệt khuẩn",
    "uv",
    "hydrogen",

    // =========================
    // NỒI CƠM ĐIỆN
    // =========================
    "nồi cơm",
    "nồi cơm điện",
    "rice cooker",
    "cao tần",
    "áp suất",
    "điện tử",
    "mini",
    "1 lít",
    "1.8 lít",
    "2 lít",
    "giữ ấm",
    "nấu nhanh",

    // =========================
    // CAMERA
    // =========================
    "camera",
    "camera wifi",
    "camera an ninh",
    "camera ip",
    "camera ngoài trời",
    "camera trong nhà",
    "camera xoay",
    "hồng ngoại",
    "ghi hình",
    "quan sát",
    "giám sát",
    "ezviz",
    "imou",
    "hikvision",
    "kbvision",
    "360 độ",

    // =========================
    // HÀNH VI MUA HÀNG
    // =========================
    "giá",
    "bao nhiêu",
    "bao nhieu",
    "khuyến mãi",
    "giảm giá",
    "sale",
    "flash sale",
    "mua",
    "đặt hàng",
    "trả góp",
    "trả góp 0%",
    "rẻ",
    "cao cấp",
    "chính hãng",
    "bảo hành",
    "ship",
    "giao hàng",

    // =========================
    // NHU CẦU NGƯỜI DÙNG
    // =========================
    "gia đình",
    "văn phòng",
    "phòng ngủ",
    "phòng khách",
    "phòng nhỏ",
    "phòng lớn",
    "tiết kiệm điện",
    "êm",
    "bền",
    "ít ồn",
    "công nghệ",
    "wifi",
    "thông minh",
    "ai",
    "smart",

    // =========================
    // THƯƠNG HIỆU
    // =========================
    "lg",
    "samsung",
    "sony",
    "xiaomi",
    "sharp",
    "toshiba",
    "panasonic",
    "daikin",
    "casper",
    "midea",
    "comfee",
    "aqua",
    "electrolux",
    "hisense",
    "beko",
    "bosch",
    "tcl",
    "dreame",
    "senko",
    "asia",
    "mitsubishi",
    "sunhouse",
    "kangaroo",
    "lifan",
    "benny",
    "hikvision",
    "imou",
    "ezviz",

    // =========================
    // TỔNG QUÁT
    // =========================
    "sản phẩm",
    "điện máy",
    "điện tử",
    "gia dụng",
    "thiết bị",
    "thiết bị điện",
    "thiết bị gia dụng",
    "đồ điện"
];

    public bool IsProductRelated(string message)
    {
        message = message.ToLower().Trim();

        return _keywords.Any(k =>
    Regex.IsMatch(message, $@"\b{Regex.Escape(k)}\b"));
    }
}