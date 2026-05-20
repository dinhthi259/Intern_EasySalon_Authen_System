import { useEffect, useState } from "react";
import classNames from "classnames/bind";
import styles from "./TaxDeclarationDetail.module.scss";
import {
  approveDeclaration,
  getDeclarationDetail,
} from "../../../api/TaxApi";

const cx = classNames.bind(styles);

export default function TaxDeclarationDetail({ id, onBack }) {
  const [data, setData] = useState(null);

  const loadDetail = async () => {
    const res = await getDeclarationDetail(id);
    setData(res.data);
  };

  useEffect(() => {
    loadDetail();
  }, [id]);

  const handleApprove = async () => {
    await approveDeclaration(id);
    alert("Duyệt tờ khai thành công!");
    loadDetail();
  };

  if (!data) return <p className={cx("loading")}>Đang tải...</p>;

  return (
    <div className={cx("wrapper")}>
      <button className={cx("back-btn")} onClick={onBack}>
        ← Quay lại
      </button>

      <div className={cx("official-form")}>
        <div className={cx("form-top")}>
          <div>
            <b>Mẫu số: 01/GTGT</b>
            <p>Ban hành kèm theo Thông tư 80/2021/TT-BTC</p>
          </div>

          <div className={cx("national-title")}>
            <b>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</b>
            <p>Độc lập - Tự do - Hạnh phúc</p>
          </div>
        </div>

        <h2 className={cx("main-title")}>TỜ KHAI THUẾ GIÁ TRỊ GIA TĂNG</h2>

        <p className={cx("center")}>
          Áp dụng đối với người nộp thuế tính thuế theo phương pháp khấu trừ
        </p>

        <div className={cx("period")}>
          <p>
            <b>Kỳ tính thuế:</b>{" "}
            {data.periodType === "MONTH"
              ? `Tháng ${data.month}/${data.year}`
              : `Quý ${data.quarter}/${data.year}`}
          </p>
          <p>
            <b>Mã tờ khai:</b> {data.declarationCode}
          </p>
          <p>
            <b>Trạng thái:</b>{" "}
            <span className={cx("status", data.status?.toLowerCase())}>
              {data.status}
            </span>
          </p>
        </div>

        <div className={cx("taxpayer-info")}>
          <p>
            <b>[01] Tên người nộp thuế:</b> CÔNG TY CỔ PHẦN TECH AI VIỆT NAM
          </p>
          <p>
            <b>[02] Mã số thuế:</b> 011223344
          </p>
          <p>
            <b>[03] Địa chỉ:</b> 1123 Ngô Quyền
          </p>
          <p>
            <b>[04] Quận/Huyện:</b> Phường An Hải
          </p>
          <p>
            <b>[05] Tỉnh/Thành phố:</b> TP. Đà Nẵng
          </p>
        </div>

        <div className={cx("table-wrap")}>
          <table className={cx("official-table")}>
            <thead>
              <tr>
                <th>STT</th>
                <th>Chỉ tiêu</th>
                <th>Mã chỉ tiêu</th>
                <th>Giá trị</th>
              </tr>
            </thead>

            <tbody>
              <tr>
                <td>1</td>
                <td>Hàng hóa, dịch vụ bán ra chịu thuế GTGT</td>
                <td>[32]</td>
                <td>{data.totalRevenue.toLocaleString()} đ</td>
              </tr>
              <tr>
                <td>2</td>
                <td>Thuế GTGT của hàng hóa, dịch vụ bán ra</td>
                <td>[33]</td>
                <td>{data.totalTaxAmount.toLocaleString()} đ</td>
              </tr>
              <tr>
                <td>3</td>
                <td>Tổng doanh thu sau thuế</td>
                <td>Demo</td>
                <td>{data.totalFinalAmount.toLocaleString()} đ</td>
              </tr>
              <tr>
                <td>4</td>
                <td>Số lượng hóa đơn kê khai</td>
                <td>Demo</td>
                <td>{data.totalInvoice}</td>
              </tr>
              <tr>
                <td>5</td>
                <td>Thuế GTGT phải nộp trong kỳ</td>
                <td>[40]</td>
                <td>{data.totalTaxAmount.toLocaleString()} đ</td>
              </tr>
            </tbody>
          </table>
        </div>

        <h3 className={cx("section-title")}>Bảng kê hóa đơn trong kỳ</h3>

        <div className={cx("table-wrap")}>
          <table className={cx("official-table")}>
            <thead>
              <tr>
                <th>STT</th>
                <th>Mã hóa đơn</th>
                <th>Khách hàng</th>
                <th>Ngày hóa đơn</th>
                <th>Doanh thu</th>
                <th>Thuế GTGT</th>
                <th>Tổng tiền</th>
              </tr>
            </thead>

            <tbody>
              {data.details.map((item, index) => (
                <tr key={item.taxDeclarationDetailId}>
                  <td>{index + 1}</td>
                  <td>{item.invoiceCode}</td>
                  <td>{item.customerName}</td>
                  <td>
                    {new Date(item.invoiceCreatedAt).toLocaleDateString()}
                  </td>
                  <td>{item.revenueAmount.toLocaleString()} đ</td>
                  <td>{item.taxAmount.toLocaleString()} đ</td>
                  <td>{item.finalAmount.toLocaleString()} đ</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className={cx("signature")}>
          <div></div>
          <div>
            <p>Ngày ..... tháng ..... năm {data.year}</p>
            <b>NGƯỜI NỘP THUẾ</b>
            <p>Ký, ghi rõ họ tên</p>
          </div>
        </div>

        <p className={cx("demo-note")}>
          Ghi chú: Đây là tờ khai mô phỏng phục vụ demo khóa luận, không có giá
          trị kê khai thuế thật.
        </p>
      </div>

      {data.status === "Draft" && (
        <button className={cx("approve-btn")} onClick={handleApprove}>
          Duyệt tờ khai
        </button>
      )}
    </div>
  );
}