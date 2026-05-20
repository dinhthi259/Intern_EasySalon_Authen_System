import { useState } from "react";
import classNames from "classnames/bind";
import styles from "./TaxDeclarationCreate.module.scss";
import { generateDeclaration } from "../../../api/TaxApi";
import { notifyError, notifySuccess } from "../../../components/Nofitication";

const cx = classNames.bind(styles);

export default function TaxDeclarationCreate({ onCreated }) {
  const [form, setForm] = useState({
    periodType: "MONTH",
    month: new Date().getMonth() + 1,
    quarter: 1,
    year: new Date().getFullYear(),
    note: "",
  });

  const handleChange = (e) => {
    setForm({
      ...form,
      [e.target.name]: e.target.value,
    });
  };

  const handleGenerate = async () => {
    const payload = {
      periodType: form.periodType,
      month: form.periodType === "MONTH" ? Number(form.month) : null,
      quarter: form.periodType === "QUARTER" ? Number(form.quarter) : null,
      year: Number(form.year),
      note: form.note,
    };

    try {
      const res = await generateDeclaration(payload);
      if (res.success) {
        notifySuccess("Tạo tờ khai thuế thành công!");
        onCreated();
      }
    } catch (error) {
      notifyError(error.res?.data?.message || "Tạo tờ khai thuế thất bại!");
    }
  };

  return (
    <div className={cx("wrapper")}>
      <h3 className={cx("title")}>Tạo tờ khai thuế GTGT</h3>

      <div className={cx("form-grid")}>
        <div className={cx("form-group")}>
          <label className={cx("label")}>Kỳ kê khai</label>

          <select
            className={cx("select")}
            name="periodType"
            value={form.periodType}
            onChange={handleChange}
          >
            <option value="MONTH">Theo tháng</option>
            <option value="QUARTER">Theo quý</option>
          </select>
        </div>

        {form.periodType === "MONTH" && (
          <div className={cx("form-group")}>
            <label className={cx("label")}>Tháng</label>

            <select
              className={cx("select")}
              name="month"
              value={form.month}
              onChange={handleChange}
            >
              {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map((m) => (
                <option key={m} value={m}>
                  Tháng {m}
                </option>
              ))}
            </select>
          </div>
        )}

        {form.periodType === "QUARTER" && (
          <div className={cx("form-group")}>
            <label className={cx("label")}>Quý</label>

            <select
              className={cx("select")}
              name="quarter"
              value={form.quarter}
              onChange={handleChange}
            >
              <option value="1">Quý I</option>
              <option value="2">Quý II</option>
              <option value="3">Quý III</option>
              <option value="4">Quý IV</option>
            </select>
          </div>
        )}

        <div className={cx("form-group")}>
          <label className={cx("label")}>Năm</label>

          <input
            className={cx("input")}
            name="year"
            type="number"
            value={form.year}
            onChange={handleChange}
          />
        </div>
      </div>

      <div className={cx("form-group", "full-width")}>
        <label className={cx("label")}>Ghi chú</label>

        <textarea
          className={cx("textarea")}
          name="note"
          value={form.note}
          onChange={handleChange}
          placeholder="Tờ khai demo phục vụ quản trị nội bộ"
        />
      </div>

      <button className={cx("primary-btn")} onClick={handleGenerate}>
        Tạo tờ khai
      </button>
    </div>
  );
}
