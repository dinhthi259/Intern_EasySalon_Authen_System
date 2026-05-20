import { useEffect, useState } from "react";
import classNames from "classnames/bind";
import styles from "./TaxDeclarationList.module.scss";
import { getDeclarations } from "../../../api/TaxApi";

const cx = classNames.bind(styles);

export default function TaxDeclarationList({ onView }) {
  const [data, setData] = useState([]);

  const loadData = async () => {
    const res = await getDeclarations();
    setData(res.data);
  };

  useEffect(() => {
    loadData();
  }, []);

  return (
    <div className={cx("wrapper")}>
      <div className={cx("header")}>
        <h3 className={cx("title")}>Danh sách tờ khai thuế</h3>

        <div className={cx("total-box")}>
          Tổng tờ khai: <strong>{data.length}</strong>
        </div>
      </div>

      <div className={cx("table-wrap")}>
        <table className={cx("tax-table")}>
          <thead>
            <tr>
              <th>Mã tờ khai</th>
              <th>Kỳ</th>
              <th>Năm</th>
              <th>Số HĐ</th>
              <th>Doanh thu</th>
              <th>Thuế GTGT</th>
              <th>Trạng thái</th>
              <th>Thao tác</th>
            </tr>
          </thead>

          <tbody>
            {data.length > 0 ? (
              data.map((item) => (
                <tr key={item.taxDeclarationId}>
                  <td className={cx("code")}>{item.declarationCode}</td>

                  <td>
                    {item.periodType === "MONTH"
                      ? `Tháng ${item.month}`
                      : `Quý ${item.quarter}`}
                  </td>

                  <td>{item.year}</td>

                  <td>{item.totalInvoice}</td>

                  <td className={cx("money")}>
                    {item.totalRevenue.toLocaleString()} đ
                  </td>

                  <td className={cx("money")}>
                    {item.totalTaxAmount.toLocaleString()} đ
                  </td>

                  <td>
                    <span
                      className={cx(
                        "status",
                        item.status?.toLowerCase()
                      )}
                    >
                      {item.status}
                    </span>
                  </td>

                  <td>
                    <button
                      className={cx("view-btn")}
                      onClick={() => onView(item.taxDeclarationId)}
                    >
                      Xem tờ khai
                    </button>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td className={cx("empty")} colSpan="8">
                  Chưa có dữ liệu tờ khai thuế
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}