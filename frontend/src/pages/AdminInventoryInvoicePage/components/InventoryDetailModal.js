import { useState } from "react";
import styles from "./InventoryDetailModal.module.scss";
import classNames from "classnames/bind";

const cx = classNames.bind(styles);

function InvoiceDetailModal({ invoice, onClose }) {
  const [isPrinting, setIsPrinting] = useState(false);

  if (!invoice) return null;

  const handlePrint = () => {
    setIsPrinting(true);
    setTimeout(() => {
      window.print();
      setIsPrinting(false);
    }, 100);
  };

  const total =
    invoice.items?.reduce((sum, item) => sum + item.quantity * item.price, 0) ||
    0;

  const createdDate = new Date(invoice.createdAt).toLocaleDateString("vi-VN");

  return (
    <div className={cx("modalOverlay")} onClick={onClose}>
      <div className={cx("modal")} onClick={(e) => e.stopPropagation()}>
        {/* Header with Controls */}
        <div className={cx("controls")}>
          <button
            className={cx("printBtn")}
            onClick={handlePrint}
            disabled={isPrinting}
          >
            🖨️ In
          </button>
          <button className={cx("closeBtn")} onClick={onClose}>
            ✕
          </button>
        </div>

        {/* Invoice Paper */}
        <div className={cx("invoicePaper")} id="invoice-content">
          {/* Company Info */}
          <div className={cx("companyHeader")}>
            <h1>CÔNG TY CỔ PHẦN TECH AI VIỆT NAM</h1>
            <p className={cx("companyName")}>Chi nhánh Đà Nẵng</p>
            <p className={cx("companyAddress")}>
              Địa chỉ: 1123 Đường Ngô Quyền, Phường An Hải, TP Đà Nẵng
            </p>
            <p className={cx("companyPhone")}>
              ĐT: 0123 456 789 | Tax: 0123456789
            </p>
          </div>

          {/* Invoice Title */}
          <div className={cx("invoiceTitle")}>
            <h2>
              {invoice.type === "IMPORT" ? "PHIẾU NHẬP KHO" : "PHIẾU XUẤT KHO"}
            </h2>
          </div>

          {/* Invoice Info */}
          <div className={cx("invoiceInfo")}>
            <div className={cx("infoRow")}>
              <div className={cx("infoCol")}>
                <label>Số phiếu:</label>
                <span>{invoice.number || invoice.id}</span>
              </div>
              <div className={cx("infoCol")}>
                <label>Ngày tạo:</label>
                <span>{createdDate}</span>
              </div>
            </div>

            <div className={cx("infoRow")}>
              <div className={cx("infoCol")}>
                <label>
                  {invoice.type === "IMPORT" ? "Nhà cung cấp:" : "Khách hàng:"}
                </label>
                <span>{invoice.partnerName || "N/A"}</span>
              </div>
              <div className={cx("infoCol")}>
                <label>Người tạo:</label>
                <span>{invoice.createdBy || "Admin"}</span>
              </div>
            </div>

            {invoice.note && (
              <div className={cx("infoCol")}>
                <label>Ghi chú:</label>
                <span>{invoice.note}</span>
              </div>
            )}
          </div>

          {/* Items Table */}
          <table className={cx("itemsTable")}>
            <thead>
              <tr>
                <th>STT</th>
                <th>Tên sản phẩm</th>
                <th>Mã</th>
                <th>ĐVT</th>
                <th className={cx("right")}>Số lượng</th>
                <th className={cx("right")}>Đơn giá</th>
                <th className={cx("right")}>Thành tiền</th>
              </tr>
            </thead>
            <tbody>
              {invoice.items?.map((item, index) => (
                <tr key={index}>
                  <td>{index + 1}</td>
                  <td>{item.productName}</td>
                  <td>{item.productId || "N/A"}</td>
                  <td>{item.unit || "Cái"}</td>
                  <td className={cx("right")}>{item.quantity}</td>
                  <td className={cx("right")}>
                    {(item.price || 0).toLocaleString("vi-VN")} ₫
                  </td>
                  <td className={cx("right")}>
                    {Math.abs(item.quantity * (item.price || 0)).toLocaleString(
                      "vi-VN",
                    )}{" "}
                    ₫
                  </td>
                </tr>
              ))}
              {/* Empty rows for paper appearance */}
              {invoice.items &&
                invoice.items.length < 8 &&
                Array.from({ length: 8 - invoice.items.length }).map((_, i) => (
                  <tr key={`empty-${i}`} className={cx("emptyRow")}>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td></td>
                  </tr>
                ))}
            </tbody>
          </table>

          {/* Total Section */}
          <div className={cx("totalSection")}>
            <div className={cx("totalRow")}>
              <span className={cx("label")}>Cộng tiền hàng:</span>
              <span className={cx("amount")}>
                {Math.abs(total).toLocaleString("vi-VN")} ₫
              </span>
            </div>
            <div className={cx("totalRow", "highlight")}>
              <span className={cx("label")}>Tổng cộng:</span>
              <span className={cx("amount")}>
                {Math.abs(total).toLocaleString("vi-VN")} ₫
              </span>
            </div>
          </div>

          {/* Signatures Section */}
          <div className={cx("signaturesSection")}>
            <div className={cx("signatureBox")}>
              <p className={cx("signatureLabel")}>Người lập phiếu</p>
              <p className={cx("signatureNote")}>(Ký, họ tên)</p>
              <div className={cx("signatureSpace")}></div>
            </div>

            <div className={cx("signatureBox")}>
              <p className={cx("signatureLabel")}>
                {invoice.type === "IMPORT" ? "Nhà cung cấp" : "Khách hàng"}
              </p>
              <p className={cx("signatureNote")}>(Ký, họ tên)</p>
              <div className={cx("signatureSpace")}></div>
            </div>

            <div className={cx("signatureBox")}>
              <p className={cx("signatureLabel")}>Người phê duyệt</p>
              <p className={cx("signatureNote")}>(Ký, họ tên)</p>
              <div className={cx("signatureSpace")}></div>
            </div>
          </div>

          {/* Footer */}
          <div className={cx("footer")}>
            <p>
              Cảm ơn quý khách. Dữ liệu này được in từ hệ thống, không cần chữ
              ký.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default InvoiceDetailModal;
