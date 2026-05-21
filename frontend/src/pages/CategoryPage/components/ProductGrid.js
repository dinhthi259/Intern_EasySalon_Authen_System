import classNames from "classnames/bind";
import styles from "./ProductGrid.module.scss";
import { useNavigate } from "react-router-dom";
import { FaHeart, FaShoppingCart, FaStar } from "react-icons/fa";
import { addToCart } from "../../../api/CartApi";
import { notifyError, notifySuccess } from "../../../components/Nofitication";

const cx = classNames.bind(styles);

export default function ProductGrid({ products }) {
  const navigate = useNavigate();
  const handleProduct = (slug) => {
    console.log(slug);
    navigate(`/product/${slug}`);
  };

  const handleAddToCart = async (id) => {
      try {
        // Logic to add product to cart
        const res = await addToCart(id, 1);
        if (res) {
          notifySuccess("Thêm vào giỏ hàng thành công!");
        }
      } catch (error) {
        notifyError("Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng!");
      }
    };
  return (
    <div className={cx("grid")}>
      {products.slice(0, 8).map((p) => (
        <div
          key={p.id}
          className={cx("card")}
          onClick={() => handleProduct(p.slug)}
        >
          {/* DISCOUNT */}
          {p.discountPercent && (
            <div className={cx("badge")}>-{p.discountPercent}%</div>
          )}

          {/* WISHLIST */}
          <button
            className={cx("wishlist")}
            onClick={(e) => e.stopPropagation()}
          >
            <FaHeart />
          </button>

          {/* IMAGE */}
          <div className={cx("imageWrap")}>
            <img src={p.thumbnail} alt={p.name} />
          </div>

          {/* INFO */}
          <div className={cx("content")}>

            <h3 className={cx("name")}>{p.name}</h3>

            {/* RATING */}
            <div className={cx("rating")}>
              <FaStar />

              <span>{p.rating || 5}</span>

              <small>({p.reviewCount || 0})</small>
            </div>

            {/* PRICE */}
            <div className={cx("priceBox")}>
              <span className={cx("salePrice")}>
                {(p.discountPrice || p.price)?.toLocaleString()}đ
              </span>

              {p.discountPrice && (
                <span className={cx("originPrice")}>
                  {p.price?.toLocaleString()}đ
                </span>
              )}
            </div>

            {/* BUTTON */}
            <button
              className={cx("addCart")}
              onClick={(e) => {
                e.stopPropagation();
                handleAddToCart(p.id);
              }}
            >
              <FaShoppingCart />
              Thêm vào giỏ
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
