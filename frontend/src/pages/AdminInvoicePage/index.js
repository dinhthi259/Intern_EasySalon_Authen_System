import { useEffect, useState } from "react";
import styles from "./AdminInvoicePage.module.scss";
import {
  FaEye,
  FaPrint,
  FaDownload,
  FaSearch,
  FaFileInvoice,
} from "react-icons/fa";
import { getAllInvoices, getInvoices } from "../../api/InvoiceApi";
import { GrPowerReset } from "react-icons/gr";

export default function AdminInvoicePage() {
  const [invoices, setInvoices] = useState([]);
  const [selectedIds, setSelectedIds] = useState([]);
  const [email, setEmail] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [errors, setErrors] = useState({});

  const API_URL = process.env.REACT_APP_API_URL;

  const fetchInvoices = async () => {
    if (!validateFilter()) return;

    const params = new URLSearchParams();

    if (email.trim()) params.append("email", email.trim());
    if (fromDate) params.append("fromDate", fromDate);
    if (toDate) params.append("toDate", toDate);

    const res = await getAllInvoices(params);
    const data = await res.data;

    setInvoices(data);
    setSelectedIds([]);
  };

  useEffect(() => {
    fetchInvoices();
  }, []);

  const validateFilter = () => {
    const newErrors = {};

    if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = "Email không hợp lệ.";
    }

    if (fromDate && toDate && new Date(fromDate) > new Date(toDate)) {
      newErrors.date = "Từ ngày không được lớn hơn đến ngày.";
    }

    setErrors(newErrors);

    return Object.keys(newErrors).length === 0;
  };

  const resetFilter = async () => {
    setEmail("");
    setFromDate("");
    setToDate("");
    setErrors({});
    setSelectedIds([]);

    const res = await getInvoices();
    const data = await res.data;

    setInvoices(data);
  };

  const toggleSelect = (id) => {
    setSelectedIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id],
    );
  };

  const toggleSelectAll = () => {
    if (selectedIds.length === invoices.length) {
      setSelectedIds([]);
    } else {
      setSelectedIds(invoices.map((x) => x.invoiceId));
    }
  };

  const viewInvoice = (pdfUrl) => {
    window.open(`${API_URL}${pdfUrl}`, "_blank");
  };

  const printInvoice = (pdfUrl) => {
    const printWindow = window.open(`${API_URL}${pdfUrl}`, "_blank");

    if (printWindow) {
      printWindow.onload = () => {
        printWindow.print();
      };
    }
  };

  const downloadBulk = async () => {
    if (selectedIds.length === 0) {
      alert("Vui lòng chọn ít nhất một hóa đơn.");
      return;
    }

    const token = localStorage.getItem("accessToken");

    const res = await fetch(`${API_URL}/api/admin/invoices/download-bulk`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(selectedIds),
    });

    if (!res.ok) {
      alert("Tải hóa đơn thất bại.");
      return;
    }

    const blob = await res.blob();
    const url = window.URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.href = url;
    a.download = "hoa-don.zip";
    a.click();

    window.URL.revokeObjectURL(url);

    setSelectedIds([]);
  };

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h2>Quản lý hóa đơn</h2>
          <p>Danh sách hóa đơn đã gửi cho khách hàng</p>
        </div>

        <button className={styles.downloadBtn} onClick={downloadBulk}>
          <FaDownload /> Tải hàng loạt
        </button>
      </div>

      <div className={styles.filterBox}>
        <div className={styles.inputGroup}>
          <label>Email khách hàng</label>
          <input
            type="text"
            placeholder="Nhập email khách hàng..."
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          {errors.email && <span className={styles.error}>{errors.email}</span>}
        </div>

        <div className={styles.inputGroup}>
          <label>Từ ngày</label>
          <input
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
          />
        </div>

        <div className={styles.inputGroup}>
          <label>Đến ngày</label>
          <input
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
          />
        </div>

        <div className={styles.buttonGroup}>
          <button className={styles.searchBtn} onClick={fetchInvoices}>
            <FaSearch /> Lọc
          </button>

          <button className={styles.resetBtn} onClick={resetFilter}>
            <GrPowerReset />
          </button>
        </div>

        {errors.date && <div className={styles.dateError}>{errors.date}</div>}
      </div>

      <div className={styles.tableCard}>
        <table>
          <thead>
            <tr>
              <th>
                <input
                  type="checkbox"
                  checked={
                    invoices.length > 0 &&
                    selectedIds.length === invoices.length
                  }
                  onChange={toggleSelectAll}
                />
              </th>
              <th>Mã hóa đơn</th>
              <th>Khách hàng</th>
              <th>Email</th>
              <th>Tổng tiền</th>
              <th>Trạng thái</th>
              <th>Ngày tạo</th>
              <th>Ngày gửi</th>
              <th>Thao tác</th>
            </tr>
          </thead>

          <tbody>
            {invoices.map((item) => (
              <tr key={item.invoiceId}>
                <td>
                  <input
                    type="checkbox"
                    checked={selectedIds.includes(item.invoiceId)}
                    onChange={() => toggleSelect(item.invoiceId)}
                  />
                </td>
                <td>{item.invoiceCode}</td>
                <td>{item.customerName}</td>
                <td>{item.customerEmail}</td>
                <td>{Number(item.finalAmount).toLocaleString()} VNĐ</td>
                <td>
                  <span className={styles.status}>{item.status}</span>
                </td>
                <td>{new Date(item.createdAt).toLocaleDateString("vi-VN")}</td>
                <td>
                  {item.sentAt
                    ? new Date(item.sentAt).toLocaleDateString("vi-VN")
                    : "Chưa gửi"}
                </td>
                <td>
                  <div className={styles.actions}>
                    <button onClick={() => viewInvoice(item.pdfUrl)}>
                      <FaEye />
                    </button>
                    <button onClick={() => printInvoice(item.pdfUrl)}>
                      <FaPrint />
                    </button>
                  </div>
                </td>
              </tr>
            ))}

            {invoices.length === 0 && (
              <tr>
                <td colSpan="9" className={styles.empty}>
                  Không có hóa đơn nào.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
