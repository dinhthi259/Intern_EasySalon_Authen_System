import classNames from "classnames/bind";
import styles from "./SidebarSettings.module.scss";
import { NavLink } from "react-router-dom";
import { useEffect, useState } from "react";
import { getCurrentUser } from "../../../api/UserApi";
import { getProfile } from "../../../api/ProfileApi";
import { IoPersonOutline } from "react-icons/io5";

const cx = classNames.bind(styles);

export default function SidebarSettings() {
  const [user, setUser] = useState({
    fullName: "",
    email: "",
  });
  const [profile, setProfile] = useState(null);

  useEffect(() => {
    getCurrentUser()
      .then(res => setUser(res.data))
      .catch(err => console.error(err));

    getProfile()
      .then(res => setProfile(res.data))
      .catch(err => console.error(err));
  }, []);

  const maskEmail = (email) => {
    if (!email) return "";
    const [name, domain] = email.split("@");
    const visible = name.slice(0, 4);
    const masked = "*".repeat(Math.max(name.length - 4, 0));
    return `${visible}${masked}@${domain}`;
  };

  return (
    <div className={cx("sidebar")}>

      {/* USER INFO */}
      <div className={cx("user")}>
        <div className={cx("avatar")}>
          {profile?.avatar ? (
            <img src={profile.avatar} alt="avatar" />
          ) : (
            <IoPersonOutline />
          )}
        </div>

        <div className={cx("info")}>
          <div className={cx("name")}>{user.fullName}</div>
          <div className={cx("email")}>
            {maskEmail(user.email)}
          </div>
        </div>
      </div>

      {/* MENU */}
      <div className={cx("menu")}>
        <NavLink to="/profile" className={({ isActive }) => cx("item", { active: isActive })}>
          Tổng quan
        </NavLink>

        <NavLink to="/my-orders" className={({ isActive }) => cx("item", { active: isActive })}>
          Đơn hàng của tôi
        </NavLink>
      </div>

      {/* EXTRA */}
      <div className={cx("menu")}>
        <NavLink to="/voucher" className={cx("item")}>
          Ưu đãi của tôi
        </NavLink>

        <NavLink to="/points" className={cx("item")}>
          Lịch sử điểm thưởng
        </NavLink>
      </div>

      <div className={cx("menu")}>
        <NavLink to="/bank-accounts" className={cx("item")}>
          Tài khoản ngân hàng
        </NavLink>

        <NavLink to="/address" className={cx("item")}>
          Sổ địa chỉ nhận hàng
        </NavLink>
      </div>
      <div className={cx("menu")}>
        <NavLink to="/session" className={cx("item")}>
          Cài đặt bảo mật
        </NavLink>
      </div>

      {/* LOGOUT */}
      <div className={cx("logout")}>
        Đăng xuất
      </div>
    </div>
  );
}