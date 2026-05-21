using System.Text;
using System.Text.RegularExpressions;

public class PromptBuilderService
    : IPromptBuilderService
{
    public string BuildProductContext(
        List<Product> products,
        string userMessage,
        List<Product>? recommendedProducts = null,
        string? memories = null
    )
    {
        var sb = new StringBuilder();

        // =====================================
        // SYSTEM PROMPT
        // =====================================

        sb.AppendLine("""
Bạn là AI assistant cho website thương mại điện tử điện máy.

NHIỆM VỤ:
- Tư vấn sản phẩm điện máy
- Gợi ý sản phẩm phù hợp
- Hỗ trợ mua hàng
- Trả lời ngắn gọn, thân thiện
- Chỉ sử dụng dữ liệu sản phẩm được cung cấp

QUY TẮC BẮT BUỘC:
- CHỈ được trả lời dựa trên dữ liệu được cung cấp
- KHÔNG được tự bịa sản phẩm
- KHÔNG được suy luận ngoài dữ liệu
- KHÔNG trả lời chủ đề ngoài điện máy / ecommerce
- KHÔNG trả Markdown
- KHÔNG trả HTML
- KHÔNG giải thích thêm ngoài JSON
- Kết quả trả về PHẢI là JSON hợp lệ

FORMAT TRẢ VỀ BẮT BUỘC:

1. Nếu người dùng hỏi gợi ý / danh sách sản phẩm:
{
  "type": "product_cards",
  "message": "Nội dung trả lời ngắn gọn",
  "data": [
    {
      "id": 0,
      "name": "Tên sản phẩm",
      "thumbnail": "URL ảnh",
      "price": 0,
      "brand": "Thương hiệu",
      "rating": 0,
      "description": "Mô tả ngắn",
      "highlights": [
        "Điểm nổi bật 1",
        "Điểm nổi bật 2"
      ]
    }
  ]
}

2. Nếu người dùng hỏi thông số kỹ thuật / cấu hình / chi tiết sản phẩm:
{
  "type": "spec_table",
  "message": "Thông số kỹ thuật của sản phẩm",
  "data": {
    "id": 0,
    "name": "Tên sản phẩm",
    "specs": [
      {
        "label": "Tên thông số",
        "value": "Giá trị"
      }
    ]
  }
}

3. Nếu người dùng hỏi so sánh sản phẩm:
{
  "type": "compare_table",
  "message": "Bảng so sánh sản phẩm",
  "data": [
    {
      "id": 0,
      "name": "Tên sản phẩm",
      "price": 0,
      "brand": "Thương hiệu",
      "specs": [
        {
          "label": "Tên thông số",
          "value": "Giá trị"
        }
      ]
    }
  ]
}

4. Nếu không tìm thấy sản phẩm phù hợp:
{
  "type": "text",
  "message": "Hiện chưa có sản phẩm phù hợp.",
  "data": null
}

5. Nếu câu hỏi không liên quan:
{
  "type": "text",
  "message": "Tôi chỉ hỗ trợ tư vấn sản phẩm điện máy trên hệ thống.",
  "data": null
}

QUAN TRỌNG:
- Trường price phải là số, không kèm VNĐ
- Trường id phải lấy đúng ID sản phẩm được cung cấp
- Không thêm sản phẩm ngoài danh sách
- Không thêm thông số nếu không có trong specifications
- Chỉ trả về một JSON object duy nhất
- JSON phải hợp lệ tuyệt đối
- Không được trả JSON bị thiếu dấu }
- Không được cắt giữa chừng
- Không được dùng markdown ```json
- Nếu dữ liệu quá dài hãy rút gọn specifications
""");

        // =====================================
        // USER MEMORY
        // =====================================

        if (memories != null &&
            memories.Any())
        {
            sb.AppendLine();
            sb.AppendLine(
                "===== THÔNG TIN HÀNH VI NGƯỜI DÙNG =====");

            foreach (var memory in memories)
            {
                sb.AppendLine($"- {memory}");
            }
        }

        // =====================================
        // MAIN PRODUCTS
        // =====================================

        sb.AppendLine();
        sb.AppendLine(
            "===== SẢN PHẨM TÌM THẤY =====");

        foreach (var p in products.Take(3))
        {
            AppendProduct(sb, p);
        }

        bool isSpecQuestion =
            userMessage.Contains("thông số") ||
            userMessage.Contains("cấu hình") ||
            userMessage.Contains("chi tiết");

        bool isCompareQuestion =
            userMessage.Contains("so sánh");

        // =====================================
        // RECOMMENDED PRODUCTS
        // =====================================

        if (recommendedProducts != null &&
            recommendedProducts.Any())
        {
            sb.AppendLine();
            sb.AppendLine(
                "===== SẢN PHẨM ĐỀ XUẤT THÊM =====");

            foreach (var p in recommendedProducts)
            {
                AppendProduct(sb, p, includeSpecs: isSpecQuestion || isCompareQuestion);
            }
        }

        // =====================================
        // USER QUESTION
        // =====================================

        sb.AppendLine();
        sb.AppendLine(
            "===== CÂU HỎI KHÁCH HÀNG =====");

        sb.AppendLine(userMessage);

        return sb.ToString();
    }



    // =====================================
    // APPEND PRODUCT
    // =====================================

    private void AppendProduct(
    StringBuilder sb,
    Product p,
    bool includeSpecs = false)
    {
        sb.AppendLine($"""
PRODUCT_ID: {p.Id}
PRODUCT_NAME: {p.Name}
PRODUCT_THUMBNAIL: {p.Thumbnail}
PRICE: {p.DiscountPrice ?? p.Price}
BRAND: {p.Brand}
RATING: {p.RatingAvg}
DESCRIPTION:{Truncate(CleanHtml(p.Description), 60)}
""");

        if (includeSpecs &&
    p.Specifications != null &&
    p.Specifications.Any())
        {
            sb.AppendLine("Thông số:");

            foreach (var spec in p.Specifications.Take(10))
            {
                sb.AppendLine(
                    $"- {spec.SpecName}: {spec.SpecValue}"
                );
            }
        }

        sb.AppendLine("------------------------");
    }

    // =====================================
    // REMOVE HTML
    // =====================================

    private string CleanHtml(
        string html)
    {
        if (string.IsNullOrEmpty(html))
            return "";

        return Regex.Replace(
            html,
            "<.*?>",
            ""
        );
    }
    private string Truncate(string text, int max)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text.Length <= max
            ? text
            : text.Substring(0, max) + "...";
    }
}