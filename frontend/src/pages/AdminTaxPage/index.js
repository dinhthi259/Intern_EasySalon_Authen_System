import { useState } from "react";
import classNames from "classnames/bind";
import TaxDeclarationList from "./components/TaxDeclarationList";
import TaxDeclarationCreate from "./components/TaxDeclarationCreate";
import TaxDeclarationDetail from "./components/TaxDeclarationDetail";
import styles from "./AdminTaxPage.module.scss";


const cx = classNames.bind(styles);

export default function AdminTaxPage() {
  const [tab, setTab] = useState("list");
  const [selectedId, setSelectedId] = useState(null);

  const openDetail = (id) => {
    setSelectedId(id);
    setTab("detail");
  };

  

  return (
    <div className={cx("tax-page")}>
      <div className={cx("tax-header")}>
        <h2>Quản lý kê khai thuế GTGT</h2>
        <p>Mô phỏng tờ khai thuế GTGT mẫu 01/GTGT</p>
      </div>

      <div className={cx("tax-tabs")}>
        <button
          className={cx("tab-btn", { active: tab === "list" })}
          onClick={() => setTab("list")}
        >
          Danh sách tờ khai
        </button>

        <button
          className={cx("tab-btn", { active: tab === "create" })}
          onClick={() => setTab("create")}
        >
          Tạo tờ khai
        </button>
      </div>

      {tab === "list" && <TaxDeclarationList onView={openDetail}/>}

      {tab === "create" && (
        <TaxDeclarationCreate onCreated={() => setTab("list")} />
      )}

      {tab === "detail" && (
        <TaxDeclarationDetail id={selectedId} onBack={() => setTab("list")} />
      )}
      
    </div>
  );
}