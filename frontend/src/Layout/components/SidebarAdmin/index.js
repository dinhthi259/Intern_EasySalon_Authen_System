import { useState } from "react";
import { NavLink } from "react-router-dom";
import classNames from "classnames/bind";
import styles from "./SidebarAdmin.module.scss";
import { RiLogoutBoxLine } from "react-icons/ri";
import {
  FaBox,
  FaList,
  FaShoppingCart,
  FaWarehouse,
  FaUsers,
  FaChartBar,
  FaTruck,
  FaFileInvoice,
  FaFileAlt,
  FaChevronDown,
  FaChevronRight,
} from "react-icons/fa";
import { BiSolidCommentDetail } from "react-icons/bi";

const cx = classNames.bind(styles);

export default function SidebarAdmin() {
  const [collapsed, setCollapsed] = useState(false);
  const [openDocumentMenu, setOpenDocumentMenu] = useState(false);

  const menu = [
    { name: "Quản lý danh mục", path: "/admin/category", icon: <FaList /> },
    { name: "Quản lý sản phẩm", path: "/admin/products", icon: <FaBox /> },
    { name: "Quản lý đơn hàng", path: "/admin/orders", icon: <FaShoppingCart /> },
    { name: "Quản lý kho", path: "/admin/inventory", icon: <FaWarehouse /> },
    { name: "Quản lý khách hàng", path: "/admin/customers", icon: <FaUsers /> },
    { name: "Quản lý nhà cung cấp", path: "/admin/suppliers", icon: <FaTruck /> },
    { name: "Quản lý đánh giá", path: "/admin/review", icon: <BiSolidCommentDetail /> },
    { name: "Báo cáo thống kê", path: "/admin/dashboard", icon: <FaChartBar /> },
  ];

  return (
    <div className={cx("sidebar", { collapsed })}>
      <div className={cx("logo")}>
        <img src="https://www.techai.ai/logo.png" alt="logo" height="40" />
      </div>

      <div className={cx("menu")}>
        {menu.map((item, index) => (
          <NavLink
            key={index}
            to={item.path}
            className={({ isActive }) => cx("item", { active: isActive })}
          >
            <span className={cx("icon")}>{item.icon}</span>
            {!collapsed && <span className={cx("text")}>{item.name}</span>}
          </NavLink>
        ))}

        <div
          className={cx("item", "parentItem")}
          onClick={() => setOpenDocumentMenu(!openDocumentMenu)}
        >
          <span className={cx("icon")}>
            <FaFileAlt />
          </span>

          {!collapsed && (
            <>
              <span className={cx("text")}>Quản lý giấy tờ</span>
              <span className={cx("arrow")}>
                {openDocumentMenu ? <FaChevronDown /> : <FaChevronRight />}
              </span>
            </>
          )}
        </div>

        {openDocumentMenu && !collapsed && (
          <div className={cx("submenu")}>
            <NavLink
              to="/admin/documents/invoices"
              className={({ isActive }) => cx("subItem", { active: isActive })}
            >
              <FaFileInvoice />
              <span>Quản lý hóa đơn</span>
            </NavLink>

            <NavLink
              to="/admin/documents/tax-declarations"
              className={({ isActive }) => cx("subItem", { active: isActive })}
            >
              <FaFileAlt />
              <span>Quản lý tờ kê khai thuế</span>
            </NavLink>

            <NavLink
              to="/admin/documents/warehouse-slips"
              className={({ isActive }) => cx("subItem", { active: isActive })}
            >
              <FaWarehouse />
              <span>Quản lý phiếu kho</span>
            </NavLink>
          </div>
        )}

        <NavLink
          to="/"
          className={({ isActive }) => cx("item", { active: isActive })}
        >
          <span className={cx("icon")}>
            <RiLogoutBoxLine />
          </span>
          {!collapsed && <span className={cx("text")}>Về trang chủ</span>}
        </NavLink>
      </div>
    </div>
  );
}