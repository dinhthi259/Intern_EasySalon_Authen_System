import classNames from "classnames/bind";
import styles from "../../MyOrderPage/components/OrderTabs.module.scss";
import { useState } from "react";

const cx = classNames.bind(styles);

const tabs = [
  { label: "Tất cả", value: "" },
  { label: "Chờ xác nhận", value: "Chờ xác nhận" },
  { label: "Chờ lấy hàng", value: "Chờ lấy hàng" },
  { label: "Đang giao", value: "Đang giao" },
  { label: "Hoàn tất", value: "Hoàn tất" },
  { label: "Đã hủy", value: "Đã hủy" },
  { label: "Chờ hoàn tiền", value: "Chờ hoàn tiền" },
];

export default function OrderTabs({ onChange, counts = {} }) {
  const [active, setActive] = useState("");

  const handleClick = (value) => {
    setActive(value);
    onChange(value);
  };

  return (
    <div className={cx("tabs")}>
      {tabs.map((t) => (
        <div
          key={t.value}
          className={cx("tab", { active: active === t.value })}
          onClick={() => handleClick(t.value)}
        >
          {t.label}
          <span className={cx("count")}>
            {t.value === ""
              ? Object.values(counts).reduce((a, b) => a + b, 0)
              : counts[t.value] || 0}
          </span>
        </div>
      ))}
    </div>
  );
}
