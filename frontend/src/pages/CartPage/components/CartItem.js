import styles from "../Cart.module.scss";
import classNames from "classnames/bind";
import { notifyWarning } from "../../../components/Nofitication";
import { FaTrashCan, FaPlus, FaMinus } from "react-icons/fa6";

const cx = classNames.bind(styles);

function CartItem({ item, onUpdate, onRemove, checked, onCheck }) {
  const price = item.discountPrice || item.price;
  const availableQuantity = Math.max(0, item.stockQuantity - 5);

  return (
    <div className={cx("cart-item")}>
      <input
        type="checkbox"
        checked={checked}
        onChange={(e) => onCheck(item.productId, e.target.checked)}
      />

      <img src={item.thumbnail} alt="" className={cx("thumb")} />

      <div className={cx("info")}>
        <h4>{item.name}</h4>

        <div className={cx("price")}>
          {item.discountPrice && (
            <span className={cx("old")}>{item.price.toLocaleString()}đ</span>
          )}
          <span className={cx("new")}>{price.toLocaleString()}đ</span>
        </div>
      </div>

      <div className={cx("quantity-wrapper")}>
        <div className={cx("quantity")}>
          <button
            className={cx("btn")}
            onClick={() => {
              onUpdate(item.productId, item.quantity - 1);
            }}
            disabled={item.quantity <= 1}
          >
            −
          </button>

          <input
            type="number"
            value={item.quantity}
            min={1}
            max={item.stockQuantity}
            onChange={(e) => {
              const value = Number(e.target.value);

              if (value > availableQuantity) {
                notifyWarning("không thể chọn quá số lượng tồn kho");

                onUpdate(item.productId, availableQuantity);
                return;
              }

              onUpdate(item.productId, Math.max(1, value));
            }}
          />

          <button
            className={cx("btn")}
            onClick={() => {
              if (item.quantity > availableQuantity) {
                notifyWarning("không thể chọn quá số lượng tồn kho");
                onUpdate(item.productId, availableQuantity);
                return;
              }

              onUpdate(item.productId, item.quantity + 1);
            }}
          >
            +
          </button>
        </div>

        <p className={cx("stock")}>Còn: {availableQuantity} sản phẩm</p>
      </div>
      <button className={cx("remove")} onClick={() => onRemove(item.productId)}>
        <FaTrashCan />
      </button>
    </div>
  );
}

export default CartItem;
