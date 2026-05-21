import classNames from "classnames/bind";
import styles from "../ProductDetail.module.scss";
import { useEffect, useState } from "react";
import { addToCart } from "../../../api/CartApi";
import {
  notifyError,
  notifySuccess,
  notifyWarning,
} from "../../../components/Nofitication";
import { useNavigate } from "react-router-dom";

const cx = classNames.bind(styles);
const ONE_HOUR = 60 * 60;

export default function ProductInfo({ product }) {
  const {
    name,
    brand,
    price,
    discountPrice,
    averageRating,
    totalReviews,
    discountPercent,
    saleMoney,
  } = product;

  const navigate = useNavigate();
  const availableQuantity = Math.max(0, (product?.stockQuantity || 0) - 5);
  const isOutOfStock = availableQuantity <= 0;

  const [timeLeft, setTimeLeft] = useState(ONE_HOUR);

  useEffect(() => {
    const timer = setInterval(() => {
      setTimeLeft((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  const formatTime = (time) => {
    const hours = String(Math.floor(time / 3600)).padStart(2, "0");
    const minutes = String(Math.floor((time % 3600) / 60)).padStart(2, "0");
    const seconds = String(time % 60).padStart(2, "0");

    return { hours, minutes, seconds };
  };

  const time = formatTime(timeLeft);

  const handleBuyNow = () => {
    if (!product?.id) return;

    if (isOutOfStock) {
      notifyWarning("Sản phẩm này hiện hết hàng, vui lòng quay lại sau!");
      return;
    }

    navigate(`/checkout?type=buy-now&productId=${product.id}&quantity=1`);
  };

  const handleAddToCart = async () => {
    if (isOutOfStock) {
      notifyWarning("Sản phẩm này hiện hết hàng, vui lòng quay lại sau!");
      return;
    }

    try {
      const res = await addToCart(product.id, 1);

      if (res) {
        notifySuccess("Thêm vào giỏ hàng thành công!");
      }
    } catch (error) {
      notifyError("Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng!");
    }
  };

  return (
    <div className={cx("info")}>
      <p className={cx("name")}>{name}</p>
      <div className={cx("rating-brand")}>
        <div className={cx("rating")}>
          <span>{`⭐ ${averageRating} (${totalReviews} reviews)`}</span>
        </div>

        <p className={cx("brand")}>{brand}</p>

        <span
          className={cx("stock-status", availableQuantity > 0 ? "in-stock" : "out-stock")}
        >
          {availableQuantity > 0 ? "Còn hàng" : "Hết hàng"}
        </span>
      </div>

      <img src="https://cdn2.fptshop.com.vn/unsafe/1080x0/filters:format(webp):quality(75)/507x85_phu_kien_1_cd87d1e516.png"></img>

      <div className={cx("price-box")}>
        <div className={cx("price-main")}>
          <span className={cx("current-price")}>
            {(discountPrice || 0).toLocaleString()}đ
          </span>

          <div className={cx("old-price-row")}>
            <span className={cx("old-price")}>
              {(price || 0).toLocaleString()}đ
            </span>
            <span className={cx("discount-percent")}>- {discountPercent}%</span>
          </div>

          <div className={cx("reward")}>🎁 +89 Điểm thưởng</div>
        </div>

        <p className={cx("promo-label")}>Chọn 1 trong các khuyến mãi sau:</p>

        {/* Flash sale */}
        <div className={cx("flash-sale")}>
          <div className={cx("flash-header")}>
            ⏰ GIỜ VÀNG GIÁ SỐC 🔥
            <span className={cx("sold")}>Đã bán 1/10 suất</span>
          </div>

          <div className={cx("flash-body")}>
            <div>
              <p>Giảm ngay</p>
              <strong>{(saleMoney || 0).toLocaleString()}đ ⚡</strong>
            </div>

            <div className={cx("countdown")}>
              <span>{time.hours}</span>:<span>{time.minutes}</span>:
              <span>{time.seconds}</span>
            </div>
          </div>
        </div>
      </div>

      <div className={cx("actions")}>
        <button className={cx("buy-btn")} onClick={handleBuyNow}>
          Mua ngay
        </button>
        <button className={cx("cart-btn")} onClick={handleAddToCart}>
          🛒
        </button>
      </div>
    </div>
  );
}
