import { useEffect, useState } from "react";
import styles from "./AdminInventoryInvoicesPage.module.scss";
import classNames from "classnames/bind";
import { FaFileExport, FaFileImport, FaPrint, FaSearch } from "react-icons/fa";
import { BiDetail } from "react-icons/bi";
import { getAllInventoryLogs } from "../../api/InventoryApi";
import InvoiceDetailModal from "./components/InventoryDetailModal";

const cx = classNames.bind(styles);

function AdminInventoryInvoicePage() {
  const [invoices, setInvoices] = useState([]);
  const [selectedInvoice, setSelectedInvoice] = useState(null);
  const [loading, setLoading] = useState(false);
  const [filterType, setFilterType] = useState("ALL");
  const [keyword, setKeyword] = useState("");

  useEffect(() => {
    fetchInvoices();
  }, []);

  const fetchInvoices = async () => {
    setLoading(true);

    try {
      const res = await getAllInventoryLogs();
      const grouped = {};

      res.data?.forEach((item) => {
        const key = item.referenceId || "NO_REF";

        if (!grouped[key]) {
          grouped[key] = {
            id: key,
            number: key,
            type: item.changeType,
            createdAt: item.createAt || new Date(),
            createdBy: item.createdBy || "Admin",
            partnerName: item.partnerName || "N/A",
            note: item.note || "",
            items: [],
          };
        }

        grouped[key].items.push({
          productId: item.productId,
          productCode: item.productCode,
          productName: item.productName,
          quantity: item.quantityChanged,
          price: item.price || 0,
          unit: item.unit || "Cái",
        });
      });

      setInvoices(
        Object.values(grouped).sort(
          (a, b) => new Date(b.createdAt) - new Date(a.createdAt),
        ),
      );
    } catch (err) {
      console.error("Error fetching invoices:", err);
    } finally {
      setLoading(false);
    }
  };

  const filteredInvoices = invoices.filter((invoice) => {
    const matchType = filterType === "ALL" || invoice.type === filterType;

    const searchText =
      `${invoice.number} ${invoice.partnerName} ${invoice.createdBy}`.toLowerCase();

    return matchType && searchText.includes(keyword.toLowerCase());
  });

  const handlePrint = (invoice) => {
    setSelectedInvoice({ ...invoice, autoPrint: true });
  };

  return (
    <div className={cx("page")}>
      <div className={cx("header")}>
        <div>
          <h2>Quản lý phiếu nhập xuất kho</h2>
        </div>
      </div>

      <div className={cx("statGrid")}>
        <div className={cx("statCard")}>
          <span>Tổng phiếu</span>
          <strong>{invoices.length}</strong>
        </div>

        <div className={cx("statCard", "import")}>
          <span>Phiếu nhập</span>
          <strong>{invoices.filter((i) => i.type === "IMPORT").length}</strong>
        </div>

        <div className={cx("statCard", "export")}>
          <span>Phiếu xuất</span>
          <strong>{invoices.filter((i) => i.type === "EXPORT").length}</strong>
        </div>
      </div>

      <div className={cx("card")}>
        <div className={cx("toolbar")}>
          <div className={cx("searchBox")}>
            <FaSearch />
            <input
              placeholder="Tìm theo số phiếu, đối tác, người tạo..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </div>

          <div className={cx("filterBar")}>
            <button
              className={cx(filterType === "ALL" && "active")}
              onClick={() => setFilterType("ALL")}
            >
              Tất cả
            </button>

            <button
              className={cx(filterType === "IMPORT" && "active")}
              onClick={() => setFilterType("IMPORT")}
            >
              <FaFileImport /> Nhập kho
            </button>

            <button
              className={cx(filterType === "EXPORT" && "active")}
              onClick={() => setFilterType("EXPORT")}
            >
              <FaFileExport /> Xuất kho
            </button>
          </div>
        </div>

        {loading ? (
          <div className={cx("message")}>Đang tải dữ liệu...</div>
        ) : filteredInvoices.length === 0 ? (
          <div className={cx("message")}>Không có phiếu nào</div>
        ) : (
          <div className={cx("tableWrapper")}>
            <table className={cx("table")}>
              <thead>
                <tr>
                  <th>Số phiếu</th>
                  <th>Ngày tạo</th>
                  <th>Loại</th>
                  <th>Nhà cung cấp / Khách hàng</th>
                  <th>Người tạo</th>
                  <th>Hành động</th>
                </tr>
              </thead>

              <tbody>
                {filteredInvoices.map((invoice) => (
                  <tr key={invoice.id}>
                    <td>
                      <strong>#{invoice.number}</strong>
                    </td>

                    <td>
                      {new Date(invoice.createdAt).toLocaleString("vi-VN")}
                    </td>

                    <td>
                      <span
                        className={cx(
                          "badge",
                          invoice.type === "IMPORT"
                            ? "importBadge"
                            : "exportBadge",
                        )}
                      >
                        {invoice.type === "IMPORT" ? "Nhập kho" : "Xuất kho"}
                      </span>
                    </td>

                    <td>{invoice.partnerName}</td>
                    <td>{invoice.createdBy}</td>

                    <td>
                      <div className={cx("actions")}>
                        <button
                          type="button"
                          className={cx("viewBtn")}
                          onClick={() => setSelectedInvoice(invoice)}
                        >
                          <BiDetail /> Xem
                        </button>

                        <button
                          type="button"
                          className={cx("printBtn")}
                          onClick={() => handlePrint(invoice)}
                        >
                          <FaPrint /> In PDF
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {selectedInvoice && (
        <InvoiceDetailModal
          invoice={selectedInvoice}
          onClose={() => setSelectedInvoice(null)}
        />
      )}
    </div>
  );
}

export default AdminInventoryInvoicePage;
