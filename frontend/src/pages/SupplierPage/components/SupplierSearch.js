import React, { useState } from "react";
import classNames from "classnames/bind";
import styles from "./SupplierSearch.module.scss";

const cx = classNames.bind(styles);

const SupplierSearch = ({ onSearch, onAdd }) => {
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");

  const handleSearch = () => {
    onSearch({ search, status });
  };

  const handleReset = () => {
    setSearch("");
    setStatus("");
    onSearch({ search: "", status: "" });
  };

  return (
    <div className={cx("searchForm")}>
      <div className={cx("searchBox")}>
        <input
          type="text"
          placeholder="Tìm kiếm tên, mã, điện thoại..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyPress={(e) => e.key === "Enter" && handleSearch()}
        />
        
      </div>

      <select
        value={status}
        onChange={(e) => setStatus(e.target.value)}
        className={cx("filterSelect")}
      >
        <option value="">Tất cả trạng thái</option>
        <option value="ACTIVE">Hoạt động</option>
        <option value="INACTIVE">Không hoạt động</option>
      </select>

      <button className={cx("resetBtn")} onClick={handleSearch}>Tìm kiếm</button>

      <button className={cx("resetBtn")} onClick={handleReset}>
        Xóa bộ lọc
      </button>

      <button className={cx("addBtn")} onClick={onAdd}>
        + Thêm nhà cung cấp
      </button>
    </div>
  );
};

export default SupplierSearch;
