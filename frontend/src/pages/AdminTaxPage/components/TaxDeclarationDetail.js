import { useEffect, useState } from "react";
import {
  approveDeclaration,
  deleteDeclaration,
  getDeclarationDetail,
} from "../../../api/TaxApi";
import styles from "./TaxDeclarationDetail.module.scss";
import classNames from "classnames/bind";

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

  const money = (value) => {
    return Number(value || 0).toLocaleString("vi-VN");
  };

  const handleApprove = async () => {
    await approveDeclaration(id);
    alert("Duyệt tờ khai thành công!");
    loadDetail();
  };

  if (!data) return <p>Đang tải...</p>;

  const hasSale = Number(data.totalInvoice || 0) > 0;
  const hasPurchase = Number(data.purchaseAmount || 0) > 0;

  const ct21 = !hasSale && !hasPurchase ? "X" : "";

  const ct22 = data.previousDeductibleTax || 0;

  const ct23 = data.purchaseAmount || 0;
  const ct24 = data.purchaseTaxAmount || 0;
  const ct25 = data.deductibleTaxAmount || 0;

  const ct26 = 0;

  const ct29 = 0;
  const ct30 = 0;
  const ct31 = 0;

  const ct32 = data.totalRevenue || 0;
  const ct33 = data.totalTaxAmount || 0;

  const ct32a = 0;

  const ct27 = ct29 + ct30 + ct32 + ct32a;
  const ct28 = ct31 + ct33;

  const ct34 = ct26 + ct27;
  const ct35 = ct28;

  const ct36 = ct35 - ct25;

  const ct37 = 0;
  const ct38 = 0;
  const ct39a = 0;

  const temp40a = ct36 - ct22 + ct37 - ct38 - ct39a;
  const ct40a = temp40a > 0 ? temp40a : 0;
  const ct40b = 0;
  const ct40 = data.vatPayable ?? ct40a;

  const ct41 = temp40a < 0 ? Math.abs(temp40a) : 0;
  const ct42 = 0;

  const ct43 = data.vatCarriedForward ?? ct41 - ct42;

  return (
    <div className={cx("tax-detail-page")}>
      <div className={cx("tax-actions", "no-print")}>
        <button onClick={onBack}>← Quay lại</button>

        {data.status === "Draft" && (
          <button className={cx("approve-btn")} onClick={handleApprove}>
            Duyệt tờ khai
          </button>
        )}

        <button onClick={() => window.print()}>In tờ khai</button>
      </div>

      <div className={cx("print-area")}>
      <  div className={cx("gtgt-form")}>
          <div className={cx("form-code")}>
            <b>Mẫu số: 01/GTGT</b>
            <p>
              Ban hành kèm theo Thông tư số 80/2021/TT-BTC ngày 29 tháng 9 năm
              2021 của Bộ trưởng Bộ Tài chính
            </p>
          </div>
  
          <div className={cx("national-header")}>
            <b>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</b>
            <p>Độc lập - Tự do - Hạnh phúc</p>
          </div>
  
          <h2>TỜ KHAI THUẾ GIÁ TRỊ GIA TĂNG</h2>
  
          <p className={cx("form-subtitle")}>
            Áp dụng đối với người nộp thuế tính thuế theo phương pháp khấu trừ có
            hoạt động sản xuất kinh doanh
          </p>
  
          <div className={cx("info-lines")}>
            <p>
              <b>[01a] Tên hoạt động sản xuất kinh doanh:</b> Hoạt động sản xuất
              kinh doanh thông thường
            </p>
  
            <p>
              <b>[01b] Kỳ tính thuế:</b>{" "}
              {data.periodType === "MONTH"
                ? `Tháng ${data.month} năm ${data.year}`
                : `Quý ${data.quarter} năm ${data.year}`}
            </p>
  
            <div className={cx("checkbox-row")}>
              <span>
                <b>[02] Lần đầu:</b> ☑
              </span>
              <span>
                <b>[03] Bổ sung lần thứ:</b> .....
              </span>
            </div>
  
            <p>
              <b>[04] Tên người nộp thuế:</b> CÔNG TY CỔ PHẦN TECH AI VIỆT NAM
            </p>
            <p>
              <b>[05] Mã số thuế:</b> 0112233
            </p>
            <p>
              <b>[06] Tên đại lý thuế nếu có:</b>{" "}
              ........................................
            </p>
            <p>
              <b>[07] Mã số thuế:</b>{" "}
              .....................................................
            </p>
            <p>
              <b>[08] Hợp đồng đại lý thuế:</b> Số ........ ngày ........
            </p>
            <p>
              <b>[09] Tên đơn vị phụ thuộc/địa điểm kinh doanh:</b>{" "}
              ........................................
            </p>
            <p>
              <b>[10] Mã số thuế đơn vị phụ thuộc/Mã số địa điểm kinh doanh:</b>{" "}
              ........................................
            </p>
            <p>
              <b>[11] Địa chỉ nơi có hoạt động sản xuất kinh doanh khác tỉnh:</b>{" "}
              ........................................
            </p>
  
            <p className={cx("currency")}>Đơn vị tiền: Đồng Việt Nam</p>
          </div>
  
          <table className={cx("gtgt-table")}>
            <thead>
              <tr>
                <th style={{ width: "6%" }}>STT</th>
                <th style={{ width: "46%" }}>Chỉ tiêu</th>
                <th style={{ width: "12%" }}>Mã chỉ tiêu</th>
                <th style={{ width: "18%" }}>
                  Giá trị hàng hóa, dịch vụ chưa có thuế GTGT
                </th>
                <th style={{ width: "18%" }}>Thuế GTGT</th>
              </tr>
            </thead>
  
            <tbody>
              <tr>
                <td>A</td>
                <td>Không phát sinh hoạt động mua, bán trong kỳ</td>
                <td>[21]</td>
                <td className={cx("center")}>{ct21}</td>
                <td></td>
              </tr>
  
              <tr>
                <td>B</td>
                <td>Thuế GTGT còn được khấu trừ kỳ trước chuyển sang</td>
                <td>[22]</td>
                <td className={cx("center")}></td>
                <td className={cx("number")}>{money(ct22)}</td>
              </tr>
  
              <tr className={cx("section-row")}>
                <td>C</td>
                <td colSpan="4">Kê khai thuế GTGT phải nộp ngân sách nhà nước</td>
              </tr>
  
              <tr className={cx("section-row")}>
                <td>I</td>
                <td colSpan="4">Hàng hóa, dịch vụ mua vào trong kỳ</td>
              </tr>
  
              <tr>
                <td>1</td>
                <td>Giá trị và thuế GTGT của hàng hóa, dịch vụ mua vào</td>
                <td>
                  [23]
                  <br />
                  [24]
                </td>
                <td className={cx("number")}>{money(ct23)}</td>
                <td className={cx("number")}>{money(ct24)}</td>
              </tr>
  
              <tr>
                <td></td>
                <td>Trong đó: hàng hóa, dịch vụ nhập khẩu</td>
                <td>
                  [23a]
                  <br />
                  [24a]
                </td>
                <td className={cx("number")}>0</td>
                <td className={cx("number")}>0</td>
              </tr>
  
              <tr>
                <td>2</td>
                <td>
                  Thuế GTGT của hàng hóa, dịch vụ mua vào được khấu trừ kỳ này
                </td>
                <td>[25]</td>
                <td></td>
                <td className={cx("number")}>{money(ct25)}</td>
              </tr>
  
              <tr className={cx("section-row")}>
                <td>II</td>
                <td colSpan="4">Hàng hóa, dịch vụ bán ra trong kỳ</td>
              </tr>
  
              <tr>
                <td>1</td>
                <td>Hàng hóa, dịch vụ bán ra không chịu thuế GTGT</td>
                <td>[26]</td>
                <td className={cx("number")}>{money(ct26)}</td>
                <td></td>
              </tr>
  
              <tr>
                <td>2</td>
                <td>
                  Hàng hóa, dịch vụ bán ra chịu thuế GTGT
                  <br />
                  <i>[27] = [29] + [30] + [32] + [32a]; [28] = [31] + [33]</i>
                </td>
                <td>
                  [27]
                  <br />
                  [28]
                </td>
                <td className={cx("number")}>{money(ct27)}</td>
                <td className={cx("number")}>{money(ct28)}</td>
              </tr>
  
              <tr>
                <td>a</td>
                <td>Hàng hóa, dịch vụ bán ra chịu thuế suất 0%</td>
                <td>[29]</td>
                <td className={cx("number")}>{money(ct29)}</td>
                <td></td>
              </tr>
  
              <tr>
                <td>b</td>
                <td>Hàng hóa, dịch vụ bán ra chịu thuế suất 5%</td>
                <td>
                  [30]
                  <br />
                  [31]
                </td>
                <td className={cx("number")}>{money(ct30)}</td>
                <td className={cx("number")}>{money(ct31)}</td>
              </tr>
  
              <tr className={cx("highlight-row")}>
                <td>c</td>
                <td>Hàng hóa, dịch vụ bán ra chịu thuế suất 10%</td>
                <td>
                  [32]
                  <br />
                  [33]
                </td>
                <td className={cx("number")}>{money(ct32)}</td>
                <td className={cx("number")}>{money(ct33)}</td>
              </tr>
  
              <tr>
                <td>d</td>
                <td>Hàng hóa, dịch vụ bán ra không tính thuế</td>
                <td>[32a]</td>
                <td className={cx("number")}>{money(ct32a)}</td>
                <td></td>
              </tr>
  
              <tr>
                <td>3</td>
                <td>
                  Tổng doanh thu và thuế GTGT của hàng hóa, dịch vụ bán ra
                  <br />
                  <i>[34] = [26] + [27]; [35] = [28]</i>
                </td>
                <td>
                  [34]
                  <br />
                  [35]
                </td>
                <td className={cx("number")}>{money(ct34)}</td>
                <td className={cx("number")}>{money(ct35)}</td>
              </tr>
  
              <tr className={cx("section-row")}>
                <td>III</td>
                <td>Thuế GTGT phát sinh trong kỳ</td>
                <td>[36]</td>
                <td></td>
                <td className={cx("number")}>{money(ct36)}</td>
              </tr>
  
              <tr className={cx("section-row")}>
                <td>IV</td>
                <td colSpan="4">
                  Điều chỉnh tăng, giảm thuế GTGT còn được khấu trừ của các kỳ
                  trước
                </td>
              </tr>
  
              <tr>
                <td>1</td>
                <td>Điều chỉnh giảm</td>
                <td>[37]</td>
                <td></td>
                <td className={cx("number")}>{money(ct37)}</td>
              </tr>
  
              <tr>
                <td>2</td>
                <td>Điều chỉnh tăng</td>
                <td>[38]</td>
                <td></td>
                <td className={cx("number")}>{money(ct38)}</td>
              </tr>
  
              <tr className={cx("section-row")}>
                <td>V</td>
                <td>Thuế GTGT nhận bàn giao được khấu trừ trong kỳ</td>
                <td>[39a]</td>
                <td></td>
                <td className={cx("number")}>{money(ct39a)}</td>
              </tr>
  
              <tr className={cx("section-row")}>
                <td>VI</td>
                <td colSpan="4">Xác định nghĩa vụ thuế GTGT phải nộp trong kỳ</td>
              </tr>
  
              <tr>
                <td>1</td>
                <td>
                  Thuế GTGT phải nộp của hoạt động sản xuất kinh doanh trong kỳ
                  <br />
                  <i>[40a] = ([36] - [22] + [37] - [38] - [39a]) ≥ 0</i>
                </td>
                <td>[40a]</td>
                <td></td>
                <td className={cx("number")}>{money(ct40a)}</td>
              </tr>
  
              <tr>
                <td>2</td>
                <td>
                  Thuế GTGT mua vào của dự án đầu tư được bù trừ với thuế GTGT còn
                  phải nộp
                </td>
                <td>[40b]</td>
                <td></td>
                <td className={cx("number")}>{money(ct40b)}</td>
              </tr>
  
              <tr className={cx("highlight-row")}>
                <td>3</td>
                <td>Thuế GTGT còn phải nộp trong kỳ</td>
                <td>[40]</td>
                <td></td>
                <td className={cx("number")}>{money(ct40)}</td>
              </tr>
  
              <tr>
                <td>4</td>
                <td>
                  Thuế GTGT chưa khấu trừ hết kỳ này
                  <br />
                  <i>[41] = ([36] - [22] + [37] - [38] - [39a]) ≤ 0</i>
                </td>
                <td>[41]</td>
                <td></td>
                <td className={cx("number")}>{money(ct41)}</td>
              </tr>
  
              <tr>
                <td>4.1</td>
                <td>Thuế GTGT đề nghị hoàn</td>
                <td>[42]</td>
                <td></td>
                <td className={cx("number")}>{money(ct42)}</td>
              </tr>
  
              <tr>
                <td>4.2</td>
                <td>Thuế GTGT còn được khấu trừ chuyển kỳ sau</td>
                <td>[43]</td>
                <td></td>
                <td className={cx("number")}>{money(ct43)}</td>
              </tr>
            </tbody>
          </table>
  
          <p className={cx("commitment")}>
            Tôi cam đoan số liệu khai trên là đúng và chịu trách nhiệm trước pháp
            luật về số liệu đã khai./.
          </p>
  
          <div className={cx("signature-area")}>
            <div>
              <b>NHÂN VIÊN ĐẠI LÝ THUẾ</b>
              <p>Họ và tên: .......................</p>
              <p>Chứng chỉ hành nghề số: ..........</p>
            </div>
  
            <div>
              <p>..., ngày ..... tháng ..... năm {data.year}</p>
              <b>NGƯỜI NỘP THUẾ</b>
              <p>hoặc ĐẠI DIỆN HỢP PHÁP CỦA NGƯỜI NỘP THUẾ</p>
              <p>(Ký, ghi rõ họ tên; chức vụ và đóng dấu nếu có)</p>
            </div>
          </div>
  
          <div className={cx("demo-warning")}>
            Đây là tờ khai mô phỏng phục vụ khóa luận tốt nghiệp, không có giá trị
            kê khai thuế thực tế.
          </div>
        </div>
      </div>
    </div>
  );
}
