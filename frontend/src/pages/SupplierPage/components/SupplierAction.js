import React, { useState } from "react";
import classNames from "classnames/bind";
import styles from "./SupplierAction.module.scss";
import { createSupplier } from "../../../api/SupplierApi";
import { notifyError, notifySuccess } from "../../../components/Nofitication";

const cx = classNames.bind(styles);

const SupplierAction = ({ onClose, onSuccess }) => {
  const [form, setForm] = useState({
    code: "",
    name: "",
    contactPerson: "",
    phone: "",
    email: "",
    address: "",
    province: "",
    district: "",
    taxCode: "",
    bankName: "",
    bankAccount: "",
    note: "",
  });
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm({ ...form, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await createSupplier(form);
      notifySuccess("Thêm nhà cung cấp thành công!");
      onSuccess?.();
    } catch (err) {
      notifyError("Lỗi: " + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={cx("modal")}>
      <div className={cx("backdrop")} onClick={onClose}></div>
      <div className={cx("content")}>
        <div className={cx("header")}>
          <h2>Thêm nhà cung cấp mới</h2>
          <button className={cx("closeBtn")} onClick={onClose}>✕</button>
        </div>

        <form onSubmit={handleSubmit} className={cx("form")}>
          <div className={cx("formGrid")}>
            <div className={cx("formGroup")}>
              <label>Mã nhà cung cấp *</label>
              <input
                type="text"
                name="code"
                value={form.code}
                onChange={handleChange}
                placeholder="Nhập mã nhà cung cấp"
                required
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Tên nhà cung cấp *</label>
              <input
                type="text"
                name="name"
                value={form.name}
                onChange={handleChange}
                placeholder="Nhập tên nhà cung cấp"
                required
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Người liên hệ</label>
              <input
                type="text"
                name="contactPerson"
                value={form.contactPerson}
                onChange={handleChange}
                placeholder="Nhập tên người liên hệ"
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Điện thoại</label>
              <input
                type="tel"
                name="phone"
                value={form.phone}
                onChange={handleChange}
                placeholder="Nhập số điện thoại"
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Email</label>
              <input
                type="email"
                name="email"
                value={form.email}
                onChange={handleChange}
                placeholder="Nhập địa chỉ email"
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Địa chỉ</label>
              <input
                type="text"
                name="address"
                value={form.address}
                onChange={handleChange}
                placeholder="Nhập địa chỉ"
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Tỉnh/Thành phố</label>
              <input
                type="text"
                name="province"
                value={form.province}
                onChange={handleChange}
                placeholder="Nhập tỉnh/thành phố"
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Quận/Huyện</label>
              <input
                type="text"
                name="district"
                value={form.district}
                onChange={handleChange}
                placeholder="Nhập quận/huyện"
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Mã số thuế</label>
              <input
                type="text"
                name="taxCode"
                value={form.taxCode}
                onChange={handleChange}
                placeholder="Nhập mã số thuế"
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Ngân hàng</label>
              <input
                type="text"
                name="bankName"
                value={form.bankName}
                onChange={handleChange}
                placeholder="Nhập tên ngân hàng"
              />
            </div>

            <div className={cx("formGroup")}>
              <label>Số tài khoản</label>
              <input
                type="text"
                name="bankAccount"
                value={form.bankAccount}
                onChange={handleChange}
                placeholder="Nhập số tài khoản"
              />
            </div>

            <div className={cx("formGroup", "fullWidth")}>
              <label>Ghi chú</label>
              <textarea
                name="note"
                value={form.note}
                onChange={handleChange}
                placeholder="Nhập ghi chú thêm"
                rows="3"
              ></textarea>
            </div>
          </div>

          <div className={cx("footer")}>
            <button type="submit" disabled={loading} className={cx("submitBtn")}>
              {loading ? "Đang lưu..." : "Thêm"}
            </button>
            <button type="button" onClick={onClose} className={cx("cancelBtn")}>
              Hủy
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default SupplierAction;
